namespace matchmaking.Tests;

public sealed class DeveloperServiceTests
{
    [Fact]
    public void GetDeveloperById_WhenDeveloperExists_ReturnsDeveloper()
    {
        var developer = new Developer { DeveloperId = 3, Name = "Dev", Password = "pwd" };
        var developerRepository = new FakeDeveloperRepository(developer);
        var postRepository = new FakePostRepository([TestDataFactory.CreatePost(postId: 2, developerId: 3)]);
        var interactionRepository = new FakeInteractionRepository([TestDataFactory.CreateInteraction(interactionId: 9, developerId: 3, postId: 2)]);
        var service = new DeveloperService(developerRepository, postRepository, interactionRepository);

        service.GetDeveloperById(3).Should().Be(developer);
    }

    [Fact]
    public void GetPosts_WhenPostsExist_ReturnsPosts()
    {
        var developer = new Developer { DeveloperId = 3, Name = "Dev", Password = "pwd" };
        var developerRepository = new FakeDeveloperRepository(developer);
        var postRepository = new FakePostRepository([TestDataFactory.CreatePost(postId: 2, developerId: 3)]);
        var interactionRepository = new FakeInteractionRepository([TestDataFactory.CreateInteraction(interactionId: 9, developerId: 3, postId: 2)]);
        var service = new DeveloperService(developerRepository, postRepository, interactionRepository);

        service.GetPosts().Should().ContainSingle();
    }

    [Fact]
    public void GetInteractions_WhenInteractionsExist_ReturnsInteractions()
    {
        var developer = new Developer { DeveloperId = 3, Name = "Dev", Password = "pwd" };
        var developerRepository = new FakeDeveloperRepository(developer);
        var postRepository = new FakePostRepository([TestDataFactory.CreatePost(postId: 2, developerId: 3)]);
        var interactionRepository = new FakeInteractionRepository([TestDataFactory.CreateInteraction(interactionId: 9, developerId: 3, postId: 2)]);
        var service = new DeveloperService(developerRepository, postRepository, interactionRepository);

        service.GetInteractions().Should().ContainSingle();
    }

    [Fact]
    public void AddPost_WhenPostAdded_PersistsUnknownParameterType()
    {
        var developerRepository = new FakeDeveloperRepository(null);
        var postRepository = new FakePostRepository([]);
        var interactionRepository = new FakeInteractionRepository([]);
        var service = new DeveloperService(developerRepository, postRepository, interactionRepository);

        service.AddPost(3, "burnout_threshold", "6");

        postRepository.AddedPosts.Should().ContainSingle();
        postRepository.AddedPosts[0].DeveloperId.Should().Be(3);
        postRepository.AddedPosts[0].ParameterType.Should().Be(PostParameterType.Unknown);
        postRepository.AddedPosts[0].Value.Should().Be("6");
    }

    [Fact]
    public void AddInteraction_WhenExisting_UpdatesInteraction()
    {
        var existing = TestDataFactory.CreateInteraction(interactionId: 12, developerId: 3, postId: 5, type: InteractionType.Dislike);
        var interactionRepository = new FakeInteractionRepository([existing]);
        var service = new DeveloperService(new FakeDeveloperRepository(null), new FakePostRepository([]), interactionRepository);

        service.AddInteraction(3, 5, InteractionType.Like);

        interactionRepository.UpdatedInteractions.Should().ContainSingle();
        interactionRepository.UpdatedInteractions[0].InteractionId.Should().Be(12);
        interactionRepository.UpdatedInteractions[0].Type.Should().Be(InteractionType.Like);
        interactionRepository.AddedInteractions.Should().BeEmpty();
    }

    [Fact]
    public void AddInteraction_WhenNoExisting_AddsInteraction()
    {
        var interactionRepository = new FakeInteractionRepository([]);
        var service = new DeveloperService(new FakeDeveloperRepository(null), new FakePostRepository([]), interactionRepository);

        service.AddInteraction(2, 8, InteractionType.Dislike);

        interactionRepository.AddedInteractions.Should().ContainSingle();
        interactionRepository.AddedInteractions[0].DeveloperId.Should().Be(2);
        interactionRepository.AddedInteractions[0].PostId.Should().Be(8);
        interactionRepository.AddedInteractions[0].Type.Should().Be(InteractionType.Dislike);
        interactionRepository.UpdatedInteractions.Should().BeEmpty();
    }

    [Fact]
    public void RemoveInteraction_WhenCalled_RemovesInteraction()
    {
        var interactionRepository = new FakeInteractionRepository([]);
        var service = new DeveloperService(new FakeDeveloperRepository(null), new FakePostRepository([]), interactionRepository);

        service.RemoveInteraction(44);

        interactionRepository.RemovedInteractionIds.Should().ContainSingle().Which.Should().Be(44);
    }

    private sealed class FakeDeveloperRepository : IDeveloperRepository
    {
        private readonly Developer? _developer;

        public FakeDeveloperRepository(Developer? developer)
        {
            _developer = developer;
        }

        public Developer? GetById(int developerId) => _developer?.DeveloperId == developerId ? _developer : null;
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly List<Post> _posts;

        public FakePostRepository(IReadOnlyList<Post> posts)
        {
            _posts = posts.ToList();
        }

        public List<Post> AddedPosts { get; } = [];

        public IReadOnlyList<Post> GetAll() => _posts;
        public void Add(Post post) => AddedPosts.Add(post);
    }

    private sealed class FakeInteractionRepository : IInteractionRepository
    {
        private readonly List<Interaction> _interactions;

        public FakeInteractionRepository(IReadOnlyList<Interaction> interactions)
        {
            _interactions = interactions.ToList();
        }

        public List<Interaction> AddedInteractions { get; } = [];
        public List<Interaction> UpdatedInteractions { get; } = [];
        public List<int> RemovedInteractionIds { get; } = [];

        public IReadOnlyList<Interaction> GetAll() => _interactions;

        public Interaction? GetByDeveloperIdAndPostId(int developerId, int postId) =>
            _interactions.FirstOrDefault(item => item.DeveloperId == developerId && item.PostId == postId);

        public void Add(Interaction interaction) => AddedInteractions.Add(interaction);
        public void Update(Interaction interaction) => UpdatedInteractions.Add(interaction);
        public void Remove(int interactionId) => RemovedInteractionIds.Add(interactionId);
    }
}
