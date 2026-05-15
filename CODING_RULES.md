# School ERP — Coding Rules & Standards

> **Every developer must follow these rules before submitting code.**
> These rules keep the codebase consistent, maintainable, and production-ready.

---

## Table of Contents
1. [Database Rules](#1-database-rules)
2. [C# Entity & DTO Rules](#2-c-entity--dto-rules)
3. [Data Access Rules](#3-data-access-rules)
4. [Security Standards](#4-security-standards)
5. [Performance Optimization](#5-performance-optimization)
6. [UI / CSS Standards — Common Classes](#6-ui--css-standards--common-classes)
7. [Grid / List View Toggle Pattern](#7-grid--list-view-toggle-pattern)
8. [Mobile-First Responsive Rules](#8-mobile-first-responsive-rules)
9. [Scoped CSS vs Global CSS Rules](#9-scoped-css-vs-global-css-rules)
10. [Blazor Component Conventions](#10-blazor-component-conventions)
11. [Custom Shared Components — SelectPicker](#11-custom-shared-components--selectpicker)

---

## 1. Database Rules

### Soft Deletion
- Every table **must** have an `IS_DELETED INT` column.
- Deletion = set `IS_DELETED = 1`. **Never** physically delete rows.
- Every `SELECT` query **must** include `WHERE IS_DELETED <> 1`.

### Status Flags
| Layer | Column | Type | Values |
|-------|--------|------|--------|
| Database | `IS_ACTIVE` / `IS_DELETED` | `INT` | `0` or `1` |
| C# Entity / DTO | `IsActive` / `IsDeleted` | `string` | `"0"` or `"1"` |

### Naming Convention
- All **table names** and **column names** must be `UPPERCASE`.
- Primary keys: `TABLE_NAME_ID` format (e.g., `STUDENT_ID`, `STAFF_ID`).
- Foreign keys: mirror the referenced PK (e.g., `TEACHER_ID` in `CLASSES`).

### Primary Keys
- GUIDs stored as `VARCHAR(36)`.
- No `CAST()` required for GUIDs in SQL — they map directly to C# `string`.

### Non-String DB Columns
Any column that is NOT a string type **must be CAST** in SQL:
```sql
-- CORRECT
SELECT CAST(IS_ACTIVE AS CHAR) AS IS_ACTIVE,
       CAST(IS_DELETED AS CHAR) AS IS_DELETED,
       CAST(CREATED_AT AS CHAR) AS CREATED_AT
FROM STUDENTS
WHERE IS_DELETED <> 1;

-- WRONG — Dapper will throw a mapping error
SELECT IS_ACTIVE, IS_DELETED, CREATED_AT FROM STUDENTS;
```

---

## 2. C# Entity & DTO Rules

- **All fields use `string` type** — including dates, GUIDs, and status flags.
- Dates formatted as `"yyyy-MM-dd HH:mm:ss"` on write; formatted for display in Razor.
- Default `IS_DELETED = "0"` and `IS_ACTIVE = "1"` on entity creation.
- Boolean status comparisons in Razor: `@(entity.IS_ACTIVE == "1" ? "Active" : "Inactive")`

```csharp
// ✅ CORRECT entity definition
public class Student
{
    public string STUDENT_ID { get; set; } = Guid.NewGuid().ToString();
    public string NAME       { get; set; } = default!;
    public string IS_ACTIVE  { get; set; } = "1";
    public string IS_DELETED { get; set; } = "0";
    public string CREATED_AT { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
}
```

---

## 3. Data Access Rules

- All SQL strings live in a dedicated `*Queries.cs` file per feature (e.g., `StudentQueries.cs`).
- **Never** embed raw SQL inside handlers, services, or Razor components.
- Execute queries via **Dapper** with parameterized inputs only.
- For shared/reusable data (dropdowns, class lists, staff lists), always use `ICommonDataService`.

```csharp
// ✅ CORRECT — Dapper with parameterized query
var result = await connection.QueryAsync<StudentDto>(
    StudentQueries.GetAllStudents,
    new { AcademicYearId = yearId, Offset = offset, Limit = limit });

// ❌ WRONG — string concatenation
var sql = $"SELECT * FROM STUDENTS WHERE NAME = '{name}'";
```

---

## 4. Security Standards

- **Password Hashing**: Store as `salt:hash`. Never plain-text or plain-hash. **CRITICAL**: The legacy `Sha256PasswordHasher` is strictly forbidden. All password hashing must use the ASP.NET Core `PasswordHasher<T>` via the `SecurePasswordHasher` implementation.
- **Input Validation & Commands**: Use **FluentValidation** (`AbstractValidator<T>`) for all incoming MediatR Commands (`IRequest`) in the Application layer to standardize data integrity checks. DataAnnotations are secondary.
- **Authorization**: **All** routable Blazor pages must carry an `[Authorize]` attribute. For administrative modules, use `[Authorize(Roles = "Admin")]` or multi-role. Never leave a management page unprotected.
- **Security Headers**: CSP, HSTS, X-Frame-Options must be set via middleware.
- **Injection Prevention**: All DB queries must be parameterized (no string concatenation).

---

## 5. Performance Optimization

- **Cache dropdowns & menus** using `IMemoryCache` — never re-query static lists on every render.
- **Paginate** all large record sets. Use `PaginatedList<T>` (already in `Common/Models`).
- **Response Compression**: Enable Gzip/Brotli for all API/page responses.
- **DB Indexing**: Index every column used in `WHERE`, `JOIN`, or `ORDER BY`.

---

## 6. UI / CSS Standards — Common Classes

> **Rule**: Use the global classes defined in `wwwroot/app.css`.  
> **Never** re-define these in scoped `.razor.css` files.

### 6.1 Page Structure

Every list/management page must follow this structure:

```razor
<div class="management-container animate-fade-in-up">

  <!-- 1. Page Header -->
  <header class="page-header">
    <div class="header-main">
      <h1>Page Title</h1>
      <p>Short description</p>
    </div>
    <button class="btn-primary-modern">Add New</button>
  </header>

  <!-- 2. Toolbar (search + filters + view toggle) -->
  <div class="list-toolbar">
    <div class="list-toolbar-left"> ... </div>
    <div class="list-toolbar-right"> ... </div>
  </div>

  <!-- 3. Content: table-card OR data-grid (see §7) -->

</div>
```

### 6.2 Table Classes (List View)

Use these classes **every time** you render a data table. Do **not** create custom table styles.

| Class | Purpose |
|-------|---------|
| `.table-card` | White card wrapper with border, shadow, rounded corners |
| `.table-responsive` | Horizontal scroll wrapper — always wrap `<table>` in this |
| `.modern-table` | Styled `<table>` with proper header, row hover, spacing |

```razor
<!-- ✅ CORRECT — always use this structure -->
<div class="table-card">
  <div class="table-responsive">
    <table class="modern-table">
      <thead>
        <tr>
          <th>Name</th>
          <th>Status</th>
          <th style="text-align: right;">Actions</th>
        </tr>
      </thead>
      <tbody>
        @foreach (var item in Items)
        {
          <tr>
            <td>@item.Name</td>
            <td><span class="badge-modern badge-success">Active</span></td>
            <td>
              <div class="action-buttons" style="justify-content: flex-end;">
                <button class="btn-icon-modern" title="Edit">...</button>
                <button class="btn-icon-modern danger" title="Delete">...</button>
              </div>
            </td>
          </tr>
        }
      </tbody>
    </table>
  </div>
</div>

<!-- ❌ WRONG — do not do this -->
<table style="width:100%; border-collapse: collapse;">...</table>
```

### 6.3 Badge / Status Classes

```razor
<!-- Status badges -->
<span class="badge-modern badge-success">Active</span>
<span class="badge-modern badge-danger">Inactive</span>
<span class="badge-modern badge-warning">Pending</span>
<span class="badge-modern badge-info">Info</span>

<!-- With animated dot -->
<span class="badge-modern badge-success">
  <span class="status-dot bg-success"></span>
  Active
</span>
```

### 6.4 Action Buttons

```razor
<!-- Icon action buttons (edit / delete) -->
<div class="action-buttons" style="justify-content: flex-end;">
  <button class="btn-icon-modern" title="Edit" @onclick="...">
    <!-- edit SVG -->
  </button>
  <button class="btn-icon-modern danger" title="Delete" @onclick="...">
    <!-- trash SVG -->
  </button>
</div>
```

### 6.5 Avatar

```razor
<!-- Small avatar with initial letter -->
<div class="avatar-modern">@item.NAME[0]</div>
```

### 6.6 Form Classes

```razor
<div class="form-card">
  <h3 class="form-title">Add New Record</h3>
  <div class="row g-4">
    <div class="col-md-6">
      <label class="form-label">Field Name</label>
      <input type="text" class="form-control-modern" />
    </div>
  </div>
  <div class="form-actions">
    <button class="btn-secondary-modern">Cancel</button>
    <button class="btn-primary-modern">Save</button>
  </div>
</div>
```

### 6.7 Empty State

Always use the `.empty-state` class — never use raw `<td colspan="n">` for empty messages.

```razor
<div class="empty-state">
  <svg .../>  <!-- Illustrative icon -->
  <p>No records found matching your criteria.</p>
</div>
```

### 6.8 Pagination

Replace all custom pagination markup with the shared pagination bar:

```razor
@if (List.TotalPages > 1)
{
  <div class="pagination-bar">
    <span class="pagination-info">
      Page @PageNumber of @List.TotalPages (@List.TotalCount total)
    </span>
    <div class="pagination-controls">
      <button class="page-btn-modern"
              disabled="@(!List.HasPreviousPage)"
              @onclick="() => LoadPage(PageNumber - 1)">
        <!-- prev arrow SVG -->
      </button>
      <button class="page-btn-modern"
              disabled="@(!List.HasNextPage)"
              @onclick="() => LoadPage(PageNumber + 1)">
        <!-- next arrow SVG -->
      </button>
    </div>
  </div>
}
```

---

## 7. Grid / List View Toggle Pattern

Every page that shows a data list **must** offer both **Grid (card)** and **List (table)** views.

### 7.1 State Variable

Add this to `@code`:
```csharp
private string ViewMode = "list"; // default: list
```

### 7.2 Toolbar Toggle Buttons

Place inside `.list-toolbar-right`:

```razor
<div class="view-toggle">
  <button class="view-btn @(ViewMode == "grid" ? "active" : "")"
          title="Grid View" @onclick='() => ViewMode = "grid"'>
    <!-- 2x2 grid icon SVG -->
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
         stroke="currentColor" stroke-width="2.2">
      <rect x="3" y="3" width="7" height="7" rx="1"/>
      <rect x="14" y="3" width="7" height="7" rx="1"/>
      <rect x="3" y="14" width="7" height="7" rx="1"/>
      <rect x="14" y="14" width="7" height="7" rx="1"/>
    </svg>
  </button>
  <button class="view-btn @(ViewMode == "list" ? "active" : "")"
          title="List View" @onclick='() => ViewMode = "list"'>
    <!-- lines icon SVG -->
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
         stroke="currentColor" stroke-width="2.2">
      <line x1="3" y1="6" x2="21" y2="6"/>
      <line x1="3" y1="12" x2="21" y2="12"/>
      <line x1="3" y1="18" x2="21" y2="18"/>
    </svg>
  </button>
</div>
```

### 7.3 Conditional Rendering

```razor
@if (!Items.Any())
{
  <div class="empty-state"> ... </div>
}
else if (ViewMode == "list")
{
  <!-- TABLE VIEW using .table-card / .modern-table -->
}
else
{
  <!-- GRID VIEW using .data-grid / .data-card -->
}
```

### 7.4 Grid Card Structure

```razor
<div class="data-grid">
  @foreach (var item in Items)
  {
    <div class="data-card">
      <!-- Avatar + Title -->
      <div class="data-card-header">
        <div class="data-card-avatar">@item.NAME[0]</div>
        <div>
          <div class="data-card-title">@item.NAME</div>
          <div class="data-card-subtitle">@item.CODE</div>
        </div>
      </div>

      <!-- Key-Value Rows -->
      <div class="data-card-body">
        <div class="data-card-row">
          <span class="data-card-label">Department</span>
          <span class="badge-modern badge-info">@item.DEPARTMENT</span>
        </div>
        <div class="data-card-row">
          <span class="data-card-label">Status</span>
          <span class="badge-modern @(item.IS_ACTIVE == "1" ? "badge-success" : "badge-danger")">
            <span class="status-dot @(item.IS_ACTIVE == "1" ? "bg-success" : "bg-danger")"></span>
            @(item.IS_ACTIVE == "1" ? "Active" : "Inactive")
          </span>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="data-card-actions">
        <button class="btn-icon-modern" @onclick="() => Edit(item)">...</button>
        <button class="btn-icon-modern danger" @onclick="() => Delete(item.ID)">...</button>
      </div>
    </div>
  }
</div>
```

### 7.5 Grid / List CSS Quick Reference

| Class | View | Purpose |
|-------|------|---------|
| `.list-toolbar` | Both | Container: search + filters + toggle |
| `.list-toolbar-left` | Both | Search, filters, count |
| `.list-toolbar-right` | Both | View toggle buttons |
| `.view-toggle` | Both | Toggle button group wrapper |
| `.view-btn` / `.view-btn.active` | Both | Individual grid/list button |
| `.result-count` | Both | Record count label |
| `.table-card` | List | White card wrapping the table |
| `.table-responsive` | List | Horizontal scroll wrapper |
| `.modern-table` | List | The `<table>` element styles |
| `.data-grid` | Grid | CSS grid layout for cards |
| `.data-card` | Grid | Individual item card |
| `.data-card-header` | Grid | Avatar + title row |
| `.data-card-avatar` | Grid | Large avatar circle |
| `.data-card-title` | Grid | Primary label |
| `.data-card-subtitle` | Grid | Secondary label / code |
| `.data-card-body` | Grid | Key-value section |
| `.data-card-row` | Grid | Single key-value pair |
| `.data-card-label` | Grid | Muted label |
| `.data-card-value` | Grid | Bold value |
| `.data-card-actions` | Grid | Buttons row at the bottom |
| `.empty-state` | Both | No-results placeholder |
| `.pagination-bar` | Both | Pagination container |
| `.pagination-info` | Both | "Page X of Y" label |
| `.pagination-controls` | Both | Prev/Next button group |
| `.page-btn-modern` | Both | Individual page button |

---

## 8. Mobile-First Responsive Rules

### 8.1 General Principles
- Design for **mobile first** — then expand for desktop via `min-width` media queries.
- The sidebar collapses on mobile (handled globally). No custom sidebar code in modules.
- Never use fixed pixel widths for containers. Use `%`, `fr`, `min-content`, or `flex: 1`.

### 8.2 List Toolbar — Mobile Behaviour

On screens ≤ 600px the toolbar **stacks vertically**. This is handled by global CSS.  
If you add a custom dropdown/filter inside the toolbar, ensure it has a `min-width` and `flex-shrink: 1`:

```css
/* In app.css — already included */
@media (max-width: 600px) {
  .list-toolbar { flex-direction: column; align-items: stretch; gap: 0.75rem; }
  .list-toolbar-left { flex-direction: column; }
  .toolbar-search { max-width: 100%; }
}
```

### 8.3 Table — Mobile Scrolling

**Always** wrap `<table class="modern-table">` inside `.table-responsive`.  
The `.table-responsive` class enables horizontal scrolling on narrow screens automatically.

```razor
<!-- ✅ Mobile-safe table -->
<div class="table-card">
  <div class="table-responsive">   ← required
    <table class="modern-table">
      ...
    </table>
  </div>
</div>

<!-- ❌ Will overflow on mobile -->
<div class="table-card">
  <table class="modern-table">...</table>
</div>
```

### 8.4 Grid View — Auto Responsive Columns

The `.data-grid` uses `auto-fill` — cards automatically reflow:
- **Mobile (< 480px)**: 1 column
- **Tablet (480–768px)**: 2 columns
- **Desktop (> 768px)**: 3–4 columns

No media queries needed when using `.data-grid`. If you need a denser grid (e.g., SupportTables), override inline:
```razor
<div class="data-grid" style="grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));">
```

### 8.5 Forms — Mobile Layout

Use the global grid system. On mobile, all `col-md-*` collapse to full width automatically:

```razor
<div class="row g-4">
  <div class="col-md-6">   ← 50% on desktop, 100% on mobile
    <label class="form-label">Name</label>
    <input type="text" class="form-control-modern" />
  </div>
  <div class="col-md-6">
    ...
  </div>
</div>
```

### 8.6 Page Header — Mobile Stack

On mobile, the page header should stack the title and button vertically:
```css
/* Already in app.css — no custom code needed */
@media (max-width: 768px) {
  .page-header { flex-wrap: wrap; }
  .page-header .btn-primary-modern { width: 100%; }
}
```

### 8.7 Action Buttons in Tables

On mobile, the action column stays right-aligned and buttons use `btn-icon-modern` (34×34px touch targets):
```razor
<div class="action-buttons" style="justify-content: flex-end;">
  <button class="btn-icon-modern" title="Edit">...</button>
  <button class="btn-icon-modern danger" title="Delete">...</button>
</div>
```

> ⚠️ **Never use text-only buttons for row actions** — they break on small screens.  
> Always use `btn-icon-modern` with an SVG icon and a `title` tooltip.

---

## 9. Scoped CSS vs Global CSS Rules

### What goes in `app.css` (global)
- Layout classes: `.table-card`, `.modern-table`, `.table-responsive`
- View toggle: `.view-toggle`, `.view-btn`, `.data-grid`, `.data-card*`
- Forms: `.form-card`, `.form-label`, `.form-control-modern`, `.form-actions`
- Buttons: `.btn-primary-modern`, `.btn-secondary-modern`, `.btn-icon-modern`
- Badges: `.badge-modern`, `.badge-success/danger/warning/info`
- Pagination: `.pagination-bar`, `.page-btn-modern`
- Utilities: `.empty-state`, `.result-count`, `.avatar-modern`
- Animation: `animate-fade-in`, `animate-fade-in-up`

### What goes in `Module.razor.css` (scoped)
- Page wrapper animations: `.students-page`, `.users-page`
- Module-specific add button: `.btn-add` (if different from global)
- Module-specific field labels: `.student-name`, `.year-value`
- Skeleton loaders specific to that module
- Mobile overrides for module-specific layouts

### ❌ Never do this in a scoped CSS file
```css
/* ❌ These are global — remove from scoped files */
.modern-table { ... }
.table-card { ... }
.form-card { ... }
.btn-primary-modern { ... }
.pagination { ... }
```

> **Why?** Blazor scoped CSS adds a `b-xxxxxxxx` attribute selector, which breaks global class inheritance and causes double-definition bugs.

---

## 10. Blazor Component Conventions

### Render Mode
All interactive pages must declare:
```razor
@rendermode @(new InteractiveServerRenderMode(prerender: false))
```

### Page Title
Every routable page must have:
```razor
<PageTitle>Module Name - School ERP</PageTitle>
```

### Authorization
```razor
@attribute [Authorize(Roles = "Admin")]          // Admin only
@attribute [Authorize(Roles = "Admin,Teacher")]  // Multi-role
```

### Loading State
Show skeleton loaders during async data fetch:
```razor
@if (IsLoading)
{
  <div class="skeleton-container">
    <div class="skeleton" style="height: 80px; margin-bottom: 1rem;"></div>
    <div class="skeleton" style="height: 400px;"></div>
  </div>
}
```

### Error Messages
```razor
@if (!string.IsNullOrEmpty(ErrorMessage))
{
  <div style="background: rgba(239,68,68,0.1); color: var(--danger-600);
              border: 1px solid var(--danger-100); padding: 1rem;
              border-radius: 12px; margin-bottom: 1.5rem;">
    ⚠️ @ErrorMessage
  </div>
}
```

### Component Modularity & Lazy Loading
- **Refactor Large Components**: Any Razor component exceeding 500 lines must be broken down into smaller, feature-specific sub-components to improve maintainability and initial rendering speed.
- **Lazy Load Tabs**: Complex management modules with multiple tabs must extract each tab into its own component and render them conditionally using `@if(ActiveTab == "...")`. This natively lazy-loads the data and UI, drastically improving performance.

### State Change After Async Ops
Always call `StateHasChanged()` after modifying UI state in background tasks or event callbacks that don't auto-trigger re-render.

---

---

## 11. Custom Shared Components — SelectPicker

The `SelectPicker` component is a premium replacement for native `<select>` and `<InputSelect>` elements. It supports search, single/multi-select, and dynamic data binding.

### 11.1 Single Select Usage

```razor
<SelectPicker Items="RolesPickerItems" 
              Value="@UserForm.RoleId" 
              ValueChanged="@(v => UserForm.RoleId = v)" 
              ValueField="Value" 
              LabelField="Label" 
              Placeholder="Select Role" 
              Searchable="true" />
```

### 11.2 Multi Select Usage

```razor
<SelectPicker Items="PermissionsPickerItems" 
              IsMultiple="true"
              SelectedValues="@UserForm.SelectedPermissionIds" 
              SelectedValuesChanged="@(v => UserForm.SelectedPermissionIds = v)" 
              ValueField="Value" 
              LabelField="Label" 
              Placeholder="Select Permissions" />
```

### 11.3 State Preparation (@code)

Always transform your DTO lists into a `IEnumerable<object>` containing simple anonymous objects with `Value` and `Label` properties.

```csharp
private IEnumerable<object> RolesPickerItems => 
    RolesList?.Select(r => (object)new { Value = r.ROLE_ID, Label = r.NAME }) ?? Enumerable.Empty<object>();

// Static lists
private static readonly IEnumerable<object> StatusPickerItems = new object[]
{
    new { Value = "Active", Label = "Active" },
    new { Value = "Inactive", Label = "Inactive" }
};
```

### 11.4 Key Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<object>` | `null` | The list of items to display |
| `Value` | `string` | `null` | Bound value for single-select |
| `SelectedValues` | `IEnumerable<string>` | `null` | Bound values for multi-select |
| `IsMultiple` | `bool` | `false` | Enables multi-select mode |
| `Searchable` | `bool` | `true` | Shows search input inside dropdown |
| `Clearable` | `bool` | `true` | Shows clear button when a value is selected |
| `Disabled` | `bool` | `false` | Disables interaction |
| `Placeholder` | `string` | `"Select..."` | Text shown when no value is selected |
| `Size` | `string` | `"md"` | `sm`, `md`, or `lg` |

---

> **Last updated**: 2026-05-13  
> Maintained by: Development Team — update this file whenever a new pattern or convention is established.
