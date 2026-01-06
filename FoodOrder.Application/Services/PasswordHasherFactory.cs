using FoodOrder.Domain.Entities;


public class PasswordHasherFactory
{
    private readonly IEnumerable<IPasswordHasherService> _hashers;

    public PasswordHasherFactory(IEnumerable<IPasswordHasherService> hashers)
    {
        _hashers = hashers;
    }

    public IPasswordHasherService GetHasher(HashMethod method)
    {
        return _hashers.FirstOrDefault(h => h.HashMethod == method)
               ?? throw new NotSupportedException($"Hash method {method} not supported");
    }
}
