using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizContentApi.Data;
using QuizContentApi.DTOs;
using QuizContentApi.Models;

namespace QuizContentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChoicesController : ControllerBase
{
    private readonly QuizDbContext _context;

    public ChoicesController(QuizDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<ChoiceDto>> CreateChoice(CreateChoiceRequest request)
    {
        var question = await _context.Questions
            .Include(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId);

        if (question == null) return NotFound("Question not found.");

        if (request.IsCorrect && question.Choices.Any(c => c.IsCorrect))
            return BadRequest("Question already has a correct choice.");

        var choice = new Choice
        {
            QuestionId = request.QuestionId,
            Text = request.Text,
            IsCorrect = request.IsCorrect
        };

        _context.Choices.Add(choice);
        await _context.SaveChangesAsync();

        return new ChoiceDto
        {
            Id = choice.Id,
            Text = choice.Text,
            IsCorrect = choice.IsCorrect
        };
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChoice(int id, UpdateChoiceRequest request)
    {
        var choice = await _context.Choices
            .Include(c => c.Question)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (choice == null) return NotFound();

        if (request.IsCorrect && choice.Question.Choices.Any(c => c.IsCorrect && c.Id != id))
            return BadRequest("Another correct choice already exists for this question.");

        choice.Text = request.Text;
        choice.IsCorrect = request.IsCorrect;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChoice(int id)
    {
        var choice = await _context.Choices.FindAsync(id);
        if (choice == null) return NotFound();

        _context.Choices.Remove(choice);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
