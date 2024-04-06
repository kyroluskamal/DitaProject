using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AppConfgDocumentation.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public record AppRegisterRequest(
        [Required] string Email, [Required] string Password, [Required] string FirstName,
        [Required] string LastName, [Required] string Role);
}
