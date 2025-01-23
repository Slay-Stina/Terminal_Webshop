﻿using Inlämningsuppgift_Webshop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Inlämningsuppgift_Webshop;

internal class Login
{
    public static User? ActiveUser { get; set; }
    public static string DefaultHeader = "Logga in";
    public static string ActiveHeader = "Inloggad";
    public static Window Window { get; set; } = new Window(125,0);
    public static List<string> DefaultText = new List<string> 
    {
        "Tryck 'L' för att logga in.",
        "Tryck 'A' för ny användare"
    };
    public static void Prompt()
    {
        List<string> loginText = new List<string>
        {
        "Användarnamn:".PadRight(27),
        ""
        };
        Window.TextRows = loginText;
        Window.Draw();
        Console.SetCursorPosition(127, 2);
        string username = Console.ReadLine();
        using (var db = new AdvNookContext())
        {
            try
            {
                ActiveUser = db.Users.Where(u => u.Username == username).FirstOrDefault();
                if(ActiveUser != null)
                ActiveUser.LoggedIn = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Användaren hittades inte, försök igen eller lägg till ny användare.");
                Console.WriteLine(ex.Message);
            }
            db.SaveChanges();
        }
    }

    internal static void DrawLogin()
    {
        Window.Header = ActiveUser is null ? DefaultHeader : ActiveHeader;
        Window.TextRows = ActiveUser is null ? DefaultText : new List<string>
        {ActiveUser.FirstName + " " + ActiveUser.LastName,
        "",
        "Tryck 'L' för att logga ut."};
        Window.Draw();
    }
}
