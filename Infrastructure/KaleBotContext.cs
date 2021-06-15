using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure
{
    public class KaleBotContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
            .UseMySql("server=localhost;user=root;database=kalebot;port=3306;Connect Timeout=5;", new MySqlServerVersion(new Version(8, 0, 23)));

    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }
}
