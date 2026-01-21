using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Add this for async EF methods
using xxxxx.Data;
using xxxxx.Models;

namespace xxxxx.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ApiController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // --- AUTHENTICATION ---

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Password == dto.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // For simplicity in this demo, we use the User ID as the "Token".
            // In a real production app, use JWT (JSON Web Tokens).
            return Ok(new { user, token = user.Id.ToString() });
        }

        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already exists" });

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password, // Remember to hash this in production!
                Bio = "New member",
                JoinedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user, token = user.Id.ToString() });
        }

        // --- PROFILE MANAGEMENT ---

        [HttpGet("user/profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await GetUserFromToken();
            if (user == null) return Unauthorized();
            return Ok(user);
        }

        [HttpPut("user/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var user = await GetUserFromToken();
            if (user == null) return Unauthorized();

            // Update fields if they are provided
            if (!string.IsNullOrEmpty(dto.Name)) user.Name = dto.Name;
            if (dto.Bio != null) user.Bio = dto.Bio;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
            if (dto.CoverUrl != null) user.CoverUrl = dto.CoverUrl;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("user/upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var user = await GetUserFromToken();
            if (user == null) return Unauthorized();

            // Robustly determine web root (use WebRootPath if set; otherwise fallback to ContentRootPath/wwwroot)
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the URL to the frontend
            var baseUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            return Ok(baseUrl);
        }

        // Helper to get user ID from the "Authorization" header
        private async Task<User?> GetUserFromToken()
        {
            // Extract "Bearer <token>"
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            var tokenId = authHeader.Replace("Bearer ", "");

            // Token was issued as user.Id.ToString() (Guid). Parse and lookup by Guid.
            if (!Guid.TryParse(tokenId, out var userGuid))
                return null;

            return await _context.Users.FindAsync(userGuid);
        }
    }

    // Add this DTO if not already defined
    public class UpdateProfileDto
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
    }
}