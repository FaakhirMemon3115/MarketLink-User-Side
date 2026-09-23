using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MarketLink.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace MarketLink.Web.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user);
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                if (roles.Contains("Farmer"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Farmer" });
                
                return RedirectToAction("Index", "Dashboard", new { area = "Customer" });
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string role = "Customer")
    {
        ViewData["Role"] = role;
        return View(new RegisterViewModel { Role = role });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var role = model.Role == "Farmer" ? "Farmer" : "Customer";
                await _userManager.AddToRoleAsync(user, role);
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                if (role == "Farmer")
                    return RedirectToAction("Index", "Dashboard", new { area = "Farmer" });
                return RedirectToAction("Index", "Dashboard", new { area = "Customer" });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
    [DataType(DataType.Password), Compare("Password")]
    public string ConfirmPassword { get; set; }
    public string Role { get; set; }
}
