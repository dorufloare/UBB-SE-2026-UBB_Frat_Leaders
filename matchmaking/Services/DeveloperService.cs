using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;
using matchmaking.Domain.Enums;

namespace matchmaking.Services;

public class DeveloperService
{
    private readonly SqlDeveloperRepository _developerRepository;
    private readonly SqlPostRepository _postRepository;
    private readonly SqlInteractionRepository _interactionRepository;

    public DeveloperService(SqlDeveloperRepository developerRepository, SqlPostRepository postRepository, SqlInteractionRepository interactionRepository)
    {
        _developerRepository = developerRepository;
        _postRepository = postRepository;
        _interactionRepository = interactionRepository;
    }
    
    public IReadOnlyList<Post> GetPosts()
    {
        return _postRepository.GetAll();
    }

    public IReadOnlyList<Interaction> GetInteractions()
    {
        return _interactionRepository.GetAll();
    }

    public void addPost(int postId, int developerId, string parameter, string value)
    {
        var post = new Post
        {
            PostId = postId,
            DeveloperId = developerId,
            Parameter = parameter,
            Value = value
        };
        _postRepository.Add(post);
    }

    public void addInteraction(int interactionId, int developerId, int postId, InteractionType type)
    {
        var interaction = new Interaction
        {
            InteractionId = interactionId,
            DeveloperId = developerId,
            PostId = postId,
            Type = type
        };
        _interactionRepository.Add(interaction);
    }
}

