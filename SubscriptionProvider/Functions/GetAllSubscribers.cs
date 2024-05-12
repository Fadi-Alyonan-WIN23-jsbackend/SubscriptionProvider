using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SubscriptionProvider.Functions;

public class GetAllSubscribers
{
    private readonly ILogger<GetAllSubscribers> _logger;
    private readonly DataContext _dataContext;

    public GetAllSubscribers(ILogger<GetAllSubscribers> logger, DataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }

    [Function("GetAllSubscribers")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var Subscribers = await _dataContext.Subscriptions.ToListAsync();
            if (Subscribers != null)
            {
                var json = JsonConvert.SerializeObject(Subscribers);
                return new OkObjectResult(json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($" Get All Subscribers :: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return new NotFoundResult();
    }
}
