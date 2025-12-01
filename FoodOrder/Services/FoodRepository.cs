using FoodOrder.Models;

namespace FoodOrder.Services
{
    public class FoodRepository
    {
        public static List<FoodCategory> Categories = new()
        {
            new FoodCategory
            {
                Id = 1,
                Name = "Pizzas",
                Description = "Handmade pizzas",
                Foods = new List<Food>
                {
                    new Food { Id = 1, Name = "Margherita", Description = "Cheese & Tomato", Price = 99, Type = FoodType.MainCourse },
                    new Food { Id = 2, Name = "Pepperoni", Description = "Spicy pepperoni", Price = 120, Type = FoodType.MainCourse }
                }
            },
            new FoodCategory
            {
                Id = 2,
                Name = "Burgers",
                Description = "Fresh beef burgers",
                Foods = new List<Food>
                {
                    new Food { Id = 3, Name = "Classic Burger", Description = "Beef patty, Bread, Pickle", Price = 75, Type = FoodType.MainCourse },
                    new Food { Id = 4, Name = "Cheese Burger", Description = "Beef patty with cheese", Price = 85, Type = FoodType.MainCourse }
                }
            },
            new FoodCategory
            {
                Id = 3,
                Name = "Drinks",
                Description = "Cold beverages",
                Foods = new List<Food>
                {
                    new Food { Id = 5, Name = "Coke", Description = "Refreshing drink", Price = 10, Type = FoodType.Drink },
                    new Food { Id = 6, Name = "Water", Description = "Still water", Price = 5, Type = FoodType.Drink }
                }
            }
        };
        public static List<Food> Foods =>
        Categories.SelectMany(c => c.Foods).ToList();
    }
}
