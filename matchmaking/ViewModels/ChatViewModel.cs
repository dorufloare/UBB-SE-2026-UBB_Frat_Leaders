using System;
using System.Collections.ObjectModel;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Session;
using matchmaking.Domain.Enums;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class ChatViewModel : ObservableObject
{
    private ObservableCollection<Chat> _chats = null!;
    private ObservableCollection<Chat> _filteredChats = null!;
    private Chat? _selectedChat;
    private ObservableCollection<Message> _messages = null!;
    private Job? _linkedJob;
    private string _activeTab = "Users";
    private string? _messageText;
    private MessageType _selectedMessageType = MessageType.Text;
    private string? _errorMessage;

    private readonly ChatService _chatService;
    private readonly JobService _jobService;
    private readonly SessionContext _sessionContext;

    public ChatViewModel(ChatService chatService, JobService jobService, SessionContext sessionContext)
    {
        _chatService = chatService;
        _jobService = jobService;
        _sessionContext = sessionContext;

        _chats = new ObservableCollection<Chat>();
        _filteredChats = new ObservableCollection<Chat>();
        _messages = new ObservableCollection<Message>();
        _activeTab = "Users";
    }

    public ObservableCollection<Chat> Chats
    {
        get => _chats;
        set => SetProperty(ref _chats, value);
    }

    public ObservableCollection<Chat> FilteredChats
    {
        get => _filteredChats;
        set => SetProperty(ref _filteredChats, value);
    }

    public Chat? SelectedChat
    {
        get => _selectedChat;
        set => SetProperty(ref _selectedChat, value);
    }

    public ObservableCollection<Message> Messages
    {
        get => _messages;
        set => SetProperty(ref _messages, value);
    }

    public Job? LinkedJob
    {
        get => _linkedJob;
        set => SetProperty(ref _linkedJob, value);
    }

    public string ActiveTab
    {
        get => _activeTab;
        set => SetProperty(ref _activeTab, value);
    }

    public string? MessageText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    public MessageType SelectedMessageType
    {
        get => _selectedMessageType;
        set => SetProperty(ref _selectedMessageType, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public void LoadChats()
    {
        var chats = _sessionContext.CurrentMode == AppMode.UserMode
            ? _chatService.GetChatsForUser(_sessionContext.CurrentUserId.Value)
            : _chatService.GetChatsForCompany(_sessionContext.CurrentCompanyId.Value);

        Chats.Clear();
        foreach (var chat in chats)
        {
            Chats.Add(chat);
        }

        if (_sessionContext.CurrentMode == AppMode.UserMode)
        {
            ApplyTabFilter();
        }
    }

    public void ApplyTabFilter()
    {
        if (_sessionContext.CurrentMode != AppMode.UserMode)
            return;

        FilteredChats.Clear();
        var filtered = ActiveTab == "Users"
            ? Chats.Where(c => c.SecondUserId.HasValue).ToList()
            : Chats.Where(c => c.CompanyId.HasValue).ToList();

        foreach (var chat in filtered)
        {
            FilteredChats.Add(chat);
        }
    }

    public void SwitchTab(string tabName)
    {
        ActiveTab = tabName;
        ApplyTabFilter();
        SelectedChat = null;
        Messages.Clear();
    }

    public void SelectChat(Chat chat)
    {
        SelectedChat = chat;

        var messages = _chatService.GetMessages(SelectedChat.ChatId);
        Messages.Clear();
        foreach (var message in messages)
        {
            Messages.Add(message);
        }

        int currentCallerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

        _chatService.MarkMessageAsRead(SelectedChat.ChatId, currentCallerId);

        if (SelectedChat.JobId.HasValue)
        {
            LinkedJob = _jobService.GetById(SelectedChat.JobId.Value);
        }
        else
        {
            LinkedJob = null;
        }
    }

    public void SendMessage()
    {
        ErrorMessage = null;

        if (SelectedChat is null || string.IsNullOrWhiteSpace(MessageText))
            return;

        if (SelectedChat.IsBlocked)
        {
            ErrorMessage = "Cannot send message in a blocked chat.";
            return;
        }

        int senderId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

        try
        {
            _chatService.SendMessage(SelectedChat.ChatId, MessageText, senderId, SelectedMessageType);
            
            // Clear compose state
            MessageText = string.Empty;
            SelectedMessageType = MessageType.Text;
            
            // Reload messages
            SelectChat(SelectedChat);
            
            // Move SelectedChat to position 0 in Chats
            if (Chats.Count > 0 && Chats[0] != SelectedChat)
            {
                Chats.Remove(SelectedChat);
                Chats.Insert(0, SelectedChat);
            }
            
            // Move SelectedChat to position 0 in FilteredChats (UserMode only)
            if (_sessionContext.CurrentMode == AppMode.UserMode && FilteredChats.Count > 0 && FilteredChats[0] != SelectedChat)
            {
                FilteredChats.Remove(SelectedChat);
                FilteredChats.Insert(0, SelectedChat);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public void HandleAttachmentSelected(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            ErrorMessage = "No file selected.";
            return;
        }

        string extension = System.IO.Path.GetExtension(filePath).ToLower();

        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
        {
            SelectedMessageType = MessageType.Image;
        }
        else if (extension == ".pdf" || extension == ".doc" || extension == ".docx")
        {
            SelectedMessageType = MessageType.File;
        }
        else
        {
            ErrorMessage = "Unsupported file type. Allowed: .jpg, .jpeg, .png, .pdf, .doc, .docx";
            return;
        }

        MessageText = filePath;
        SendMessage();
    }
}

