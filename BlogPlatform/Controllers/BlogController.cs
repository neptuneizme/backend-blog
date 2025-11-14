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