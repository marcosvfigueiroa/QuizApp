using QuizContentApi.Models;

namespace QuizContentApi.DTOs;

public class CreateQuestionRequest
{
    public int QuizId { get; set; }
    public string Statement { get; set; } = null!;
    public QuestionCategory Category { get; set; }
    public int Weight { get; set; } = 1;
}

public class UpdateQuestionRequest
{
    public string Statement { get; set; } = null!;
    public QuestionCategory Category { get; set; }
    public int Weight { get; set; } = 1;
}

public class CreateChoiceRequest
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

public class UpdateChoiceRequest
{
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}
