﻿using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure
{
    public class KaleBotContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
            .UseMySql("server=localhost;user=root;database=kalebot;port=3306;Connect Timeout=5;", new MySqlServerVersion(new Version(8, 0, 23)));

    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }

    public class Rank
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }
    public class AutoRole
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }
}
