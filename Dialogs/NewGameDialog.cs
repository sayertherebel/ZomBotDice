using AdaptiveCards;
using ZomBotDice.LUIS;
using ZomBotDice.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZomBotDice.Helpers;
using ZomBotDice.Models;
using ZomBotDice.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace ZomBotDice.Dialogs
{
    public class NewGameDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        protected readonly ConversationState CState;
        protected readonly UserState UState;
        protected readonly GameStateHandler GameHandler;

        public NewGameDialog(
            ILogger<JoinGameDialog> _logger,
            ConversationState _conversationState,
            UserState _userState,
            GameStateHandler _gameState

            )
            : base("new-game")
        {
            
            Logger = _logger;
            
            CState = _conversationState;
            UState = _userState;
            GameHandler = _gameState;
            AddDialog(new TextPrompt("nameprompt"));
            AddDialog(new WaterfallDialog("new-game-waterfall", new WaterfallStep[]
            {
                PromptNameStep,
                JoinGameStep

            }));

            InitialDialogId = "new-game-waterfall";
        }




        private async Task<DialogTurnResult> PromptNameStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var prompt = MessageFactory.Text("What's your player name?");
                var retryPromot = MessageFactory.Text("Please try again.");
                return await stepContext.PromptAsync("nameprompt", new PromptOptions { Prompt = prompt, RetryPrompt = retryPromot }, cancellationToken);

        }

        private async Task<DialogTurnResult> JoinGameStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var userStateAccessors = UState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());

            string GameId = await GameHandler.NewGame(stepContext.Context.Activity.GetConversationReference(), stepContext.Result.ToString());
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"New game with id [{GameId}] has begun :-)", null, InputHints.IgnoringInput), cancellationToken);
            userProfile.gameid = GameId; 
            
            return await ResetHelper.Reset(stepContext);

        }



        //private async Task<DialogTurnResult> ShowIncidentStep2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{

        //    // TODO when MS get their finger out and fix AdaptiveCards data bindings

        //    var incident = stepContext.Context.TurnState["incidentid"].ToString();

        //    var vmIncident = await Functions.GetIncident(incident.ToUpper());

        //    if (vmIncident == null || vmIncident.id == "")
        //    {
        //        var reply = MessageFactory.Text("Sorry, I didn't find a ticket with that IR reference 🤷‍♂️");
        //        await stepContext.Context.SendWithRetry(reply, cancellationToken);
        //    }
        //    else
        //    {

        //        var cardResourcePath = "ZomBotDice.Cards.incidentcard.json";

        //        using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
        //        {
        //            using (var reader = new StreamReader(stream))
        //            {
        //                var adaptiveCardJson = reader.ReadToEnd();

        //                var cleanedDesc = JsonConvert.ToString(vmIncident.description);

        //                adaptiveCardJson = adaptiveCardJson.Replace("{incident.description}", cleanedDesc);

        //                var card = AdaptiveCard.FromJson(adaptiveCardJson).Card;

        //                Attachment attachment = new Attachment()
        //                {
        //                    ContentType = AdaptiveCard.ContentType,
        //                    Content = card
        //                };

        //                // Pop the last note onto the waterfall context for the next step

        //                stepContext.Values["lastnote"] = vmIncident.lastnote;


        //                var reply = MessageFactory.Attachment(new List<Attachment>() { attachment });

        //                await stepContext.Context.SendWithRetry(reply, cancellationToken);
        //            }
        //        }

        //    }

        //    return await ResetHelper.Reset(stepContext);

        //}



    }
}
