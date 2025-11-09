using BackEnd.Domin.Entity;
using BackEnd.Domin.ValueObjects;
using BackEnd.Domin.ValueObjects.ValueObjectsUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.DataBase
{
    public class DbApp: DbContext
    {
        public DbApp(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AuthoRepository> AuthoRepositories { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }

    }
}
