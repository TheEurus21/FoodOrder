namespace FoodOrder.Application.Services
{
    public class RestaurantService
    {
        private readonly IConfiguration _configuration;
        public RestaurantService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ValidateOwnerCanAdd(int userId, int currentCount)
        {
            var maxAllowed =
                _configuration.GetValue<int>("BussinessRules:MaxRestaurantPerOwner");
            if (currentCount >= maxAllowed)
            {
                throw new InvalidOperationException(
                    $"only {maxAllowed} restaurants are allowed");
            }
        } 
    }
}
