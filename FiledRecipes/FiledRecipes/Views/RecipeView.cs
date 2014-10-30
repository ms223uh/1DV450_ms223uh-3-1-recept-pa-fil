using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView
    {

        public virtual void Show(IRecipe recipe)
        {

            Header = recipe.Name; // Skriver ut namnet för receptet.
            ShowHeaderPanel(); // Visar bakgrundsfärg.

            Console.WriteLine();
            Console.WriteLine("INGREDIENSER");
            Console.WriteLine("══════════");
            foreach (var ingredient in recipe.Ingredients) // Skriver ut ingredienser i recptet.
            {
                Console.WriteLine(ingredient.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("INSTRUKTIONER");
            Console.WriteLine("═══════════");
            foreach (var instruction in recipe.Instructions) // Skriver ut instrutionerna i recptet.
            {
                Console.WriteLine(instruction);
            }
        }
        public virtual void Show(IEnumerable<IRecipe> recipes)
        {
            foreach (var recipe in recipes) // Visar alla recept, men bara ett åt gången. Nytt recept varje gång du trycker på en tangent.
            {
                Show(recipe);
                ContinueOnKeyPressed();
            }

        }
    }
}
