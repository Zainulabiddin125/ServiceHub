using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Areas.HR.Models;

namespace ServiceHub.Data;

public class ServiceHubContext : IdentityDbContext<ApplicationUser>
{
    public ServiceHubContext(DbContextOptions<ServiceHubContext> options)
        : base(options)
    {
    }
    public DbSet<AttendanceMachine> AttendenceMachines { get; set; }
    public DbSet<AttendanceMachineConnectionLog> AttendenceMachineConnectionLogs { get; set; }
    public DbSet<HRSwapRecord> HR_Swap_Record { get; set; }
    public DbSet<PasswordChangeLog> PasswordChangeLog { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);        

    }
}
