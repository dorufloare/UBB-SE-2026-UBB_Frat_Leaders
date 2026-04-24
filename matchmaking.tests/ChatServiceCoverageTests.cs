using System.IO;
using System.Reflection;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class ChatServiceCoverageTests
{
    [Fact]
    public void GetChatsForUser_WhenBlockedByOtherParty_ExcludesChat()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true, BlockedByUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        var result = harness.Service.GetChatsForUser(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetChatsForCompany_WhenDeletedAndNoNewMessages_ExcludesChat()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, CompanyId = 1, DeletedAtBySecondParty = DateTime.UtcNow };
        var harness = CreateHarness(chats: new[] { chat });

        var result = harness.Service.GetChatsForCompany(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetMessages_WhenCallerIsParticipant_ReturnsMessages()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var message = new Message { ChatId = 1, SenderId = 1, Content = "hello", Type = MessageType.Text, Timestamp = DateTime.UtcNow };
        var harness = CreateHarness(chats: new[] { chat }, messages: new[] { message });

        var result = harness.Service.GetMessages(1, 1);

        result.Should().ContainSingle(item => item.Content == "hello");
    }

    [Fact]
    public void SendMessage_WhenImageFileDoesNotExist_ThrowsFileNotFoundException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.SendMessage(1, @"C:\missing\image.png", 1, MessageType.Image);

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void SendMessage_WhenFileExtensionIsUnsupported_ThrowsNotSupportedException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, "test");

        try
        {
            Action act = () => harness.Service.SendMessage(1, path, 1, MessageType.File);

            act.Should().Throw<NotSupportedException>();
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void SendMessage_WhenChatIsBlockedByOtherUser_DoesNothing()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true, BlockedByUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.SendMessage(1, "hello", 1, MessageType.Text);

        harness.MessageRepository.AddedMessages.Should().BeEmpty();
    }

    [Fact]
    public void BlockUser_WhenChatAlreadyBlocked_ThrowsInvalidOperationException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.BlockUser(1, 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UnblockUser_WhenCallerIsNotBlocker_ThrowsUnauthorizedAccessException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true, BlockedByUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.UnblockUser(1, 1);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void DeleteChat_WhenCallerIsParticipant_DelegatesDeletion()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.DeleteChat(1, 1);

        harness.ChatRepository.DeletedByUserCalls.Should().ContainSingle(call => call.ChatId == 1 && call.CallerId == 1);
    }

    [Fact]
    public void GetMessages_WhenCallerIsNotParticipant_ThrowsUnauthorizedAccessException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.GetMessages(1, 99);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void SendMessage_WhenContentIsEmpty_ThrowsArgumentException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.SendMessage(1, string.Empty, 1, MessageType.Text);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SendMessage_WhenChatIsBlockedByCurrentUser_ThrowsInvalidOperationException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true, BlockedByUserId = 1 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.SendMessage(1, "hello", 1, MessageType.Text);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UnblockUser_WhenChatIsNotBlocked_ThrowsInvalidOperationException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = false };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.UnblockUser(1, 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkMessageAsRead_WhenCalled_DelegatesMarkAsRead()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.MarkMessageAsRead(1, 1);

        harness.MessageRepository.MarkReadCalls.Should().ContainSingle(call => call.ChatId == 1 && call.ReaderId == 1);
    }

    [Fact]
    public void FindOrCreateUserCompanyChat_WhenBlockedByOtherParty_ReturnsNull()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, CompanyId = 1, IsBlocked = true, BlockedByUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        var result = harness.Service.FindOrCreateUserCompanyChat(1, 1, null);

        result.Should().BeNull();
    }

    [Fact]
    public void FindOrCreateUserCompanyChat_WhenExistingChatFound_ReturnsExisting()
    {
        var existing = new Chat { ChatId = 1, UserId = 1, CompanyId = 1 };
        var harness = CreateHarness(chats: new[] { existing });

        var result = harness.Service.FindOrCreateUserCompanyChat(1, 1, null);

        result.Should().BeSameAs(existing);
        harness.ChatRepository.AddedChats.Should().BeEmpty();
    }

    [Fact]
    public void FindOrCreateUserCompanyChat_WhenNoChatExists_CreatesNew()
    {
        var harness = CreateHarness();

        var result = harness.Service.FindOrCreateUserCompanyChat(1, 1, null);

        result.Should().NotBeNull();
        harness.ChatRepository.AddedChats.Should().ContainSingle();
    }

    [Fact]
    public void FindOrCreateUserUserChat_WhenBlockedByOtherParty_ReturnsNull()
    {
        var chat = new Chat { ChatId = 1, UserId = 2, SecondUserId = 1, IsBlocked = true, BlockedByUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        var result = harness.Service.FindOrCreateUserUserChat(1, 2);

        result.Should().BeNull();
    }

    [Fact]
    public void FindOrCreateUserUserChat_WhenExistingChatFound_ReturnsExisting()
    {
        var existing = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { existing });

        var result = harness.Service.FindOrCreateUserUserChat(1, 2);

        result.Should().BeSameAs(existing);
    }

    [Fact]
    public void SearchCompanies_WhenQueryIsEmpty_ReturnsEmpty()
    {
        var harness = CreateHarness();

        var result = harness.Service.SearchCompanies(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchUsers_WhenQueryIsEmpty_ReturnsEmpty()
    {
        var harness = CreateHarness();

        var result = harness.Service.SearchUsers(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchCompanies_WhenQueryMatches_ReturnsMatchingCompanies()
    {
        var harness = CreateHarness();

        var result = harness.Service.SearchCompanies("TechNova");

        result.Should().ContainSingle(company => company.CompanyName == "TechNova");
    }

    [Fact]
    public void SearchUsers_WhenQueryMatches_ReturnsMatchingUsers()
    {
        var harness = CreateHarness();

        var result = harness.Service.SearchUsers("Alice");

        result.Should().ContainSingle(user => user.Name.StartsWith("Alice", StringComparison.Ordinal));
    }

    [Fact]
    public void DeleteChat_WhenCallerIsNotParticipant_ThrowsUnauthorizedAccessException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.DeleteChat(1, 99);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void DeleteChat_WhenCallerIsSecondParty_DelegatesToSecondParty()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.DeleteChat(1, 2);

        harness.ChatRepository.DeletedBySecondPartyCalls.Should().ContainSingle(call => call.ChatId == 1 && call.CallerId == 2);
    }

    [Fact]
    public void BlockUser_WhenChatNotBlocked_BlocksSuccessfully()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = false };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.BlockUser(1, 1);

        harness.ChatRepository.GetChatById(1).IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void FindOrCreateUserUserChat_WhenNoChatExists_CreatesNew()
    {
        var harness = CreateHarness();

        var result = harness.Service.FindOrCreateUserUserChat(1, 2);

        result.Should().NotBeNull();
        result!.SecondUserId.Should().Be(2);
        harness.ChatRepository.AddedChats.Should().ContainSingle();
    }

    [Fact]
    public void GetChatsForUser_WhenDeletedButHasNewMessage_IncludesChat()
    {
        var deletedAt = DateTime.UtcNow.AddMinutes(-5);
        var chat = new Chat
        {
            ChatId = 1,
            UserId = 1,
            CompanyId = 5,
            DeletedAtByUser = deletedAt
        };
        var harness = CreateHarness(
            chats: new[] { chat },
            latestMessageTimestamps: new Dictionary<int, DateTime?> { [1] = DateTime.UtcNow });

        var result = harness.Service.GetChatsForUser(1);

        result.Should().ContainSingle(item => item.ChatId == 1);
    }

    [Fact]
    public void GetChatsForUser_WhenNoMessageTimestampAvailable_SortsByChatIdDescending()
    {
        var older = new Chat { ChatId = 1, UserId = 1, CompanyId = 1 };
        var newer = new Chat { ChatId = 2, UserId = 1, CompanyId = 2 };
        var harness = CreateHarness(chats: new[] { older, newer });

        var result = harness.Service.GetChatsForUser(1);

        result.Select(item => item.ChatId).Should().Equal(2, 1);
    }

    [Fact]
    public void GetChatsForCompany_WhenBlockedByOtherParty_ExcludesChat()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, CompanyId = 10, IsBlocked = true, BlockedByUserId = 1 };
        var harness = CreateHarness(chats: new[] { chat });

        var result = harness.Service.GetChatsForCompany(10);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetChatsForCompany_WhenNoMessageTimestampAvailable_SortsByChatIdDescending()
    {
        var first = new Chat { ChatId = 1, UserId = 1, CompanyId = 10 };
        var second = new Chat { ChatId = 2, UserId = 2, CompanyId = 10 };
        var harness = CreateHarness(chats: new[] { first, second });

        var result = harness.Service.GetChatsForCompany(10);

        result.Select(item => item.ChatId).Should().Equal(2, 1);
    }

    [Fact]
    public void SendMessage_WhenTextExceedsMaxLength_ThrowsArgumentException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        Action act = () => harness.Service.SendMessage(1, new string('a', 2001), 1, MessageType.Text);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SendMessage_WhenTextIsValid_AddsMessageToRepository()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.SendMessage(1, "hello", 1, MessageType.Text);

        harness.MessageRepository.AddedMessages.Should().ContainSingle(message =>
            message.ChatId == 1 &&
            message.SenderId == 1 &&
            message.Content == "hello" &&
            message.Type == MessageType.Text);
    }

    [Fact]
    public void SendMessage_WhenAttachmentHasNoExtension_ThrowsNotSupportedException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });
        var source = Path.GetTempFileName();
        var noExtensionPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            File.Move(source, noExtensionPath);

            Action act = () => harness.Service.SendMessage(1, noExtensionPath, 1, MessageType.File);

            act.Should().Throw<NotSupportedException>();
        }
        finally
        {
            if (File.Exists(source))
            {
                File.Delete(source);
            }

            if (File.Exists(noExtensionPath))
            {
                File.Delete(noExtensionPath);
            }
        }
    }

    [Fact]
    public void SendMessage_WhenImageExtensionIsUnsupported_ThrowsNotSupportedException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        File.WriteAllText(path, "test");

        try
        {
            Action act = () => harness.Service.SendMessage(1, path, 1, MessageType.Image);

            act.Should().Throw<NotSupportedException>();
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void SendMessage_WhenImageExceedsLimit_ThrowsInvalidOperationException()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var harness = CreateHarness(chats: new[] { chat });
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

        try
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.SetLength((10 * 1024 * 1024) + 1L);
            }

            Action act = () => harness.Service.SendMessage(1, path, 1, MessageType.Image);

            act.Should().Throw<InvalidOperationException>();
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void SendMessage_WhenImageAttachmentIsValidPath_StoresAttachmentAndAddsMessage()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var attachmentRoot = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}");
        var harness = CreateHarness(chats: new[] { chat }, attachmentRootPathProvider: () => attachmentRoot);
        var source = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        File.WriteAllText(source, "image");
        string? storedPath = null;

        try
        {
            harness.Service.SendMessage(1, source, 1, MessageType.Image);

            harness.MessageRepository.AddedMessages.Should().ContainSingle();
            storedPath = harness.MessageRepository.AddedMessages[0].Content;
            File.Exists(storedPath).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(source))
            {
                File.Delete(source);
            }

            if (!string.IsNullOrEmpty(storedPath) && File.Exists(storedPath))
            {
                File.Delete(storedPath);
            }

            if (Directory.Exists(attachmentRoot))
            {
                Directory.Delete(attachmentRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void GetDefaultAttachmentRootPath_WhenInvoked_ReturnsPathUnderLocalApplicationData()
    {
        var method = typeof(ChatService).GetMethod("GetDefaultAttachmentRootPath", BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, null) as string;

        result.Should().NotBeNullOrWhiteSpace();
        result!.Should().EndWith(Path.Combine("matchmaking", "attachments"));
    }

    [Fact]
    public void UnblockUser_WhenCalledByBlocker_DelegatesToRepository()
    {
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true, BlockedByUserId = 1 };
        var harness = CreateHarness(chats: new[] { chat });

        harness.Service.UnblockUser(1, 1);

        harness.ChatRepository.GetChatById(1).IsBlocked.Should().BeFalse();
    }

    private static ChatServiceHarness CreateHarness(
        IReadOnlyList<Chat>? chats = null,
        IReadOnlyList<Message>? messages = null,
        IReadOnlyDictionary<int, DateTime?>? latestMessageTimestamps = null,
        Func<string>? attachmentRootPathProvider = null)
    {
        var chatRepository = new FakeChatRepository(chats ?? Array.Empty<Chat>(), latestMessageTimestamps);
        var messageRepository = new FakeMessageRepository(messages ?? Array.Empty<Message>());
        var userRepository = new FakeUserRepository(new[] { TestDataFactory.CreateUser() });
        var companyRepository = new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany() });

        return new ChatServiceHarness(
            new ChatService(chatRepository, messageRepository, userRepository, companyRepository, attachmentRootPathProvider),
            chatRepository,
            messageRepository);
    }

    private sealed class ChatServiceHarness
    {
        public ChatServiceHarness(ChatService service, FakeChatRepository chatRepository, FakeMessageRepository messageRepository)
        {
            Service = service;
            ChatRepository = chatRepository;
            MessageRepository = messageRepository;
        }

        public ChatService Service { get; }
        public FakeChatRepository ChatRepository { get; }
        public FakeMessageRepository MessageRepository { get; }
    }

    private sealed class FakeChatRepository : IChatRepository
    {
        private readonly List<Chat> chats;
        private readonly IReadOnlyDictionary<int, DateTime?> latestMessageTimestamps;

        public FakeChatRepository(IReadOnlyList<Chat> chats, IReadOnlyDictionary<int, DateTime?>? latestMessageTimestamps)
        {
            this.chats = chats.ToList();
            this.latestMessageTimestamps = latestMessageTimestamps ?? new Dictionary<int, DateTime?>();
        }

        public List<(int ChatId, int CallerId)> DeletedByUserCalls { get; } = new();
        public List<(int ChatId, int CallerId)> DeletedBySecondPartyCalls { get; } = new();
        public List<Chat> AddedChats { get; } = new();

        public Chat GetChatById(int chatId) => chats.First(chat => chat.ChatId == chatId);
        public IReadOnlyList<Chat> GetByUserId(int userId) => chats.Where(chat => chat.UserId == userId || chat.SecondUserId == userId).ToList();
        public IReadOnlyList<Chat> GetByCompanyId(int companyId) => chats.Where(chat => chat.CompanyId == companyId).ToList();
        public Chat? GetByUserAndCompany(int userId, int companyId, int? jobId = null) => chats.FirstOrDefault(chat => chat.UserId == userId && chat.CompanyId == companyId && chat.JobId == jobId);
        public Chat? GetByUsers(int userId, int secondUserId) => chats.FirstOrDefault(chat => chat.UserId == userId && chat.SecondUserId == secondUserId);
        public IReadOnlyDictionary<int, DateTime?> GetLatestMessageTimestamps(IEnumerable<int> chatIds)
            => chatIds.ToDictionary(
                id => id,
                id => latestMessageTimestamps.TryGetValue(id, out var timestamp) ? timestamp : null);
        public void Add(Chat chat)
        {
            chats.Add(chat);
            AddedChats.Add(chat);
        }
        public void BlockChat(int chatId, int blockerId) => chats.First(chat => chat.ChatId == chatId).IsBlocked = true;
        public void UnblockUser(int chatId, int unblockerId) => chats.First(chat => chat.ChatId == chatId).IsBlocked = false;
        public void DeletedByUser(int chatId, int userId) => DeletedByUserCalls.Add((chatId, userId));
        public void DeletedBySecondParty(int chatId, int secondPartyId) => DeletedBySecondPartyCalls.Add((chatId, secondPartyId));
    }

    private sealed class FakeMessageRepository : IMessageRepository
    {
        public FakeMessageRepository(IReadOnlyList<Message> messages)
        {
            Messages = messages.ToList();
        }

        public List<Message> Messages { get; }
        public List<Message> AddedMessages { get; } = new();
        public List<(int ChatId, int ReaderId)> MarkReadCalls { get; } = new();

        public IReadOnlyList<Message> GetByChatId(int chatId, DateTime? visibleAfter = null) => Messages.Where(message => message.ChatId == chatId).ToList();
        public void Add(Message message) => AddedMessages.Add(message);
        public void MarkAsRead(int chatId, int readerId) => MarkReadCalls.Add((chatId, readerId));
    }
}
