
using System.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultString") 
    ?? throw new InvalidOperationException("Connection string 'DefaultString' not found.");

// Add services to the container.
builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(builder.Configuration.GetConnectionString("DefaultString")));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAwardService, AwardService>();
builder.Services.AddScoped<IUserTimeIntervalsService, UserTimeIntervalsService>();
builder.Services.AddTransient<UserSearchService>();
builder.Services.AddMemoryCache();

//builder.Services.AddHostedService<AwardBackgroundService>();

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
}

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
