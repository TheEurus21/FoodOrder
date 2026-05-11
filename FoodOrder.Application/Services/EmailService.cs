namespace FoodOrder.Application.Services
{
    public class EmailService
    {
        public void SendWelcomeEmail(string email)
        {
            Console.WriteLine($"welcome email sent to {email}");
        }
    }
}
