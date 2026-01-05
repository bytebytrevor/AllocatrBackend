using System;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Data;

public class AllocatrDbContext(DbContextOptions<AllocatrDbContext> options)
    : IdentityDbContext<AllocatrUser>(options) {}
