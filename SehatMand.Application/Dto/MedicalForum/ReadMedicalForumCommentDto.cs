namespace SehatMand.Application.Dto.MedicalForum;

public record ReadMedicalForumCommentDto(
    string Id,
    string AuthorName,
    string AuthorId,
    string Content,
    int UpVotes,
    int DownVotes,
    string CreatedAt
    );