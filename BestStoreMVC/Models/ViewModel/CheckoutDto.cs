using System.ComponentModel.DataAnnotations;

namespace BestStoreMVC.Models.ViewModel
{
    public class CheckoutDto
    {
        [Required(ErrorMessage = "The Delivery Address is required.")]
        [MaxLength(200)]
        public string DeliveryAddress { get; set; } = "";

        public string PaymentMethod { get; set; } = "";
    }
}
