using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public class UserProfileViewModelTests
{
    [Fact]
    public void Load_with_zero_id_shows_unknown_user()
    {
        var vm = new UserProfileViewModel(new InMemoryUsers());

        vm.Load(0);

        vm.Name.Should().Be("Unknown user");
        vm.Meta.Should().BeEmpty();
        vm.Contact.Should().BeEmpty();
        vm.Resume.Should().BeEmpty();
    }

    [Fact]
    public void Load_with_an_id_that_doesnt_exist_shows_user_not_found()
    {
        var vm = new UserProfileViewModel(new InMemoryUsers());

        vm.Load(123);

        vm.Name.Should().Be("User not found");
    }

    [Fact]
    public void Load_populates_view_model_from_the_user_record()
    {
        var users = new InMemoryUsers();
        users.Items.Add(new User
        {
            UserId = 5,
            Name = "Alice",
            Location = "Cluj",
            YearsOfExperience = 4,
            Education = "CS",
            Email = "alice@example.com",
            Phone = "0700",
            Resume = "ten years of stuff"
        });
        var vm = new UserProfileViewModel(users);

        vm.Load(5);

        vm.Name.Should().Be("Alice");
        vm.Meta.Should().Be("Cluj · 4 years · CS");
        vm.Contact.Should().Be("alice@example.com · 0700");
        vm.Resume.Should().Be("ten years of stuff");
    }

    private sealed class InMemoryUsers : IUserRepository
    {
        public List<User> Items { get; } = new();
        public User? GetById(int userId) => Items.FirstOrDefault(u => u.UserId == userId);
        public IReadOnlyList<User> GetAll() => Items;
        public void Add(User user) => Items.Add(user);
        public void Update(User user) { }
        public void Remove(int userId) { }
    }
}
