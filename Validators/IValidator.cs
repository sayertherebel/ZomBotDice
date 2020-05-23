using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ZomBotDice.Validators
{
    public interface IValidator
    {
        Task<bool> Validate(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken);
        Task<Tuple<bool, string>> Validate(string value, ITurnContext turnContext);
    }
}
