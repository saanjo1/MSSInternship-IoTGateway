using IoTGatewayAPI;
using IoTGatewayAPI.RabbitMQ;
using IoTGatewayAPI.Services;
using IoTGatewayAPI.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

string connectionString = Environment.GetEnvironmentVariable("AzureTableStorage");
string TableName = Environment.GetEnvironmentVariable("TableName");

 string _hostname = Environment.GetEnvironmentVariable("HostName");
 string _username = Environment.GetEnvironmentVariable("UserName");
 string _password = Environment.GetEnvironmentVariable("Password");
 int _port = Convert.ToInt32(Environment.GetEnvironmentVariable("Port"));
 string _queueName = Environment.GetEnvironmentVariable("QueueName");

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMessageProducer>(x => new Producer(_hostname, _username, _password, _port, _queueName));
builder.Services.AddScoped<IAzureTableRepository<IotDeviceTableEntity>>(x => new AzureTableRepository(connectionString, TableName));
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("corsapp");

app.MapControllers();

app.Run();
