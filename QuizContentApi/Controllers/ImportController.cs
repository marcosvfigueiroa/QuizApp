using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizContentApi.Data;
using QuizContentApi.DTOs;
using QuizContentApi.Models;

namespace QuizContentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly QuizDbContext _context;

    public ImportController(QuizDbContext context)
    {
        _context = context;
    }

    [HttpPost("{quizId}/csv")]
    public async Task<IActionResult> ImportCsv(int quizId, IFormFile file)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null) return NotFound("Quiz not found.");

        if (file == null || file.Length == 0) return BadRequest("Empty file.");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        // skip header
        var header = await reader.ReadLineAsync();

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var parts = line.Split(';');

            if (parts.Length < 7) continue; // need at least statement;cat;weight;choice1;isCorrect1;choice2;isCorrect2

            var statement = parts[0];
            var categoryStr = parts[1];
            var weight = int.TryParse(parts[2], out var w) ? w : 1;

            if (!Enum.TryParse<QuestionCategory>(categoryStr, true, out var category))
                category = QuestionCategory.Facile;

            var question = new Question
            {
                QuizId = quizId,
                Statement = statement,
                Category = category,
                Weight = weight
            };

            for (int i = 3; i + 1 < parts.Length; i += 2)
            {
                var text = parts[i];
                var isCorrectStr = parts[i + 1];
                bool isCorrect = bool.TryParse(isCorrectStr, out var b) ? b : false;

                question.Choices.Add(new Choice
                {
                    Text = text,
                    IsCorrect = isCorrect
                });
            }

            if (question.Choices.Count >= 2 &&
                question.Choices.Count(c => c.IsCorrect) == 1)
            {
                _context.Questions.Add(question);
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{quizId}/json")]
    public async Task<IActionResult> ImportJson(int quizId, IFormFile file)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null) return NotFound("Quiz not found.");

        if (file == null || file.Length == 0) return BadRequest("Empty file.");

        using var stream = file.OpenReadStream();

        // options pour ignorer la casse des noms de propriétés
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var data = await JsonSerializer.DeserializeAsync<List<ImportQuestionJsonDto>>(stream, options);

        if (data == null) return BadRequest("Invalid JSON.");

        foreach (var q in data)
        {
            if (!Enum.TryParse<QuestionCategory>(q.Category, true, out var category))
                category = QuestionCategory.Facile;

            var question = new Question
            {
                QuizId = quizId,
                Statement = q.Statement,
                Category = category,
                Weight = q.Weight
            };

            foreach (var c in q.Choices)
            {
                question.Choices.Add(new Choice
                {
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                });
            }

            if (question.Choices.Count >= 2 &&
                question.Choices.Count(c => c.IsCorrect) == 1)
            {
                _context.Questions.Add(question);
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

}
