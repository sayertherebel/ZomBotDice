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

//namespace ZomBotDice.Dialogs
//{
//    public class SetOnHoldDateDialog : ComponentDialog
//    {
//        protected readonly ILogger Logger;
//        protected readonly ConversationState CState;
//        protected readonly ClientFunctions Functions;
//        protected readonly IIRNumberValidator IRNumberValidator;

//        public SetOnHoldDateDialog(
//            ILogger<SetOnHoldDateDialog> _logger,
//            ClientFunctions _functions,
//            IIRNumberValidator _irNumberValidator
//        )
//            : base("set-on-hold")
//        {
//            Logger = _logger;
//            Functions = _functions;
//            IRNumberValidator = _irNumberValidator;

//            AddDialog(new TextPrompt("irnumberprompt", IRNumberValidator.Validate));
//            AddDialog(new TextPrompt("dtprompt"));
//            AddDialog(new ConfirmPrompt("set-on-hold-confirm"));
//            AddDialog(new WaterfallDialog("set-on-hold-waterfall", new WaterfallStep[]
//            {
//                ParseIncomingFields,
//                PromptIRNumberStep,
//                PromptDT,
//                ConfirmStep,
//                ProcessStep
//            }));

//            InitialDialogId = "set-on-hold-waterfall";

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
//                //Tuple<bool, string> validateResult = await NoteBodyValidator.Va(action.comment);
//                //if (!(validateResult.Item1))
//                //{
//                //    // Invalid notebody
//                //    await stepContext.Context.SendWithRetry(validateResult.Item2);
//                //    return await ResetHelper.Reset(stepContext);
//                //}

//                //stepContext.Values["onholddate"] = action.onholddate;
//                //stepContext.Values["irnumber"] = action.ID.ToUpper();
//                //stepContext.Values["skipconfirm"] = "true";

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


//                    if (Intents.Entities != null)
//                    {
//                        if (Intents.Entities.incident != null)
//                        {
//                            if (!String.IsNullOrEmpty(Intents.Entities.incident[0]))
//                            {
//                                Tuple<bool, string> validateResult = await IRNumberValidator.Validate(Intents.Entities.incident[0], stepContext.Context);
//                                if (validateResult.Item1)
//                                {
//                                    stepContext.Values["irnumber"] = Intents.Entities.incident[0].ToUpper();
//                                }
//                                else
//                                {
//                                    await stepContext.Context.SendWithRetry(validateResult.Item2, cancellationToken);
//                                    return await ResetHelper.Reset(stepContext);
//                                }
//                            }
//                        }
//                        if (Intents.Entities.datetime != null)
//                        {
//                            if (!(Intents.Entities.datetime == null))
//                            {
                         
//                                switch(Intents.Entities.datetime[0].Type)
//                                {
//                                    case "duration":
//                                        Regex timexDuration = new Regex(@"(P|PT)(\d*)(D|W|M|Y|H)");
//                                        MatchCollection duration = timexDuration.Matches(Intents.Entities.datetime[0].Expressions[0]);
//                                        DateTime onHoldDate = DateTime.Now;
//                                        switch (duration[0].Groups[3].Value) {
//                                            case "D":
//                                                onHoldDate = DateTime.Now.AddDays(int.Parse(duration[0].Groups[2].Value));
//                                                break;
//                                            case "H":
//                                                onHoldDate = DateTime.Now.AddHours(int.Parse(duration[0].Groups[2].Value));
//                                                break;
//                                            case "W":
//                                                onHoldDate = DateTime.Now.AddDays(int.Parse(duration[0].Groups[2].Value) * 7);
//                                                break;
//                                        }

//                                        stepContext.Values["onholddate"] = onHoldDate.ToString();
//                                        break;
//                                    case "daterange":
//                                    default:
//                                        stepContext.Values["onholddate"] = Intents.Entities.datetime[0].Expressions[0];
//                                        break;
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

//        private async Task<DialogTurnResult> PromptDT(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            if (!((!stepContext.Values.ContainsKey("irnumber")) && String.IsNullOrEmpty(stepContext.Result.ToString())))
//            {

//                if (!stepContext.Values.ContainsKey("irnumber")) { stepContext.Values["irnumber"] = stepContext.Result.ToString().ToUpper(); }

//                if (!stepContext.Values.ContainsKey("onholddate"))
//                {
//                    var prompt = MessageFactory.Text(IdiomGenerator.RequestOnHoldDate());
//                    return await stepContext.PromptAsync("dtprompt", new PromptOptions { Prompt = prompt }, cancellationToken);
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
//            if (!((!stepContext.Values.ContainsKey("onholddate")) && String.IsNullOrEmpty(stepContext.Result.ToString())))
//            {
//                if (!stepContext.Values.ContainsKey("onholddate"))
//                {
//                    stepContext.Values["onholddate"] = stepContext.Result.ToString();
//                }

//                if (!stepContext.Values.ContainsKey("skipconfirm"))
//                {

//                    // Run confirmation

//                    // Get required action from the original LUIS intent

//                    string confirmationPrompt = "Set {1} to On Hold until {0}?";

//                    return await stepContext.PromptAsync("set-on-hold-confirm",
//                        new PromptOptions
//                        {
//                            Prompt = MessageFactory.Text(
//                                    String.Format(confirmationPrompt,
//                                    stepContext.Values["onholddate"],
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

//                    await Functions.SetOnHoldDate(stepContext.Values["irnumber"].ToString(), stepContext.Values["onholddate"].ToString());

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

//            return await ResetHelper.Reset(stepContext);

//        }


//    }
//}
