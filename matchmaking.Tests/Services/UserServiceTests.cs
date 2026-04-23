namespace matchmaking.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public void DelegatesGetAddUpdateRemoveToRepository()
    {
        var existingUser = TestDataFactory.CreateUser(7);
        var repository = new FakeUserRepository([existingUser]);
        var service = new UserService(repository);
        var newUser = TestDataFactory.CreateUser(8);

        service.GetById(existingUser.UserId).Should().Be(existingUser);
        service.GetAll().Should().ContainSingle().Which.Should().Be(existingUser);

        service.Add(newUser);
        service.Update(existingUser);
        service.Remove(existingUser.UserId);

        repository.AddedUsers.Should().ContainSingle().Which.Should().Be(newUser);
        repository.UpdatedUsers.Should().ContainSingle().Which.Should().Be(existingUser);
        repository.RemovedUserIds.Should().ContainSingle().Which.Should().Be(existingUser.UserId);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public FakeUserRepository(IReadOnlyList<User> users)
        {
            _users = users.ToList();
        }

        public List<User> AddedUsers { get; } = [];
        public List<User> UpdatedUsers { get; } = [];
        public List<int> RemovedUserIds { get; } = [];

        public User? GetById(int userId) => _users.FirstOrDefault(user => user.UserId == userId);
        public IReadOnlyList<User> GetAll() => _users;
        public void Add(User user) => AddedUsers.Add(user);
        public void Update(User user) => UpdatedUsers.Add(user);
        public void Remove(int userId) => RemovedUserIds.Add(userId);
    }
}
