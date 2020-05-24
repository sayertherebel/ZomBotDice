using ZomBotDice.LUIS;
using ZomBotDice.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZomBotDice.Helpers;
using ZomBotDice.Models;

using System.Collections.Concurrent;
using System.Linq;

namespace ZomBotDice.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private readonly ScribeRecognizer _scribeRecognizer;
        protected readonly ILogger Logger;
        protected readonly ConversationState CState;
        protected readonly UserState UState;
        private ConcurrentDictionary<string,string> UnassigedTicketSubsribers;
        protected readonly GameStateHandler GameHandler;

        public RootDialog(ScribeRecognizer scribeRecognizer,
            ILogger<RootDialog> logger,
            ConversationState conversationState,
            UserState userState,
            GameStateHandler gameStateHandler,
            JoinGameDialog _joinGameDialog,
            NewGameDialog _newGameDialog,

            ConcurrentDictionary<string,string> _unassignedTicketSubscribers
            )
            : base(nameof(RootDialog))
        {
            _scribeRecognizer = scribeRecognizer;
            Logger = logger;
            CState = conversationState;
            UState = userState;
            UnassigedTicketSubsribers = _unassignedTicketSubscribers;
            GameHandler = gameStateHandler;

            AddDialog(new TextPrompt("emptyprompt"));
            AddDialog(_joinGameDialog);
            AddDialog(_newGameDialog);



            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {

                WaitForInput,
                UnderstandStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> WaitForInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(stepContext.Context.Activity.Text))
            {
                return await stepContext.PromptAsync("emptyprompt", new PromptOptions { }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync();
            }
        }


        private async Task<DialogTurnResult> UnderstandStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var luisResult = await _scribeRecognizer.RecognizeAsync<ZombieIntents>(stepContext.Context, cancellationToken);

            string user = "User";

            if (!String.IsNullOrEmpty(stepContext.Context.Activity.From.Name))
            {
                user = stepContext.Context.Activity.From.Name;
                string[] splitName = user.Split(' ');
                user = splitName[0];
            }

            var userStateAccessors = UState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());

            switch (luisResult.TopIntent().intent)
            {
                case ZombieIntents.Intent.New_Game:
                    return await stepContext.BeginDialogAsync("new-game", luisResult, cancellationToken);

                case ZombieIntents.Intent.Debug:                  
                                
                    var subscribersMessage2 = MessageFactory.Text($"User joined to {userProfile.gameid} ", null, InputHints.IgnoringInput);
                    await stepContext.Context.SendWithRetry(subscribersMessage2, cancellationToken);
                    break;
                case ZombieIntents.Intent.Whos_Turn:
                    if (!String.IsNullOrEmpty(userProfile.gameid))
                    {
                        var GameState = GameHandler.GetGameById(userProfile.gameid);
                        await stepContext.Context.SendWithRetry(MessageFactory.Text($"It's {GameState.WhosTurn()}'s go."), cancellationToken);
                    }
                        
                    break;
                case ZombieIntents.Intent.Begin_Game:
                    if(!String.IsNullOrEmpty(userProfile.gameid))
                    {
                        var GameState = GameHandler.GetGameById(userProfile.gameid);
                        if(GameState.IsCreator(stepContext.Context.Activity.GetConversationReference()))
                        {
                            await stepContext.Context.SendWithRetry(MessageFactory.Text("Let's do this thing!"), cancellationToken);
                            await GameHandler.Begin(GameState.GameID);
                        }
                        else
                        {
                            await stepContext.Context.SendWithRetry(MessageFactory.Text("Sorry bud, you're not the organiser."), cancellationToken);
                        }
                    }
                    else
                    {
                        await stepContext.Context.SendWithRetry(MessageFactory.Text("You're not joined to a game.. You can start a new one or join with a code.."), cancellationToken);
                    }
                    break;

                case ZombieIntents.Intent.Roll:
                    if (!String.IsNullOrEmpty(userProfile.gameid))
                    {
                        var GameState = GameHandler.GetGameById(userProfile.gameid);
                        if (GameState.IsCurrentPlayer(stepContext.Context.Activity.GetConversationReference()))
                        {
                            await GameHandler.Roll(GameState.GameID);
                        }
                        else
                        {
                            await stepContext.Context.SendWithRetry(MessageFactory.Text($"Sorry bud, it's not your turn.. {GameState.CurrentPlayer.displayName}'s up"), cancellationToken);
                        }
                    }
                    else
                    {
                        await stepContext.Context.SendWithRetry(MessageFactory.Text("You're not joined to a game.. You can start a new one or join with a code.."), cancellationToken);
                    }
                    break;

                case ZombieIntents.Intent.Quit:
                    if (!String.IsNullOrEmpty(userProfile.gameid))
                    {
                        var GameState = GameHandler.GetGameById(userProfile.gameid);

                        if (GameState.IsCreator(stepContext.Context.Activity.GetConversationReference()))
                        {
                            await GameHandler.AbandonGame(GameState.GameID);
                        }
                        else
                        {
                            if (!GameState.Started)
                            {
                                await GameHandler.UserQuits(GameState.GameID, stepContext.Context.Activity.GetConversationReference());
                            }
                            else
                            {
                                await stepContext.Context.SendWithRetry(MessageFactory.Text($"You can't quit now, the game's running.."), cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        await stepContext.Context.SendWithRetry(MessageFactory.Text("You're not joined to a game.. You can start a new one or join with a code.."), cancellationToken);
                    }
                    break;
                case ZombieIntents.Intent.Stick:
                    if (!String.IsNullOrEmpty(userProfile.gameid))
                    {
                        var GameState = GameHandler.GetGameById(userProfile.gameid);
                        if (GameState.IsCurrentPlayer(stepContext.Context.Activity.GetConversationReference()))
                        {
                            await GameHandler.Stick(GameState.GameID);
                        }
                        else
                        {
                            await stepContext.Context.SendWithRetry(MessageFactory.Text($"Sorry bud, it's not your turn.. {GameState.CurrentPlayer.displayName}'s up"), cancellationToken);
                        }
                    }
                    else
                    {
                        await stepContext.Context.SendWithRetry(MessageFactory.Text("You're not joined to a game.. You can start a new one or join with a code.."), cancellationToken);
                    }
                    break;

                case ZombieIntents.Intent.Scores:
                    if (!String.IsNullOrEmpty(userProfile.gameid))
                    {
                        var GameState = GameHandler.GetGameById(userProfile.gameid);

                        string scoresList = "";

                        foreach (PlayerState player in GameState.PlayerStates)
                        {
                            scoresList = scoresList + ($"{player.displayName} has {player.score}, ");
                        }
                        await stepContext.Context.SendWithRetry(MessageFactory.Text(scoresList), cancellationToken);

                    }
                    else
                    {
                        await stepContext.Context.SendWithRetry(MessageFactory.Text("You're not joined to a game.. You can start a new one or join with a code.."), cancellationToken);
                    }
                    break;

                case ZombieIntents.Intent.Join_Game:
                    return await stepContext.BeginDialogAsync("join-game", luisResult, cancellationToken);


                default:
                    var greetingMessage2 = MessageFactory.Text(IdiomGenerator.Greeting(user), null, InputHints.IgnoringInput);
                    await stepContext.Context.SendWithRetry(greetingMessage2, cancellationToken);
                    break;
            }

            return await ResetHelper.Reset(stepContext);

        }


    }
}
