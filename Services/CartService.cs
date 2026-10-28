using System.Text.Json;

namespace Forkful.Services;

public class CartService(IHttpContextAccessor accessor, AppDbContext db,
    DishCustomizationService customizer)
{
    private const string Key = "forkful-cart";
    private ISession Session => accessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Cart requires an active session.");

    public List<CartLine> GetLines()
    {
        var raw = Session.GetString(Key);
        if (string.IsNullOrWhiteSpace(raw)) return new();
        try
        {
            var lines = JsonSerializer.Deserialize<List<CartLine>>(raw) ?? new();
            // Upgrade existing session carts from the first version of the demo.
            var changed = false;
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line.LineId)) continue;
                line.LineId = Guid.NewGuid().ToString("N");
                changed = true;
            }
            if (changed) Save(lines);
            return lines;
        }
        catch (JsonException) { return new(); }
    }

    private void Save(List<CartLine> lines) => Session.SetString(Key, JsonSerializer.Serialize(lines));
    public int Count => GetLines().Sum(x => x.Quantity);

    public void Add(int productId, int quantity, string spiceLevel,
        IReadOnlyList<string> addOnCodes, string? instructions)
    {
        var lines = GetLines();
        var codes = addOnCodes.OrderBy(code => code, StringComparer.Ordinal).ToList();
        var existing = lines.FirstOrDefault(line => line.ProductId == productId &&
            line.SpiceLevel == spiceLevel &&
            string.Equals(line.SpecialInstructions, instructions, StringComparison.Ordinal) &&
            (line.AddOnCodes ?? new()).OrderBy(code => code, StringComparer.Ordinal).SequenceEqual(codes));
        if (existing is null)
        {
            lines.Add(new CartLine
            {
                ProductId = productId, Quantity = Math.Clamp(quantity, 1, 20),
                SpiceLevel = spiceLevel, AddOnCodes = codes, SpecialInstructions = instructions
            });
        }
        else existing.Quantity = Math.Clamp(existing.Quantity + quantity, 1, 20);
        Save(lines);
    }

    public void Update(string lineId, int quantity)
    {
        var lines = GetLines();
        var line = lines.FirstOrDefault(x => x.LineId == lineId);
        if (line is null) return;
        if (quantity <= 0) lines.Remove(line);
        else line.Quantity = Math.Clamp(quantity, 1, 20);
        Save(lines);
    }

    public void Remove(string lineId)
    {
        var lines = GetLines();
        lines.RemoveAll(x => x.LineId == lineId);
        Save(lines);
    }

    public void Clear() => Session.Remove(Key);

    public async Task<CartViewModel> GetSummaryAsync()
    {
        var lines = GetLines();
        var ids = lines.Select(x => x.ProductId).ToArray();
        var products = await db.Products.AsNoTracking().Include(x => x.Category)
            .Where(x => ids.Contains(x.Id) && x.IsAvailable).ToDictionaryAsync(x => x.Id);
        return new CartViewModel
        {
            Items = lines.Where(x => products.ContainsKey(x.ProductId))
                .Select(line =>
                {
                    var product = products[line.ProductId];
                    var selected = customizer.GetAddOns(product)
                        .Where(option => (line.AddOnCodes ?? new()).Contains(option.Code, StringComparer.Ordinal))
                        .ToList();
                    return new CartItemViewModel
                    {
                        LineId = line.LineId, Product = product, Quantity = line.Quantity,
                        SpiceLevel = customizer.IsValidSpice(product, line.SpiceLevel)
                            ? customizer.NormalizeSpice(product, line.SpiceLevel)
                            : customizer.NormalizeSpice(product, null),
                        AddOns = selected, SpecialInstructions = line.SpecialInstructions
                    };
                }).ToList()
        };
    }
}
