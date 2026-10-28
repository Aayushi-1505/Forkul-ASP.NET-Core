# forkful. — customizable food-ordering website

A responsive **ASP.NET Core MVC (.NET 8)** food-ordering course project. Browse **56 seeded dishes** across eight categories, search and sort the paginated menu, filter Veg/Non-Veg, customize each dish, add it to a session cart, and place a demo delivery or pickup order. Product management forms and a JSON CRUD API share the same EF Core data model.

> **Classroom demo, not a real restaurant.** No actual payment is taken and no food is delivered. Admin forms and write APIs are intentionally open for learning; add authentication, authorization, rate limiting and production security before public deployment.

## Run in VS Code

1. Install the **.NET 8 SDK** and VS Code's **C# Dev Kit** extension.
2. In VS Code select **File → Open Folder** and choose this `Forkful` folder (the one containing `Forkful.csproj`).
3. Open **Run and Debug** (`Ctrl+Shift+D`), choose **Forkful MVC (HTTP 5000)** and press **F5**.
4. Visit **http://localhost:5000** if the browser does not open automatically.

For HTTPS, use the **Forkful MVC (HTTPS 5001 + HTTP 5000)** debug option. Run `dotnet dev-certs https --trust` if your browser warns about the development certificate. The included `.vscode/launch.json`, `.vscode/tasks.json`, and `Properties/launchSettings.json` configure both ports.

Or run from **Terminal → New Terminal** in VS Code:

```bash
dotnet restore
dotnet run --launch-profile http
# Or: dotnet run --launch-profile https
```

Stop with `Ctrl+C`. The project uses **SQLite by default** and creates/seeds the database automatically; MySQL is optional.

### Existing databases

**Going from the previous 15-dish version to this 56-dish menu does not require a reset.** The startup seed inserts dishes whose slugs are missing, preserves existing orders and admin-created products, and updates three original vegetarian photo references only when they still point to the old stock images. Restart the app to load the new catalog.

If you are instead upgrading a much older copy *without the customization columns on `OrderItems`*, **stop the app, back up any orders you want to keep, then delete `forkful.db`** and restart. `EnsureCreated()` does not modify tables in an existing database. That reset deletes old local orders. For an older MySQL schema, use a proper EF Core migration or recreate the demo database after backing it up.

## Try the website

1. Open **Explore menu**. Choose a category—including the dedicated **Non-Veg** category—or the **All / Veg / Non-Veg** dietary switch. The 56-item catalog displays **12 dishes per page**; numbered page links retain your search, category, diet and sort choices, while changing a filter starts again at page 1.
2. Choose a dish and click **Customize**. Savory dishes offer **Mild / Medium / Hot** spice. Each dish has relevant optional add-ons and a special-instructions field. Drinks and desserts skip the spice selector.
3. Watch the estimated total update as you choose add-ons or quantity. Click **Add to cart**. The server validates add-on codes and recalculates prices; it will not let a vegetarian dish acquire a meat add-on.
4. In the cart, the same dish with different customizations appears as separate lines. Change quantity or remove one variant without affecting the other.
5. Check out for delivery or pickup. Your spice level, extras, instructions and final price are saved with the order and shown on the confirmation page.

Delivery costs ₹39 below ₹599 and is free at or above ₹599; pickup is free. The site uses INR and Surat local time (Asia/Kolkata).

| Menu category | Seeded dishes |
| --- | ---: |
| Bowls | 8 |
| Pizza | 7 |
| Pasta & Noodles | 8 |
| Handhelds | 7 |
| Non-Veg | 10 |
| Sides | 6 |
| Drinks | 5 |
| Desserts | 5 |
| **Total** | **56** |

## Main routes

| Route | Purpose |
| --- | --- |
| `/` | Home storefront (`HomeController.Index()` returns `View("MyView")`) |
| `/menu` | Paginated menu with search, category, diet and sorting filters |
| `/menu?page=2` | Second page of 12 items; page links keep active filters |
| `/menu?category=non-veg` | Dedicated non-vegetarian section |
| `/menu?diet=veg` / `/menu?diet=nonveg` | Veg/Non-Veg filter |
| `/menu/{slug}` | Dish details: spice, add-ons, notes and quantity |
| `/cart` | Session cart with per-variant quantities |
| `/checkout` | Validated delivery/pickup checkout |
| `/order/success/{guid}` | Order confirmation and saved customization details |
| `/admin/products` | MVC product create/read/update/delete forms (demo only) |
| `/api/products` | JSON product CRUD API (demo only) |
| `/greetings` | Time-aware greeting page retained from the syllabus |

The customer-facing **Learning Lab page and its routes have been removed**. The storefront and core MVC/EF concepts remain; there is no Learning Lab link in the navigation or footer.

## Switch to MySQL (optional)

The same EF Core `AppDbContext` and `Product`, `Category`, `Supplier`, `Order` and `OrderItem` models work with SQLite or MySQL. Start a local MySQL 8 server, or use the included development-only Compose file:

```bash
docker compose up -d mysql
```

In Bash, set the provider and connection string before running:

```bash
export Database__Provider=MySql
export ConnectionStrings__MySql='server=localhost;port=3306;database=forkful;user=forkful;password=forkful_dev_only'
dotnet run --launch-profile http
```

In PowerShell:

```powershell
$env:Database__Provider = 'MySql'
$env:ConnectionStrings__MySql = 'server=localhost;port=3306;database=forkful;user=forkful;password=forkful_dev_only'
dotnet run --launch-profile http
```

Change the sample password outside local development. Pomelo's EF Core MySQL provider creates the demo schema from the models with `Database.EnsureCreated()` and seeds missing dishes. **`EnsureCreated` is for this demo, not schema migrations**; use migrations for evolving production databases.

### Example API requests

```bash
curl http://localhost:5000/api/products
curl http://localhost:5000/api/products/1

# Vegetarian dish in Bowls (category 1)
curl -X POST http://localhost:5000/api/products \
  -H 'Content-Type: application/json' \
  -d '{"name":"Lemon Herb Bowl","description":"Fresh greens, herbs and a bright citrus dressing.","price":285,"imageUrl":"/images/menu/salad.jpg","categoryId":1,"supplierId":1,"prepMinutes":20,"rating":4.7,"isVegetarian":true,"isFeatured":false,"isAvailable":true}'

# Non-vegetarian dishes must use the Non-Veg category (category 5)
# Use the returned product ID for PUT /api/products/{id} or DELETE /api/products/{id}.
```

The product manager and write API reject inconsistent category/diet combinations. The write API is **open for coursework only**.

## Implementation notes

- `Services/DishCustomizationService.cs` defines allowed, category-appropriate add-ons and prices. The browser posts only add-on codes; prices are resolved on the server each time the cart is displayed and again when an order is saved.
- `Services/CartService.cs` stores a unique line ID per configuration in session, so separate spice/add-on/note combinations remain separate.
- `ViewModels/StoreViewModels.cs` and `CheckoutController` demonstrate attribute-based and explicit validation. `HomeController` demonstrates `ViewData` and `ViewBag`; cart feedback uses `TempData`.
- The home page uses `IMemoryCache` for featured dishes; response-caching services remain configured. EF Core code-first is used for the catalog and orders.
- The hero, three original chicken photos and ten additional dish photos were generated for this project. Other menu photos are from [Unsplash](https://unsplash.com/license); photos are illustrative, and some related dishes share a photo. Local DM Sans / DM Serif Display fonts have OFL license texts in `wwwroot/fonts/`. Local jQuery and validation assets have their license texts in `wwwroot/vendor/`. The site needs no CDN to render.
- Example orders/payment, public admin access and API writes are **not production-ready**. Use proper authentication, payments, secrets management, migrations and a secure deployment workflow for a real business.
