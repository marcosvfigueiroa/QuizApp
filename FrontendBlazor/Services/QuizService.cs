using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FrontendBlazor.Services;

public class QuizService
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenState _state;

    public QuizService(IHttpClientFactory factory, TokenState state)
    {
        _factory = factory;
        _state = state;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("GatewayClient");

        if (!string.IsNullOrEmpty(_state.Token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _state.Token);
        }
        else
        {
            client.DefaultRequestHeaders.Authorization = null;
        }

        return client;
    }

    // ------------ DTOs ------------

    public record QuizListItem(int Id, string Title, bool IsPublished, int QuestionCount);

    public class QuizDetail
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public bool IsPublished { get; set; }
        public List<Question> Questions { get; set; } = new();
    }

    public class Question
    {
        public int Id { get; set; }
        public string Statement { get; set; } = "";
        // IMPORTANT : int pour matcher l'enum QuestionCategory (0 = Facile, 1 = Moyenne, 2 = Difficile)
        public int Category { get; set; } = 0;
        public int Weight { get; set; } = 1;
        public List<Choice> Choices { get; set; } = new();
    }

    public class Choice
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public bool IsCorrect { get; set; }
    }

    // ------------ Quizzes ------------

    public async Task<List<QuizListItem>> GetQuizzesAsync(string? search = null)
    {
        var client = CreateClient();

        var url = "quizzes"; // ApiGateway → QuizContentApi: /api/quizzes
        if (!string.IsNullOrWhiteSpace(search))
            url += $"?search={Uri.EscapeDataString(search)}";

        var result = await client.GetFromJsonAsync<List<QuizListItem>>(url);
        return result ?? new();
    }

    public async Task<QuizDetail?> GetQuizAsync(int id)
    {
        var client = CreateClient();
        return await client.GetFromJsonAsync<QuizDetail>($"quizzes/{id}");
    }

    public async Task<int?> CreateQuizAsync(string title, string? description)
    {
        var client = CreateClient();
        var body = new QuizDetail { Title = title, Description = description };

        var response = await client.PostAsJsonAsync("quizzes", body);
        if (!response.IsSuccessStatusCode) return null;

        var created = await response.Content.ReadFromJsonAsync<QuizDetail>();
        return created?.Id;
    }

    public async Task<bool> UpdateQuizAsync(QuizDetail quiz)
    {
        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"quizzes/{quiz.Id}", quiz);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteQuizAsync(int id)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"quizzes/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PublishAsync(int id)
    {
        var client = CreateClient();
        var response = await client.PostAsync($"quizzes/{id}/publish", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnpublishAsync(int id)
    {
        var client = CreateClient();
        var response = await client.PostAsync($"quizzes/{id}/unpublish", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<QuizDetail?> DuplicateAsync(int id)
    {
        var client = CreateClient();
        var response = await client.PostAsync($"quizzes/{id}/duplicate", null);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<QuizDetail>();
    }

    // ------------ Questions / Choices ------------

    // Category en int pour matcher l'enum QuestionCategory côté API
    public record CreateQuestionRequest(int QuizId, string Statement, int Category, int Weight);
    public record UpdateQuestionRequest(string Statement, int Category, int Weight);
    public record CreateChoiceRequest(int QuestionId, string Text, bool IsCorrect);
    public record UpdateChoiceRequest(string Text, bool IsCorrect);

    public async Task<int?> AddQuestionAsync(int quizId, string statement, int category, int weight)
    {
        var client = CreateClient();
        var body = new CreateQuestionRequest(quizId, statement, category, weight);

        var response = await client.PostAsJsonAsync("questions", body);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<Question>();
        return result?.Id;
    }

    public async Task<bool> UpdateQuestionAsync(int id, string statement, int category, int weight)
    {
        var client = CreateClient();
        var body = new UpdateQuestionRequest(statement, category, weight);
        var response = await client.PutAsJsonAsync($"questions/{id}", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteQuestionAsync(int id)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"questions/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<int?> AddChoiceAsync(int questionId, string text, bool isCorrect)
    {
        var client = CreateClient();
        var body = new CreateChoiceRequest(questionId, text, isCorrect);
        var response = await client.PostAsJsonAsync("choices", body);

        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<Choice>();
        return result?.Id;
    }

    public async Task<bool> UpdateChoiceAsync(int id, string text, bool isCorrect)
    {
        var client = CreateClient();
        var body = new UpdateChoiceRequest(text, isCorrect);
        var response = await client.PutAsJsonAsync($"choices/{id}", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteChoiceAsync(int id)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"choices/{id}");
        return response.IsSuccessStatusCode;
    }

    // ------------ Import ------------

    public async Task<bool> ImportCsvAsync(int quizId, Stream fileStream, string fileName)
    {
        var client = CreateClient();
        var content = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "file", fileName }
        };
        var response = await client.PostAsync($"import/{quizId}/csv", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ImportJsonAsync(int quizId, Stream fileStream, string fileName)
    {
        var client = CreateClient();
        var content = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "file", fileName }
        };
        var response = await client.PostAsync($"import/{quizId}/json", content);
        return response.IsSuccessStatusCode;
    }
}
