using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Customer
{
    public class CustomerFormViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Customer ID must be greater than 0.")]
        [Display(Name = "Customer ID")]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
