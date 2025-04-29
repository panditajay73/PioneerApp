using System.ComponentModel.DataAnnotations;

namespace Pioneer.Models
{
    public class Freshman
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter your registration number")]
        public string RegistrationNumber { get; set; }
        [Required(ErrorMessage = "Please enter your name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please re-enter your password")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Please enter your age")]
        public int Age { get; set; }
        [Required(ErrorMessage = "Please enter your mobile number")]
        public long MobileNumber { get; set; }
    }
}
