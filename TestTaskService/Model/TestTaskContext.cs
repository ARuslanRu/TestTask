using System.Data.Entity;

namespace TestTaskService.Model
{
    class TestTaskContext : DbContext
    {
        public TestTaskContext()
            : base("DbConnection")
        { }

        public DbSet<Currency> Currencies { get; set; }
        public DbSet<CurrencyRate> CurrencyRates { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrencyRate>().Property(p => p.Сourse)
        .HasPrecision(18, 4);
        }      
    }
}
