using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MarketLink.Core.Interfaces;
using MarketLink.Core.Entities;
using MarketLink.Core.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    private async Task<(ApplicationUser? user, MarketLink.Core.Entities.Customer? customer)> GetCurrentCustomerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return (null, null);

        var user = await _userManager.FindByIdAsync(userId);
        var customers = await _unitOfWork.Repository<MarketLink.Core.Entities.Customer>()
            .FindAsync(c => c.UserId == userId);
        var customer = customers.FirstOrDefault();

        // If customer record doesn't exist for this user, create it automatically
        if (customer == null && user != null)
        {
            customer = new MarketLink.Core.Entities.Customer
            {
                UserId = user.Id,
                Bio = "Local produce enthusiast"
            };
            await _unitOfWork.Repository<MarketLink.Core.Entities.Customer>().AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
        }

        return (user, customer);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (user, customer) = await GetCurrentCustomerAsync();
        if (user == null || customer == null)
            return RedirectToAction("Login", "Auth", new { area = "" });

        var addresses = await _unitOfWork.Repository<CustomerAddress>()
            .FindAsync(a => a.CustomerId == customer.Id);

        var totalOrders = await _unitOfWork.Repository<Order>()
            .CountAsync(o => o.CustomerId == customer.Id);

        var totalFavorites = await _unitOfWork.Repository<Favorite>()
            .CountAsync(f => f.CustomerId == customer.Id);

        var model = new CustomerProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Bio = customer.Bio,
            DefaultCity = customer.DefaultCity,
            ProfileImageUrl = user.ProfileImageUrl,
            MemberSince = user.CreatedAt,
            TotalOrders = totalOrders,
            TotalFavorites = totalFavorites,
            TotalAddresses = addresses.Count(),
            Addresses = addresses.OrderByDescending(a => a.IsDefault).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CustomerProfileViewModel model)
    {
        var (user, customer) = await GetCurrentCustomerAsync();
        if (user == null || customer == null)
            return RedirectToAction("Login", "Auth", new { area = "" });

        if (!ModelState.IsValid)
        {
            var addresses = await _unitOfWork.Repository<CustomerAddress>()
                .FindAsync(a => a.CustomerId == customer.Id);
            model.Addresses = addresses;
            return View(model);
        }

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.PhoneNumber = model.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            var addresses = await _unitOfWork.Repository<CustomerAddress>()
                .FindAsync(a => a.CustomerId == customer.Id);
            model.Addresses = addresses;
            return View(model);
        }

        customer.Bio = model.Bio?.Trim();
        customer.DefaultCity = model.DefaultCity?.Trim();
        _unitOfWork.Repository<MarketLink.Core.Entities.Customer>().Update(customer);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Profile updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Addresses()
    {
        var (user, customer) = await GetCurrentCustomerAsync();
        if (customer == null)
            return RedirectToAction("Login", "Auth", new { area = "" });

        var addresses = await _unitOfWork.Repository<CustomerAddress>()
            .FindAsync(a => a.CustomerId == customer.Id);

        ViewBag.Customer = customer;
        return View(addresses.OrderByDescending(a => a.IsDefault).ThenBy(a => a.Label).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(CustomerAddressViewModel model)
    {
        var (user, customer) = await GetCurrentCustomerAsync();
        if (customer == null) return RedirectToAction("Login", "Auth", new { area = "" });

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required address fields correctly.";
            return RedirectToAction(nameof(Addresses));
        }

        var addressRepo = _unitOfWork.Repository<CustomerAddress>();
        var existing = await addressRepo.FindAsync(a => a.CustomerId == customer.Id);

        // If this is the first address, or marked as default, handle default flag
        bool makeDefault = model.IsDefault || !existing.Any();
        if (makeDefault)
        {
            foreach (var addr in existing)
            {
                if (addr.IsDefault)
                {
                    addr.IsDefault = false;
                    addressRepo.Update(addr);
                }
            }
        }

        var newAddress = new CustomerAddress
        {
            CustomerId = customer.Id,
            Label = model.Label.Trim(),
            AddressLine1 = model.AddressLine1.Trim(),
            AddressLine2 = model.AddressLine2?.Trim(),
            City = model.City.Trim(),
            State = model.State.Trim(),
            PostalCode = model.PostalCode.Trim(),
            IsDefault = makeDefault
        };

        await addressRepo.AddAsync(newAddress);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Address added successfully!";
        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(CustomerAddressViewModel model)
    {
        var (user, customer) = await GetCurrentCustomerAsync();
        if (customer == null) return RedirectToAction("Login", "Auth", new { area = "" });

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please ensure all required address fields are entered.";
            return RedirectToAction(nameof(Addresses));
        }

        var addressRepo = _unitOfWork.Repository<CustomerAddress>();
        var address = await addressRepo.GetByIdAsync(model.Id);

        if (address == null || address.CustomerId != customer.Id)
        {
            TempData["Error"] = "Address not found.";
            return RedirectToAction(nameof(Addresses));
        }

        if (model.IsDefault && !address.IsDefault)
        {
            var existing = await addressRepo.FindAsync(a => a.CustomerId == customer.Id);
            foreach (var addr in existing)
            {
                if (addr.IsDefault)
                {
                    addr.IsDefault = false;
                    addressRepo.Update(addr);
                }
            }
        }

        address.Label = model.Label.Trim();
        address.AddressLine1 = model.AddressLine1.Trim();
        address.AddressLine2 = model.AddressLine2?.Trim();
        address.City = model.City.Trim();
        address.State = model.State.Trim();
        address.PostalCode = model.PostalCode.Trim();
        address.IsDefault = model.IsDefault;

        addressRepo.Update(address);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Address updated successfully!";
        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        var (user, customer) = await GetCurrentCustomerAsync();
        if (customer == null) return RedirectToAction("Login", "Auth", new { area = "" });

        var addressRepo = _unitOfWork.Repository<CustomerAddress>();
        var addresses = await addressRepo.FindAsync(a => a.CustomerId == customer.Id);

        var target = addresses.FirstOrDefault(a => a.Id == id);
        if (target != null)
        {
            foreach (var addr in addresses)
            {
                addr.IsDefault = (addr.Id == id);
                addressRepo.Update(addr);
            }
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = $"'{target.Label}' set as default address.";
        }

        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var (user, customer) = await GetCurrentCustomerAsync();
        if (customer == null) return RedirectToAction("Login", "Auth", new { area = "" });

        var addressRepo = _unitOfWork.Repository<CustomerAddress>();
        var address = await addressRepo.GetByIdAsync(id);

        if (address != null && address.CustomerId == customer.Id)
        {
            addressRepo.Remove(address);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Address deleted successfully.";
        }

        return RedirectToAction(nameof(Addresses));
    }
}
