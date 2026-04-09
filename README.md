# QuizApp — Merged Platform

A full-stack quiz management platform merging two projects:

- **Project 1 (QuizApp.MvcHost)**: QuizRunner component — interactive quiz-taking with progress bar, navigation, and scoring
- **Project 2 (QuizSolutionPartieProf)**: Microservices backend — JWT auth, quiz CRUD, publish/unpublish, CSV/JSON import, Ocelot API gateway

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                  FrontendBlazor                     │
│          (Blazor Server — port 5288)                │
│                                                     │
│  Pages: Login · Register · Profile                  │
│         Quizzes · QuizCreate · QuizEdit             │
│         QuizPreview · QuizImport                    │
│         QuizRunner ← merged from Project 1          │
└──────────────────┬──────────────────────────────────┘
                   │ HTTP via Ocelot Gateway
┌──────────────────▼──────────────────────────────────┐
│              ApiGateway (port 7264)                 │
│           Ocelot reverse proxy                      │
└─────────┬───────────────────────┬───────────────────┘
          │                       │
┌─────────▼────────┐   ┌──────────▼────────────────────┐
│    AuthApi       │   │      QuizContentApi            │
│  (port 5001)     │   │       (port 5002)              │
│                  │   │                                │
│  POST /register  │   │  GET/POST    /api/quizzes      │
│  POST /login     │   │  PUT/DELETE  /api/quizzes/{id} │
│  GET/PUT /profile│   │  POST        /api/quizzes/{id}/publish  │
│  SQLite + JWT    │   │  POST        /api/quizzes/{id}/duplicate│
└──────────────────┘   │  CRUD        /api/questions    │
                       │  CRUD        /api/choices      │
                       │  POST        /api/import/{id}/csv|json  │
                       │  SQLite EF Core                │
                       └────────────────────────────────┘
```

---

## Running the Solution

### Prerequisites
- .NET 9 SDK
- Ports 5001, 5002, 7264, 5288 available

### Start all services (4 terminals)

```bash
# Terminal 1 — Auth API
cd AuthApi && dotnet run

# Terminal 2 — Quiz Content API
cd QuizContentApi && dotnet run

# Terminal 3 — API Gateway
cd ApiGateway && dotnet run

# Terminal 4 — Frontend
cd FrontendBlazor && dotnet run
```

Then open: **https://localhost:5288**
