using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubscriptionProvider.Models;

namespace SubscriptionProvider.Functions
{
    public class UpdateSubscriberInfo
    {
        private readonly ILogger<UpdateSubscriberInfo> _logger;
        private readonly DataContext _dataContext;

        public UpdateSubscriberInfo(ILogger<UpdateSubscriberInfo> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        [Function("UpdateSubscriberInfo")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($" StreamReader UpdateSubscriberInfo :: {ex.Message}");
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
                    _logger.LogError($" JsonConvert.DeserializeObject<SubscriptionModel/Update> :: {ex.Message} ");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                if (sm != null && !string.IsNullOrEmpty(sm.Email))
                {
                    var subscriberToUpdate = _dataContext.Subscriptions.FirstOrDefault(x => x.Email == sm.Email);
                    if (subscriberToUpdate != null)
                    {
                        subscriberToUpdate.Email = sm.Email;
                        subscriberToUpdate.DailyNewsletter = sm.DailyNewsletter;
                        subscriberToUpdate.AdvertisingUpdates = sm.AdvertisingUpdates;
                        subscriberToUpdate.WeekinReview = sm.WeekinReview;
                        subscriberToUpdate.EventUpdates = sm.EventUpdates;
                        subscriberToUpdate.StartupsWeekly = sm.StartupsWeekly;
                        subscriberToUpdate.Podcasts = sm.Podcasts;

                        try
                        {
                            _dataContext.Subscriptions.Update(subscriberToUpdate);
                            await _dataContext.SaveChangesAsync();
                            var json = JsonConvert.SerializeObject(subscriberToUpdate);
                            return new OkObjectResult(json);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($" Update subscriber  :: {ex.Message}");
                            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                        }
                    }
                    else
                    {
                        return new NotFoundResult();
                    }
                }
            }
            return new BadRequestResult();
        }
    }
}
