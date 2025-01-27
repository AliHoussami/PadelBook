using System.ComponentModel.DataAnnotations;

namespace Padel_Court_Booking.Models
{
    public class Court
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; }

        public decimal PricePerHour { get; set; }     
    }    
}
