using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NearByBook.Data;
using NearByBook.DTO;
using NearByBook.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public UsersController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;

    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest("Email already exists.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "User registered successfully",
            user.Id,
            user.Email
        });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return Unauthorized("Invalid email or password.");

        var hashedPassword = HashPassword(dto.Password);

        if (user.PasswordHash != hashedPassword)
            return Unauthorized("Invalid email or password.");

        if (!user.IsActive)
            return BadRequest("User account is inactive.");

        var token = GenerateJwtToken(user);


        return Ok(new
        {
            Message = "Login successful",
            Token = token
        });
    }


    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddBook(CreateBookDto dto)
    {
        // 🔐 Check if user authenticated
        if (!User.Identity.IsAuthenticated)
            return Unauthorized("User is not authenticated.");

        // 🔥 Get UserId from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized("Invalid token.");

        int userId = int.Parse(userIdClaim.Value);

        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            Category = dto.Category,
            Condition = dto.Condition,
            Price = dto.Price,
            Description = dto.Description,
            SellerId = userId, // 🔥 assign from token
            IsApproved = false
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Book submitted successfully. Waiting for admin approval."
        });
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role)
    };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            expires : DateTime.UtcNow.AddMinutes(10), // 👈 10 minutes validity
            claims: claims,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}