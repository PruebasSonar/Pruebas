#nullable disable
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PIPMUNI_ARG.Areas.Review.Models;
using PIPMUNI_ARG.Models.Domain;
using PIPMUNI_ARG.Models.Reports;

namespace PIPMUNI_ARG.Data
{
	public partial class PIPMUNI_ARGDbContext : IdentityDbContext
	{

		public DbSet<DashboardInfo> DashboardContractInfo { get; set; }
		public DbSet<ItemQty> SectorsQty { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder
				.Entity<DashboardInfo>(
					eb =>
					{
						eb.HasNoKey();
						eb.ToView("dashboard_info");
						eb.Property(v => v.CancelledContractsCount).HasColumnName("CancelledContractsCount");
					});
			modelBuilder
				.Entity<ItemQty>(
					eb =>
					{
                        eb.HasNoKey();
                        eb.ToView("dashboard_ContractsPerSector");
                    });
			base.OnModelCreating(modelBuilder);
		}

	}
}
