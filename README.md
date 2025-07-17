# Infonetica_intern_task

# .NET Minimal Workflow Engine

A lightweight, in-memory workflow engine built with ASP.NET Core Minimal API (.NET 8).  
Provides endpoints to define workflows, start instances, perform transitions, and view history.

---

##  Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
- Runs on Windows, macOS, or Linux

### Build & Run

```bash
git clone <repo-url>
cd <repo-folder>
dotnet run


### Example Requests
1. Define a workflow

bash
Copy
Edit
curl -X POST http://localhost:5000/workflow-definitions \
  -H "Content-Type: application/json" \
  -d '{
    "Id":"order",
    "States":[
      {"Id":"Pending","IsInitial":true,"IsFinal":false,"Enabled":true},
      {"Id":"Approved","IsInitial":false,"IsFinal":true,"Enabled":true}
    ],
    "Actions":[
      {"Id":"approve","Enabled":true,"FromStates":["Pending"],"ToState":"Approved"}
    ]
  }'
2. Start an instance

bash
Copy
Edit
curl -X POST http://localhost:5000/workflow-definitions/order/instances
3. Execute a transition

bash
Copy
Edit
curl -X POST http://localhost:5000/instances/<instanceId>/actions/approve
4. Get instance details

bash
Copy
Edit
curl http://localhost:5000/instances/<instanceId>
