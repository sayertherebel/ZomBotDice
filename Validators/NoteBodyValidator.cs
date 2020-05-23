using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZomBotDice.Helpers;

namespace ZomBotDice.Validators
{
    public class NoteBodyValidator : INotebodyValidator
    {

        public NoteBodyValidator()
        {
        }


        public async Task<Tuple<bool, string>> Validate(string value, ITurnContext context)
        {

            if (value.Length < 8)
            {
                return new Tuple<bool, string>(false, "Note body must be at least 8 characters.");
            }

            return new Tuple<bool, string>(true, "OK");
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