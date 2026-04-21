# CollabSpace — Real-Time Team Collaboration Platform

A full-stack, production-deployed team collaboration platform built with
ASP.NET Core 10, React 18, and SignalR.

Live demo: https://collabspace.yourdomain.tk
API: https://api.collabspace.yourdomain.tk/swagger

## What it does

CollabSpace lets teams collaborate in real time across shared boards,
workspaces, and chat. Card updates, messages, and notifications appear
instantly for all connected users without page refreshes.

## Technical highlights

- Real-time communication via SignalR WebSockets
- JWT authentication with refresh token rotation
- Two-layer role system: global roles and workspace-scoped roles
- Optimistic UI updates on card drag-and-drop
- Cursor-based pagination for chat history
- Factory pattern for notification creation
- Strategy pattern for notification delivery
- Observer pattern via SignalR hub groups
- Repository pattern with EF Core and interface abstractions
- 30+ unit tests with xUnit and Moq
- Automated CI/CD via GitHub Actions deploying to Railway and Netlify

## Architecture

Backend: ASP.NET Core 10 Web API
├── Controllers (HTTP only, no business logic)
├── Services (all business logic)
├── Repositories via EF Core (data access only)
├── SignalR Hub (real-time event broadcasting)
└── Middleware (global exception handling, JWT)

Frontend: React 18
├── Redux Toolkit (global state)
├── Axios with interceptors (JWT attachment, 401 handling)
├── SignalR JS client (real-time updates)
└── @hello-pangea/dnd (drag and drop)

## Running locally

Backend:
  cd CollabSpace
  dotnet user-secrets set "JwtSettings:SecretKey" "your-key"
  dotnet ef database update
  dotnet run

Frontend:
  cd collabspace-client
  npm install
  npm run dev

Tests:
  dotnet test
