namespace Forkful.Services;

public record AddOnOption(string Code, string Name, decimal Price, bool IsVegetarian = true);

/// <summary>
/// Central source of truth for allowed extras and their prices. The browser sends
/// only option codes; the server resolves and prices them again for every cart view.
/// Vegetarian dishes never offer meat or egg add-ons.
/// </summary>
public class DishCustomizationService
{
    public static readonly IReadOnlyList<string> SpiceLevels = new[] { "Mild", "Medium", "Hot" };

    private static readonly AddOnOption[] BowlExtras =
    {
        new("avocado", "Extra avocado", 55),
        new("paneer", "Extra paneer", 75),
        new("mint-dip", "Mint yogurt dip", 25)
    };
    private static readonly AddOnOption[] PizzaExtras =
    {
        new("cheese", "Extra mozzarella", 55),
        new("olives", "Olives", 35),
        new("paneer", "Extra paneer", 75)
    };
    private static readonly AddOnOption[] PastaExtras =
    {
        new("cheese", "Extra parmesan", 45),
        new("mushrooms", "Sautéed mushrooms", 45),
        new("paneer", "Extra paneer", 75)
    };
    private static readonly AddOnOption[] HandheldExtras =
    {
        new("cheese", "Extra cheese", 45),
        new("avocado", "Extra avocado", 55),
        new("mint-dip", "Mint yogurt dip", 25)
    };
    private static readonly AddOnOption[] NonVegExtras =
    {
        new("chicken", "Extra chicken", 90, false),
        new("egg", "Fried egg", 45, false),
        new("cheese", "Extra cheese", 45),
        new("mint-dip", "Mint yogurt dip", 25)
    };
    private static readonly AddOnOption[] PrawnRamenExtras =
    {
        new("prawns", "Extra prawns", 105, false),
        new("egg", "Extra egg", 45, false),
        new("chili-oil", "Chilli oil", 25)
    };
    private static readonly AddOnOption[] SideExtras =
    {
        new("cheese", "Extra parmesan", 45),
        new("mint-dip", "Mint yogurt dip", 25),
        new("spicy-sauce", "Spicy sauce", 25)
    };
    private static readonly AddOnOption[] SmoothieExtras =
    {
        new("fruit", "Extra fruit", 35),
        new("chia", "Chia seeds", 30),
        new("cream", "Whipped cream", 25)
    };
    private static readonly AddOnOption[] CoffeeExtras =
    {
        new("espresso", "Extra espresso shot", 45),
        new("oat-milk", "Oat milk", 35),
        new("vanilla", "Vanilla syrup", 25)
    };
    private static readonly AddOnOption[] MatchaExtras =
    {
        new("matcha", "Extra matcha", 45),
        new("oat-milk", "Oat milk", 35),
        new("vanilla", "Vanilla syrup", 25)
    };
    private static readonly AddOnOption[] FizzExtras =
    {
        new("lime", "Extra lime", 20),
        new("mint", "Extra mint", 20),
        new("ginger", "Fresh ginger", 25)
    };
    private static readonly AddOnOption[] DessertExtras =
    {
        new("ice-cream", "Vanilla ice cream", 65),
        new("chocolate", "Chocolate drizzle", 30),
        new("cream", "Whipped cream", 25)
    };

    public bool SupportsSpice(Product product) =>
        product.Category?.Slug is not "drinks" and not "desserts";

    public IReadOnlyList<AddOnOption> GetAddOns(Product product) => product.Category?.Slug switch
    {
        "bowls" => BowlExtras,
        "pizza" => PizzaExtras,
        "pasta-noodles" => PastaExtras,
        "handhelds" => HandheldExtras,
        "non-veg" when product.Slug.Contains("prawn", StringComparison.OrdinalIgnoreCase) => PrawnRamenExtras,
        "non-veg" => NonVegExtras,
        "sides" => SideExtras,
        "drinks" when product.Slug.Contains("cappuccino", StringComparison.OrdinalIgnoreCase) => CoffeeExtras,
        "drinks" when product.Slug.Contains("matcha", StringComparison.OrdinalIgnoreCase) => MatchaExtras,
        "drinks" when product.Slug.Contains("fizz", StringComparison.OrdinalIgnoreCase) => FizzExtras,
        "drinks" => SmoothieExtras,
        "desserts" => DessertExtras,
        _ => Array.Empty<AddOnOption>()
    };

    public bool IsValidSpice(Product product, string? spice) => SupportsSpice(product)
        ? string.IsNullOrEmpty(spice) || SpiceLevels.Contains(spice, StringComparer.Ordinal)
        : string.IsNullOrEmpty(spice) || spice == "None";

    public string NormalizeSpice(Product product, string? spice) =>
        SupportsSpice(product) ? (string.IsNullOrEmpty(spice) ? "Medium" : spice) : "None";

    public bool TryResolveAddOns(Product product, IEnumerable<string>? codes,
        out List<AddOnOption> selected)
    {
        selected = new();
        var requested = (codes ?? Array.Empty<string>()).Distinct(StringComparer.Ordinal).ToList();
        var available = GetAddOns(product);
        if (requested.Count > available.Count ||
            requested.Any(code => !available.Any(option => option.Code == code))) return false;
        selected = available.Where(option => requested.Contains(option.Code, StringComparer.Ordinal)).ToList();
        return true;
    }
}
