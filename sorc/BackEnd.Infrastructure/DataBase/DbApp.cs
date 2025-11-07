using BackEnd.Domin.Entity;
using Microsoft.EntityFrameworkCore;
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


    }
}
