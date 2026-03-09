using OnlineShopping.Models;
using OnlineShopping.Repositories;
using OnlineShopping.Utilities;

namespace OnlineShopping.Tests;

public sealed class RepositoryTests
{
    [Fact]
    public void UserRepository_AddAndFind_ShouldPersistUser()
    {
        var dbPath = BuildTempDatabasePath();
        try
        {
            var context = new AppDataContext(dbPath);
            var repo = new UserRepository(context);
            var session = new RepositorySession(context);

            var user = new Customer(repo.NextId(), "alice", "password1");
            repo.Add(user);
            session.SaveChanges();

            var loaded = repo.FindByUsername("alice");
            Assert.NotNull(loaded);
            Assert.Equal("alice", loaded!.Username);
            Assert.True(repo.UsernameExists("ALICE"));
        }
        finally
        {
            Cleanup(dbPath);
        }
    }

    private static string BuildTempDatabasePath()
    {
        return Path.Combine(Path.GetTempPath(), $"online-shopping-tests-{Guid.NewGuid():N}.json");
    }

    private static void Cleanup(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
