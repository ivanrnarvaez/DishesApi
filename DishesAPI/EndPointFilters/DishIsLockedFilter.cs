namespace DishesAPI.EndPointFilters;

public class DishIsLockedFilter: IEndpointFilter
{
    private readonly Guid _lockedDishId;
    public DishIsLockedFilter(Guid lockedDishId)
    {
        _lockedDishId = lockedDishId;
    }
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        Guid dishId;

        if (context.HttpContext.Request.Method == "PUT")
        {
            dishId = context.GetArgument<Guid>(2); // index = 2 for dish Id => UpdateDishAsync
        }
        else if (context.HttpContext.Request.Method == "DELETE")
        {
            dishId = context.GetArgument<Guid>(1); // index = 1 for dish Id => DeleteDishAsync
        }
        else
        {
            throw new NotSupportedException("This filter is not supported for this scenario");
        }

        // var rendangId = new Guid("fd630a57-2352-4731-b25c-db9cc7601b16");

        if (dishId == _lockedDishId)
        {
            return TypedResults.Problem(new()
            {
                Status = 400,
                Title = "Dish is perfect and cannot be changed",
                Detail = "You cannot update or delete perfection"
            });
        }

        //invoke the next filter
        var result = await next.Invoke(context);
        return result;
    }
}