using Data.Context;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubscriptionProvider.Models;
using System.Runtime.Intrinsics.X86;

namespace SubscriptionProvider.Functions
{
    public class Subscribe
    {
        private readonly ILogger<Subscribe> _logger;
        private readonly DataContext _dataContext;
        public Subscribe(ILogger<Subscribe> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        [Function("Subscribe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex) { _logger.LogError($" StreamReader Subscribe :: {ex.Message}"); }
            if (body != null)
            {
                SubscriptionModel sm = null!;
                try
                {
                    sm = JsonConvert.DeserializeObject<SubscriptionModel>(body)!;
                }
                catch (Exception ex) 
                { 
                    _logger.LogError($" JsonConvert.DeserializeObject<SubscriptionModel/Create> :: {ex.Message} ");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                if (sm != null && !string.IsNullOrEmpty(sm.Email))
                {
                    if (!await _dataContext.Subscriptions.AnyAsync(x => x.Email == sm.Email))
                    {
                        var subscriptionModel = new SubscriptionEntity
                        {
                            Email = sm.Email,
                            DailyNewsletter = sm.DailyNewsletter,
                            AdvertisingUpdates = sm.AdvertisingUpdates,
                            WeekinReview = sm.WeekinReview,
                            EventUpdates = sm.EventUpdates,
                            StartupsWeekly = sm.StartupsWeekly,
                            Podcasts = sm.Podcasts
                        };
                        try
                        {
                            _dataContext.Subscriptions.Add(subscriptionModel);
                            await _dataContext.SaveChangesAsync();
                            return new OkResult();

                        }
                        catch (Exception ex) 
                        { 
                            _logger.LogError($" Subscribe :: {ex.Message}");
                            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                        }

                    }
                }
                return new ConflictResult();
            }
            return new BadRequestResult();
        }
    }
}
