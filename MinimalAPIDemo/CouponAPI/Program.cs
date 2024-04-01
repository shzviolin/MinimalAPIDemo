using CouponAPI.Data;
using CouponAPI.Models;
using CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

//app.MapGet("/api/coupon", () => Results.Ok(CouponStore.CouponList)); 
app.MapGet("/api/coupon", (ILogger<Program> _logger) =>
{
    _logger.Log(LogLevel.Information, "Getting all Coupons");
    return Results.Ok(CouponStore.CouponList);
}).WithName("GetCoupons")
  .Produces<IEnumerable<Coupon>>(200);

app.MapGet("/api/coupon/{id:int}", (int id) =>
{
    return Results.Ok(CouponStore.CouponList.FirstOrDefault(x => x.Id == id));
}).WithName("GetCoupon")
  .Produces<Coupon>(200);

app.MapPost("/api/coupon", ([FromBody] CouponCreateDTO couponCreateDTO) =>
{
    if (string.IsNullOrEmpty(couponCreateDTO.Name))
        return Results.BadRequest("Invalid Coupon Name");

    if (CouponStore.CouponList.Any(x => x.Name!.ToLower() == couponCreateDTO.Name.ToLower()))
        return Results.BadRequest("Coupon Name already exists");

    Coupon coupon = new()
    {
        Name = couponCreateDTO.Name,
        Percent = couponCreateDTO.Percent,
        IsActive = couponCreateDTO.IsActive,
    };

    coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;
    CouponStore.CouponList.Add(coupon);

    CouponDTO couponDTO = new()
    {
        Id = coupon.Id,
        Name = coupon.Name,
        Percent = coupon.Percent,
        IsActive = coupon.IsActive,
        Created = coupon.Created,
    };

    //return Results.Ok(coupon);
    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
    return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);

}).WithName("CreateCoupon")
  .Accepts<Coupon>("application/json")
  .Produces<Coupon>(201)
  .Produces(400);



app.UseHttpsRedirection();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
