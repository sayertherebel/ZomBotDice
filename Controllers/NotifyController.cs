//// Copyright (c) Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License.

//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using AdaptiveCards;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Integration.AspNet.Core;
//using Microsoft.Bot.Schema;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using ScribeBotV4;
//using ZomBotDice.Helpers;
//using uk.co.silversands.servicemanager.api;
//using uk.co.silversands.servicemanager.library.viewmodels;

//namespace ProactiveBot.Controllers
//{
//    [Route("api/notify")]
//    [ApiController]
//    public class NotifyController : ControllerBase
//    {
//        private readonly IBotFrameworkHttpAdapter _adapter;
//        private readonly string _appId;
//        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
//        private readonly ClientFunctions _functions;
//        private ConcurrentDictionary<string, string> UnassignedTicketSubscribers;
//        private readonly ILogger<NotifyController> _logger;
//        private readonly IConfiguration _configuration;

//        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, string> unassigedTicketSubscribers,
//            ConcurrentDictionary<string, ConversationReference> conversationReferences, ClientFunctions functions, ILogger<NotifyController> logger)
//        {
//            _adapter = adapter;
//            _conversationReferences = conversationReferences;
//            _appId = configuration["MicrosoftAppId"];
//            _functions = functions;
//            UnassignedTicketSubscribers = unassigedTicketSubscribers;
//            _logger = logger;
//            _configuration = configuration;

//            // If the channel is the Emulator, and authentication is not in use,
//            // the AppId will be null.  We generate a random AppId for this case only.
//            // This is not required for production, since the AppId will have a value.
//            if (string.IsNullOrEmpty(_appId))
//            {
//                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
//            }
//        }

//        //[Authorize(Policy = "ApiKeyPolicy")]
//        public async Task<IActionResult> Get()
//        {

//            //Get new assignments from API

            
//            Dictionary<String, List<VMIncident>> newAssignments = (await _functions.GetNewAssignments());

            

//            if (newAssignments != null)
//            {
//                string assignmentsString = JsonConvert.SerializeObject(newAssignments);

//                foreach (String key in newAssignments.Keys)
//                {
//                    if (_conversationReferences.ContainsKey(key) || key.Equals(""))
//                    {

//                        var reply = MessageFactory.Attachment(new List<Attachment>());
//                        reply.Text = key.Equals("") ? "There are new unassigned tickets.." : "You've been assigned new tickets..";

//                        foreach (VMIncident incident in newAssignments[key])
//                        {
//                            //Rebind necessary to get full Incident representation. Getting the NewAssignements method to do it is too slow
//                            // so it uses the Incident (Typical) projection instead
//                            VMIncident reboundIncidentVM = await _functions.GetIncident(incident.id); 
//                            string idMap = Guid.NewGuid().ToString();
//                            //var reply = MessageFactory.Attachment(new List<Attachment>());
//                            /// https://stackoverflow.com/questions/55532375/how-to-update-an-adaptive-card-which-is-already-sent-to-user-from-bot
//                            var incidentAdaptiveCard = await AdaptiveCardBuilders.IncidentACFromVMIncident(reboundIncidentVM, idMap, "");

//                            Attachment attachment = new Attachment()
//                            {
//                                ContentType = AdaptiveCard.ContentType,
//                                Content = incidentAdaptiveCard
//                            };
//                            reply.Attachments.Add(attachment);
//                        }

//                        if(key.Equals(""))
//                        {
//                            // Notify helpdesk members of unassigned tickets
//                            foreach (string helpdeskUser in UnassignedTicketSubscribers.Keys)
//                            {
//                                if(_conversationReferences.ContainsKey(helpdeskUser))
//                                {
//                                    await DoNotify(_conversationReferences[helpdeskUser], reply);
//                                }
//                            }
                            

//                        }
//                        else
//                        {
//                            // Notify new assignees directly
//                            await DoNotify(_conversationReferences[key], reply);
//                        }

                        
                        
//                    }
//                    else
//                    {
//                        _logger.LogDebug("No Conversation Reference", new Dictionary<string, string> { { "Username", key } });
//                    }

//                }

//                return new ContentResult()
//                {
//                    Content = String.Format("<html><body><H2>Assignments</H2>{0}<H2>{1}</H2></body></html>", assignmentsString, _conversationReferences.Keys),
//                    ContentType = "text/html",
//                    StatusCode = (int)HttpStatusCode.OK,
//                };
//            }
//            else
//            {
//                return new ContentResult()
//                {
//                    Content = "<html><body><h1>Null response.</h1></body></html>",
//                    ContentType = "text/html",
//                    StatusCode = (int)HttpStatusCode.OK,
//                };
//            }

//        }

//        private async Task DoNotify(ConversationReference Reference, IMessageActivity Activity)
//        {

//            int maxRetry = 2;
//            int retryCount = 0;

//            while (true)
//            {
//                try
//                {
//                    await ((BotAdapter)_adapter)
//                                    .ContinueConversationAsync(
//                                        _appId,
//                                        Reference,
//                                        async (context, token) =>
//                                        {
//                                            await context.SendWithRetry(Activity, token);
//                                        },
//                                        default(CancellationToken)
//                                    );
//                    return;
//                }
//                catch (Exception e)
//                {
//                    _logger.LogError(e, "An error occurred while attempting to send a proactive message.");

//                    if(++retryCount >= maxRetry)
//                    {
//                        throw e;
//                    }
//                }
//            }
//        }   

//    }
//}
