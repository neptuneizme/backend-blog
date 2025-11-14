# Xây Dựng Blog Platform API với ASP.NET Core và Entity Framework

Hướng dẫn này sẽ giúp bạn tạo một RESTful API cho nền tảng blog sử dụng ASP.NET Core 10.0, Entity Framework Core và SQLite.

## Mục Lục

1. [Yêu Cầu Trước Khi Bắt Đầu](#yêu-cầu-trước-khi-bắt-đầu)
2. [Thiết Lập Dự Án](#thiết-lập-dự-án)
3. [Cài Đặt Các Thư Viện](#cài-đặt-các-thư-viện)
4. [Tạo Model Blog](#tạo-model-blog)
5. [Thiết Lập Database Context](#thiết-lập-database-context)
6. [Cấu Hình Ứng Dụng](#cấu-hình-ứng-dụng)
7. [Tạo Blog Controller](#tạo-blog-controller)
8. [Chạy Database Migrations](#chạy-database-migrations)
9. [Kiểm Thử API](#kiểm-thử-api)
10. [Các Bước Tiếp Theo](#các-bước-tiếp-theo)

## Yêu Cầu Trước Khi Bắt Đầu

Trước khi bắt đầu, hãy đảm bảo bạn đã cài đặt:

- .NET 10.0 SDK hoặc phiên bản mới hơn
- Một trình soạn thảo code (Visual Studio Code, Visual Studio, hoặc Rider)
- Kiến thức cơ bản về C# và REST APIs

## Thiết Lập Dự Án

### Bước 1: Tạo Dự Án ASP.NET Core Web API Mới

Mở terminal và chạy lệnh:

```bash
dotnet new webapi -n BlogPlatform
cd BlogPlatform
```

Lệnh này tạo một dự án Web API mới với cấu trúc cơ bản.

### Bước 2: Tạo Solution File (Tùy Chọn Nhưng Được Khuyến Nghị)

Di chuyển về thư mục cha và tạo solution:

```bash
cd ..
dotnet new sln -n backend-blog
dotnet sln add BlogPlatform/BlogPlatform.csproj
```

## Cài Đặt Các Thư Viện

### Bước 3: Thêm Các NuGet Packages Cần Thiết

Di chuyển trở lại thư mục dự án và cài đặt các package Entity Framework Core:

```bash
cd BlogPlatform
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

Các package này cung cấp:

- **Microsoft.EntityFrameworkCore**: Chức năng cốt lõi của EF
- **Microsoft.EntityFrameworkCore.Sqlite**: Provider cho cơ sở dữ liệu SQLite
- **Microsoft.EntityFrameworkCore.Tools**: Công cụ migration và scaffolding

## Tạo Model Blog

### Bước 4: Tạo Thư Mục Models và Class Blog

Tạo thư mục `Models` và thêm file `Blog.cs`:

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

**Giải Thích:**

- `Id`: Khóa chính, tự động tăng
- `Title`: Tiêu đề bài viết blog
- `Content`: Nội dung bài viết blog
- `CreatedAt`: Thời gian tạo blog
- `= null!`: Toán tử null-forgiving cho biết các thuộc tính này sẽ được khởi tạo

## Thiết Lập Database Context

### Bước 5: Tạo Thư Mục Data và DbContext

Tạo thư mục `Data` và thêm file `BlogDbContext.cs`:

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

**Giải Thích:**

- **DbContext**: Lớp cơ sở cho các thao tác cơ sở dữ liệu
- **DbSet<Blog> Blogs**: Đại diện cho bảng Blogs
- **OnModelCreating**: Cấu hình các thuộc tính và ràng buộc của entity
  - Đặt `Id` làm khóa chính
  - Đặt `Title` là bắt buộc với tối đa 200 ký tự
  - Đặt `Content` là bắt buộc
  - Đặt giá trị mặc định cho `CreatedAt` là thời gian hiện tại

## Cấu Hình Ứng Dụng

### Bước 6: Cập Nhật Program.cs

Thay thế nội dung của `Program.cs` bằng:

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Thêm services vào container.
builder.Services.AddOpenApi();

builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlite("Data Source=blog.db"));

builder.Services.AddControllers();

var app = builder.Build();

// Cấu hình HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
```

**Giải Thích:**

- **AddDbContext**: Đăng ký database context với dependency injection
- **UseSqlite**: Cấu hình SQLite làm provider cho cơ sở dữ liệu
- **"Data Source=blog.db"**: Connection string trỏ đến file cơ sở dữ liệu SQLite
- **AddControllers**: Kích hoạt hỗ trợ MVC controller
- **MapControllers**: Ánh xạ các controller endpoints

## Tạo Blog Controller

### Bước 7: Tạo Thư Mục Controllers và BlogController

Tạo thư mục `Controllers` và thêm file `BlogController.cs`:

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

**Giải Thích:**

- **[ApiController]**: Kích hoạt các hành vi đặc biệt cho API (tự động kiểm tra model, v.v.)
- **[Route("api/[controller]")]**: Đặt route cơ sở là `/api/Blog`
- **Primary Constructor**: Tính năng C# 12 cho dependency injection
- **GetBlogs**: Endpoint GET để lấy tất cả các blog
- **CreateBlog**: Endpoint POST để tạo blog mới
- **[FromBody]**: Gắn request body với tham số Blog

## Chạy Database Migrations

### Bước 8: Tạo và Áp Dụng Migration Ban Đầu

Tạo migration cơ sở dữ liệu ban đầu:

```bash
dotnet ef migrations add InitialCreate
```

Áp dụng migration để tạo cơ sở dữ liệu:

```bash
dotnet ef database update
```

**Điều gì xảy ra:**

- Thư mục `Migrations` được tạo với các file migration
- File cơ sở dữ liệu SQLite `blog.db` được tạo
- Bảng `Blogs` được tạo với schema đã định nghĩa

## Kiểm Thử API

### Bước 9: Chạy Ứng Dụng

Khởi động ứng dụng:

```bash
dotnet run
```

API sẽ khởi động, thường là trên `https://localhost:5001` hoặc `http://localhost:5000`.

### Bước 10: Kiểm Thử với HTTP Requests

Bạn có thể kiểm thử API bằng curl, Postman, hoặc tạo file `.http`:

**BlogPlatform.http:**

```http
### Lấy tất cả blogs
GET https://localhost:5001/api/Blog

### Tạo blog mới
POST https://localhost:5001/api/Blog
Content-Type: application/json

{
  "title": "Bài Viết Blog Đầu Tiên",
  "content": "Đây là nội dung của bài viết blog đầu tiên của tôi!",
  "createdAt": "2025-11-14T00:00:00"
}

### Tạo blog khác
POST https://localhost:5001/api/Blog
Content-Type: application/json

{
  "title": "Học ASP.NET Core",
  "content": "Hôm nay tôi đã học cách xây dựng REST API với ASP.NET Core và Entity Framework!",
  "createdAt": "2025-11-14T12:00:00"
}
```

## Tổng Quan Cấu Trúc Dự Án

```
backend-blog/
├── backend-blog.sln
└── BlogPlatform/
    ├── BlogPlatform.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── blog.db (tạo sau khi migration)
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

## Các Bước Tiếp Theo

Bây giờ bạn đã có một blog API cơ bản, hãy xem xét việc thêm:

1. **Các Thao Tác CRUD Bổ Sung:**

   - GET theo Id: Lấy một bài viết blog cụ thể
   - PUT: Cập nhật bài viết blog hiện có
   - DELETE: Xóa bài viết blog

2. **Validation (Kiểm Tra Dữ Liệu):**

   - Thêm data annotations cho validation
   - Triển khai logic validation tùy chỉnh

3. **Xử Lý Lỗi:**

   - Thêm xử lý exception toàn cục
   - Trả về các mã trạng thái HTTP phù hợp

4. **Phân Trang:**

   - Triển khai phân trang cho endpoint GET all blogs

5. **Xác Thực & Phân Quyền:**

   - Thêm xác thực JWT
   - Triển khai blog theo từng người dùng

6. **Các Tính Năng Bổ Sung:**

   - Thêm categories/tags (danh mục/thẻ)
   - Thêm hệ thống bình luận
   - Thêm thông tin tác giả
   - Thêm chức năng tìm kiếm

7. **Tài Liệu API:**

   - Cấu hình Swagger/OpenAPI để tài liệu API tốt hơn

8. **Kiểm Thử:**
   - Thêm unit tests
   - Thêm integration tests

## Tham Khảo Các Lệnh Thường Dùng

```bash
# Khôi phục dependencies
dotnet restore

# Build dự án
dotnet build

# Chạy ứng dụng
dotnet run

# Tạo migration mới
dotnet ef migrations add <TênMigration>

# Áp dụng migrations
dotnet ef database update

# Xóa migration cuối cùng
dotnet ef migrations remove

# Xóa cơ sở dữ liệu
dotnet ef database drop
```

## Xử Lý Sự Cố

**Lỗi: "No DbContext was found"**

- Đảm bảo bạn đã thêm DbContext service trong `Program.cs`
- Đảm bảo class DbContext là public

**Lỗi: "Unable to create migrations"**

- Đảm bảo package `Microsoft.EntityFrameworkCore.Tools` đã được cài đặt
- Thử chạy `dotnet restore`

**Lỗi: "Database locked"**

- Đóng bất kỳ trình duyệt/công cụ cơ sở dữ liệu nào đang truy cập `blog.db`
- Dừng ứng dụng trước khi chạy migrations

## Kết Luận

Bạn đã tạo thành công một blog platform API với:

- ✅ ASP.NET Core 10.0 Web API
- ✅ Entity Framework Core với SQLite
- ✅ RESTful endpoints để tạo và lấy blogs
- ✅ Database migrations
- ✅ Cấu trúc dự án đúng chuẩn

Chúc bạn coding vui vẻ! 🚀
