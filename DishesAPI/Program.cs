using System.Net;
using System.Security.Claims;
using AutoMapper;
using DishesAPI.DbContexts;
using DishesAPI.Entities;
using DishesAPI.Extensions;
using DishesAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o =>
    o.UseSqlite(builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddProblemDetails();

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminFromBelgium", policy =>
        policy.RequireRole("admin")
            .RequireClaim("country", "Belgium")
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("TokenAuthNZ",
        new()
        {
            Name = "Authorization",
            Description = "Token-based authentication and authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            In = ParameterLocation.Header
        });
    options.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "TokenAuthNZ"
                }
            }, new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

//**Catching exceptions+
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    // app.UseExceptionHandler(configureApplicationBuilder =>
    //     {
    //         configureApplicationBuilder.Run(
    //             async context =>
    //             {
    //                 context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    //                 context.Response.ContentType = "text/html";
    //                 await context.Response.WriteAsync("An unexpected problem happened");
    //             }
    //         );
    //     }
    // );
}



app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();


// var dishesEndpoints = app.MapGroup("/dishes");
// var dishesWithGuidIdEndpoints = dishesEndpoints.MapGroup("/{dishId:guid}");
// var ingredientsEndpoints = dishesWithGuidIdEndpoints.MapGroup("/ingredients");
//
//
// dishesEndpoints.MapGet("", async Task<Ok<IEnumerable<DishDto>>>(DishesDbContext dishesDbContext,
//     ClaimsPrincipal claimsPrincipal,
//     IMapper mapper,
//      string? name) =>
// {
//     Console.WriteLine($"User authenticated?{claimsPrincipal.Identity?.IsAuthenticated}");
//     
//     return TypedResults.Ok(mapper.Map<IEnumerable<DishDto>>(await dishesDbContext.Dishes
//         .Where(d => name == null || d.Name.Contains(name)).ToListAsync()
//     ));
// });
//
// dishesWithGuidIdEndpoints.MapGet("", async Task<Results<NotFound, Ok<DishDto>>>(DishesDbContext dishesDbContext, 
//     IMapper mapper, 
//     Guid dishId) =>
// {
//     var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dishId);
//     if (dishEntity == null)
//     {
//         return TypedResults.NotFound();
//     }
//
//     return TypedResults.Ok(mapper.Map<DishDto>(dishEntity));
// }).WithName("GetDish");
//
// ingredientsEndpoints.MapGet("", async Task<Results<NotFound, Ok<IEnumerable<IngredientDto>>>>(DishesDbContext dishesDbContext, 
//     IMapper mapper,
//     Guid dishId) =>
// {
//     var dishEntity = await dishesDbContext.Dishes
//         .FirstOrDefaultAsync(d => d.Id == dishId);
//     if (dishEntity == null)
//     {
//         return TypedResults.NotFound();
//     }
//
//     return TypedResults.Ok(mapper.Map<IEnumerable<IngredientDto>>((await dishesDbContext.Dishes
//         .FirstOrDefaultAsync(d => d.Id == dishId))?.Ingredients));
// });
//
// dishesEndpoints.MapGet("/{dishName}", async Task<Ok<DishDto>>(DishesDbContext dishesDbContext, 
//     IMapper mapper, 
//     string dishName) =>
// {
//     return TypedResults.Ok(mapper.Map<DishDto>(await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Name == dishName)));
// });
//
//
// //POST
// dishesEndpoints.MapPost("",  async Task<CreatedAtRoute<DishDto>>(DishesDbContext dishesDbContext,
//     IMapper mapper,
//     DishForCreationDto dishForCreationDto
//     // ,
//     // LinkGenerator linkGenerator,
//     // HttpContext httpContext
//     ) =>
// {
//     var dishEntity = mapper.Map<Dish>(dishForCreationDto);
//     dishesDbContext.Add(dishEntity);
//     await dishesDbContext.SaveChangesAsync();
//
//     var dishToReturn = mapper.Map<DishDto>(dishEntity);
//
//     return TypedResults.CreatedAtRoute(
//         dishToReturn,
//         "GetDish",
//         new { dishId = dishToReturn.Id });
//
//     // var linkToDish = linkGenerator.GetUriByName(httpContext, "GetDish", new { dishId = dishToReturn.Id });
//     //
//     // return TypedResults.Created(linkToDish, dishToReturn);
// });
//
// dishesWithGuidIdEndpoints.MapPut("", async Task<Results<NotFound, NoContent> >(DishesDbContext dishesDbContext, 
//         IMapper mapper,
//         Guid dishId,
//         DishForUpdateDto dishForUpdateDto
//         ) =>
// {
//     var dishEntity = await dishesDbContext.Dishes
//         .FirstOrDefaultAsync(d => d.Id == dishId);
//
//     if (dishEntity == null)
//     {
//         return TypedResults.NotFound();
//     }
//
//     mapper.Map(dishForUpdateDto, dishEntity);
//
//     await dishesDbContext.SaveChangesAsync();
//
//     return TypedResults.NoContent();
// });
//
// dishesWithGuidIdEndpoints.MapDelete("", async Task<Results<NotFound, NoContent>>(DishesDbContext dishesDbContext,
//     Guid dishId) =>
// {
//     var dishEntity = await dishesDbContext.Dishes
//         .FirstOrDefaultAsync(d => d.Id == dishId);
//
//     if (dishEntity == null)
//     {
//         return TypedResults.NotFound();
//     }
//
//     dishesDbContext.Dishes.Remove(dishEntity);
//     await dishesDbContext.SaveChangesAsync();
//     return TypedResults.NoContent();
// });

app.RegisterDishesEndpoints();
app.RegisterIngredientsEndpoints();

//Re create & Migrate the database on each run, for demo purposes
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureDeleted();
    context.Database.Migrate();
}

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
