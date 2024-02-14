#nullable disable
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PIPMUNI_ARG.Models.Domain;

namespace PIPMUNI_ARG.Data
{
    public partial class PIPMUNI_ARGDbContext : IdentityDbContext
    {
        public PIPMUNI_ARGDbContext(DbContextOptions<PIPMUNI_ARGDbContext>  options) : base(options)
        {
        }


        public DbSet<Extension> Extension { get; set; }
        public DbSet<ExtensionAttachment> ExtensionAttachment { get; set; }
        public DbSet<PaymentAttachment> PaymentAttachment { get; set; }
        public DbSet<ProjectAttachment> ProjectAttachment { get; set; }
        public DbSet<AdditionAttachment> AdditionAttachment { get; set; }
        public DbSet<VarianteAttachment> VarianteAttachment { get; set; }
        public DbSet<Office> Office { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Contractor> Contractor { get; set; }
        public DbSet<PaymentStage> PaymentStage { get; set; }
        public DbSet<ContractStage> ContractStage { get; set; }
        public DbSet<AdditionStage> AdditionStage { get; set; }
        public DbSet<ProjectStage> ProjectStage { get; set; }
        public DbSet<ProjectSource> ProjectSource { get; set; }
        public DbSet<ProjectImage> ProjectImage { get; set; }
        public DbSet<ContractType> ContractType { get; set; }
        public DbSet<VarianteMotivo> VarianteMotivo { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Contract> Contract { get; set; }
        public DbSet<FundingSource> FundingSource { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }
        public DbSet<Province> Province { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Addition> Addition { get; set; }
        public DbSet<Sector> Sector { get; set; }
        public DbSet<Subsector> Subsector { get; set; }
        public DbSet<PaymentType> PaymentType { get; set; }
        public DbSet<FundingType> FundingType { get; set; }
        public DbSet<Variante> Variante { get; set; }
        public DbSet<ProjectVideo> ProjectVideo { get; set; }
    }
}
