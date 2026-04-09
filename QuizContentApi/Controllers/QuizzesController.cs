using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizContentApi.Data;
using QuizContentApi.DTOs;
using QuizContentApi.Models;

namespace QuizContentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizzesController : ControllerBase
{
    private readonly QuizDbContext _context;

    public QuizzesController(QuizDbContext context)
    {
        _context = context;
    }

    // Liste + recherche
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizListItemDto>>> GetQuizzes([FromQuery] string? search)
    {
        var query = _context.Quizzes
            .Include(q => q.Questions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(q => q.Title.Contains(search));
        }

        var result = await query
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuizListItemDto
            {
                Id = q.Id,
                Title = q.Title,
                IsPublished = q.IsPublished,
                QuestionCount = q.Questions.Count
            })
            .ToListAsync();

        return Ok(result);
    }

    // Détail
    [HttpGet("{id}")]
    public async Task<ActionResult<QuizDetailDto>> GetQuiz(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null) return NotFound();

        return new QuizDetailDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            IsPublished = quiz.IsPublished,
            Questions = quiz.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Statement = q.Statement,
                Category = q.Category,
                Weight = q.Weight,
                Choices = q.Choices.Select(c => new ChoiceDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                }).ToList()
            }).ToList()
        };
    }

    // Créer
    [HttpPost]
    public async Task<ActionResult<QuizDetailDto>> CreateQuiz(QuizDetailDto dto)
    {
        var quiz = new Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            IsPublished = false
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        dto.Id = quiz.Id;
        dto.IsPublished = quiz.IsPublished;
        dto.Questions = new();
        return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, dto);
    }

    // Modifier
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuiz(int id, QuizDetailDto dto)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null) return NotFound();

        quiz.Title = dto.Title;
        quiz.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Supprimer
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null) return NotFound();

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Publier (avec contraintes)
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null) return NotFound();

        if (!quiz.Questions.Any())
            return BadRequest("A published quiz must contain at least one question.");

        foreach (var question in quiz.Questions)
        {
            if (question.Choices.Count < 2)
                return BadRequest($"Question {question.Id} must have at least 2 choices.");

            var correctCount = question.Choices.Count(c => c.IsCorrect);
            if (correctCount != 1)
                return BadRequest($"Question {question.Id} must have exactly 1 correct choice.");
        }

        quiz.IsPublished = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Retirer
    [HttpPost("{id}/unpublish")]
    public async Task<IActionResult> Unpublish(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null) return NotFound();

        quiz.IsPublished = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Dupliquer
    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<QuizDetailDto>> Duplicate(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null) return NotFound();

        var copy = new Quiz
        {
            Title = quiz.Title + " (copy)",
            Description = quiz.Description,
            IsPublished = false,
            Questions = quiz.Questions.Select(q => new Question
            {
                Statement = q.Statement,
                Category = q.Category,
                Weight = q.Weight,
                Choices = q.Choices.Select(c => new Choice
                {
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                }).ToList()
            }).ToList()
        };

        _context.Quizzes.Add(copy);
        await _context.SaveChangesAsync();

        return await GetQuiz(copy.Id);
    }
}
