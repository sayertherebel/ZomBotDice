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
//using ZomBotDice.Validators;
//using ZomBotDice.Models;
//using uk.co.silversands.servicemanager.library.viewmodels;
//using uk.co.silversands.servicemanager.api;

//namespace ZomBotDice.Dialogs
//{
//    public class ReassignDialog : ComponentDialog
//    {
//        protected readonly ILogger Logger;
//        protected readonly ConversationState CState;
//        protected readonly ClientFunctions Functions;
//        protected readonly IIRNumberValidator IRNumberValidator;
//        protected readonly IAssigneeValidator AssigneeValidator;

//        public ReassignDialog(
//            ILogger<ReassignDialog> logger,
//            ClientFunctions _functions,
//            IIRNumberValidator _irNumberValidator,
//            IAssigneeValidator _assigneeValidator,
//            ZombieIntents intents = null
//            )
//            : base("reassign-dialog")
//        {
//            Logger = logger;
//            Functions = _functions;
//            IRNumberValidator = _irNumberValidator;
//            AssigneeValidator = _assigneeValidator;

//            AddDialog(new TextPrompt("irnumberprompt", IRNumberValidator.Validate));
//            AddDialog(new TextPrompt("assigneeprompt", AssigneeValidator.Validate));
//            AddDialog(new ConfirmPrompt("reassign-confirm"));
//            AddDialog(new WaterfallDialog("reassign-waterfall", new WaterfallStep[]
//            {
//                ParseIncomingFields,
//                PromptIRNumberStep,
//                PromptAssigneeStep,
//                ConfirmStep,
//                ProcessStep
//            }));

//            InitialDialogId = "reassign-waterfall";

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
//                if (action.ID != null)
//                {
//                    stepContext.Values["irnumber"] = action.ID;
//                }
//                if (action.newValue != null)
//                {
//                    stepContext.Values["assignee"] = action.newValue;
//                    await AssigneeValidator.Validate(action.newValue, stepContext.Context);
//                    //stepContext.Values["skipconfirm"] = true;
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
//                                    return await ResetHelper.Reset(stepContext);
//                                }
//                            }
//                        }
//                        if (Intents.Entities.assignee != null)
//                        {
//                            if (!String.IsNullOrEmpty(Intents.Entities.assignee[0]))
//                            {
//                                stepContext.Values["assignee"] = Intents.Entities.assignee[0];

//                                Tuple<bool, string> validateResult = await AssigneeValidator.Validate(Intents.Entities.assignee[0], stepContext.Context);
//                                if (validateResult.Item1)
//                                {
//                                    stepContext.Values["assignee"] = validateResult.Item2;
//                                }
//                                else
//                                {
//                                    await stepContext.Context.SendWithRetry(validateResult.Item2, cancellationToken);
//                                    return await ResetHelper.Reset(stepContext);
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

//        private async Task<DialogTurnResult> PromptAssigneeStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            if (!((!stepContext.Values.ContainsKey("irnumber")) && String.IsNullOrEmpty(stepContext.Result.ToString())))
//            {

//                if (!stepContext.Values.ContainsKey("irnumber")) { stepContext.Values["irnumber"] = stepContext.Result.ToString(); }

//                if (!stepContext.Values.ContainsKey("assignee"))
//                {
//                    var prompt = MessageFactory.Text(IdiomGenerator.RequestAssignee());
//                    return await stepContext.PromptAsync("assigneeprompt", new PromptOptions { Prompt = prompt }, cancellationToken);
//                }
//                else
//                {
//                    return await stepContext.NextAsync();
//                }
//            }
//            await stepContext.Context.SendWithRetry("Error", cancellationToken);
//            return await ResetHelper.Reset(stepContext);
//        }
//        private async Task<DialogTurnResult> ConfirmStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            if (stepContext.Context.TurnState.ContainsKey("resolvedAssignee"))
//            {
//                if (!stepContext.Values.ContainsKey("assignee"))
//                {
                    
//                    stepContext.Values["assignee"] = stepContext.Context.TurnState["resolvedAssignee"].ToString();
//                }

//                if(!stepContext.Values["assignee"].ToString().Contains("@"))
//                {
//                    stepContext.Values["assignee"] = (stepContext.Values["assignee"].ToString().Replace(" ", ".") + "@silversands.co.uk");
//                }

//                if (!stepContext.Values.ContainsKey("skipconfirm"))
//                {

//                    if(stepContext.Context.TurnState.ContainsKey("incidentvm"))
//                    {
//                        VMIncident incidentVM = (VMIncident)stepContext.Context.TurnState["incidentvm"];

//                        string confirmationPrompt = "Reassign <b>{1}</b> ({2}) to <b>{0}</b>?";

//                        return await stepContext.PromptAsync("reassign-confirm",
//                            new PromptOptions
//                            {
//                                Prompt = MessageFactory.Text(
//                                        String.Format(confirmationPrompt,
//                                        stepContext.Values["assignee"].ToString(),//.Split('@')[0].Replace(".", " "),
//                                        stepContext.Values["irnumber"].ToString().ToUpper(),
//                                        incidentVM.title
//                                    )
//                                )
//                            }, cancellationToken);

//                    }
//                    else
//                    {
//                        string confirmationPrompt = "Reassign <b>{1}</b> to <b>{0}</b>?";

//                        return await stepContext.PromptAsync("reassign-confirm",
//                            new PromptOptions
//                            {
//                                Prompt = MessageFactory.Text(
//                                        String.Format(confirmationPrompt,
//                                        stepContext.Values["assignee"].ToString().Split('@')[0].Replace(".", " "),
//                                        stepContext.Values["irnumber"].ToString().ToUpper()
//                                    )
//                                )
//                            }, cancellationToken);
//                    }


//                }
//                else
//                {
//                    // Skip confirm

//                    return await stepContext.NextAsync();
//                }
//            }
//            else
//            {
//                await stepContext.Context.SendWithRetry("Error (no resolved assignee.)", cancellationToken);

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


//                    await Functions.Reassign(stepContext.Values["irnumber"].ToString(), stepContext.Values["assignee"].ToString());


//                    await stepContext.Context.SendWithRetry(IdiomGenerator.TaskCompletedAffirmative, cancellationToken);
//                    stepContext.Context.TurnState["irnumber"] = stepContext.Values["irnumber"].ToString();
//                    return await stepContext.BeginDialogAsync("show-ticket");
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

//            return await ResetHelper.Reset(stepContext);
//        }


//    }
//}
