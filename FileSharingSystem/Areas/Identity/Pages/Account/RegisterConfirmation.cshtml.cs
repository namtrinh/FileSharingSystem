using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FileSharingSystem.Areas.Identity.Pages.Account
{
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; set; } // Define the Email property

        public void OnGet(string email)
        {
            Email = email; // Assign the email parameter to the Email property
        }
    }
}