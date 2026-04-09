using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizContentApi.Data;
using QuizContentApi.DTOs;
using QuizContentApi.Models;

namespace QuizContentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly QuizDbContext _context;

    public QuestionsController(QuizDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionRequest request)
    {
        var quiz = await _context.Quizzes.FindAsync(request.QuizId);
        if (quiz == null) return NotFound("Quiz not found.");

        var question = new Question
        {
            QuizId = request.QuizId,
            Statement = request.Statement,
            Category = request.Category,
            Weight = request.Weight
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        return new QuestionDto
        {
            Id = question.Id,
            Statement = question.Statement,
            Category = question.Category,
            Weight = question.Weight,
            Choices = new()
        };
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuestion(int id, UpdateQuestionRequest request)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null) return NotFound();

        question.Statement = request.Statement;
        question.Category = request.Category;
        question.Weight = request.Weight;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null) return NotFound();

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
