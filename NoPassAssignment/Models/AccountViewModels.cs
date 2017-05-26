using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoPassAssignment.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Security Code"), MaxLength(5, ErrorMessage = "Must be less than 6 digits."), NotMapped, Required(ErrorMessage = "Enter the security code")]
        public string SecurityCode { get; set; }

    }
}
