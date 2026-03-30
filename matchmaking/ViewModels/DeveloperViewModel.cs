using System;
using System.Collections.ObjectModel;
using System.Linq;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.Repositories;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class DeveloperViewModel : ObservableObject
{
    private readonly DeveloperService _developerService;
    private readonly SessionContext _session;

    public ObservableCollection<PostViewModel> Posts { get; } = new();

    public DeveloperViewModel(DeveloperService developerService, SessionContext sessionContext)
    {
        _developerService = developerService;
        _session = sessionContext;
        LoadData();
    }

    public void AddPost(string parameter, string value)
    {
        var developerId = _session.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");


        _developerService.addPost(developerId, parameter, value);
        LoadData();
    }

    public void HandleLike(int postId)
    {
        var developerId = _session.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");


        _developerService.addInteraction(developerId, postId, InteractionType.Like);
        LoadData();
    }

    public void HandleDislike(int postId)
    {
        var developerId = _session.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");


        _developerService.addInteraction(developerId, postId, InteractionType.Dislike);
        LoadData();
    }

    private void LoadData()
    {
        var posts = _developerService.GetPosts();
        var interactions = _developerService.GetInteractions();

        Posts.Clear();
        foreach (var post in posts)
        {
            var postInteractions = interactions.Where(i => i.PostId == post.PostId);
            Posts.Add(new PostViewModel(post, postInteractions));
        }
    }
}
