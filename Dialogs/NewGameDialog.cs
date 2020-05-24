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

            var prompt = MessageFactory.Text("What's your player name? (Or say Cancel)");
                var retryPromot = MessageFactory.Text("Please try again.");
                return await stepContext.PromptAsync("nameprompt", new PromptOptions { Prompt = prompt, RetryPrompt = retryPromot }, cancellationToken);

        }

        private async Task<DialogTurnResult> JoinGameStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var userStateAccessors = UState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());

            string GameId = await GameHandler.NewGame(stepContext.Context.Activity.GetConversationReference(), stepContext.Result.ToString());
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"New game with id {GameId} has been created 😎. Other players can now join, when you're ready to start say 'begin game.'", null, InputHints.IgnoringInput), cancellationToken);
            
            userProfile.gameid = GameId;

            var card = new HeroCard
            {
                Text = "When all the players have joined, begin the game.",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, title: "Begin", value: "Begin")
                },
            };

            var rollPrompt = MessageFactory.Attachment(card.ToAttachment());

            await stepContext.Context.SendActivityAsync(rollPrompt);

            return await ResetHelper.Reset(stepContext);

        }





    }
}
