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
    public class JoinGameDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
                protected readonly IIRNumberValidator IRNumberValidator;
        protected readonly ConversationState CState;
        protected readonly UserState UState;
        protected readonly GameStateHandler GameHandler;

        public JoinGameDialog(
            ILogger<JoinGameDialog> _logger,
            GameIDValidator _gameidvalidator,
            ConversationState _conversationState,
            UserState _userState,
            GameStateHandler _gameState

            )
            : base("join-game")
        {
            
            Logger = _logger;
            
            CState = _conversationState;
            UState = _userState;
            GameHandler = _gameState;
            AddDialog(new TextPrompt("gameidprompt", _gameidvalidator.Validate));
            AddDialog(new TextPrompt("nameprompt"));
            AddDialog(new WaterfallDialog("join-game-waterfall", new WaterfallStep[]
            {
                ParseIncomingFields,
                PromptGameIdStep,
                PromptNameStep,
                JoinGameStep

            }));

            InitialDialogId = "join-game-waterfall";
        }


        private async Task<DialogTurnResult> ParseIncomingFields(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            // Try from LUIS entities

            ZombieIntents Intents = null;

            try
            {
                Intents = (ZombieIntents)stepContext.Options;
            }
            catch { }

            if (Intents != null)
            {

                if (Intents.Entities != null)
                {
                    if (Intents.Entities.GameID != null)
                    {
                        if (!String.IsNullOrEmpty(Intents.Entities.GameID[0]))
                        {

                            stepContext.Values["gameid"] = Intents.Entities.GameID[0].TrimStart('[').TrimEnd(']');
                        }
                    }

                }
            }

            

            return await stepContext.NextAsync(null, cancellationToken);

        }
        private async Task<DialogTurnResult> PromptGameIdStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (!stepContext.Values.ContainsKey("gameid"))
            {
                var prompt = MessageFactory.Text("What's the GameID? (Or say Cancel)");
                var retryPromot = MessageFactory.Text("Please try again.");
                return await stepContext.PromptAsync("gameidprompt", new PromptOptions { Prompt = prompt, RetryPrompt = retryPromot }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync();
            }
        }

        private async Task<DialogTurnResult> PromptNameStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!stepContext.Values.ContainsKey("gameid")) { stepContext.Values["gameid"] = stepContext.Result.ToString().TrimStart('[').TrimEnd(']'); }

            var prompt = MessageFactory.Text("What's your player name? (Or say Cancel)");
                var retryPromot = MessageFactory.Text("Please try again.");
                return await stepContext.PromptAsync("nameprompt", new PromptOptions { Prompt = prompt, RetryPrompt = retryPromot }, cancellationToken);

        }

        private async Task<DialogTurnResult> JoinGameStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var userStateAccessors = UState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());

            var GameState = GameHandler.GetGameById(stepContext.Values["gameid"].ToString());

            if (GameState != null)
            {
                await GameHandler.Joiner(stepContext.Values["gameid"].ToString(), stepContext.Context.Activity.GetConversationReference(), stepContext.Result.ToString());
                userProfile.gameid = stepContext.Values["gameid"].ToString();
            }
            else
            {
                await stepContext.Context.SendWithRetry(MessageFactory.Text("No such game id 🙁"), cancellationToken);
            }

            return await ResetHelper.Reset(stepContext);

        }






    }
}
