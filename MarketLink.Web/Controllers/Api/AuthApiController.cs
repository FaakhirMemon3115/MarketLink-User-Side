using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MarketLink.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MarketLink.Web.Controllers.Api;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthApiController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthApiController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] ApiLoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid credentials format.", errors = ModelState });

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new { success = false, message = "Invalid email or account is inactive." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new { success = false, message = "Invalid password." });

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            success = true,
            message = "Authentication successful.",
            user = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Roles = roles
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] ApiRegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Validation failed.", errors = ModelState });

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return Conflict(new { success = false, message = "An account with this email already exists." });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { success = false, errors = result.Errors.Select(e => e.Description) });

        var role = request.Role?.ToLower() == "farmer" ? "Farmer" : "Customer";
        await _userManager.AddToRoleAsync(user, role);

        return StatusCode(201, new
        {
            success = true,
            message = "Account registered successfully.",
            userId = user.Id,
            role
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "Not authenticated." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { success = false, message = "User not found." });

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            success = true,
            user = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.CreatedAt,
                Roles = roles
            }
        });
    }
}

public class ApiLoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ApiRegisterRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public string? Role { get; set; } = "Customer";
}
