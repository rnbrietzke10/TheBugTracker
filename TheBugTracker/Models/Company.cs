using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TheBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; }

        [DisplayName("Company Name")]
        public string Name { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }


        // Navigation Properties -- Creates Relatioship in DB

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        // Create relationship to invites
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();


    }
}
