using Enima_AuthJwt;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

 

// Add services to the container.
{
    var Services = builder.Services;
    var env = builder.Environment;



    Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    Services.AddEndpointsApiExplorer();

  //  Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName: "Enima_dbJwt"), ServiceLifetime.Singleton);
    Services.AddDbContext<AppDbContext>(op =>
          op.UseSqlServer
          (builder.Configuration.GetConnectionString("DefaultConnection"),
                  b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), ServiceLifetime.Scoped);


    Services.AddAuthentication(x =>
   {
       x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
       x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
   })
   .AddJwtBearer(x =>
   {
       x.RequireHttpsMetadata = false;
       x.SaveToken = true;
       x.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuerSigningKey = true,
           IssuerSigningKey = new SymmetricSecurityKey(JwtSetting.key),
           ValidateIssuer = false,
           ValidateAudience = false,
           ValidateLifetime = true,
           ClockSkew = TimeSpan.Zero,
       };
   });


    Services.AddSwaggerGen(option =>
   {
       option.SwaggerDoc("v1", new OpenApiInfo { Title = "Enima AuthJwt API", Version = "v1" });
       option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
       {
           In = ParameterLocation.Header,
           Description = "Please enter a valid token",
           Name = "Authorization",
           Type = SecuritySchemeType.Http,
           BearerFormat = "JWT",
           Scheme = "Bearer"
       });
       option.AddSecurityRequirement(new OpenApiSecurityRequirement
       {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
       });
   });
   //  Services.AddSingleton<AppDbContext>();
    Services.AddScoped<IJWTAuthManager, JWTAuthManager>();
 
}

var app = builder.Build();

// Configure the HTTP request pipeline.
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}


 


 
