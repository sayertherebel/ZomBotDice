using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZomBotDice.Helpers;

namespace ZomBotDice.Models
{

    public class PlayerState
    {
        public ConversationReference cref { get; set; }
        public bool isPlaying { get; set; }
        public int score { get; set; }
        public string displayName { get; set; }
        public bool isCreator { get; set; }
        public string id { get; set; }

        public bool finalPlayer { get; set; }
        

        public PlayerState(ConversationReference cref, string displayName, bool isPlaying,bool isCreator)
        {
            this.cref = cref;
            this.id = cref.User.Id;
            this.displayName = displayName;
            this.isPlaying = isPlaying;
            this.isCreator = isCreator;
            this.finalPlayer = false;
            score = 0;
        }

    }
    public class GameState
    {
        public List<PlayerState> PlayerStates;
        public ZombieRound Round;
        public string GameID;
        private int currentPlayerIndex;
        public bool endGame { get; set; }
        public PlayerState CurrentPlayer { get { return PlayerStates[currentPlayerIndex]; } }
        public bool Started { get; set; }

        public bool IsAbdandoned { get; set; }

        public GameState(string id)
        {
            PlayerStates = new List<PlayerState>();
            Round = new ZombieRound();
            GameID = id;
            currentPlayerIndex = 0;
            Started = false;
        }

        public void Reset()
        {
            currentPlayerIndex = 0;
            Round = new ZombieRound();
            Started = false;
            foreach (PlayerState player in PlayerStates)
            {
                player.score = 0;
                player.finalPlayer = false;
                player.isPlaying = true;
            }
                    
        }

        public bool IsCreator(ConversationReference player)
        {
            return player.User.Id == PlayerStates.Where(x => x.isCreator).First().id;
        }

        public bool IsCurrentPlayer(ConversationReference player)
        {
            return player.User.Id == PlayerStates[currentPlayerIndex].id;
        }

        public string WhosTurn()
        {
            return PlayerStates[currentPlayerIndex].displayName;
        }

        public void NextPlayer()
        {
            if(currentPlayerIndex+1 == PlayerStates.Count)
            {
                currentPlayerIndex = 0;
            }
            else
            {
                currentPlayerIndex++;
            }

            if(!CurrentPlayer.isPlaying) { NextPlayer(); } // Skip over inactive players

            Round = new ZombieRound();
        }

        
    }



    public class GameStateHandler
    {
        private List<GameState> gameStates = new List<GameState>();
        private IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        public GameStateHandler(IBotFrameworkHttpAdapter adapter, IConfiguration configuration)
        {
            _adapter = adapter;
            _appId = configuration["MicrosoftAppId"];
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }

        }


        public async Task<string> NewGame(ConversationReference starterConversationReference, string starterDisplayName)
        {
            string newGameId = (Guid.NewGuid().ToString().Substring(0, 4));
            GameState newGame = new GameState(newGameId);
            newGame.PlayerStates.Add(new PlayerState(starterConversationReference, starterDisplayName, true, true));
            gameStates.Add(newGame);


            AdaptiveCard resultCard = await AdaptiveCardBuilders.TitleCard();

            var reply = MessageFactory.Attachment(new List<Attachment>());
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = resultCard
            };
            reply.Attachments.Add(attachment);

            await DoNotify(starterConversationReference, reply);

            return newGameId;
        }

        public async Task Joiner(string gameid, ConversationReference cref, string displayName)
        {

            var game = gameStates.Where(x => x.GameID.Equals(gameid)).FirstOrDefault();

            if (!game.IsAbdandoned)
            {

                AdaptiveCard resultCard = await AdaptiveCardBuilders.TitleCard();

                var reply = MessageFactory.Attachment(new List<Attachment>());
                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = resultCard
                };
                reply.Attachments.Add(attachment);

                await DoNotify(cref, reply);

                if (game.Started)
                {
                    game.PlayerStates.Add(new PlayerState(cref, displayName, false, false));

                    foreach (PlayerState player in game.PlayerStates)
                    {
                        await DoNotify(player.cref, MessageFactory.Text($"{displayName} has joined as watcher. They'll be able to play in the next game."));
                    }
                }
                else
                {
                    game.PlayerStates.Add(new PlayerState(cref, displayName, true, false));

                    foreach (PlayerState player in game.PlayerStates)
                    {
                        await DoNotify(player.cref, MessageFactory.Text($"{displayName} has joined."));
                    }
                }
            }
            else
            {
                await DoNotify(cref, MessageFactory.Text("This game has been abandoned 😪 Please start or join another.. )"));
            }


        }

        public async Task UserQuits(string gameid, ConversationReference cref)
        {

            var game = gameStates.Where(x => x.GameID.Equals(gameid)).FirstOrDefault();

            
            var playerToRemove = game.PlayerStates.Where(x => x.id.Equals(cref.User.Id.ToString())).FirstOrDefault();

            game.PlayerStates.Remove(playerToRemove);

            foreach (PlayerState player in game.PlayerStates)
            {
                await DoNotify(player.cref, MessageFactory.Text($"{playerToRemove.displayName} has left."));
            }

        }

        public async Task AbandonGame(string gameid)
        {

            var game = gameStates.Where(x => x.GameID.Equals(gameid)).FirstOrDefault();

            game.IsAbdandoned = true;

            foreach (PlayerState player in game.PlayerStates)
            {
                await DoNotify(player.cref, MessageFactory.Text($"Game has been adandoned 😪 Join or create a new one.."));
            }

        }



        public async Task Begin(string gameid)
        {

            var game = gameStates.Where(x => x.GameID.Equals(gameid)).FirstOrDefault();

            foreach (PlayerState player in game.PlayerStates)
            {
                await DoNotify(player.cref, MessageFactory.Text("The game begins.."));
            }

            game.Started = true;
            await DoNotify(game.CurrentPlayer.cref, MessageFactory.Text("It's your turn. Roll damnit!"));

        }

        public async Task Roll(string gameid)
        {

            var game = gameStates.Where(x => x.GameID.Equals(gameid)).FirstOrDefault();

            if (!game.IsAbdandoned)
            {

                RollResult result = game.Round.Roll();
                result.brainsBanked = game.CurrentPlayer.score;
                result.playerdisplayname = game.CurrentPlayer.displayName;

                AdaptiveCard resultCard = await AdaptiveCardBuilders.FromResult(result);

                var reply = MessageFactory.Attachment(new List<Attachment>());
                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = resultCard
                };
                reply.Attachments.Add(attachment);


                foreach (PlayerState player in game.PlayerStates)
                {
                    await DoNotify(player.cref, reply);
                }

                if (!result.isDead)
                {
                    await DoNotify(game.CurrentPlayer.cref, MessageFactory.Text("It's your turn. roll or stick?"));
                }
                else
                {
                    await DoNotify(game.CurrentPlayer.cref, MessageFactory.Text("Bang, you're dead! "));

                    foreach (PlayerState player in game.PlayerStates.Where(x => x.id != game.CurrentPlayer.id))
                    {
                        await DoNotify(player.cref, MessageFactory.Text($"{player.displayName} died."));
                    }

                    game.NextPlayer();

                    if (game.endGame && game.CurrentPlayer.finalPlayer)
                    {
                        await ScoreGame(game);
                    }
                    else
                    {
                        await DoNotify(game.CurrentPlayer.cref, MessageFactory.Text("It's your turn. Roll damnit! "));
                    }


                }
            
        }
            else
            {
                await DoNotify(game.CurrentPlayer.cref, MessageFactory.Text("This game has been abandoned 😪 Please start or join another.. )"));
            }


}


        public async Task Stick(string gameid)
        {

            var game = gameStates.Where(x => x.GameID.Equals(gameid)).FirstOrDefault();

            game.CurrentPlayer.score = game.CurrentPlayer.score + game.Round.brainsWon;

            if (game.CurrentPlayer.score > 13 && game.endGame == false)
            {
                game.endGame = true;
                game.CurrentPlayer.finalPlayer = true;

                foreach (PlayerState player in game.PlayerStates)
                {
                    await DoNotify(player.cref, MessageFactory.Text($"{game.CurrentPlayer.displayName} is sticking on {game.CurrentPlayer.score}, one last round!"));
                }
            }
            else
            {
                foreach (PlayerState player in game.PlayerStates)
                {
                    await DoNotify(player.cref, MessageFactory.Text($"{game.CurrentPlayer.displayName} is sticking on {game.CurrentPlayer.score}."));
                }
            }

            game.NextPlayer();

            if (game.endGame && game.CurrentPlayer.finalPlayer)
            {
                await ScoreGame(game);
            }
            else
            {
                await DoNotify(game.CurrentPlayer.cref, MessageFactory.Text("It's your turn. Roll damnit! "));
            }



        }

        public async Task ScoreGame(GameState game)
        {
            PlayerState winner = game.PlayerStates.OrderByDescending(x => x.score).First();

            foreach (PlayerState player in game.PlayerStates)
            {
                await DoNotify(player.cref, MessageFactory.Text($"{winner.displayName} is the winner with {winner.score} brains!"));
               
            }

            game.Reset();

        }

        public GameState GetGameById(string id)
        {
            return gameStates.Where(x => x.GameID.Equals(id)).FirstOrDefault();
        }

        private async Task DoNotify(ConversationReference Reference, IMessageActivity Activity)
        {

            int maxRetry = 2;
            int retryCount = 0;

            while (true)
            {
                try
                {
                    await ((BotAdapter)_adapter)
                                    .ContinueConversationAsync(
                                        _appId,
                                        Reference,
                                        async (context, token) =>
                                        {
                                            await context.SendActivityAsync(Activity, token);
                                        },
                                        default(CancellationToken)
                                    );
                    return;
                }
                catch (Exception e)
                {
                    //_logger.LogError(e, "An error occurred while attempting to send a proactive message.");

                    if (++retryCount >= maxRetry)
                    {
                        throw e;
                    }
                }
            }
        }
    }
        
}
