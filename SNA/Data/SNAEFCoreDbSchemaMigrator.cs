﻿using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;

namespace SNA.Data;

public class SNAEFCoreDbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public SNAEFCoreDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the SNADbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SNADbContext>()
            .Database
            .MigrateAsync();
    }
}
