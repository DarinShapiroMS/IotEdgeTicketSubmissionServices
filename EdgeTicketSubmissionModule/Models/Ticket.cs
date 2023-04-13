using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EdgeTicketSubmissionService.Models
{

    
    public class TicketContext : DbContext
    {
        public TicketContext(DbContextOptions<TicketContext> options)
            : base(options)
        {
        }

        public TicketContext()
        {            

        }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Persist Security Info=False;Database=Tickets;User ID=sa;Password=P@ssw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
           
        }
    }

    public  class Ticket
    {
        public string Id { get; set; }

        public DateTime TransactionDate { get;  }

        public int StoreId { get; set; }   
        
        public string Json { get; set; }

        public string Status { get; set; }
    }
}
