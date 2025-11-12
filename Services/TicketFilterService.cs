using NoSQLproject.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoSQLproject.Services
{
    public class TicketFilterService
    {
        public IEnumerable<Ticket> ApplySearchAndSort(IEnumerable<Ticket> tickets, string searchQuery, string sortOrder)
        {
            // Search logic
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string query = searchQuery.ToLower();

                var terms = query.Split(new[] { " and ", " or " }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim())
                                 .ToList();

                if (query.Contains(" and "))
                {
                    tickets = tickets.Where(t =>
                        terms.All(term =>
                            (t.Title?.ToLower().Contains(term) ?? false) ||
                            (t.Description?.ToLower().Contains(term) ?? false)
                        ));
                }
                else if (query.Contains(" or "))
                {
                    tickets = tickets.Where(t =>
                        terms.Any(term =>
                            (t.Title?.ToLower().Contains(term) ?? false) ||
                            (t.Description?.ToLower().Contains(term) ?? false)
                        ));
                }
                else
                {
                    tickets = tickets.Where(t =>
                        (t.Title?.ToLower().Contains(query) ?? false) ||
                        (t.Description?.ToLower().Contains(query) ?? false)
                    );
                }
            }

            // Sorting logic
            tickets = sortOrder switch
            {
                "id_desc" => tickets.OrderByDescending(t => t.TicketNumber),
                "id_asc" => tickets.OrderBy(t => t.TicketNumber),
                "user_desc" => tickets.OrderByDescending(t => t.CreatedBy?.FullName),
                "user_asc" => tickets.OrderBy(t => t.CreatedBy?.FullName),
                "date_desc" => tickets.OrderByDescending(t => t.CreatedAt),
                "date_asc" => tickets.OrderBy(t => t.CreatedAt),
                "deadline_desc" => tickets.OrderByDescending(t => t.Deadline),
                "deadline_asc" => tickets.OrderBy(t => t.Deadline),
                _ => tickets.OrderByDescending(t => t.CreatedAt) // most recent first as default
            };

            return tickets;
        }
    }
}
