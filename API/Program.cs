
   var builder = WebApplication.CreateBuilder(args);

    // add services to the container.

        // our own application services
        builder.Services.AddApplicationServices(builder.Configuration);

        // default services 
        builder.Services.AddControllers();
        builder.Services.AddCors();
            
        // our own identity services
        builder.Services.AddIdentityServices(builder.Configuration);

        // default Swagger config
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "DatingAPI", Version = "v1" });
        });


    // Database connection string
     var connStr ="";

    // Depending on if in development or production, use either-provided
    // connection string, or development connection string from env var.
    if (builder.Environment.IsDevelopment())
    {
        // Use connection string from file.
        connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    }
    else
    {
        // Use connection string provided at runtime by Deployment Service.
        var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        // Parse connection URL to connection string for Npgsql
        connUrl = connUrl.Replace("postgres://", string.Empty);
        var pgUserPass = connUrl.Split("@")[0];
        var pgHostPortDb = connUrl.Split("@")[1];
        var pgHostPort = pgHostPortDb.Split("/")[0];
        var pgDb = pgHostPortDb.Split("/")[1];
        var pgUser = pgUserPass.Split(":")[0];
        var pgPass = pgUserPass.Split(":")[1];
        var pgHost = pgHostPort.Split(":")[0];
        var pgPort = pgHostPort.Split(":")[1];

        connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
    } 
    
    // Whether the connection string came from the local development configuration file
    // or from the environment variable from Heroku, use it to set up your DbContext.
    builder.Services.AddDbContext<DataContext>(opt => {
        opt.UseNpgsql(connStr);
    });

    // configure the HTTP request pipeline.

    var app = builder.Build();
    var env = app.Environment;


        app.UseMiddleware<ExceptionMiddleware>();
            
        if (env.IsDevelopment())
        {
            // app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
        }

        app.UseHttpsRedirection();

        //obsolete for net6 - app.UseRouting();

        app.UseCors(x => x.AllowCredentials().AllowAnyHeader().AllowAnyMethod()
            .WithOrigins("http://localhost:4200")
            .WithOrigins("https://localhost:4200"));

        app.UseAuthentication();
        app.UseAuthorization();

        // to host client angular app inside the API server.
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapControllers();
        app.MapHub<PresenceHub>("hubs/presence");
        app.MapHub<MessageHub>("hubs/message");
        app.MapFallbackToController("Index", "FallBack");

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try 
            {
                var context = services.GetRequiredService<DataContext>();
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
                await context.Database.MigrateAsync();
                //await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]");
                //await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
                await Seed.ClearConnections(context);
                await Seed.SeedUsers(userManager, roleManager);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured during seed or migration");
            }

            await app.RunAsync();
