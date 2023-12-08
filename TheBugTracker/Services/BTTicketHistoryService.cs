using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;
        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddHistoryAsync(Ticket oldticket, Ticket newticket, string userId)
        {

            if (oldticket == null && newticket != null)
            {
                TicketHistory history = new TicketHistory()
                {
                    TicketId = newticket.Id,
                    Property = "",
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = "New Ticket Created"
                };

                try
                {
                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                // Check Ticket Title
                if(oldticket.Title != newticket.Title)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newticket.Id,
                        Property = "Title",
                        OldValue = oldticket.Title,
                        NewValue = newticket.Title,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket title {newticket.Title}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                    
                }
                // Check Ticket Description
                if(oldticket.Description != newticket.Description)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newticket.Id,
                        Property = "Description",
                        OldValue = oldticket.Description,
                        NewValue = newticket.Description,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket description {newticket.Description}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // Check Ticket Priority
                if (oldticket.TicketPriorityId != newticket.TicketPriorityId)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newticket.Id,
                        Property = "TicketPriority",
                        OldValue = oldticket.TicketPriority.Name,
                        NewValue = newticket.TicketPriority.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket ticket priority {newticket.TicketPriority.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // Check Ticket Status
                if (oldticket.TicketStatusId != newticket.TicketStatusId)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newticket.Id,
                        Property = "TicketStatus",
                        OldValue = oldticket.TicketStatus.Name,
                        NewValue = newticket.TicketStatus.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket ticket status {newticket.TicketStatus.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // Check Ticket Type
                if (oldticket.TicketTypeId != newticket.TicketTypeId)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newticket.Id,
                        Property = "TicketType",
                        OldValue = oldticket.TicketType.Name,
                        NewValue = newticket.TicketType.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket ticket type {newticket.TicketType.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // Check Ticket Developer
                if (oldticket.DeveloperUserId != newticket.DeveloperUserId)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newticket.Id,
                        Property = "DeveloperUser",
                        OldValue = oldticket.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newticket.DeveloperUser?.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket ticket developer {newticket.DeveloperUser.FullName}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                try
                {
                    // Save the TicketHistory DataBaseSet to the database
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
                
            }
        }

		public async Task AddHistoryAsync(int ticketId, string model, string userId)
		{
            try
            {
                Ticket ticket = await _context.Tickets.FindAsync(ticketId);
                string description = model.ToLower().Replace("ticket", "");

                description = $"New {description} added to ticket: {ticket.Title}";

                TicketHistory history = new TicketHistory()
                {
                    TicketId = ticketId,
                    Property = model,
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = description
                };

                await _context.TicketHistories.AddAsync(history);
                await _context.SaveChangesAsync();  

            }
            catch (Exception)
            {

                throw;
            }
		}

		public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            List<Project> projects = (await _context.Companies
                                                   .Include(c => c.Projects)
                                                        .ThenInclude(p => p.Tickets)
                                                            .ThenInclude(t => t.History)
                                                                .ThenInclude(h => h.User)
                                                    .FirstOrDefaultAsync(c => c.Id == companyId)).Projects.ToList();

            List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();
            List<TicketHistory> history = tickets.SelectMany(t => t.History).ToList();

            return history;

        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            try
            {
                Project project = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.History)
                                                                .ThenInclude(h => h.User)
                                                          .FirstOrDefaultAsync(p => p.Id == projectId);

                List<TicketHistory> ticketHistory = project.Tickets.SelectMany(t => t.History).ToList();

                return ticketHistory;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
