using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        public void Load()
        {
            // Skapar en lista över recepten
            List<IRecipe> recipes = new List<IRecipe>(); // Skapar en lista över recepten.
            RecipeReadStatus status = RecipeReadStatus.Indefinite; 
            Recipe recipe = null; // Recept måste ha ett värde.
        
        using (StreamReader reader = new StreamReader(_path)) // StreamReader måste användas för att läsa en textfil. Läser ifrån _path.
            {

                string line; // Döper texten (objektet string) till line.

                while ((line = reader.ReadLine()) != null) // Läser hela textfilen tills det inte finns något kvar att läsa.
                {

                    if (line != "") // Om raden inte är tom gör följande....
                    {
                        if (line == SectionRecipe) // Om line stämmer överrens med SectionRecipe så skapas ett nytt recept/menyalternativ
                        {
                            status = RecipeReadStatus.New; 
                        }

                        else if (line == SectionIngredients) // Om det finns text i line under SectionIngredients visa text.
                        {
                            status = RecipeReadStatus.Ingredient;
                        }

                        else if (line == SectionInstructions) // Om det finns text i line under SectionInstructions visa text.
                        {
                            status = RecipeReadStatus.Instruction;
                        }


                        else // Om raden är tom så skapas ett nytt recept.
                        {
                            if (status == RecipeReadStatus.New) 
                            {
                                recipe = new Recipe(line); // Skapar ett nytt recept (text).
                                recipes.Add(recipe); // Lägger det nya receptet i listan.
                            }


                            else if (status == RecipeReadStatus.Ingredient) // Om Ingredient är tom så skapas nya ingredienser.
                            {

                                string[] Ingredient = line.Split(';'); // Tar bort ';' med hjälp av split. Array

                                if (Ingredient.Length != 3) // Om längden inte lika 3 så kastas ett nytt undantag.
                                {
                                    throw new FileFormatException();
                                }

                                Ingredient newIngredient = new Ingredient(); // Skapar ett nytt objekt.
                                newIngredient.Amount = Ingredient[0]; // Initierar platshållaren [0] i arrayen med "mängd"
                                newIngredient.Measure = Ingredient[1]; // Initierar platshållaren [1] i arrayen med "mått"
                                newIngredient.Name = Ingredient[2]; // Initierar platshållaren [2] i arrayen med "namn"
                                recipe.Add(newIngredient); // Lägger till alla värden i objektet.

                            }

                            else if (status == RecipeReadStatus.Instruction) // Om raden är tom så skapas nya instruktioner.
                            {
                                recipe.Add(line); // Lägger till instruktioner till strängen.
                            }

                            else
                            {
                                throw new FileFormatException(); // Annars så kastas ett nytt udantag.
                            } 
                        }


                        _recipes = recipes.OrderBy(r => r.Name).ToList(); // Det privata fältet _recipes är lika med listan (sorterad)
                        
                        OnRecipesChanged(EventArgs.Empty);
                        IsModified = false;

                    }
               


                    
                    }
            
            
            }

        }


        public void Save()
        {

            using (StreamWriter writer = new StreamWriter(_path)) // StreamWriter används för att kunna skriva till en textfil.
            {


                foreach (var recipe in _recipes) // För varje recept i _recept skriv till SectionRecipe & name i listan.
                {
                    writer.WriteLine(SectionRecipe);
                    writer.WriteLine(recipe.Name);



                    writer.WriteLine(SectionIngredients); // För varje newIngredient i listan för ingredienser så skrivs värdena ut i sina platshållare.
                    foreach (var newIngredient in recipe.Ingredients)
                    {
                        writer.WriteLine("{0};{1};{2}", newIngredient.Amount, newIngredient.Measure, newIngredient.Name);
                    }


                    writer.WriteLine(SectionInstructions); // För varje instruktion i listan för instruktioner skriv in det nya värdet.
                foreach (var instruction in recipe.Instructions)
                {
                    writer.WriteLine(instruction);


                }            
            
                
                }

                OnRecipesChanged(EventArgs.Empty);
                IsModified = false;


        }
    }
    }
    }