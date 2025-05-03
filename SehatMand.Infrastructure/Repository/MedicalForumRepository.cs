using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Repository;

public class MedicalForumRepository(
    SmDbContext context,
    IPatientRepository patientRepo,
    ILogger<MedicalForumRepository> logger 
    ): IMedicalForumRepository
{
    public async Task<MedicalForumPost> CreatePostAsync(MedicalForumPost post)
    {
        var saved = await context.MedicalForumPost.AddAsync(post);
        await context.SaveChangesAsync();
        return saved.Entity;
    }

    public async Task<List<MedicalForumPost>> GetAllPosts()
    {
        return await context.MedicalForumPost
            .Include(p => p.Votes)
            .Include(p => p.author)
            /*.Include(p => p.MedicalForumComment)
            .ThenInclude(c => c.Votes)*/
            .OrderByDescending(a => a.created_at)
            .ToListAsync();
    }

    public async Task<(int, int, string? authorId)> 
        Vote(string postId, string userId, string type)
    {
       var post = await context.MedicalForumPost
           .Include(p => p.Votes)
           .Include(p => p.author)
           .FirstOrDefaultAsync(p => p.id == postId);
       if (post == null) throw new Exception("Post Not Found");
       var patientId = await patientRepo.GetPatientIdByUserId(userId);
       if (patientId == post.author_id) throw new Exception("You can't vote on your own post! >:(");
       
       var vote = post.Votes.FirstOrDefault(v => v.voted_by == userId);
       if (vote != null)
       {
           if ((vote.IsUpVote && type == "upvote") || (vote.IsDownVote && type == "downvote"))
           {
               logger.LogInformation("User {userId} removed {type} from post {postId}", userId, type, postId);
               post.Votes.Remove(vote);
           }
           else if((vote.IsUpVote && type == "downvote") || (vote.IsDownVote && type == "upvote"))
           {
                logger.LogInformation("User {userId} added a {type} on post {postId}", userId, type, postId);
               vote.vote_type = !vote.vote_type;
           }
       }
       else
       {
              logger.LogInformation("User {userId} added a {type} on post {postId}", userId, type, postId);
              post.Votes.Add(new MedicalForumPostVotes
              {
                voted_by = userId,
                vote_type = type == "upvote",
                post_id = postId,
                created_at = DateTime.Now
              });
       }

       await context.SaveChangesAsync();
       return (post.Votes.Count(v => v.IsUpVote), post.Votes.Count(v => v.IsDownVote), post.author?.UserId);
    }

    public async Task<MedicalForumComment> CreateCommentAsync(MedicalForumComment comment)
    {
        await context.MedicalForumComment.AddAsync(comment);
        await context.SaveChangesAsync();

        var commentSaved = await context.MedicalForumComment
            .Include(c => c.author)
            .Include(c => c.post)
            .ThenInclude(c => c!.author)
            .Include(c => c.Votes)
            .FirstOrDefaultAsync(c => c.id == comment.id);
        return commentSaved!;
    }

    public Task<List<MedicalForumComment>> GetComments(string postId)
    {
        return context.MedicalForumComment
            .Include(c => c.author)
            .Include(c => c.Votes)
            .Where(c => c.post_id == postId)
            .OrderByDescending(a => a.created_at)
            .ToListAsync();
    }

    public async Task<(int upVotes, int downVotes, string? authorId)> VoteComment(string commentId, string doctorId, string type)
    {
        var comment = await context.MedicalForumComment
            .Include(c => c.Votes)
            .Include(a => a.author)
            .FirstOrDefaultAsync(c => c.id == commentId);
        if (comment == null) throw new Exception("Comment Not Found");
        if (comment.author_id == doctorId) throw new Exception("You can't vote on your own comment! >:(");
        
        var vote = comment.Votes.FirstOrDefault(v => v.doctor_id == doctorId);
        if (vote != null)
        {
            if ((vote.IsUpVote && type == "upvote") || (vote.IsDownVote && type == "downvote"))
            {
                logger.LogInformation("Doctor {doctorId} removed {type} from comment {commentId}", doctorId, type, commentId);
                comment.Votes.Remove(vote);
            }
            else if((vote.IsUpVote && type == "downvote") || (vote.IsDownVote && type == "upvote"))
            {
                logger.LogInformation("Doctor {doctorId} added a {type} on comment {commentId}", doctorId, type, commentId);
                vote.vote_type = !vote.vote_type;
            }
        }
        else
        {
            logger.LogInformation("Doctor {doctorId} added a {type} on comment {commentId}", doctorId, type, commentId);
            comment.Votes.Add(new MedicalForumCommentVotes
            {
                doctor_id = doctorId,
                vote_type = type == "upvote",
                comment_id = commentId,
                created_at = DateTime.Now
            });
        }
        await context.SaveChangesAsync();
        return (comment.Votes.Count(v => v.IsUpVote), comment.Votes.Count(v => v.IsDownVote), comment.author.UserId);
    }

    public Task<int> GetTotalForumPostsAsync()
    {
        return context.MedicalForumPost.CountAsync();
    }

    public async Task<List<MedicalForumPost>> GetRecentPosts(int count = 10)
    {
        return await context.MedicalForumPost
            .Include(p => p.Votes)
            .Include(p => p.author)
            .OrderByDescending(a => a.created_at)
            .Select(f => new MedicalForumPost
            {
                id = f.id,
                heading = f.heading,
                created_at = f.created_at,
                author_id = f.author_id,
                author = new Patient
                {
                    Id = f.author!.Id,
                    Name = f.author.Name,
                },
                Votes = f.Votes
            })
            .Take(count)
            .ToListAsync();
    }
}