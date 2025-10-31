using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using INSY7315.Models;

namespace INSY7315.Pages.Account
{
    // ==============================================
    // PROFILE MANAGEMENT PAGE MODEL
    // ----------------------------------------------
    // Allows a signed-in user to:
    //  • View their email address (non-editable)
    //  • Update their display name (optional)
    //  • Change their password securely
    // ==============================================
    [Authorize] // Ensures only authenticated users can access this page
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;
        private readonly ILogger<ProfileModel> _logger;

        // Constructor - injects Identity managers and a logger for diagnostics
        public ProfileModel(
            UserManager<ApplicationUser> userMgr,
            SignInManager<ApplicationUser> signInMgr,
            ILogger<ProfileModel> logger)
        {
            _userMgr = userMgr;
            _signInMgr = signInMgr;
            _logger = logger;
        }

        // Logged-in user's email (read-only display only)
        public string? Email { get; set; }

        // ==============================================
        // NESTED INPUT MODELS
        // Separate input models for each form
        // Prevents cross-validation issues between forms
        // ==============================================

        // Input model for "Profile Details" section
        public class ProfileInput
        {
            [StringLength(100)] // Prevent overly long names
            [Display(Name = "Display Name")]
            public string? DisplayName { get; set; }
        }

        // Input model for "Change Password" section
        public class PasswordInput
        {
            [Required, DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string CurrentPassword { get; set; } = "";

            [Required, DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; } = "";

            [Required, DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = "";
        }

        // Bindable properties to capture form data on POST
        [BindProperty] public ProfileInput Profile { get; set; } = new();
        [BindProperty] public PasswordInput Password { get; set; } = new();

        // ==============================================
        // ONGET HANDLER
        // Loads the user's current information into the form
        // ==============================================
        public async Task<IActionResult> OnGet()
        {
            var user = await _userMgr.GetUserAsync(User);
            if (user is null)
                return Challenge(); // Re-authenticate if session expired

            Email = user.Email;
            Profile.DisplayName = user.DisplayName;
            return Page();
        }

        // ==============================================
        // ONPOST: SAVE PROFILE HANDLER
        // Updates the user's Display Name only.
        // ----------------------------------------------
        // - Clears previous model validation state.
        // - Validates only the Profile sub-model.
        // - Updates the current ApplicationUser record.
        // - Refreshes sign-in to reflect changes immediately.
        // ==============================================
        public async Task<IActionResult> OnPostSaveProfileAsync()
        {
            // Clear previous validation and re-validate only Profile form
            ModelState.Clear();
            if (!TryValidateModel(Profile, nameof(Profile)))
            {
                var me = await _userMgr.GetUserAsync(User);
                Email = me?.Email; // repopulate Email field
                return Page();
            }

            var user = await _userMgr.GetUserAsync(User);
            if (user is null)
                return Challenge();

            // Trim input and allow blank DisplayName
            user.DisplayName = string.IsNullOrWhiteSpace(Profile.DisplayName)
                ? null
                : Profile.DisplayName.Trim();

            // Update in database via UserManager
            var result = await _userMgr.UpdateAsync(user);
            if (!result.Succeeded)
            {
                // Log and surface any identity errors
                foreach (var e in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                    _logger.LogWarning("Profile update failed: {Code} {Desc}", e.Code, e.Description);
                }
                Email = user.Email;
                return Page();
            }

            // Refresh login cookie so new name is reflected immediately (e.g. navbar greeting)
            await _signInMgr.RefreshSignInAsync(user);

            TempData["Message"] = "Profile updated.";
            return RedirectToPage(); // Post-Redirect-Get pattern
        }

        // ==============================================
        // ONPOST: CHANGE PASSWORD HANDLER
        // Allows the user to securely change their password.
        // ----------------------------------------------
        // - Validates only the Password sub-model.
        // - Calls UserManager.ChangePasswordAsync for proper hashing.
        // - Refreshes the sign-in to keep user authenticated.
        // ==============================================
        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            // Clear old validation and re-validate password form only
            ModelState.Clear();
            if (!TryValidateModel(Password, nameof(Password)))
            {
                var me = await _userMgr.GetUserAsync(User);
                Email = me?.Email;
                return Page();
            }

            var user = await _userMgr.GetUserAsync(User);
            if (user is null)
                return Challenge();

            // Change password using Identity-provided hashing and checks
            var result = await _userMgr.ChangePasswordAsync(user, Password.CurrentPassword, Password.NewPassword);
            if (!result.Succeeded)
            {
                // Log and show identity errors (wrong current password, weak new password, etc.)
                foreach (var e in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                    _logger.LogWarning("Password change failed: {Code} {Desc}", e.Code, e.Description);
                }
                Email = user.Email;
                return Page();
            }

            // Refresh cookie to ensure user stays logged in with new credentials
            await _signInMgr.RefreshSignInAsync(user);

            TempData["Message"] = "Password changed.";
            return RedirectToPage(); // PRG for clean reload
        }
    }
}
