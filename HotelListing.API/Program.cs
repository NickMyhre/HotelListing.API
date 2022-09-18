using HotelListing.API.Core.Configurations;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Middleware;
using HotelListing.API.Core.Repository;
using HotelListing.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
    .AddDefaultTokenProviders(); //allows more functionality to use other token providers


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//configire swagger for jwt auth
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Listing API", Version ="v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        //add descriptions and info for clients to swagger
        //description is directions to put token into swagger to get authenticated for endpoints

        Description = @"JWT Authorization header using the Bearer scheme.
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example 'Bearer 1234asdff'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    //initialize swagger security and enforce
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "0auth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

//configure API to allow requests that are not on the same server that the API is running
builder.Services.AddCors(options =>
{
    //Add cors policy with name AllowAll and these restrictions
    options.AddPolicy("AllowAll", n =>
    n.AllowAnyHeader()
    .AllowAnyOrigin()
    .AllowAnyMethod());
});

//add support for api versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); //( major, minor) e.g. 1.0
    options.ReportApiVersions = true;
    //specify template for clients to specify api version
    options.ApiVersionReader = ApiVersionReader.Combine(
        //format for querying a specific api, e.g. https://localhost:7048/weatherforecast?api-version=1
        new QueryStringApiVersionReader("api-version"),
        //allows you to specify desired api version in the header, e.g key = "X-version" value = 1
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
        );
});

builder.Services.AddVersionedApiExplorer(
    options =>
    {
        //specify versioning format
        options.GroupNameFormat = "'v'VVV";
        //include version in url
        options.SubstituteApiVersionInUrl = true;
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

//add caching to services
builder.Services.AddResponseCaching(options =>
{
    //max 1MB cached data
    options.MaximumBodySize = 1024;
    //urls with different capitilizations will be treated differently
    options.UseCaseSensitivePaths = true;
});
//add controllers in in defualt web api project, addodata allows the extra functionality
builder.Services.AddControllers().AddOData(options =>
{
    //allow select queries, filtering, ordering through OData package
    options.Select().Filter().OrderBy();
});
var app = builder.Build();

// Configure the HTTP request pipeline. these are all middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //hide schemas from user, (only used in Dev Environment atm currently but could be useful for other scenarios)
    app.UseSwaggerUI(options =>
        options.DefaultModelsExpandDepth(-1));
}

//use custom middleware into the request pipeline
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

//use Cors policy with the name AllowAll
app.UseCors("AllowAll");

app.UseResponseCaching();

//middleware directly in program.cs file, this middleware is used for caching
app.Use(async (context, next) =>
{
    //when cache control is added, add new headers related to cache control
    context.Response.GetTypedHeaders().CacheControl =
    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
    {
        //cache is public
        Public = true,
        //only cache for 10 seconds
        MaxAge = TimeSpan.FromSeconds(10)
    };
    // cache response datatype may vary
    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
    new string[] { "Accept-Encoding" };

    //execute next command
    await next();
});
//include authentication in the program for JWT token use
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
