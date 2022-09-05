using HotelListing.API.Configurations;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.--------------------------------------------------------------------------

//Get connection string from config file (appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");

//Add DbContext with connection string in options
builder.Services.AddDbContext<HotelListingDbContext>(options => {
    options.UseSqlServer(connectionString);
});

//add Authentication services
builder.Services.AddIdentityCore<ApiUser>() // add authentication service with extended identity user that has username, password, phone num etc w/ encryption
    .AddRoles<IdentityRole>()   //Add services to use roles in the program(check authorization)
    .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingApi") //jwt authentication using same name as provider name
    .AddEntityFrameworkStores<HotelListingDbContext>() //Only use HotelListingDbContext data store(can also include database that authenticates users here)
    .AddDefaultTokenProviders(); //allows more functionality to other token providers

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//configure API to allow requests that are not on the same server that the API is running
builder.Services.AddCors(options =>
{
    //Add cors policy with name AllowAll and these restrictions
    options.AddPolicy("AllowAllow", b =>
    b.AllowAnyHeader()
    .AllowAnyOrigin()
    .AllowAnyMethod());
});

//use Serilog in application for logging purposes
//ctx is builder context, lc is logger configuration
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

//register configuration for automapper, allows the injection of automapper anywhere in the program
builder.Services.AddAutoMapper(typeof(MapperConfig));

//register repositories in program, associates abstract delaration with implementation of the abstraction
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //Represents "Bearer" Auth
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;  
}).AddJwtBearer(options =>
{
    //set configuration parameters for bearer token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        //create symmetric key to validate token
        ValidateIssuerSigningKey = true,
        //Validate that token came from this API
        ValidateIssuer = true,
        //Validate entity that has the token
        ValidateAudience = true,
        //check lifetime
        ValidateLifetime = true,
        //Don't provide any leeway for differing times between computers/entities 
        ClockSkew = TimeSpan.Zero,
        //these values come from appsettings.json file--------------------------(should be moved to config file)
        //Declare issuers
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        //Declare Audience
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//use Cors policy with the name AllowAll
app.UseCors("AllowAll");

//include authentication in the program
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
