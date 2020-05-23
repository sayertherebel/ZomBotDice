//using AdaptiveCards;
//using ZomBotDice.LUIS;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Schema;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using ZomBotDice.Helpers;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Web;
//using uk.co.silversands.servicemanager.api;
//using uk.co.silversands.servicemanager.library.viewmodels;

//namespace ZomBotDice.Dialogs
//{
//    public class MyTicketsDialog : ComponentDialog
//    {
//        protected readonly ILogger Logger;
//        protected readonly ClientFunctions Functions;
        
//        public MyTicketsDialog(
//            ILogger<MyTicketsDialog> _logger,
//            ClientFunctions _functions
//        )
//            : base("my-tickets")
//        {
//            Functions = _functions;
//            Logger = _logger;

//            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
//            {
//                ShowIncidentsStep
//            }));
//        }


//        private async Task<DialogTurnResult> ShowIncidentsStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {

//            string user = stepContext.Context.Activity.From.Name;
//#if DEBUG
//            {
//                user = "Jamie Sayer";
//            }
//#endif
//            VMIncident[] incidents = null;

//            stepContext.Context.SendWithRetry(MessageFactory.Text("Sure, this'll take a few seconds.."), cancellationToken);

//            ZombieIntents Intents = null;

//            try
//            {
//                Intents = (ZombieIntents)stepContext.Options;
//            }
//            catch { }

//            if (Intents != null)
//            {

//                // Store original intent as this is a multi-function dialog..

//                stepContext.Values["intent"] = Intents.TopIntent().intent;

//                switch (Intents.TopIntent().intent)
//                {
//                    case ZombieIntents.Intent.Unassigned:
//                        incidents = await Functions.GetUserActiveIncidents(""); //GetActiveUserIncidents with empty string will instead return unassigned incidents

//                        if ((!(incidents == null)) && (incidents.Length > 0))
//                        {
//                            AdaptiveCard ticketsCard = await AdaptiveCardBuilders.IncidentListACFromVMIncidentArray(incidents);

//                            Attachment attachment = new Attachment()
//                            {
//                                ContentType = "application/vnd.microsoft.card.custom", //Rewritten in middleware
//                                Content = ticketsCard
//                            };

//                            var reply = MessageFactory.Attachment(attachment);
//                            await stepContext.Context.SendWithRetry(reply, cancellationToken);

//                        }
//                        else
//                        {
//                            await stepContext.Context.SendWithRetry(MessageFactory.Text("Doesn't look like there are any unassigned tickets \U0001F603"), cancellationToken);

//                        }

//                        return await ResetHelper.Reset(stepContext);
//                        break;
//                    default:
//                    case ZombieIntents.Intent.MyTickets:

//                        incidents = await Functions.GetUserActiveIncidents(user);

//                        if ((!(incidents == null)) && (incidents.Length > 0))
//                        {
//                            AdaptiveCard ticketsCard = await AdaptiveCardBuilders.IncidentListACFromVMIncidentArray(incidents);

//                            Attachment attachment = new Attachment()
//                            {
//                                ContentType = "application/vnd.microsoft.card.custom", //Rewritten in middleware
//                                Content = ticketsCard
//                            };

//                            var reply = MessageFactory.Attachment(attachment);
//                            await stepContext.Context.SendWithRetry(reply, cancellationToken);

//                        }
//                        else
//                        {
//                            await stepContext.Context.SendWithRetry(MessageFactory.Text("You don't seem to have any tickets assigned to you \U0001F603"), cancellationToken);

//                        }

//                        return await ResetHelper.Reset(stepContext);

//                        break;
//                }

                
//            }

//            return await ResetHelper.Reset(stepContext);


//        }



//        private async Task<DialogTurnResult> ShowIncidentsStepCarousel(WaterfallStepContext stepContext, CancellationToken cancellationToken) // Parked because Teams doesn't reliably support this mode :-(
//        {

//            string user = stepContext.Context.Activity.From.Name;
//            //user = "Jamie Sayer";
//            VMIncident[] incidents = null;

//            stepContext.Context.SendWithRetry(MessageFactory.Text("Sure, this'll take a few seconds.."), cancellationToken);

//            incidents = await Functions.GetUserActiveIncidents(user);

//            if ((!(incidents == null)) && (incidents.Length > 0))
//            {

//                if (incidents.Length > 10) //Carousel can only display 10
//                {

//                    stepContext.Context.SendWithRetry(MessageFactory.Text("As you have more than 10 tickets, I can only display your tickets as a list.."), cancellationToken);

//                    string list = "";

//                    foreach (VMIncident vmincident in incidents)
//                    {
//                        list = String.Format("{0}{1}", list, String.Format("{0} - {1} <br />", vmincident.id, vmincident.title));
//                    }

//                    var reply = MessageFactory.Text(list);
//                    await stepContext.Context.SendWithRetry(reply, cancellationToken);


//                }
//                else
//                {

//                    List<Attachment> attachments = new List<Attachment>();

//                    var tasks = new List<Task<AdaptiveCard>>();

//                    foreach (VMIncident vmIncident in incidents)
//                    {
//                        tasks.Add(Task.Run(async () =>
//                        {
//                            return await AdaptiveCardBuilders.IncidentACFromVMIncident(vmIncident,"0", stepContext.Context.Activity.From.Id);
//                        }));
//                    }

//                    var cards = Task.WhenAll(tasks);

//                    foreach (AdaptiveCard acIncident in cards.Result)
//                    {

//                        Attachment attachment = new Attachment()
//                        {
//                            ContentType = AdaptiveCard.ContentType,
//                            Content = acIncident
//                        };

//                        attachments.Add(attachment);

//                    }


//                    var reply = MessageFactory.Attachment(attachments);
//                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
//                    await stepContext.Context.SendWithRetry(reply, cancellationToken);

//                }



//            }
//            else
//            {
//                stepContext.Context.SendWithRetry(MessageFactory.Text("You don't seem to have any tickets assigned to you \U0001F603"), cancellationToken);

//            }

//            return await stepContext.EndDialogAsync();

//        }


//    }
//}
