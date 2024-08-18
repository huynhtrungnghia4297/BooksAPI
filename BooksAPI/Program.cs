using Microsoft.EntityFrameworkCore;
using BooksAPI.Data;
using Microsoft.OpenApi.Models;

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Books API", Version = "v1" });

            // Cấu hình thêm header tùy chỉnh vào Swagger
            c.AddSecurityDefinition("xAuth", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "xAuth",
                Type = SecuritySchemeType.ApiKey,
                Description = "Please enter your xAuth header value",
                    
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "xAuth"
                }
            },
            new string[] {}
        }
            });
        });





        // Add services to the container.
        builder.Services.AddDbContext<ApiContext>(opt => opt.UseInMemoryDatabase("BookDb"));


        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();






        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();