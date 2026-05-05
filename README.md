# CollabSpace — Real-Time Team Collaboration Platform

> Bring your team together. One workspace for tasks, chat, and collaboration — updating live for everyone.

**Live demo:** [https://gleeful-conkies-f3063f.netlify.app](https://gleeful-conkies-f3063f.netlify.app)
**API docs:** [https://collabspace-production.up.railway.app/swagger](https://collabspace-production.up.railway.app/swagger)

---

## What Is CollabSpace

CollabSpace is a production-deployed, full-stack real-time collaboration platform that consolidates task management, team chat, and workspace coordination into a single unified tool. Card updates, messages, and notifications appear instantly for all connected users — no page refresh required.

Built as a capstone project demonstrating professional-grade software engineering across the full stack.

---

## Features

| Feature | Description | Real-Time |
|---|---|---|
| **Kanban Boards** | Drag-and-drop cards across Todo, In Progress, Done | Yes — SignalR |
| **Team Chat** | Workspace group chat and private direct messages | Yes — SignalR |
| **Notifications** | In-app alerts for mentions, assignments, and updates | Yes — SignalR |
| **Workspaces** | Create workspaces, invite via code, manage roles | REST |
| **Comments** | Threaded comments with @mention detection | REST |
| **Activity Feed** | Recent workspace activity and task statistics | REST |
| **Authentication** | JWT login with refresh token rotation | REST |

---

## Technical Highlights

- Real-time communication via SignalR WebSockets
- JWT authentication with refresh token rotation and two-layer role system
- Optimistic UI updates on card drag-and-drop
- Cursor-based pagination for chat history
- Factory pattern for notification creation
- Strategy pattern for notification delivery
- Observer pattern via SignalR hub groups
- Repository pattern with EF Core and interface abstractions
- 30+ unit tests with xUnit and Moq
- Automated CI/CD via GitHub Actions deploying to Railway and Netlify

---

## Architecture

```
Backend — ASP.NET Core 10
├── Controllers       HTTP only, no business logic
├── Services          All business logic and orchestration
├── EF Core / Repos   Data access and migrations
├── SignalR Hub       Real-time event broadcasting
└── Middleware        Global exception handling, JWT, CORS

Frontend — React 18
├── Redux Toolkit     Global state management
├── Axios             JWT interceptors and 401 handling
├── SignalR JS Client Real-time event listeners
└── @hello-pangea/dnd Accessible drag-and-drop
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 10 |
| Real-Time | SignalR (WebSockets) |
| Database | PostgreSQL via Entity Framework Core |
| Auth | JWT Bearer + BCrypt |
| Validation | FluentValidation |
| Frontend | React 18 + Redux Toolkit |
| HTTP Client | Axios |
| Hosting | Railway (API) + Netlify (Frontend) |
| CI/CD | GitHub Actions |

---

## Running Locally

**Backend**
```bash
cd CollabSpace
dotnet user-secrets set "JwtSettings:SecretKey" "your-32-char-key-here"
dotnet ef database update
dotnet run
```

**Frontend**
```bash
cd collabspace-client
npm install
npm run dev
```

Create a `.env` file in `collabspace-client/`:
```
VITE_API_URL=http://localhost:5068/api/v1
```

**Tests**
```bash
dotnet test
```

---

## Project Structure

```
CollabSpace/
├── Controllers/          API endpoints
├── Services/             Business logic
├── Data/                 EF Core DbContext and entities
├── Models/               DTOs, entities, settings
├── Hubs/                 SignalR CollabHub
├── Middleware/           Global exception handler
├── Factories/            NotificationFactory
├── Validators/           FluentValidation rules
├── Migrations/           EF Core migrations
├── CollabSpace.Tests/    Unit tests (xUnit + Moq)
└── collabspace-client/   React 18 frontend
```

---

## Environment Variables

| Variable | Where | Description |
|---|---|---|
| `JwtSettings__SecretKey` | Railway | Minimum 32 characters |
| `ConnectionStrings__DatabaseConnection` | Railway | `${{Postgres.DATABASE_URL}}` |
| `AllowedOrigins__0` | Railway | Your Netlify URL |
| `ASPNETCORE_ENVIRONMENT` | Railway | `Production` |
| `VITE_API_URL` | Netlify | Railway API base URL |

---

## License

This project is for portfolio and academic use.
