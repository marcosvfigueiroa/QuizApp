using QuizContentApi.Models;

namespace QuizContentApi.DTOs;

public class QuizListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsPublished { get; set; }
    public int QuestionCount { get; set; }
}

public class QuizDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Statement { get; set; } = null!;
    public QuestionCategory Category { get; set; }
    public int Weight { get; set; }
    public List<ChoiceDto> Choices { get; set; } = new();
}

public class ChoiceDto
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}
