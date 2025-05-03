namespace SehatMand.Application.Dto.MedicalForum;

public record ReadForumPostAdminDto(
    string Title,
    string Author,
    string AuthorId,
    int CommentCount,
    int UpVotes,
    int DownVotes,
    DateTime CreatedAt
    );