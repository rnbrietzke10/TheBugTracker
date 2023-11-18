using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TheBugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

 
        [DisplayName("Priority Name")]
        public string Name { get; set; }
    }
}
