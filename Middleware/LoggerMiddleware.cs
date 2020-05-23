using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ScribeBotV4
{
    public class ScribeBotMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                // run full pipeline
                

                foreach (var activity in activities)
                {
                    RewriteAttachmentType(activity);
                }
                var responses = await nextSend().ConfigureAwait(false);
                return responses;
            });

            await next(cancellationToken);
            //await turnContext.SendWithRetry("After");
        }

        private void RewriteAttachmentType(IActivity activity)
        {
            if (activity.Type == "message")
            {
                if (activity.AsMessageActivity().Attachments != null)
                {
                    if (activity.AsMessageActivity().Attachments.Count > 0)
                    {
                        foreach (Attachment attachment in activity.AsMessageActivity().Attachments)
                        {
                            if (attachment.ContentType.ToString().Equals("application/vnd.microsoft.card.custom"))
                            {
                                attachment.ContentType = "application/vnd.microsoft.card.adaptive";
                            }
                        }
                    }
                }
            }
        }

        
    }
}
