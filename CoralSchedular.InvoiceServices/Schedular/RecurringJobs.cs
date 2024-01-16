using Hangfire;

namespace CoralSchedular.InvoiceServices.Schedular
{
    public static class RecurringJobs
    {
        public static void GetDailyFlightInvoiceReport()
        {
            var jobId = "FlightInvoiceChecker";

            //Fix Problem for duplication the same method in Hangfire
            RecurringJob.RemoveIfExists(jobId);

            RecurringJob.AddOrUpdate<IFinanceReport>(jobId, 
                x => x.CheckFlightInvoice(), 
                Cron.Daily(5, 30),                              //run once every day at 05:30
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
                });

        }
    }
}
