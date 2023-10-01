﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;



namespace BankSystemAPI
{
    public class AppDbContext:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Data Source=(local);Initial Catalog=EFCoreBankSystem1; Integrated Security=true; TrustServerCertificate=True");
        }

    }
}