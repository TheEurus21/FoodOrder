using FoodOrder.Application.Options;
using MassTransit.Configuration;
using Microsoft.Extensions.Options;

namespace FoodOrder.Application.Services
{
    public class RestaurantService
    {
        private readonly IConfiguration _configuration;
        private readonly BussinessRulesOptions _rules;
        public RestaurantService(IConfiguration configuration,IOptions<BussinessRulesOptions>rules)
        {
            _configuration = configuration;
            _rules = rules.Value;
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
        public void ValidateOwnerCanPlaceOrder(int todayOrderCount)
        {
            if (todayOrderCount >= _rules.MaxOrderPerDay)
            {
                throw new InvalidOperationException(
                    $"only {_rules.MaxOrderPerDay} are allowed");
            }
        }
    }
}
