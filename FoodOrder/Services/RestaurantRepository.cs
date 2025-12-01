using FoodOrder.Models;

namespace FoodOrder.Services
{
    public class RestaurantRepository
    {
        public static List<Restaurant> Restaurants = new()
        {
            new Restaurant
            {
                Id = 1,
                Name = "Shila",
                Address = "Jordan",
                Categories = new List<FoodCategory>
                {
                    new FoodCategory
                    {
                        Id = 1,
                        Name = "Pizzas",
                        Description = "Handmade pizzas",
                        Foods = new List<Food>
                        {
                            new Food { Id = 1, Name = "Margherita", Description = "Cheese & Tomato", Price = 899, Type = FoodType.MainCourse },
                            new Food { Id = 2, Name = "Pepperoni", Description = "Spicy pepperoni", Price = 999, Type = FoodType.MainCourse }
                        }
                    },
                    new FoodCategory
                    {
                        Id = 2,
                        Name = "Drinks",
                        Description = "Cold beverages",
                        Foods = new List<Food>
                        {
                            new Food { Id = 3, Name = "Coke", Description = "Refreshing drink", Price = 199, Type = FoodType.Drink },
                            new Food { Id = 4, Name = "Water", Description = "Still water", Price = 99, Type = FoodType.Drink }
                        }
                    }
                }
            },
            new Restaurant
            {
                Id = 2,
                Name = "Doner",
                Address = "Dolat",
                Categories = new List<FoodCategory>
                {
                    new FoodCategory
                    {
                        Id = 3,
                        Name = "Sandwiches",
                        Description = "Fresh doner sandwiches",
                        Foods = new List<Food>
                        {
                            new Food { Id = 5, Name = "Chicken Doner", Description = "Grilled chicken doner", Price = 750, Type = FoodType.MainCourse },
                            new Food { Id = 6, Name = "Beef Doner", Description = "Juicy beef doner", Price = 850, Type = FoodType.MainCourse }
                        }
                    }
                }
            }
        };
    }
}
