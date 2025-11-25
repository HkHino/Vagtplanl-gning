using System.ComponentModel.DataAnnotations;

namespace Vagtplanlægning.DTOs
{
    public class CreateEmployeeDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public int ExperienceLevel { get; set; }
    }
    namespace Vagtplanlægning.DTOs
    {
        public class EmployeeDto
        {
            public int EmployeeId { get; set; }

            public string FirstName { get; set; } = "";

            public string LastName { get; set; } = "";

            public string Address { get; set; } = "";

            public string Phone { get; set; } = "";

            public string Email { get; set; } = "";

            public int ExperienceLevel { get; set; }
        }

        public class CreateEmployeeDto
        {
            [Required]
            [MaxLength(255)]
            public string FirstName { get; set; } = "";

            [Required]
            [MaxLength(255)]
            public string LastName { get; set; } = "";

            [MaxLength(255)]
            public string Address { get; set; } = "";

            [Required]
            [RegularExpression(@"^([0-9]{8}|\+45[0-9]{8})$",
                ErrorMessage = "Phone must be 8 digits or '+45' followed by 8 digits.")]
            public string Phone { get; set; } = "";

            [Required]
            [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                ErrorMessage = "Email must be in the form name@example.com.")]
            public string Email { get; set; } = "";
        }
    }

}
