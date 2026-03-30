using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.ViewModels;

public class PostViewModel
{
    public int PostId { get; }
    public string AuthorName { get; }
    public string AuthorInitial { get; }
    public string TypeLabel { get; }
    public SolidColorBrush TypeBadgeBackground { get; }
    public string ParameterDisplayName { get; }
    public string ValueDisplay { get; }
    public int LikeCount { get; }
    public int DislikeCount { get; }

    public PostViewModel(Post post, IEnumerable<Interaction> postInteractions)
    {
        var interactions = postInteractions.ToList();

        PostId              = post.PostId;
        AuthorName          = $"Developer #{post.DeveloperId}";
        AuthorInitial       = "D";
        TypeLabel           = post.ParameterType == PostParameterType.RelevantKeyword ? "Keyword" : "Parameter";
        TypeBadgeBackground = post.ParameterType == PostParameterType.RelevantKeyword
            ? new SolidColorBrush(Color.FromArgb(0xFF, 0x16, 0xA3, 0x4A))
            : new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x63, 0xEB));
        ParameterDisplayName = PostParameterTypeMapper.ToStorageValue(post.ParameterType);
        ValueDisplay         = post.Value;
        LikeCount            = interactions.Count(i => i.Type == InteractionType.Like);
        DislikeCount         = interactions.Count(i => i.Type == InteractionType.Dislike);
    }
}
