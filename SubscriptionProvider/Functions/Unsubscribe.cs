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
    public class UnSubscribe
    {
        private readonly ILogger<UnSubscribe> _logger;
        private readonly DataContext _dataContext;

        public UnSubscribe(ILogger<UnSubscribe> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        [Function("Unsubscribe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($" StreamReader Unsubscribe :: {ex.Message}");
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
                    _logger.LogError($" JsonConvert.DeserializeObject<SubscriptionModel/delete> :: {ex.Message} ");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                if (sm != null && !string.IsNullOrEmpty(sm.Email))
                {

                    try
                    {
                        var course = await _dataContext.Subscriptions.FirstOrDefaultAsync(x => x.Email == sm.Email);
                        if (course != null)
                        {
                            _dataContext.Subscriptions.Remove(course);
                            var res = await _dataContext.SaveChangesAsync();
                            return new OkResult();

                        }
                        else
                        {
                            return new NotFoundResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($" Unsubscribe :: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }

                }
            }
            return new BadRequestResult();
        }
    }
}
