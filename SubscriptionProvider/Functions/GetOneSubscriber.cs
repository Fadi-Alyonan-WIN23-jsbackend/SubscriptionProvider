using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubscriptionProvider.Models;

namespace SubscriptionProvider.Functions
{
    public class GetOneSubscriber
    {
        private readonly ILogger<GetOneSubscriber> _logger;
        private readonly DataContext _dataContext;

        public GetOneSubscriber(ILogger<GetOneSubscriber> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        [Function("GetOneSubscriber")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex) 
            { 
                _logger.LogError($" StreamReader GetOneSubscriber :: {ex.Message}"); 
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            if (body != null)
            {
                SubscriptionModel sm = null!;
                try
                {
                    sm = JsonConvert.DeserializeObject<SubscriptionModel>(body)!;
                }
                catch (Exception ex) 
                {
                    _logger.LogError($" JsonConvert.DeserializeObject<SubscriptionModel/GetOne> :: {ex.Message} "); 
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                if (sm != null && !string.IsNullOrEmpty(sm.Email))
                {
                    try
                    {
                        var Subscriber = await _dataContext.Subscriptions.FirstOrDefaultAsync(x => x.Email == sm.Email);
                        if (Subscriber != null)
                        {
                            var json = JsonConvert.SerializeObject(Subscriber);
                            return new OkObjectResult(json);
                        }
                        else
                        {
                            return new NotFoundResult();
                        }
                    }
                    catch (Exception ex) 
                    { 
                        _logger.LogError($" Get One Subscriber :: {ex.Message}"); 
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }

                }
            }
            return new BadRequestResult();
        }
    }
}
