using AutoMapper;
using CouponAPI;
using CouponAPI.Data;
using CouponAPI.Models;
using CouponAPI.Models.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
    APIResponse response = new();
    _logger.Log(LogLevel.Information, "Getting all Coupons");
    response.Result = CouponStore.CouponList;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;

    return Results.Ok(response);
}).WithName("GetCoupons")
  .Produces<APIResponse>(200);


app.MapGet("/api/coupon/{id:int}", (ILogger<Program> _logger, int id) =>
{
    APIResponse response = new();
    _logger.Log(LogLevel.Information, "Getting a Coupon");
    var coupon = CouponStore.CouponList.FirstOrDefault(x => x.Id == id);
    response.Result = coupon ?? null;
    response.IsSuccess = coupon != null ? true : false;
    response.StatusCode = coupon != null ? HttpStatusCode.OK : HttpStatusCode.NotFound;
    if (coupon != null)
        response.ErrorMessages!.Add("Coupon founded");
    else
        response.ErrorMessages!.Add("Coupon not founded");

    return Results.Ok(response);
}).WithName("GetCoupon")
  .Produces<APIResponse>(200);


app.MapPost("/api/coupon", async (IMapper _mapper, IValidator<CouponCreateDTO> _validation, [FromBody] CouponCreateDTO couponCreateDTO) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    var validationResult = await _validation.ValidateAsync(couponCreateDTO);

    if (!validationResult.IsValid)
    {
        response.ErrorMessages!.Add(validationResult.Errors.FirstOrDefault()!.ToString());
        return Results.BadRequest(response);
    }

    if (CouponStore.CouponList.Any(x => x.Name!.ToLower() == couponCreateDTO.Name!.ToLower()))
    {
        response.ErrorMessages!.Add("Coupon Name already exists");
        return Results.BadRequest(response);
    }

    Coupon coupon = _mapper.Map<Coupon>(couponCreateDTO);

    coupon.Id = CouponStore.CouponList.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;
    CouponStore.CouponList.Add(coupon);

    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

    response.Result = couponDTO;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.Created;

    return Results.Ok(response);
    //return Results.Ok(coupon);
    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
    //return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);

}).WithName("CreateCoupon")
  .Accepts<APIResponse>("application/json")
  .Produces<APIResponse>(201)
  .Produces(400);


app.MapPut("/api/coupon", async (IMapper _mapper, IValidator<CouponUpdateDTO> _validation, [FromBody] CouponUpdateDTO couponUpdateDTO) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    var validationResult = await _validation.ValidateAsync(couponUpdateDTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages!.Add(validationResult.Errors.FirstOrDefault()!.ToString());
        return Results.BadRequest(response);
    }

    if (CouponStore.CouponList.Any(x => x.Name!.ToLower() == couponUpdateDTO.Name!.ToLower()))
    {
        response.ErrorMessages!.Add("Coupon Name already exists");
        return Results.BadRequest(response);
    }

    Coupon couponFromStore = CouponStore.CouponList.FirstOrDefault(x => x.Id == couponUpdateDTO.Id);
    if (couponFromStore != null)
    {
        couponFromStore.IsActive = couponUpdateDTO.IsActive;
        couponFromStore.Name = couponUpdateDTO?.Name;
        couponFromStore.Percent = couponUpdateDTO.Percent;
        couponFromStore.LastUpdated = DateTime.Now;

        response.Result = _mapper.Map<Coupon>(couponUpdateDTO);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        response.ErrorMessages.Add("Coupon updated successfully");

        return Results.Ok(response);
    }
    else
    {
        response.Result = null;
        response.IsSuccess = false;
        response.StatusCode = HttpStatusCode.NotFound;
        response.ErrorMessages.Add("Coupon not found");
        return Results.NotFound(response);
    }
}).WithName("UpdateCoupon")
  .Accepts<CouponUpdateDTO>("application/json")
  .Produces<APIResponse>(200)
  .Produces<APIResponse>(400);



app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.NotFound };

    Coupon couponFromStore = CouponStore.CouponList.FirstOrDefault(x => x.Id == id);
    if (couponFromStore != null)
    {
        CouponStore.CouponList.Remove(couponFromStore);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.NoContent;
        return Results.Ok(response);
    }
    else
    {
        response.ErrorMessages.Add("Invalid Id");
        return Results.NotFound(response);
    }
}).WithName("RemoveCoupon")
  .Produces<APIResponse>(200)
  .Produces<APIResponse>(404);


app.UseHttpsRedirection();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
