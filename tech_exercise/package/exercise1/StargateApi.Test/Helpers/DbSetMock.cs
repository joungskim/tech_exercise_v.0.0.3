// TODO: Implement if you have time
// Example below:
/*
public interface IDbContext
{
    DbSet<User> Users { get; }
    // Other DbSet properties...
}

public class MockDbContext : IDbContext
{
    public MockDbContext()
    {
        Users = new DbMockSet<User>();
        // Initialize other mock sets as needed
    }

    public DbSet<User> Users { get; private set; }
    // Implement other IDbContext properties...
}

public class UserService
{
    private readonly IDbContext _dbContext;

    public UserService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddUser(User user)
    {
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
    }
}
*/