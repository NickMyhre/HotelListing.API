
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.API.Data.Configurations
{
    //class to configure IdentityRole type
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        //set up configurations for the identity role table/data type
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            //add roles to programs
            builder.HasData(
                new IdentityRole
                {
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR"
                },
                new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "USER"
                }
                );
        }
    }
}
