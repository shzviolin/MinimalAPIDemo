using AutoMapper;
using CouponAPI;
using CouponAPI.Data;
using CouponAPI.Models;
using CouponAPI.Models.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();


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

app.MapGet("/api/coupon/{id:int}", (ILogger < Program > _logger,int id) =>
{
    _logger.Log(LogLevel.Information, "Getting a Coupon");
    return Results.Ok(CouponStore.CouponList.FirstOrDefault(x => x.Id == id));
}).WithName("GetCoupon")
  .Produces<Coupon>(200);

app.MapPost("/api/coupon", async (IMapper _mapper, IValidator<CouponCreateDTO> _validation, [FromBody] CouponCreateDTO couponCreateDTO) =>
{
    var validationResult = await _validation.ValidateAsync(couponCreateDTO);

    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors.FirstOrDefault()!.ToString());

    if (CouponStore.CouponList.Any(x => x.Name!.ToLower() == couponCreateDTO.Name!.ToLower()))
        return Results.BadRequest("Coupon Name already exists");

    Coupon coupon = _mapper.Map<Coupon>(couponCreateDTO);

    coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;
    CouponStore.CouponList.Add(coupon);

    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

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
