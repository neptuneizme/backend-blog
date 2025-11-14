# Building a Blog Platform API with ASP.NET Core and Entity Framework

This tutorial will guide you through creating a RESTful API for a blog platform using ASP.NET Core 10.0, Entity Framework Core, and SQLite.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Project Setup](#project-setup)
3. [Installing Dependencies](#installing-dependencies)
4. [Creating the Blog Model](#creating-the-blog-model)
5. [Setting Up the Database Context](#setting-up-the-database-context)
6. [Configuring the Application](#configuring-the-application)
7. [Creating the Blog Controller](#creating-the-blog-controller)
8. [Running Database Migrations](#running-database-migrations)
9. [Testing the API](#testing-the-api)
10. [Next Steps](#next-steps)

## Prerequisites

Before starting, ensure you have the following installed:

- .NET 10.0 SDK or later
- A code editor (Visual Studio Code, Visual Studio, or Rider)
- Basic knowledge of C# and REST APIs

## Project Setup

### Step 1: Create a New ASP.NET Core Web API Project

Open your terminal and run:

```bash
dotnet new webapi -n BlogPlatform
cd BlogPlatform
```

This creates a new Web API project with the basic structure.

### Step 2: Create a Solution File (Optional but Recommended)

Navigate to the parent directory and create a solution:

```bash
cd ..
dotnet new sln -n backend-blog
dotnet sln add BlogPlatform/BlogPlatform.csproj
```

## Installing Dependencies

### Step 3: Add Required NuGet Packages

Navigate back to the project directory and install Entity Framework Core packages:

```bash
cd BlogPlatform
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

These packages provide:

- **Microsoft.EntityFrameworkCore**: Core EF functionality
- **Microsoft.EntityFrameworkCore.Sqlite**: SQLite database provider
- **Microsoft.EntityFrameworkCore.Tools**: Migration and scaffolding tools

## Creating the Blog Model

### Step 4: Create the Models Directory and Blog Class

Create a `Models` folder and add a `Blog.cs` file:

```bash
mkdir Models
```

**Models/Blog.cs:**

```csharp
public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
```

**Explanation:**

- `Id`: Primary key, auto-incremented
- `Title`: Blog post title
- `Content`: Blog post content
- `CreatedAt`: Timestamp when the blog was created
- `= null!`: Null-forgiving operator indicating these properties will be initialized

## Setting Up the Database Context

### Step 5: Create the Data Directory and DbContext

Create a `Data` folder and add `BlogDbContext.cs`:

```bash
mkdir Data
```

**Data/BlogDbContext.cs:**

```csharp
using Microsoft.EntityFrameworkCore;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
        });
    }
}
```

**Explanation:**

- **DbContext**: Base class for database operations
- **DbSet<Blog> Blogs**: Represents the Blogs table
- **OnModelCreating**: Configures entity properties and constraints
  - Sets `Id` as primary key
  - Makes `Title` required with max 200 characters
  - Makes `Content` required
  - Sets `CreatedAt` default to current datetime

## Configuring the Application

### Step 6: Update Program.cs

Replace the contents of `Program.cs` with:

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlite("Data Source=blog.db"));

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
```

**Explanation:**

- **AddDbContext**: Registers the database context with dependency injection
- **UseSqlite**: Configures SQLite as the database provider
- **"Data Source=blog.db"**: Connection string pointing to SQLite database file
- **AddControllers**: Enables MVC controller support
- **MapControllers**: Maps controller endpoints

## Creating the Blog Controller

### Step 7: Create the Controllers Directory and BlogController

Create a `Controllers` folder and add `BlogController.cs`:

```bash
mkdir Controllers
```

**Controllers/BlogController.cs:**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BlogPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogController(BlogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBlogs()
    {
        var blogs = await dbContext.Blogs.ToListAsync();
        return Ok(blogs);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBlog([FromBody] Blog blog)
    {
        dbContext.Blogs.Add(blog);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBlogs), new { id = blog.Id }, blog);
    }
}
```

**Explanation:**

- **[ApiController]**: Enables API-specific behaviors (automatic model validation, etc.)
- **[Route("api/[controller]")]**: Sets base route to `/api/Blog`
- **Primary Constructor**: C# 12 feature for dependency injection
- **GetBlogs**: GET endpoint to retrieve all blogs
- **CreateBlog**: POST endpoint to create a new blog
- **[FromBody]**: Binds request body to the Blog parameter

## Running Database Migrations

### Step 8: Create and Apply Initial Migration

Create the initial database migration:

```bash
dotnet ef migrations add InitialCreate
```

Apply the migration to create the database:

```bash
dotnet ef database update
```

**What happens:**

- A `Migrations` folder is created with migration files
- A `blog.db` SQLite database file is created
- The `Blogs` table is created with the defined schema

## Testing the API

### Step 9: Run the Application

Start the application:

```bash
dotnet run
```

The API will start, typically on `https://localhost:5001` or `http://localhost:5000`.

### Step 10: Test with HTTP Requests

You can test the API using curl, Postman, or create a `.http` file:

**BlogPlatform.http:**

```http
### Get all blogs
GET https://localhost:5001/api/Blog

### Create a new blog
POST https://localhost:5001/api/Blog
Content-Type: application/json

{
  "title": "My First Blog Post",
  "content": "This is the content of my first blog post!",
  "createdAt": "2025-11-14T00:00:00"
}

### Create another blog
POST https://localhost:5001/api/Blog
Content-Type: application/json

{
  "title": "Learning ASP.NET Core",
  "content": "Today I learned how to build a REST API with ASP.NET Core and Entity Framework!",
  "createdAt": "2025-11-14T12:00:00"
}
```

## Project Structure Overview

```
backend-blog/
├── backend-blog.sln
└── BlogPlatform/
    ├── BlogPlatform.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── blog.db (created after migration)
    ├── Models/
    │   └── Blog.cs
    ├── Data/
    │   └── BlogDbContext.cs
    ├── Controllers/
    │   └── BlogController.cs
    ├── Migrations/
    │   ├── 20251114021425_InitialCreate.cs
    │   ├── 20251114021425_InitialCreate.Designer.cs
    │   └── BlogDbContextModelSnapshot.cs
    └── Properties/
        └── launchSettings.json
```

## Next Steps

Now that you have a basic blog API, consider adding:

1. **Additional CRUD Operations:**

   - GET by Id: Retrieve a specific blog post
   - PUT: Update an existing blog post
   - DELETE: Delete a blog post

2. **Validation:**

   - Add data annotations for validation
   - Implement custom validation logic

3. **Error Handling:**

   - Add global exception handling
   - Return appropriate HTTP status codes

4. **Pagination:**

   - Implement pagination for the GET all blogs endpoint

5. **Authentication & Authorization:**

   - Add JWT authentication
   - Implement user-specific blogs

6. **Additional Features:**

   - Add categories/tags
   - Add comments system
   - Add author information
   - Add search functionality

7. **API Documentation:**

   - Configure Swagger/OpenAPI for better API documentation

8. **Testing:**
   - Add unit tests
   - Add integration tests

## Common Commands Reference

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Create a new migration
dotnet ef migrations add <MigrationName>

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Drop the database
dotnet ef database drop
```

## Troubleshooting

**Issue: "No DbContext was found"**

- Make sure you've added the DbContext service in `Program.cs`
- Ensure the DbContext class is public

**Issue: "Unable to create migrations"**

- Ensure `Microsoft.EntityFrameworkCore.Tools` package is installed
- Try running `dotnet restore`

**Issue: "Database locked"**

- Close any database browsers/tools accessing `blog.db`
- Stop the application before running migrations

## Conclusion

You've successfully created a blog platform API with:

- ✅ ASP.NET Core 10.0 Web API
- ✅ Entity Framework Core with SQLite
- ✅ RESTful endpoints for creating and retrieving blogs
- ✅ Database migrations
- ✅ Proper project structure

Happy coding! 🚀
