//using AdaptiveCards;
//using ZomBotDice.LUIS;
//using ZomBotDice.Models;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Schema;

//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Text.RegularExpressions;
//using ZomBotDice.Helpers;
//using ZomBotDice.Models;
//using ZomBotDice.Validators;
//using uk.co.silversands.servicemanager.api;
//using uk.co.silversands.servicemanager.library.viewmodels;

//namespace ZomBotDice.Dialogs
//{
//    public class SetIncidentStatusDialog : ComponentDialog
//    {
//        protected readonly ILogger Logger;
//        protected readonly ConversationState CState;
//        protected readonly ClientFunctions Functions;
//        protected readonly IIRNumberValidator IRNumberValidator;


//        public SetIncidentStatusDialog(
//            ILogger<SetIncidentStatusDialog> _logger,
//            ClientFunctions _functions,
//            IIRNumberValidator _irNumberValidator
//        )
//            : base("set-status")
//        {
//            Logger = _logger;
//            Functions = _functions;
//            IRNumberValidator = _irNumberValidator;

//            AddDialog(new TextPrompt("irnumberprompt", IRNumberValidator.Validate));
//            AddDialog(new ConfirmPrompt("set-status-confirm"));
//            AddDialog(new WaterfallDialog("set-status-waterfall", new WaterfallStep[]
//            {
//                ParseIncomingFields,
//                PromptIRNumberStep,
//                ConfirmStep,
//                ProcessStep
//            }));

//            InitialDialogId = "set-status-waterfall";

//        }

//        private async Task<DialogTurnResult> ParseIncomingFields(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            stepContext.Values.Clear();

//            // Try for an encoded ScribeAction in the Text

//            ScribeAction action = null;

//            try
//            {
//                action = JsonConvert.DeserializeObject<ScribeAction>(stepContext.Context.Activity.Value.ToString());
//            }
//            catch (Exception e) { }



//            if (action != null)
//            {
//                stepContext.Values["irnumber"] = action.ID;
//                stepContext.Values["skipconfirm"] = "true";

//                switch (action.action.ToLower())
//                {
//                    case "sfr":
//                        stepContext.Values["intent"] = ZombieIntents.Intent.Set_First_Response;
//                        break;
//                    case "ac":
//                        stepContext.Values["intent"] = ZombieIntents.Intent.Await_Customer;
//                        break;
//                    case "sa":
//                        stepContext.Values["intent"] = ZombieIntents.Intent.Set_Active;
//                        break;
//                }
//            }
//            else
//            {

//                // Try from LUIS entities

//                ZombieIntents Intents = null;

//                try
//                {
//                    Intents = (ZombieIntents)stepContext.Options;
//                }
//                catch { }

//                if (Intents != null)
//                {

//                    // Store original intent as this is a multi-function dialog..

//                    stepContext.Values["intent"] = Intents.TopIntent().intent;

//                    if (Intents.Entities != null)
//                    {
//                        if (Intents.Entities.incident != null)
//                        {
//                            if (!String.IsNullOrEmpty(Intents.Entities.incident[0]))
//                            {
//                                Tuple<bool, string> validateResult = await IRNumberValidator.Validate(Intents.Entities.incident[0], stepContext.Context);
//                                if (validateResult.Item1)
//                                {
//                                    stepContext.Values["irnumber"] = Intents.Entities.incident[0];
//                                }
//                                else
//                                {
//                                    await stepContext.Context.SendWithRetry(validateResult.Item2, cancellationToken);
//                                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog));
//                                }
//                            }
//                        }

//                    }
//                }

//            }

//            return await stepContext.NextAsync(null, cancellationToken);

//        }

//        private async Task<DialogTurnResult> PromptIRNumberStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {

//            if (!stepContext.Values.ContainsKey("irnumber"))
//            {
//                var prompt = MessageFactory.Text(IdiomGenerator.RequestIRNumber());
//                var retryPromot = MessageFactory.Text("Please try again.");
//                return await stepContext.PromptAsync("irnumberprompt", new PromptOptions { Prompt = prompt, RetryPrompt = retryPromot }, cancellationToken);
//            }
//            else
//            {
//                return await stepContext.NextAsync();
//            }
//        }

//        private async Task<DialogTurnResult> ConfirmStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            if (!((!stepContext.Values.ContainsKey("irnumber")) && String.IsNullOrEmpty(stepContext.Result.ToString())))
//            {
//                if (!stepContext.Values.ContainsKey("irnumber")) { stepContext.Values["irnumber"] = stepContext.Result.ToString(); }

//                if (!stepContext.Values.ContainsKey("skipconfirm"))
//                {

//                    // Run confirmation

//                    // Get required status from the original LUIS intent

//                    string prompt = "";

//                    switch ((ZombieIntents.Intent)stepContext.Values["intent"])
//                    {
//                        case ZombieIntents.Intent.Set_First_Response:
//                            prompt = "Set First Response on {0}?";
//                            break;
//                        case ZombieIntents.Intent.Await_Customer:
//                            prompt = "Set {0} to 'Await Customer'?";
//                            break;
//                        case ZombieIntents.Intent.Set_Active:
//                            prompt = "Set {0} to 'Active'?";
//                            break;
//                            // Default should not be needed as we filter before now
//                      }

//                    return await stepContext.PromptAsync("set-status-confirm",
//                        new PromptOptions
//                        {
//                            Prompt = MessageFactory.Text(
//                                    String.Format(
//                                    prompt,
//                                    stepContext.Values["irnumber"].ToString()
//                                )
//                            )
//                        }, cancellationToken);

//                }
//                else
//                {
//                    // Skip confirm

//                    return await stepContext.NextAsync();
//                }
//            }
//            else
//            {
//                await stepContext.Context.SendWithRetry("Error", cancellationToken);

//            }

//            return await ResetHelper.Reset(stepContext);
//        }
//        private async Task<DialogTurnResult> ProcessStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            bool confirmed = false;
//            try
//            {
//                if (stepContext.Result != null) { confirmed = (bool)stepContext.Result; }
//                if (stepContext.Values.ContainsKey("skipconfirm")) { confirmed = true; }
//            }
//            catch { }

//            if (confirmed)
//            {

//                // User confirmed

//                try
//                {
//                    // Get required status from the original LUIS intent

//                    switch ((ZombieIntents.Intent)stepContext.Values["intent"])
//                    {
//                        case ZombieIntents.Intent.Set_First_Response:
//                            await Functions.SFR(stepContext.Values["irnumber"].ToString());
//                            break;
//                        case ZombieIntents.Intent.Set_Active:
//                            await Functions.SetStatus(new VMTicketStatus("Active", stepContext.Values["irnumber"].ToString()));
//                            break;
//                        case ZombieIntents.Intent.place_on_hold:
//                            await Functions.SetStatus(new VMTicketStatus("OnHold", stepContext.Values["irnumber"].ToString()));
//                            break;
//                        case ZombieIntents.Intent.Await_Customer:
//                            await Functions.SetStatus(new VMTicketStatus("AwaitingCustomer", stepContext.Values["irnumber"].ToString()));
//                            break;
//                    }

//                    await stepContext.Context.SendWithRetry(IdiomGenerator.TaskCompletedAffirmative, cancellationToken);
//                }
//                catch
//                {
//                    await stepContext.Context.SendWithRetry("Error", cancellationToken);

//                }

//            }
//            else
//            {
//                // User did not confirm
//                await stepContext.Context.SendWithRetry(IdiomGenerator.TaskCancelled(), cancellationToken);
//            }
//            stepContext.Context.TurnState["irnumber"] = stepContext.Values["irnumber"].ToString();
//            return await stepContext.BeginDialogAsync("show-ticket");
//            return await ResetHelper.Reset(stepContext);
//        }


//    }
//}
