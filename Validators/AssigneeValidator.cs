//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Dialogs;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using uk.co.silversands.servicemanager.api;
//using ZomBotDice.Helpers;

//namespace ZomBotDice.Validators
//{
//    public class AssigneeValidator : IAssigneeValidator
//    {

//        private readonly ClientFunctions Functions;

//        public AssigneeValidator(ClientFunctions _functions)
//        {
//            Functions = _functions;
//        }

//        public async Task<Tuple<bool, string>> Validate(string value, ITurnContext turnContext = null)
//        {

//            // Response isn't a valid IR number
//            if (String.IsNullOrEmpty(value))
//            {
//                return new Tuple<bool, string>(false, "Sorry, an empty assignee was passed.");
//            }

//            var assigneeResolvedName = await Functions.ResolveAssignee(value);

//            if (String.IsNullOrEmpty(assigneeResolvedName))
//            {
//                return new Tuple<bool, string>(false, "Sorry, I didn't find anyone matching that name 🤷‍");
//            }
//            else
//            {

//                if (turnContext != null)
//                {
//                    turnContext.TurnState["resolvedAssignee"] = assigneeResolvedName;
//                }
//            }

//            return new Tuple<bool, string>(true, assigneeResolvedName);
//        }

//        public async Task<bool> Validate(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
//        {
//            // General prompt failure

//            if (!promptContext.Recognized.Succeeded)
//            {
//                await promptContext.Context.SendWithRetry("Sorry, I didn't seem to get a valid response, please try again.", cancellationToken);
//                return false;
//            }



//            Tuple<bool, string> validatorResult = await Validate(promptContext.Recognized.Value, promptContext.Context);

//            if (!validatorResult.Item1)
//            {
//                await promptContext.Context.SendWithRetry(validatorResult.Item2, cancellationToken);
//            }
//            return validatorResult.Item1;

//        }
//    }
//}