namespace QuizContentApi.DTOs;

public class ImportQuestionJsonDto
{
    public string Statement { get; set; } = null!;
    public string Category { get; set; } = "Facile";
    public int Weight { get; set; } = 1;
    public List<ImportChoiceJsonDto> Choices { get; set; } = new();
}

public class ImportChoiceJsonDto
{
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}
