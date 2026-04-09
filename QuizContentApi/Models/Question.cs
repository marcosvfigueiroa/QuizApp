namespace QuizContentApi.Models;

public enum QuestionCategory
{
    Facile,
    Moyenne,
    Difficile
}

public class Question
{
    public int Id { get; set; }
    public string Statement { get; set; } = null!;
    public QuestionCategory Category { get; set; }
    public int Weight { get; set; } = 1;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
}
