﻿using Assignment_Webshop.Models;
using Inlämningsuppgift_Webshop;
using Microsoft.EntityFrameworkCore;

namespace Assignment_Webshop;

internal class Start
{
    private static List<string> _list = new List<string>
    {
        "'H'ome",
        "'C'ategories.",
        "'P'roducts.",
        "'S'earch product"
    };
    private static Window _menu = new Window("Main Menu", 25, 0, _list);
    public static List<Product> ProductList = new List<Product>();
    public static Window PageWindow = new Window(2, 10);

    public static async void Page(Task checkLogin)
    {
        Banner();
        _menu.Draw();

        if (!checkLogin.IsCompleted)
        {
            await checkLogin;
        }

        Basket.DrawBasket();

        switch (Program.ActiveSubPage)
        {
            case SubPage.Default:
                PageWindow.Header = "";
                FeaturedProducts();
                break;
            case SubPage.Categories:
                ListCategories();
                break;
            case SubPage.CategoryProducts:
                ListCategoryProducts();
                break;
            case SubPage.Search:
                SearchProducts();
                break;
            case SubPage.Products:
                ListProducts();
                break;
            case SubPage.ProductDetails:
                ProductDetails();
                break;
        }
    }

    public static void SelectItem(ConsoleKey key)
    {
        if (key == ConsoleKey.RightArrow)
        {
            switch (Program.ActiveSubPage)
            {
                case SubPage.Categories:
                    Program.ActiveSubPage = SubPage.CategoryProducts;
                    break;
                case SubPage.Products:
                    Program.ActiveSubPage = SubPage.ProductDetails;
                    break;
            }
        }

        if (key == ConsoleKey.LeftArrow)
        {
            switch (Program.ActiveSubPage)
            {
                case SubPage.ProductDetails:
                    Program.ActiveSubPage = SubPage.Products;
                    break;
                case SubPage.CategoryProducts:
                    Program.ActiveSubPage = SubPage.Categories;
                    break;
                case SubPage.Products:
                    ProductList.Clear();
                    break;
            }
        }
    }

    private static void ListProducts()
    {
        PageWindow.Header = "Products - ↑↓ Navigate - → Select";

        if (!ProductList.Any())
        {
            using (var db = new AdvNookContext())
            {
                ProductList = db.Products.ToList();
            }
        }

        PageWindow.TextRows = ProductList.Select(p => $"{p.Name} - {p.Price.ToString("C")}").ToList();
        PageWindow.Navigate();
    }


    internal static void ProductDetails()
    {
        using (var db = new AdvNookContext())
        {
            string selectedRow = PageWindow.TextRows[(int)PageWindow.SelectedIndex];
            Product product = db.Products
                                .Include(p => p.Categories)
                                .FirstOrDefault(p => selectedRow.Contains(p.Name));

            List<string> details = new List<string>
            {
                $"Price: {product.Price.ToString("C")}",
                $"Categories: {string.Join(", ", product.Categories.Select(c => c.Name))}",
                ""
            };

            details.AddRange(Methods.WrapText(product.Description, 40));

            details.AddRange("", "'Enter' to add to cart");

            Window productDetailWindow = new Window($"{product.Name}", 50, 10, details);
            productDetailWindow.Draw();
            PageWindow.Draw();

            if (Program.KeyInfo.Key == ConsoleKey.Enter)
            {
                AddToBasket(product, db);
            }
        }
    }
    internal static void AddToBasket(Product product, AdvNookContext? db = null)
    {
        if (product.Amount <= 0)
        {
            Window noStockWindow = new Window("Stock Error", $"{product.Name} is out of stock and could not be added.");
            noStockWindow.Draw();
            return;
        }

        // Hitta den aktuella användarens varukorg
        var userBasket = Login.ActiveUser != null
            ? db.Baskets.Include(b => b.BasketProducts).ThenInclude(bp => bp.Product).FirstOrDefault(b => b.Id == Login.ActiveUser.Basket.Id)
            : Basket.GuestBasket;  // För gäst användare

        if (userBasket != null)
        {
            // Hitta den aktuella produktens basket product
            var existingProduct = userBasket.BasketProducts.FirstOrDefault(bp => bp.ProductId == product.Id);

            if (existingProduct != null)
            {
                // Om produkten redan finns i varukorgen, uppdatera kvantiteten
                existingProduct.Quantity++;
            }
            else
            {
                // Om produkten inte finns i varukorgen, skapa en ny post med kvantitet 1
                userBasket.BasketProducts.Add(new BasketProduct
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1
                });
            }

            db.SaveChanges();
        }
    }

    private static void ListCategories()
    {
        PageWindow.Header = "Categories - ↑↓ Navigate - → Select";

        using (var db = new AdvNookContext())
        {
            var categories = db.Categories.ToList();
            PageWindow.TextRows = categories.Select(c => c.Name).ToList();
        }

        PageWindow.Navigate();
    }

    internal static void ListCategoryProducts()
    {
        using (var db = new AdvNookContext())
        {
            int categoryId = (int)PageWindow.SelectedIndex;
            var category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.Id == categoryId);

            if (category != null)
            {
                ProductList = category.Products.ToList();
                PageWindow.TextRows = ProductList.Select(p => $"{p.Name} - {p.Price.ToString("C")}").ToList();

                Program.ActiveSubPage = SubPage.Products;
                PageWindow.SelectedIndex = 0;
            }
            else
            {
                Window errorWindow = new Window("ERROR", "Category is empty.");
                errorWindow.Draw();
            }
        }
    }

    internal static void SearchProducts()
    {
        Window searchWindow = new Window("Search Products", "");
        searchWindow.Draw();

        Console.SetCursorPosition(51, 21);
        string searchTerm = Console.ReadLine();

        using (var db = new AdvNookContext())
        {
            var searchResults = db.Products
                                  .Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%") ||
                                              EF.Functions.Like(p.Description, $"%{searchTerm}%"))
                                  .ToList();

            ProductList.Clear();
            if (searchResults.Any())
            {
                ProductList.AddRange(searchResults);
            }
            else
            {
                searchWindow.TextRows = new List<string> { "No results found." };
                searchWindow.Draw();
            }
        }

        Program.ActiveSubPage = SubPage.Products;
    }

    private static void FeaturedProducts()
    {
        List<Product> featuredProducts = new List<Product>();
        using (var db = new AdvNookContext())
        {
            featuredProducts = db.Products
                                  .Where(p => p.Featured)
                                  .Take(3)
                                  .ToList();
        }

        while (featuredProducts.Count < 3)
        {
            featuredProducts.Add(new Product { Name = "No featured product", Price = 0, Featured = false });
        }

        char[] xyz = { 'X', 'Y', 'Z' };
        for (int i = 0; i < 3; i++)
        {
            var product = featuredProducts[i];
            List<string> featureDetails = new List<string>
        {
            product.Name.PadRight(30),
            product.Price.ToString("C"),
        };
            featureDetails.AddRange(Methods.WrapText(product.Description, 30));

            Window featuredWindow = new Window($"{xyz[i]}", 2 + (35 * i), 10, featureDetails);
            featuredWindow.Draw();
        }

        switch (Program.KeyInfo.Key)
        {
            case ConsoleKey.X:
            case ConsoleKey.Y:
            case ConsoleKey.Z:
                int selectedProductIndex = Array.IndexOf(xyz, Program.KeyInfo.KeyChar);
                if (selectedProductIndex >= 0 && selectedProductIndex < featuredProducts.Count)
                {
                    // Lägg till produkten i varukorgen
                    AddToBasket(featuredProducts[selectedProductIndex]);
                }
                break;
        }
    }

    private static void Banner()
    {
        List<string> banner = new List<string>
        {
            "╔═╗╔╦╗╦  ╦╔═╗╔╗╔╔╦╗╦ ╦╦═╗╔═╗  ╔╗╔╔═╗╔═╗╦╔═",
            "╠═╣ ║║╚╗╔╝║╣ ║║║ ║ ║ ║╠╦╝║╣   ║║║║ ║║ ║╠╩╗",
            "╩ ╩═╩╝ ╚╝ ╚═╝╝╚╝ ╩ ╚═╝╩╚═╚═╝  ╝╚╝╚═╝╚═╝╩ ╩"
        };
        int bannerLength = banner[0].Length;
        int leftPos = (Console.WindowWidth - bannerLength) / 2;
        Window title = new Window("", leftPos, 0, banner);
        title.Draw();
    }
}
