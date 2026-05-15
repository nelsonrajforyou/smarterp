# School ERP Developer Guide

This document explains the application's architecture and provides a step-by-step guide on how to extend the system by adding new modules.

## Architecture Flow

The School ERP system relies on **Clean Architecture** combined with **Vertical Slice Architecture**, leveraging a **custom lightweight Mediator** and Dapper.
1. **Web Layer (Blazor Server)**: Handles UI, user interactions, and DI. Calls the custom Mediator to trigger actions.
2. **Application Layer**: Contains business logic categorized by feature (e.g., Users, Students, Menus).
   - **Commands/Queries**: Plain C# records/classes defining inputs.
   - **Handlers**: Receive Mediator requests, execute Dapper queries against the DB, and return responses.
   - **Queries Class**: E.g., `UserQueries.cs`. Contains constant SQL strings specific to the module.
3. **Infrastructure Layer**: Handles external concerns (e.g., Database Connections, JWT Generation).
4. **Domain Layer**: Contains fundamental Entity classes (`User.cs`, `Student.cs`).

### Rule Highlights
- **All Data Types**: DTOs and Entities use `string` types. Dates are formatted as `yyyy-MM-dd HH:mm:ss`.
- **Soft Delete**: `IS_DELETED` is mapped to `"1"` (True) or `"0"` (False).
- **Queries**: Modularized. Do not use a central `SqlQueries.cs`. Instead, use `FeatureNameQueries.cs`.

---

## How to Add a New Module (e.g., Teachers)

To add a new module like **Teachers**, follow these steps:

### 1. Create the Domain Entity
Create a file at `SchoolErp.Domain/Entities/Teacher.cs`.
```csharp
namespace SchoolErp.Domain.Entities;
public class Teacher
{
    public string TEACHER_ID { get; set; } = Guid.NewGuid().ToString();
    public string NAME { get; set; } = default!;
    public string DEPARTMENT { get; set; } = default!;
    public string IS_ACTIVE { get; set; } = "1";
    public string IS_DELETED { get; set; } = "0";
    public string CREATED_AT { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
}
```

### 2. Create the Application Feature Slice
Create a new directory: `SchoolErp.Application/Features/Teachers`.

#### A. Define the Queries (`TeacherQueries.cs`)
```csharp
namespace SchoolErp.Application.Features.Teachers;

public static class TeacherQueries
{
    public const string GetAllTeachers = @"
        SELECT TEACHER_ID, NAME, DEPARTMENT,
               CAST(IS_ACTIVE AS CHAR) as IS_ACTIVE, CAST(IS_DELETED AS CHAR) as IS_DELETED, 
               CAST(CREATED_AT AS CHAR) as CREATED_AT
        FROM TEACHERS WHERE IS_DELETED = 0 LIMIT @Limit OFFSET @Offset;";
    
    // Add Insert, Update, Delete SQLs here...
}
```

#### B. Define the DTO and Query Handlers (`GetTeachersQuery.cs`)
```csharp
namespace SchoolErp.Application.Features.Teachers;

public class TeacherDto 
{
    public string TEACHER_ID { get; set; } = default!;
    public string NAME { get; set; } = default!;
    public string DEPARTMENT { get; set; } = default!;
}

public class GetTeachersQuery : IRequest<PaginatedList<TeacherDto>> { ... }

public class GetTeachersQueryHandler : IRequestHandler<GetTeachersQuery, PaginatedList<TeacherDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    public GetTeachersQueryHandler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<PaginatedList<TeacherDto>> Handle(GetTeachersQuery request, CancellationToken token)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Execute TeacherQueries.GetAllTeachers using connection.QueryAsync
        // Return PaginatedList
    }
}
```

#### C. Define Commands (`TeacherCommands.cs`)
Create `CreateTeacherCommand`, `UpdateTeacherCommand`, `DeleteTeacherCommand` along with their custom Mediator handlers. Use `TeacherQueries.InsertTeacher` inside the Dapper Execution block.
Ensure you assign `IsDeleted = "0"` and format dates correctly during entity creation.

### 3. Build the UI (Blazor)
Create `SchoolErp.Web/Components/Pages/Teachers.razor`.
- Use `[Authorize(Roles = "Admin")]` to restrict access.
- Inject `IMediator` (from `SchoolErp.Application.Common.Mediator`) to trigger Queries (`GetTeachersQuery`) and Commands (`CreateTeacherCommand`).
- Add the `Teachers` link to your `Menus` table in the database so it renders dynamically in the sidebar.

### 4. Build and Test
Run `dotnet build` to ensure no errors, and start the app to interact with your new module.
