using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TheBugTracker.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }

        [DisplayName("Status Name")]
        public string Name { get; set; }
    }
}
