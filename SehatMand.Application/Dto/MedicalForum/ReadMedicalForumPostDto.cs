namespace SehatMand.Application.Dto.MedicalForum;

public record ReadMedicalForumPostDto(
    string Id,
    string AuthorName,
    string Title,
    string Content,
    int UpVotes,
    int DownVotes,
    string CreatedAt
    );