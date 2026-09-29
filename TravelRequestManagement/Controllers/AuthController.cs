
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelManagement.Data.Entities;

namespace TravelRequestManagement.Controllers;

[Route("auth")]
public sealed class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> signIn;
    private readonly UserManager<ApplicationUser> users;

    public AuthController(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser> users)
    {
        this.signIn = signIn;
        this.users = users;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromForm] string? email,
        [FromForm] string? password,
        [FromForm] string? returnUrl)
    {
        var destination = SafeReturnUrl(returnUrl);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return Redirect(
                $"/login?loginError=1&returnUrl={Uri.EscapeDataString(destination)}");
        }

        var result = await signIn.PasswordSignInAsync(
            email.Trim(),
            password,
            isPersistent: true,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return LocalRedirect(destination);
        }

        var error = result.IsLockedOut
            ? "locked"
            : "1";

        return Redirect(
            $"/login?loginError={error}&returnUrl={Uri.EscapeDataString(destination)}");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromForm] string? returnUrl)
    {
        await signIn.SignOutAsync();

        return LocalRedirect(
            SafeReturnUrl(returnUrl, "/login"));
    }

    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromForm] string? currentPassword,
        [FromForm] string? newPassword,
        [FromForm] string? confirmNewPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) ||
            string.IsNullOrWhiteSpace(newPassword))
        {
            return Redirect(
                "/profile/change-password?error=Enter your current password and a new password.");
        }

        if (!string.Equals(
            newPassword,
            confirmNewPassword,
            StringComparison.Ordinal))
        {
            return Redirect(
                "/profile/change-password?error=The new password and confirmation do not match.");
        }

        var user = await users.GetUserAsync(User);

        if (user is null)
        {
            return Redirect("/login");
        }

        var result = await users.ChangePasswordAsync(
            user,
            currentPassword,
            newPassword);

        if (!result.Succeeded)
        {
            var error = string.Join(
                " ",
                result.Errors.Select(x => x.Description));

            return Redirect(
                $"/profile/change-password?error={Uri.EscapeDataString(error)}");
        }

        await signIn.RefreshSignInAsync(user);

        return Redirect("/profile?passwordChanged=1");
    }

    private bool IsLocalRequest(string? value)
    {
        return Url.IsLocalUrl(value);
    }

    private string SafeReturnUrl(
        string? returnUrl,
        string fallback = "/")
    {
        return IsLocalRequest(returnUrl)
            ? returnUrl!
            : fallback;
    }
}


