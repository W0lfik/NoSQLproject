using MongoDB.Driver;
using NoSQLproject.Models;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Services

{
    public class TicketArchiveService : ITicketArchiveService
    {
        public int ArchiveOldTickets()
        {
            var client = new MongoClient("mongodb+srv://volkovpayeer_db_user:t9lnWteMrpmFMqHV@nosqlproject.lvah5jv.mongodb.net/");
            var database = client.GetDatabase("noSQL_project");

            var ticketsCollection = database.GetCollection<Ticket>("ticket");
            var archiveCollection = database.GetCollection<Ticket>("tickets_archive");

            //  1-year cutoff
            var cutoffDate = DateTime.UtcNow.AddYears(-1);

            // Step 1: find all tickets older than 1 year
            var filter = Builders<Ticket>.Filter.Lt(t => t.CreatedAt, cutoffDate);
            var oldTickets = ticketsCollection.Find(filter).ToList();

            if (!oldTickets.Any())
                return 0;

            // Step 2: move them to archive collection
            archiveCollection.InsertMany(oldTickets);

            // Step 3: delete them from the main collection
            var ids = oldTickets.Select(t => t.Id).ToList();
            var deleteFilter = Builders<Ticket>.Filter.In(t => t.Id, ids);
            ticketsCollection.DeleteMany(deleteFilter);

            return oldTickets.Count;
        }
    }
}
