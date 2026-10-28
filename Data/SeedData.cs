namespace Forkful.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        // Seed by slug so an existing demo database gains new dishes without
        // replacing prices, availability, or products edited in the admin UI.
        var categorySeed = new[]
        {
            new Category { Name = "Bowls", Slug = "bowls", ImageUrl = "/images/hero-bowl.jpg", SortOrder = 1 },
            new Category { Name = "Pizza", Slug = "pizza", ImageUrl = "/images/menu/margherita-pizza.jpg", SortOrder = 2 },
            new Category { Name = "Pasta & Noodles", Slug = "pasta-noodles", ImageUrl = "/images/menu/pasta.jpg", SortOrder = 3 },
            new Category { Name = "Handhelds", Slug = "handhelds", ImageUrl = "/images/menu/falafel-pita.jpg", SortOrder = 4 },
            new Category { Name = "Non-Veg", Slug = "non-veg", ImageUrl = "/images/menu/chicken-bowl.jpg", SortOrder = 5 },
            new Category { Name = "Sides", Slug = "sides", ImageUrl = "/images/menu/fries.jpg", SortOrder = 6 },
            new Category { Name = "Drinks", Slug = "drinks", ImageUrl = "/images/menu/smoothie.jpg", SortOrder = 7 },
            new Category { Name = "Desserts", Slug = "desserts", ImageUrl = "/images/menu/cake.jpg", SortOrder = 8 }
        };
        var existingCategories = db.Categories.ToList();
        foreach (var category in categorySeed)
        {
            var existing = existingCategories.FirstOrDefault(c => c.Slug == category.Slug);
            if (existing is null)
                db.Categories.Add(category);
            else if ((category.Slug == "pizza" && existing.ImageUrl == "/images/menu/pizza.jpg") ||
                     (category.Slug == "handhelds" && existing.ImageUrl == "/images/menu/wrap.jpg"))
                existing.ImageUrl = category.ImageUrl; // Avoid meat photos on vegetarian category cards.
        }
        db.SaveChanges();
        var categories = db.Categories.ToDictionary(c => c.Slug);

        var kitchen = db.Suppliers.FirstOrDefault(s => s.Name == "Forkful Kitchen")
            ?? new Supplier { Name = "Forkful Kitchen", City = "Surat, Gujarat" };
        var pantry = db.Suppliers.FirstOrDefault(s => s.Name == "Surat Fresh Pantry")
            ?? new Supplier { Name = "Surat Fresh Pantry", City = "Surat, Gujarat" };
        if (kitchen.Id == 0) db.Suppliers.Add(kitchen);
        if (pantry.Id == 0) db.Suppliers.Add(pantry);
        db.SaveChanges();

        Product Dish(string name, string description, decimal price, string image, string category,
            int minutes, decimal rating, bool featured = false, bool vegetarian = true, Supplier? supplier = null) =>
            new()
            {
                Name = name, Slug = SlugHelper.From(name), Description = description, Price = price,
                ImageUrl = image, CategoryId = categories[category].Id,
                SupplierId = (supplier ?? kitchen).Id, PrepMinutes = minutes, Rating = rating,
                IsFeatured = featured, IsVegetarian = vegetarian
            };

        var dishes = new[]
        {
            // The original 15 dishes, with vegetarian-appropriate photos where needed.
            Dish("Smoky Paneer Bowl", "Chargrilled paneer, golden rice, avocado & herby drizzle.", 329, "/images/hero-bowl.jpg", "bowls", 25, 4.9m, true),
            Dish("Garden Pesto Pasta", "Basil pesto, cherry tomatoes & a generous sprinkle of parmesan.", 349, "/images/menu/pasta.jpg", "pasta-noodles", 30, 4.8m, true),
            Dish("Farmhouse Pizza", "Stone-baked crust, roasted vegetables, melty mozzarella.", 389, "/images/menu/paneer-pizza.jpg", "pizza", 30, 4.9m, true),
            Dish("Green Goddess Bowl", "Crunchy greens, chickpeas, avocado & lemon-tahini dressing.", 299, "/images/menu/salad.jpg", "bowls", 20, 4.7m, supplier: pantry),
            Dish("Chili Sesame Noodles", "Silky noodles, vegetables & our sweet-spicy sesame sauce.", 319, "/images/menu/veg-noodles.jpg", "pasta-noodles", 25, 4.8m),
            Dish("Tandoori Veg Wrap", "Warm flatbread packed with spiced vegetables & mint yogurt.", 279, "/images/menu/falafel-pita.jpg", "handhelds", 20, 4.7m),
            Dish("The Classic Burger", "A juicy patty, crisp lettuce, tomato & our house sauce.", 339, "/images/menu/burger.jpg", "non-veg", 25, 4.8m, vegetarian: false),
            Dish("Golden Cheese Toastie", "Toasty sourdough, melting cheese & a little extra comfort.", 249, "/images/menu/sandwich.jpg", "handhelds", 18, 4.6m),
            Dish("Parmesan Masala Fries", "Crispy golden fries with parmesan & a gentle masala kick.", 179, "/images/menu/fries.jpg", "sides", 15, 4.8m),
            Dish("Chocolate Fudge Cake", "Rich, chocolatey, and always worth saving room for.", 229, "/images/menu/cake.jpg", "desserts", 10, 4.9m),
            Dish("Sunset Smoothie", "A fruity, creamy pick-me-up served refreshingly cold.", 199, "/images/menu/smoothie.jpg", "drinks", 10, 4.7m),
            Dish("House Cappuccino", "Velvety espresso and steamed milk, made just right.", 149, "/images/menu/coffee.jpg", "drinks", 8, 4.8m),
            Dish("Tandoori Chicken Bowl", "Smoky grilled chicken, saffron rice, crunchy greens & mint yogurt.", 429, "/images/menu/chicken-bowl.jpg", "non-veg", 30, 4.9m, true, vegetarian: false),
            Dish("Tandoori Chicken Wrap", "Juicy chicken, pickled onion, lettuce & cooling mint sauce.", 349, "/images/menu/chicken-wrap.jpg", "non-veg", 25, 4.8m, vegetarian: false),
            Dish("Chilli Chicken Noodles", "Glazed chicken, garlic noodles, peppers & sesame.", 399, "/images/menu/chicken-noodles.jpg", "non-veg", 30, 4.8m, vegetarian: false),

            // Bowls — 8 total.
            Dish("Mediterranean Hummus Bowl", "Whipped hummus, herbed chickpeas, cucumber, pickled onion & warm pita.", 319, "/images/menu/hummus-bowl.jpg", "bowls", 20, 4.8m, supplier: pantry),
            Dish("Teriyaki Tofu Rice Bowl", "Charred tofu, steamed rice, broccoli & glossy ginger-soy glaze.", 349, "/images/menu/tofu-bowl.jpg", "bowls", 25, 4.8m),
            Dish("Harvest Quinoa Bowl", "Quinoa, sweet potato, chickpeas, leafy greens & lemon-tahini.", 329, "/images/menu/salad.jpg", "bowls", 20, 4.7m, supplier: pantry),
            Dish("Avocado Crunch Bowl", "Avocado, crisp greens, chickpeas, crunchy cabbage & herb dressing.", 319, "/images/menu/salad.jpg", "bowls", 20, 4.7m, supplier: pantry),
            Dish("Smoky Chipotle Tofu Bowl", "Spiced tofu, steamed rice, greens & a smoky chipotle drizzle.", 359, "/images/menu/tofu-bowl.jpg", "bowls", 25, 4.8m),
            Dish("Lemon Herb Chickpea Bowl", "Chickpeas, seasonal greens, sweet peppers & a bright citrus dressing.", 309, "/images/menu/salad.jpg", "bowls", 20, 4.6m, supplier: pantry),

            // Pizza — 7 total. Chicken pizza appears in Non-Veg instead.
            Dish("Classic Margherita Pizza", "Tomato, fresh basil & bubbling mozzarella on a crisp crust.", 329, "/images/menu/margherita-pizza.jpg", "pizza", 25, 4.9m, true),
            Dish("Paneer Tikka Pizza", "Spiced paneer, sweet peppers, onions & a generous cheese pull.", 429, "/images/menu/paneer-pizza.jpg", "pizza", 30, 4.9m, true),
            Dish("Smoky Paneer & Peppers Pizza", "Roasted peppers, paneer, mozzarella & a smoky tomato base.", 409, "/images/menu/paneer-pizza.jpg", "pizza", 30, 4.8m),
            Dish("Four Cheese Pizza", "Mozzarella, parmesan, cheddar & cream cheese baked until golden.", 399, "/images/menu/margherita-pizza.jpg", "pizza", 28, 4.8m),
            Dish("Basil Pesto Pizza", "Basil pesto, tomatoes, mozzarella & a finishing drizzle of olive oil.", 379, "/images/menu/margherita-pizza.jpg", "pizza", 28, 4.7m),
            Dish("Fiery Jalapeño Pizza", "Roasted peppers, jalapeños & mozzarella with a lively chilli kick.", 409, "/images/menu/paneer-pizza.jpg", "pizza", 30, 4.8m),

            // Pasta & Noodles — 8 total.
            Dish("Creamy Mushroom Alfredo", "Fettuccine, sautéed mushrooms & silky parmesan cream sauce.", 369, "/images/menu/alfredo-pasta.jpg", "pasta-noodles", 28, 4.8m, true),
            Dish("Spicy Arrabbiata Penne", "Penne in fiery tomato sauce with basil and a touch of parmesan.", 339, "/images/menu/tomato-pasta.jpg", "pasta-noodles", 25, 4.8m),
            Dish("Roasted Tomato Pasta", "Slow-roasted tomatoes, garlic, fresh herbs & twirly pasta.", 349, "/images/menu/tomato-pasta.jpg", "pasta-noodles", 25, 4.7m),
            Dish("Parmesan Herb Fettuccine", "Tender fettuccine with herbed cream, mushrooms & shaved cheese.", 379, "/images/menu/alfredo-pasta.jpg", "pasta-noodles", 28, 4.7m),
            Dish("Thai Basil Veg Noodles", "Stir-fried noodles, colorful vegetables & fragrant Thai basil.", 329, "/images/menu/veg-noodles.jpg", "pasta-noodles", 25, 4.7m),
            Dish("Garlic Sesame Veg Noodles", "Springy noodles, crisp vegetables, garlic & toasted sesame.", 339, "/images/menu/veg-noodles.jpg", "pasta-noodles", 25, 4.8m),

            // Handhelds — 7 total.
            Dish("Falafel Pita Pocket", "Crispy chickpea falafel, crunchy salad & creamy tahini in warm pita.", 279, "/images/menu/falafel-pita.jpg", "handhelds", 20, 4.8m, true),
            Dish("Crispy Paneer Burger", "Golden paneer patty, lettuce, tomato & herby sauce in a toasted bun.", 329, "/images/menu/paneer-burger.jpg", "handhelds", 25, 4.8m),
            Dish("Roasted Veg Quesadilla", "Roasted peppers, corn & melted cheese folded into toasted flatbread.", 289, "/images/menu/sandwich.jpg", "handhelds", 22, 4.7m),
            Dish("Mushroom Cheese Melt", "Sautéed mushrooms, melted cheese & herbs on crisp sourdough.", 269, "/images/menu/sandwich.jpg", "handhelds", 20, 4.7m),
            Dish("Chipotle Paneer Burger", "Grilled paneer, crunchy greens & smoky chipotle sauce in a soft bun.", 339, "/images/menu/paneer-burger.jpg", "handhelds", 25, 4.8m),

            // Non-Veg — 10 total; meat, egg and seafood stay in this category.
            Dish("Butter Chicken Rice Bowl", "Tender chicken in creamy tomato gravy, fluffy rice & fresh herbs.", 449, "/images/menu/butter-chicken.jpg", "non-veg", 30, 4.9m, true, vegetarian: false),
            Dish("Peri Peri Chicken Burger", "Juicy chicken patty, crisp lettuce & tangy peri peri mayo.", 379, "/images/menu/burger.jpg", "non-veg", 25, 4.8m, vegetarian: false),
            Dish("Garlic Chicken Noodles", "Chicken, springy noodles, greens & a savory garlic glaze.", 409, "/images/menu/chicken-noodles.jpg", "non-veg", 30, 4.8m, vegetarian: false),
            Dish("Lemon Herb Chicken Wrap", "Grilled chicken, crunchy slaw & a bright lemon-herb sauce.", 359, "/images/menu/wrap.jpg", "non-veg", 25, 4.7m, vegetarian: false),
            Dish("Peri Peri Chicken Pizza", "Spicy chicken, red onion, peppers & molten mozzarella.", 449, "/images/menu/pizza.jpg", "non-veg", 30, 4.8m, vegetarian: false),
            Dish("Prawn & Egg Ramen", "Juicy prawns, a jammy egg, greens & ginger-sesame broth.", 459, "/images/menu/noodles.jpg", "non-veg", 28, 4.8m, vegetarian: false),

            // Sides — 6 total.
            Dish("Peri Peri Fries", "Hot, crisp fries dusted with our bold peri peri seasoning.", 189, "/images/menu/fries.jpg", "sides", 15, 4.7m),
            Dish("Cheesy Garlic Bread", "Toasted garlic bread with herbs & a blanket of melty cheese.", 199, "/images/menu/garlic-bread.jpg", "sides", 15, 4.8m),
            Dish("Loaded Veg Nachos", "Crisp tortilla chips, cheese, beans, jalapeños & cool guacamole.", 249, "/images/menu/nachos.jpg", "sides", 18, 4.8m),
            Dish("Garlic Parmesan Bites", "Warm golden garlic bites with parmesan & a tomato dip.", 209, "/images/menu/garlic-bread.jpg", "sides", 15, 4.7m),
            Dish("Nachos & Guacamole", "Crunchy tortilla chips with creamy guacamole & fresh salsa.", 229, "/images/menu/nachos.jpg", "sides", 15, 4.7m),

            // Drinks — 5 total.
            Dish("Iced Matcha Latte", "Earthy matcha, chilled milk & plenty of ice.", 219, "/images/menu/matcha-latte.jpg", "drinks", 8, 4.8m),
            Dish("Mint Lime Fizz", "Fresh lime, mint & sparkling soda poured over ice.", 169, "/images/menu/lemon-fizz.jpg", "drinks", 8, 4.7m),
            Dish("Chocolate Banana Smoothie", "Banana, cocoa & cold milk blended smooth and creamy.", 229, "/images/menu/smoothie.jpg", "drinks", 10, 4.8m),

            // Desserts — 5 total.
            Dish("Berry Cheesecake Slice", "Creamy cheesecake on a buttery base with a glossy berry topping.", 259, "/images/menu/berry-cheesecake.jpg", "desserts", 10, 4.9m),
            Dish("Warm Chocolate Brownie", "A fudgy chocolate square served warm for an extra cozy finish.", 219, "/images/menu/cake.jpg", "desserts", 10, 4.8m),
            Dish("Churros & Chocolate", "Cinnamon-sugar churros with a cup of rich chocolate dip.", 229, "/images/menu/churros.jpg", "desserts", 12, 4.8m),
            Dish("Tropical Fruit Parfait", "Creamy yogurt, crunchy granola & a colorful mix of fruit.", 239, "/images/menu/smoothie.jpg", "desserts", 10, 4.7m)
        };
        var existingSlugs = db.Products.Select(p => p.Slug).ToHashSet();
        foreach (var dish in dishes)
            if (existingSlugs.Add(dish.Slug)) db.Products.Add(dish);

        // Correct only original stock-photo references; preserve admin-changed photos.
        void UpdateSeedImage(string slug, string previous, string updated)
        {
            var product = db.Products.FirstOrDefault(p => p.Slug == slug);
            if (product?.ImageUrl == previous) product.ImageUrl = updated;
        }
        UpdateSeedImage("farmhouse-pizza", "/images/menu/pizza.jpg", "/images/menu/paneer-pizza.jpg");
        UpdateSeedImage("tandoori-veg-wrap", "/images/menu/wrap.jpg", "/images/menu/falafel-pita.jpg");
        UpdateSeedImage("chili-sesame-noodles", "/images/menu/noodles.jpg", "/images/menu/veg-noodles.jpg");

        // Keep the original non-veg burger inside its dedicated category on upgrades.
        var burger = db.Products.FirstOrDefault(p => p.Slug == "the-classic-burger");
        if (burger is not null && burger.CategoryId != categories["non-veg"].Id)
        {
            burger.CategoryId = categories["non-veg"].Id;
            burger.IsVegetarian = false;
        }
        var oldFeaturedBowl = db.Products.FirstOrDefault(p => p.Slug == "green-goddess-bowl");
        if (oldFeaturedBowl is not null && oldFeaturedBowl.IsFeatured)
            oldFeaturedBowl.IsFeatured = false;
        db.SaveChanges();
    }
}
