using System.ComponentModel.DataAnnotations;

namespace Pioneer.Models
{
    public class PioneerLogin
    {
        [Required(ErrorMessage = "Please enter your registration number")]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
