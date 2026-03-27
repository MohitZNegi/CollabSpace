# CollabSpace

CollabSpace is a real-time team collaboration platform designed to unify communication, task management, and workspace collaboration into a single system.

## Project Overview

Modern teams often use multiple tools for messaging, task tracking, and collaboration. This leads to fragmentation and reduced productivity. CollabSpace solves this by providing an integrated platform where teams can communicate, manage tasks, and collaborate in real time.

## Core Features

- JWT-based authentication with role management (Admin, Team Lead, Member)
- Workspace creation and member management via invite codes
- Real-time collaborative boards with drag-and-drop task management
- Workspace group chat and direct messaging
- Threaded comments on tasks and boards
- Real-time notification system with persistence
- Activity dashboard for tracking tasks and user activity
- Search across users, messages, and tasks

## Tech Stack

- .NET Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- SignalR (real-time communication)
- Azure (CI/CD deployment)
- xUnit (testing)

## Project Scope

### In Scope

- Authentication and authorization system
- Workspace and membership management
- Real-time collaboration features
- Messaging system (group and direct)
- Notifications and activity tracking
- Search functionality
- CI/CD deployment pipeline

### Out of Scope

- Email notifications
- Mobile applications (iOS/Android)
- File uploads and media handling
- Video/voice communication
- Third-party integrations (GitHub, Jira, Slack)

## Getting Started

1. Clone the repository  
2. Update `appsettings.json`:
   - Add database connection string
   - Configure JWT settings  
3. Apply database migrations:

dotnet ef database update

4. Run the application:

dotnet run


## API Testing (Swagger)

- Run the application
- Open:

https://localhost:{port}/swagger

- Use Swagger UI to test endpoints like `/api/v1/auth/register`

## Authentication Endpoints

### Register

POST /api/v1/auth/register


Request body:

{
"username": "testuser",
"email": "test@example.com
",
"password": "SecurePass123!"
}


### Login

POST /api/v1/auth/login


### Refresh Token

POST /api/v1/auth/refresh


### Logout

POST /api/v1/auth/logout


## Testing

Run tests using:

dotnet test


- Uses in-memory database
- Ensures no impact on real data
- Covers workspace authorization logic

## Project Structure


CollabSpace/
├── Controllers/
├── Services/
├── Data/
├── Models/
└── Settings/

CollabSpace.Tests/
└── Services/


## Notes

- Ensure only one BCrypt package is installed to avoid conflicts
- Keep JWT secret key secure
- Update vulnerable dependencies regularly
