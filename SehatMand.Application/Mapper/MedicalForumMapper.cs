using SehatMand.Application.Dto.MedicalForum;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class MedicalForumMapper
{
    public static MedicalForumPost ToMedicalForumPost(this CreateMedicalForumPost post, string id)
    {
        return new MedicalForumPost
        {
            author_id = id,
            heading = post.Title,
            content = post.Content,
            created_at = DateTime.Now
        };
    }
    
    public static ReadMedicalForumPostDto ToReadMedicalForumPostDto(this MedicalForumPost post)
    {
        return new ReadMedicalForumPostDto(
            post.id,
            post.author?.Name ?? "Anonymous",
            post.heading,
            post.content,
            post.Votes.Count(v => v.IsUpVote),
            post.Votes.Count(v => v.IsDownVote),
            post.created_at.ToString("yy-MMM-dd ddd (hh:mm tt)")
            );
    }
    
    public static MedicalForumComment ToMedicalForumComment(this CreateMedicalForumCommentDto commentDto, string postId, string id)
    {
        return new MedicalForumComment
        {
            author_id = id,
            post_id = postId,
            content = commentDto.Content,
            created_at = DateTime.Now
        };
    }
    
    public static ReadMedicalForumCommentDto ToReadMedicalForumCommentDto(this MedicalForumComment comment)
    {
        return new ReadMedicalForumCommentDto(
            comment.id,
            comment.author?.Name ?? "Anonymous",
            comment.author_id,
            comment.content,
            comment.Votes.Count(v => v.IsUpVote),
            comment.Votes.Count(v => v.IsDownVote),
            comment.created_at.ToString("yy-MMM-dd ddd (hh:mm tt)")
            );
    }
}