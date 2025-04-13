﻿using IRecharge_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace IRecharge_API.DAL
{
    public class IRechargeDbContext : DbContext 
    {
        public IRechargeDbContext(DbContextOptions<IRechargeDbContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
    }
}
