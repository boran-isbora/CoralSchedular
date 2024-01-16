using CoralSchedular.InvoiceServices.Data;
using CoralSchedular.InvoiceServices.Data.Repositories;
using CoralSchedular.InvoiceServices.Mappings;
using CoralSchedular.InvoiceServices.Schedular;
using CoralSchedular.InvoiceServices.Services;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Add Hangfire
builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("SqlConHangfire")));
builder.Services.AddHangfireServer();

//Add DbContex - Catalog: CoralFlightInvoice
builder.Services.AddDbContext<CoralDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConReservationDb")));

//Add Services
builder.Services.AddScoped<IFinanceReport, FinanceReport>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<ReservationMap>();


var app = builder.Build();


//BEGIN - Hangfire Dashboard Configuration
IConfiguration _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

app.UseHangfireDashboard("/job", new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireCustomBasicAuthenticationFilter
        {
            User = _configuration.GetSection("HangfireSettings:UserName").Value,
            Pass = _configuration.GetSection("HangfireSettings:Password").Value
        }
    }
});
//END - Hangfire Dashboard Configuration


//BEGIN - Configure Services for Hangfire
GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });
RecurringJobs.GetDailyFlightInvoiceReport();
//END - Configure Services for Hangfire

app.Run();
