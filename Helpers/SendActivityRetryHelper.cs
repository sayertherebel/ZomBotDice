using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ZomBotDice.Helpers
{
    public static class SendActivityRetryHelper
    {

        //private readonly IConfiguration _configuration;
        //private readonly ILogger<SendActivityRetryHelper> _logger;
        //public SendActivityRetryHelper(IConfiguration configuration, ILogger<SendActivityRetryHelper> logger)
        //{
        //    _configuration = configuration;
        //    _logger = logger;
        //}

        //public async Task<ResourceResponse> SendWithRetry(WaterfallStepContext stepContext, string messageText, CancellationToken cancellationToken)
        //{
        //    var reply = MessageFactory.Text(messageText);
        //    return await SendWithRetry(stepContext, reply, cancellationToken);
        //}

        //public async Task<ResourceResponse> SendWithRetry(WaterfallStepContext stepContext, Activity reply, CancellationToken cancellationToken)
        //{
        //    int maxRetry = _configuration["SentActivityMaxRetry"] == null ? 2 : int.Parse(_configuration["ProactiveOperationMaxRetry"]);
        //    int retryCount = 0;

        //    while (true)
        //    {
        //        try
        //        {
        //            return await stepContext.Context.SendWithRetry(reply, cancellationToken);
        //        }
        //        catch (Exception e)
        //        {
        //            _logger.LogError(e, "An error occurred while attempting to send a message.");

        //            if (++retryCount >= maxRetry)
        //            {
        //                throw e;
        //            }
        //        }
        //    }
        //}

        public static async Task<ResourceResponse> SendWithRetry(this ITurnContext ctx, string Text, CancellationToken cancellationToken, int MaxRetries = 2)
        {
            var reply = MessageFactory.Text(Text);
            return await SendWithRetry(ctx, reply, cancellationToken, MaxRetries);
        }

         public static async Task<ResourceResponse> SendWithRetry(this ITurnContext ctx, IMessageActivity Reply, CancellationToken cancellationToken, int MaxRetries = 2)
         {
            int retryCount = 0;
            if (MaxRetries == 0) { MaxRetries = 2; };

            while (true)
            {
                try
                {
                    return await ctx.SendActivityAsync(Reply, cancellationToken);
                    
                }
                catch (Exception e)
                {
                    

                    if (++retryCount >= MaxRetries)
                    {
                        throw e;
                    }
                }
            }
         }
    }
}
