using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

[assembly: FunctionsStartup(typeof(UserAPI.Startup))]

namespace UserAPI
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var SqlConnection = "Server=HP-FIDELITY\\SQLEXPRESS;Database=<DATABASE_NAME>;User Id=<USERNAME>;password=<PASSWORD>;Trusted_Connection=False;MultipleActiveResultSets=true;";

            builder.Services.AddDbContext<UserDbContext>(
                options => options.UseSqlServer(SqlConnection));

        }
    }
}
