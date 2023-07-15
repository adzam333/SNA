using Microsoft.EntityFrameworkCore;
using SNA.Entities;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace SNA.Data;

public class SNADbContext : AbpDbContext<SNADbContext>
{
    public DbSet<DatasetItem> DatasetItems { get; set; }
    public DbSet<Dataset> Datasets { get; set; }
    public SNADbContext(DbContextOptions<SNADbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own entities here */
        builder.Entity<DatasetItem>(b =>
        {
            b.ToTable("DatasetItems");
            b.ConfigureByConvention();
        });
        builder.Entity<Dataset>(b =>
        {
            b.ToTable("Datasets");
            b.ConfigureByConvention();
            b.HasKey(b => b.Name);


        });
    }
}
