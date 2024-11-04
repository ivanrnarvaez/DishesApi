using DishesAPI.EndPointFilters;
using DishesAPI.EndPointHandlers;
using DishesAPI.Models;
using Microsoft.AspNetCore.Http.Metadata;

namespace DishesAPI.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void RegisterDishesEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var dishesEndpoints = endpointRouteBuilder.MapGroup("/dishes")
            .RequireAuthorization();
        var dishesWithGuidIdEndpoints = dishesEndpoints.MapGroup("/{dishId:guid}");

        var dishWithGuidIdEndpointsAndLockFilters = endpointRouteBuilder.MapGroup("/dishes/{dishId:guid}")
            .RequireAuthorization("RequireAdminFromBelgium")
            .AddEndpointFilter(new DishIsLockedFilter(new("fd630a57-2352-4731-b25c-db9cc7601b16")))
            .AddEndpointFilter(new DishIsLockedFilter(new("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06")));

        dishesEndpoints.MapGet("", DishesHandlers.GetDishesAsync);
        dishesWithGuidIdEndpoints.MapGet("", DishesHandlers.GetDishesByIdAsync)
            .WithName("GetDish")
           
            .WithSummary("Get a dish by providing an id.")
            .WithDescription("Dishes are identified by URI containing a dish identifier. This identifier is a GUID. " +
                             "You can get one specific dish via this endpoint by providing the identifier.")
            ;
        dishesEndpoints.MapGet("/{dishName}", DishesHandlers.GetDishesByNameAsync)
            .AllowAnonymous()
            .WithOpenApi(operation =>
            {
                operation.Deprecated = true;
                return operation;
            });
        dishesEndpoints.MapPost("", DishesHandlers.CreateDishAsync)
            .RequireAuthorization("RequireAdminFromBelgium")
            .AddEndpointFilter<ValidateAnnotationsFilter>()
            .ProducesValidationProblem(400)
            .Accepts<DishForCreationDto>(
                "application/json",
                "application/vnd.marvin.dishforcreation+json"
                );
        dishWithGuidIdEndpointsAndLockFilters.MapPut("", DishesHandlers.UpdateDishAsync);

        dishWithGuidIdEndpointsAndLockFilters.MapDelete("", DishesHandlers.DeleteDishAsync)
            .AddEndpointFilter<LogNotFoundResponseFilter>();


    }

    public static void RegisterIngredientsEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var dishesEndpoints = endpointRouteBuilder.MapGroup("/dishes");
        var dishesWithGuidIdEndpoints = dishesEndpoints.MapGroup("/{dishId:guid}");
        var ingredientsEndpoints = dishesWithGuidIdEndpoints.MapGroup("/ingredients")
            .RequireAuthorization();
        ingredientsEndpoints.MapGet("", IngredientsHandlers.GetIngredients);
        ingredientsEndpoints.MapPost("", () =>
        {
            throw new NotImplementedException();

        });
    }

}