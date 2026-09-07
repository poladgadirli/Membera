using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Merchants.CreateMerchant;
using Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;
using Membera.Merchant.Application.Merchants.UpdateMerchant;
using Membera.Merchant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext qeydiyyatı
builder.Services.AddDbContext<MerchantDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MerchantDb")));

// Repository
builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();

// Handler-lər
builder.Services.AddScoped<CreateMerchantHandler>();
builder.Services.AddScoped<GetMerchantByOwnerIdHandler>();
builder.Services.AddScoped<UpdateMerchantHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();