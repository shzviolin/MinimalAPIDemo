using CouponAPI.Data;
using CouponAPI.Models;
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
app.MapGet("/api/coupon", () =>
{
    return Results.Ok(CouponStore.CouponList);
}).WithName("GetCoupons")
  .Produces<IEnumerable<Coupon>>(200);

app.MapGet("/api/coupon/{id:int}", (int id) =>
{
    return Results.Ok(CouponStore.CouponList.FirstOrDefault(x => x.Id == id));
}).WithName("GetCoupon")
  .Produces<Coupon>(200);

app.MapPost("/api/coupon", ([FromBody] Coupon coupon) =>
{
    if (coupon.Id != 0 || string.IsNullOrEmpty(coupon.Name))
        return Results.BadRequest("Invalid Id or Coupon Name");

    if (CouponStore.CouponList.Any(x => x.Name.ToLower() == coupon.Name.ToLower()))
        return Results.BadRequest("Coupon Name already exists");

    coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
    CouponStore.CouponList.Add(coupon);

    //return Results.Ok(coupon);
    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
    return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, coupon);

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
