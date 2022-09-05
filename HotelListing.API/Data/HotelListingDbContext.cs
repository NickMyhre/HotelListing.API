using HotelListing.API.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Data
{
    //Inherit from IdenityDbContext instead of DbContext so that the context knows that it needs to authenticate users
    //Api user is the user type that is configured to interact with the database
    public class HotelListingDbContext : IdentityDbContext<ApiUser>
    {
        public HotelListingDbContext(DbContextOptions options) : base(options)
        {

        }

        //let dbcontext know about db tables
        //create a list for the database of type hotel
        public DbSet<Hotel> Hotels { get; set; }

        //create a list for the database of type country
        public DbSet<Country> Countries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base is relative to DbContext
            base.OnModelCreating(modelBuilder);

            //add data to the DB when the DbContext model is created
            //adding countries first because hotels are dependent upon countries
            //the implementation for the configurations are located in the Configuration folder
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new CountryConfiguration());
            modelBuilder.ApplyConfiguration(new HotelConfiguration());

        }
    }
}
