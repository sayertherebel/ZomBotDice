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
//using uk.co.silversands.servicemanager.api;

//namespace ZomBotDice.Dialogs
//{
//    public class AddNoteOrResolveDialog : ComponentDialog
//    {
//        protected readonly ILogger Logger;
//        protected readonly ConversationState CState;
//        protected readonly ClientFunctions Functions;
//        protected readonly IIRNumberValidator IRNumberValidator;
//        protected readonly INotebodyValidator NoteBodyValidator;

//        public AddNoteOrResolveDialog(
//            ILogger<AddNoteOrResolveDialog> _logger,
//            ClientFunctions _functions,
//            IIRNumberValidator _irvalidator,
//            INotebodyValidator _notebodyvalidator
//            )
//            : base("add-note-or-resolve")
//        {
//            Logger = _logger;
//            Functions = _functions;
//            IRNumberValidator = _irvalidator;
//            NoteBodyValidator = _notebodyvalidator;

//            AddDialog(new TextPrompt("irnumberprompt", IRNumberValidator.Validate));
//            AddDialog(new TextPrompt("notebodyprompt", NoteBodyValidator.Validate));
//            AddDialog(new ConfirmPrompt("add-note-or-resolve-confirm"));
//            AddDialog(new WaterfallDialog("add-note-or-resolve-waterfall", new WaterfallStep[]
//            {
//                ParseIncomingFields,
//                PromptIRNumberStep,
//                PromptNoteBodyStep,
//                ConfirmStep,
//                ProcessStep
//            }));

//            InitialDialogId = "add-note-or-resolve-waterfall";

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
//                Tuple<bool, string> validateResult = await NoteBodyValidator.Validate(action.comment, stepContext.Context);
//                if (!(validateResult.Item1))
//                {
//                    // Invalid notebody
//                    await stepContext.Context.SendWithRetry(validateResult.Item2, cancellationToken);
//                    return await ResetHelper.Reset(stepContext);
//                }

//                stepContext.Values["notebody"] = action.comment;
//                stepContext.Values["irnumber"] = action.ID;
//                stepContext.Values["skipconfirm"] = "true";

//                switch (action.action.ToLower())
//                {
//                    case "addnote":
//                        stepContext.Values["intent"] = ZombieIntents.Intent.Add_Note;
//                        break;
//                    case "resolve":
//                        stepContext.Values["intent"] = ZombieIntents.Intent.Resolve;
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
//                                    return await ResetHelper.Reset(stepContext);
//                                }
//                            }
//                        }
//                        if (Intents.Entities.freenotebody != null)
//                        {
//                            if (!String.IsNullOrEmpty(Intents.Entities.freenotebody[0]))
//                            {
//                                stepContext.Values["notebody"] = Intents.Entities.freenotebody[0];

//                                Tuple<bool, string> validateResult = await NoteBodyValidator.Validate(Intents.Entities.freenotebody[0], stepContext.Context);
//                                if (validateResult.Item1)
//                                {
//                                    stepContext.Values["notebody"] = Intents.Entities.freenotebody[0].ToString().Trim('\"');
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

//        private async Task<DialogTurnResult> PromptNoteBodyStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            if (!((!stepContext.Values.ContainsKey("irnumber")) && String.IsNullOrEmpty(stepContext.Result.ToString())))
//            {

//                if (!stepContext.Values.ContainsKey("irnumber")) { stepContext.Values["irnumber"] = stepContext.Result.ToString(); }

//                if (!stepContext.Values.ContainsKey("notebody"))
//                {
//                    var prompt = MessageFactory.Text(IdiomGenerator.RequestNoteBody());
//                    return await stepContext.PromptAsync("notebodyprompt", new PromptOptions { Prompt = prompt }, cancellationToken);
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
//            if (!((!stepContext.Values.ContainsKey("notebody")) && String.IsNullOrEmpty(stepContext.Result.ToString())))
//            {
//                if (!stepContext.Values.ContainsKey("notebody")) {
//                    stepContext.Values["notebody"] = stepContext.Result.ToString();
//                }

//                if (!stepContext.Values.ContainsKey("skipconfirm"))
//                {

//                    // Run confirmation

//                    // Get required action from the original LUIS intent

//                    string confirmationPrompt = "";

//                    switch((ZombieIntents.Intent)stepContext.Values["intent"])
//                    {
//                        case ZombieIntents.Intent.Add_Note:
//                            confirmationPrompt = "Add note '{0}' to '{1}'?";
//                            break;
//                        case ZombieIntents.Intent.Resolve:
//                            confirmationPrompt = "Resolve {1} with note '{0}'?";
//                            break;
//                    }

//                    return await stepContext.PromptAsync("add-note-or-resolve-confirm",
//                        new PromptOptions
//                        {
//                            Prompt = MessageFactory.Text(
//                                    String.Format(confirmationPrompt,
//                                    stepContext.Values["notebody"].ToString(),
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

//                    // Get required action from the original LUIS intent

//                    switch((ZombieIntents.Intent)stepContext.Values["intent"])
//                    {
//                        case ZombieIntents.Intent.Add_Note:
//                            await Functions.AddNote(stepContext.Values["irnumber"].ToString(), stepContext.Context.Activity.From.Name, stepContext.Values["notebody"].ToString());
//                            break;
//                        case ZombieIntents.Intent.Resolve:
//                            // I tried doing these two tasks in parallel but this fails, possible due collision updating the same incident..
//                            //Task t = Task.WhenAll(new List<Task>() {
//                            //    ScribeBot.Functions.AddNote(stepContext.Values["irnumber"].ToString(), stepContext.Context.Activity.From.Name, stepContext.Values["notebody"].ToString()),
//                            //    ScribeBot.Functions.Resolve(stepContext.Values["irnumber"].ToString(), stepContext.Context.Activity.From.Name, stepContext.Values["notebody"].ToString())
//                            //});
//                            //await t;
//                            await Functions.AddNote(stepContext.Values["irnumber"].ToString(), stepContext.Context.Activity.From.Name, stepContext.Values["notebody"].ToString());
//                            await Functions.Resolve(stepContext.Values["irnumber"].ToString(), stepContext.Context.Activity.From.Name, stepContext.Values["notebody"].ToString());
//                            break;
//                    }

//                    await stepContext.Context.SendWithRetry(IdiomGenerator.TaskCompletedAffirmative, cancellationToken);
//                    stepContext.Context.TurnState["irnumber"] = stepContext.Values["irnumber"].ToString();
//                    return await stepContext.BeginDialogAsync("show-ticket");
//                }
//                catch
//                {
//                    await stepContext.Context.SendWithRetry("Error",  cancellationToken);

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
