# 📄 Driving Licence Onboarding Workflow System

This project is a backend system that simulates a driving licence onboarding workflow with asynchronous processing, state transitions, and audit tracking.

It is built using:

- .NET 8
- ASP.NET Core Web API
- PostgreSQL (EF Core)
- NATS messaging
- Background Worker Service
- Custom state machine for workflow processing

---

## 🧭 Architecture Overview

### 🧩 Solution Structure

```text
Solution
├── WebApi              → Entry point (HTTP API)
├── Worker              → 2 Background message processors
├── Infrastructure      → EF Core, database access
└── IntegrationTests    → End-to-end tests
```

### 🔄 High-Level System Design
```text
flowchart TD
    Client --> WebAPI
    WebAPI --> PostgreSQL
    WebAPI --> Worker
    Worker --> NATS
    NATS --> Worker
    Worker --> PostgreSQL
```

### ⚙️ Tech Stack
```text
.NET 8
ASP.NET Core Web API
PostgreSQL (EF Core)
NATS (messaging / async processing)
Background Worker Service
xUnit + Testcontainers (integration tests)
Swagger
Docker Compose
```

### 🚀 How to Run the Project
```text
📌 Prerequisites
.NET 8 SDK
Docker Desktop
Git
```

### 1️⃣ Clone the repository
```text
git clone <repo-url>
cd <solution-folder>
```

### 2️⃣ Start infrastructure services
```text

The system depends on external services:

PostgreSQL
NATS message broker

docker-compose up -d

Swagger:
http://localhost:8080/swagger

The worker consumes messages from NATS and processes workflow transitions asynchronously.
```

### 5️⃣ Run tests
```text
dotnet test
tests include unit tests and integration tests
integration test are containerized meaning Docker needs to be running to run them properly
```

### 🧪 Testing Stack
```text
Testcontainers (PostgreSQL)
WebApplicationFactory
xUnit
FluentAssertions
```

### 🎯 Coverage
```text
End-to-end workflow execution
State transitions
Failure scenarios
```

## 🔄 System Workflow

### 📌 Main Flow
```text
Client submits application
API stores application in PostgreSQL
API stores Applicationpending by using the Outbox Pattern
Worker processes the application and sends to NATS
Worker consumes event asynchronously
Worker executes workflow state machine:
Data validation
Photo verification
Risk assessment
Application transitions through states
Every transition is persisted in audit log
```

### 🧠 Workflow / State Machine Design
```text
📌 Approach
A custom lightweight state machine is implemented inside the Worker.
```



### 📍 Supported States
```text
Pending,
Submitted
PendingProcess
ValidatingData
CrossCheckingRegistry
CheckingPhoto
RiskAssessment
PendingManualReview
Approved
Rejected
Failed
```

### 📌 Design Choice
```text
Instead of using a full workflow engine (MassTransit Saga / Elsa / Temporal), 
a custom state machine was chosen to:

Keep full control over transitions
Reduce complexity within the 6–8 hour scope
Make business rules explicit and testable
```


## 📡 Asynchronous Processing

### 📌 Messaging System
The system uses NATS as a lightweight message broker.


### ✅ Why NATS
```text
Fast and lightweight
Simple pub/sub model
Easy Docker setup
Ideal for event-driven workflows
```

### 🗄️ Data Persistence
```text
PostgreSQL is the primary database.

EF Core handles ORM and migrations.

All applications and audit logs are persisted.

Captured information:
State changes
Triggering events
Manual review decisions
Failure reasons

✔ Provides full traceability of the workflow lifecycle
```

### 🔀 Worker Separation (Design Simplification)
```text
The system uses two background workers: one responsible for publishing events
to NATS and another for consuming those events and executing the workflow. 
This separation is mainly for simplicity and to clearly split responsibilities 
between event publishing and business processing. In a real-world scenario,
these workers could be deployed as fully independent services in a distributed system.
```

## ⚠️ Reliability Considerations

### 🔁 Duplicate Message Processing

The system is designed for at-least-once delivery, so the same message may be processed more than once.  
To handle this, each message includes an idempotency key (GUID), and the worker checks whether the operation has already been applied before executing it.  
This ensures that duplicate messages do not cause duplicate state transitions or audit entries.

---

### 📡 API saves application but fails to publish message

If the API successfully writes the application to the database but fails to publish the event, the system relies on the Outbox Pattern.  
Events are stored in the database first and published asynchronously by a background worker, ensuring no event is lost.  
This prevents inconsistencies between the database state and the message broker.

---

### 🧵 Worker crashes during processing

If a worker crashes while processing a message, the message is not lost and will be retried depending on NATS delivery configuration.  
Since processing is idempotent, reprocessing the same message is safe.  
The workflow can resume without corrupting state.

---

### 🔄 Invalid workflow transitions

Invalid transitions are prevented inside the custom state machine by enforcing a strict set of allowed state transitions.  
Any attempt to move to an invalid state is rejected at runtime.  
This guarantees that the workflow remains consistent and predictable.

---

### 🧾 Failure handling

Failures during processing are captured and persisted in the database as part of the application state.
If the workflow transitions to a Failed state, the system records the error along with the failed transition.
This ensures traceability of failed workflow executions and their originating state transitions.

---

### 🔁 Retries and Dead-letter handling

Retries are handled through message redelivery in NATS combined with idempotent processing on the worker side.  
If processing continues to fail, the message will keep being retried based on NATS delivery behavior and worker restart cycles.  
Dead-letter handling was not implemented as part of the current scope.

---

### 🚀 Production improvements

Before production, the system could be improved by:

- Adding full distributed tracing (e.g., OpenTelemetry)
- Introducing a proper dead-letter queue strategy with monitoring
- Improving retry policies with exponential backoff
- Adding centralized observability dashboards
- Splitting workers into independently scalable services
- Strengthening consistency with a fully managed workflow engine if complexity grows

### 📦 Docker Setup
```text
Includes:

WebApi
Worker
PostgreSQL
NATS
```

### 🔁 Idempotency (Event Deduplication)
```text
The system uses an idempotency key (GUID) in each NATS message to ensure safe 
processing in an at-least-once delivery model. This prevents duplicate 
execution of the same workflow step when messages are retried or re-delivered.
```

### 📦 Outbox Pattern
```text
A simplified Outbox Pattern is used to reliably bridge database changes 
and event publishing to NATS. Events are first persisted in the database
and then published asynchronously by a background worker, preventing message
loss if publishing fails.
```

### 📷 Extra Photo Endpoint
```text
The photo upload was implemented as a separate endpoint to decouple
file handling from the main application creation flow. 
This keeps the core request lightweight and avoids mixing binary 
data with business data. It also better reflects real-world 
systems where files are often handled and stored independently 
(e.g., object storage).

Trade-offs: It requires an additional API call and slightly
more client-side coordination, but improves separation of
concerns and flexibility.
```

### ⚡ Extra Submit Endpoint
```text
An additional submit endpoint was introduced to simulate a clearer separation
between application creation and workflow execution. Although not required by
the assignment, it reflects a more realistic production-style business flow 
where submission explicitly triggers processing.
```

### 📊 Logging & Observability
```text
The system uses Serilog for structured logging across API and Worker services.
Instead of plain text logs, the application produces structured (tokenized) logs,
where each log entry contains named properties rather than raw strings.
```

### ☁️ Cloud-Ready Logging (Azure / AWS)
```text
Structured logs are designed to integrate easily with cloud observability stacks
```

## 📈 Key Design Decisions

### 🔄 Event-driven architecture (NATS)
```text
Enables asynchronous processing
Decouples API from business logic
Tradeoff: eventual consistency
```

### 🧠 Custom state machine
```text
Explicit workflow control
Easy to test and reason about
Tradeoff: less powerful than full workflow engines
```

### 🔀 Separation of API and Worker
```text
Independent scaling possible
Clear separation of concerns
```

### 🗄️ EF Core
```text
Rapid development
Built-in migrations support
Tradeoff: less low-level DB control
```

### 📌 Summary
```text
This system demonstrates:

Event-driven backend architecture
Workflow/state machine design
Asynchronous processing with messaging
Auditability and traceability
End-to-end integration testing with real infrastructure
```