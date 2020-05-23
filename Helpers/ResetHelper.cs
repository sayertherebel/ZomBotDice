using Microsoft.Bot.Builder.Dialogs;
using ZomBotDice.Dialogs;
using System.Threading.Tasks;

namespace ZomBotDice.Helpers
{
    public static class ResetHelper
    {
        public static async Task<DialogTurnResult> Reset(WaterfallStepContext stepContext)
        {
            stepContext.Context.Activity.Text = "";
            stepContext.Values.Clear();
            return await stepContext.ReplaceDialogAsync(nameof(RootDialog));
        }
    }
}
