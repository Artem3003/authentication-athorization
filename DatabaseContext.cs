using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityUserRegistration.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityUserRegistration;

public class DatabaseContext : IdentityDbContext<User>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) 
    : base(options)
    {   
    }
}
