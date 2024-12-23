using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IMedicalForumRepository
{
    public Task<MedicalForumPost> CreatePostAsync(MedicalForumPost post);
    Task<List<MedicalForumPost>> GetAllPosts();
    Task<(int, int, string? authorId)> Vote(string postId, string userId, string type);
    Task<MedicalForumComment> CreateCommentAsync(MedicalForumComment toMedicalForumComment);
    Task<List<MedicalForumComment>> GetComments(string postId);
    Task<(int upVotes, int downVotes, string? authorId)> VoteComment(string commentId, string doctorId, string type);
}