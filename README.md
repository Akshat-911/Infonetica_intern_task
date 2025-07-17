# Infonetica_intern_task

# .NET Minimal Workflow Engine

A lightweight, in-memory workflow engine built with ASP.NET Core Minimal API (.NET 8).  
Provides endpoints to define workflows, start instances, perform transitions, and view history.

---

##  Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
- Runs on Windows, macOS, or Linux

### Assumptions & Limitations
1. In-memory storage only: All data resets on app restart; no database.

2. Single initial state: Exactly one state must have IsInitial = true.

3. Enable/disable flags: States and actions can be toggled, but no post-creation management.

4. Final-state enforcement: No transitions allowed once in a IsFinal state.

5. No authentication/security: Meant for internal or demo use.

6. Simple history tracking: Logs only DateTime.UtcNow; no formatting or timezone handling.

7. Minimal feature set: No Swagger/OpenAPI, validation, or UI by default.
 
### Extension Opportunities
1. Use a database (e.g., via EF Core, Redis, or SQL)

2. Add Swagger/OpenAPI (AddEndpointsApiExplorer, AddSwaggerGen)

3. Validate input using DataAnnotations or FluentValidation

4. Implement authentication and authorization

5. Build a UI with Blazor, React, Angular, etc

### Build & Run

```bash
git clone <repo-url>
cd <repo-folder>
dotnet run

