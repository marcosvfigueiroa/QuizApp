using FrontendBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// HttpClient vers le gateway
builder.Services.AddHttpClient("GatewayClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7264/"); // HTTPS du ApiGateway
});

// état du token
builder.Services.AddScoped<TokenState>();

// services applicatifs
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<ProfileService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
