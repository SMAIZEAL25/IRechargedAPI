namespace IRechargedAPI.Migrations
{
    [DbContext(typeof(IRechargeAuthDB))]
    [Migration("20250426073948_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            // Existing model-building logic here...
#pragma warning restore 612, 618
        }
    }
}
