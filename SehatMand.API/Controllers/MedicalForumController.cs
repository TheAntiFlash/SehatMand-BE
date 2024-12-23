using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Dto.MedicalForum;
using SehatMand.Application.Mapper;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Domain.Utils.Notification;

namespace SehatMand.API.Controllers;

/// <summary>
/// Medical Forum Controller
/// This controller is responsible for handling all the medical forum related requests.
/// It includes the following functionalities:
/// - Post a question
/// - Get all questions
/// - Vote on a question
/// - Get comments on a question
/// - Comment on a question
/// - Vote on a comment
/// </summary>
/// <param name="forumRepo"></param>
/// <param name="patientRepo"></param>
/// <param name="doctorRepo"></param>
/// <param name="logger"></param>
[ApiController]
[Route("api/medical-forum")]
public class MedicalForumController(
    IMedicalForumRepository forumRepo,
    IPatientRepository patientRepo,
    IDoctorRepository doctorRepo,
    IPushNotificationService notificationServ,
    ILogger<MedicalForumController> logger
    ): ControllerBase
{
    
    /// <summary>
    /// Post a question
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpPost]
    [Route("post")]
    public async Task<IActionResult> Post([FromBody] CreateMedicalForumPost dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var patientId = await patientRepo.GetPatientIdByUserId(id);
            if (patientId == null) throw new Exception("Logged In Patient not found. Please log in again.");
            var question = await forumRepo.CreatePostAsync(dto.ToMedicalForumPost(patientId));
            return CreatedAtAction(nameof(Post), question.id ,question.ToReadMedicalForumPostDto());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to post question");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to post question",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Get all questions
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var questions = await forumRepo.GetAllPosts();
            
            return Ok(questions.Select(q => q.ToReadMedicalForumPostDto()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get questions");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get questions",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Vote on a question
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpPost]
    [Route("{postId}/vote/{type}")]
    public async Task<IActionResult> Vote([FromRoute] string postId, [FromRoute, RegularExpression("^upvote|downvote")] string type)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            
            var (upVotes, downVotes, authorId) = await forumRepo.Vote(postId, id, type);
            await notificationServ.SendPushNotificationAsync(
                $"Someone {type}d your question",
                $"",
                $"You now have {upVotes} upvotes and {downVotes} downvotes on your question",
                [authorId?? ""], NotificationContext.MEDICAL_FORUM);
            return Ok(new
            {
                upVotes,
                downVotes
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to vote");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to vote",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Get comments on a question
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("{postId}/comment")]
    public async Task<IActionResult> GetComments([FromRoute] string postId)
    {
        try
        {
            var comments = await forumRepo.GetComments(postId);
            return Ok(comments.Select(c => c.ToReadMedicalForumCommentDto()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get comments");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get comments",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Comment on a question
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpPost]
    [Route("{postId}/comment")]
    public async Task<IActionResult> Comment([FromRoute] string postId, [FromBody] CreateMedicalForumCommentDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var doctorId = await doctorRepo.GetDoctorIdByUserId(id);
            if (doctorId == null) throw new Exception("Logged In Doctor not found. Please log in again.");
            
            var comment = await forumRepo.CreateCommentAsync(dto.ToMedicalForumComment(postId, doctorId));
            
            await notificationServ.SendPushNotificationAsync(
                $"A doctor commented on your question",
                $"Dr. {comment.author?.Name} commented: ",
                $"{comment.content.Substring(0, Math.Min(50, comment.content.Length))}...",
                [comment.post?.author?.UserId?? ""], NotificationContext.MEDICAL_FORUM);
            return CreatedAtAction(nameof(Comment), comment.id, comment.ToReadMedicalForumCommentDto());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to post comment");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to post comment",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Vote on a comment
    /// </summary>
    /// <param name="commentId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpPost]
    [Route("comment/{commentId}/vote/{type}")]
    public async Task<IActionResult> VoteComment([FromRoute] string commentId, [FromRoute, RegularExpression("^upvote|downvote")] string type)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var doctorId = await doctorRepo.GetDoctorIdByUserId(id);
            if (doctorId == null) throw new Exception("Logged In Doctor not found. Please log in again.");
            var (upVotes, downVotes, authorId) = await forumRepo.VoteComment(commentId, doctorId, type);
            
            await notificationServ.SendPushNotificationAsync(
                $"Someone {type}d your comment",
                $"",
                $"You now have {upVotes} upvotes and {downVotes} downvotes on your question",
                [authorId?? ""], NotificationContext.MEDICAL_FORUM);
            return Ok(new
            {
                upVotes,
                downVotes
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to vote comment");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to vote comment",
                e.Message
            ));
        }
    }
    
    
}