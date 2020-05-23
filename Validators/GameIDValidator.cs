using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using ZomBotDice.Helpers;
using ZomBotDice.Models;

namespace ZomBotDice.Validators
{
    public class GameIDValidator 
    {

        private readonly GameStateHandler GameState;

        public GameIDValidator(GameStateHandler _gameState)
        {
            GameState = _gameState;
        }

        public async Task<Tuple<bool, string>> Validate(string value, ITurnContext turnContext = null)
        {
            string pattern = @"(.{4})";
            Match m = Regex.Match(value, pattern);

            //Response isn't a valid IR number
            if (!m.Success)
            {
                return new Tuple<bool, string>(false, "Sorry, that doesn't look like a valid game id. Valid ones look like 'abcd'.");
            }

            var game = GameState.GetGameById(value.TrimStart('[').TrimEnd(']'));

            if (game == null)
            {
                return new Tuple<bool, string>(false, "That looks like a valid game id, but there isn't an active game matching it..");
            }
            else
            {

                
            }

            return new Tuple<bool, string>(true, game.GameID);
        }

        public async Task<bool> Validate(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // General prompt failure

            if (!promptContext.Recognized.Succeeded)
            {
                await promptContext.Context.SendWithRetry("Sorry, I didn't seem to get a valid response, please try again.", cancellationToken);
                return false;
            }



            Tuple<bool, string> validatorResult = await Validate(promptContext.Recognized.Value, promptContext.Context);

            if (!validatorResult.Item1)
            {
                await promptContext.Context.SendWithRetry(validatorResult.Item2, cancellationToken);
            }
            return validatorResult.Item1;

        }
    }
}