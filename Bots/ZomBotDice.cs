// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using Newtonsoft.Json;
using ZomBotDice.Dialogs;
using ZomBotDice.Models;
using System.Collections.Concurrent;
using ZomBotDice.Helpers;

namespace ZomBotDice.Bots
{
    public class ZomBotDice<T> : ActivityHandler
         where T : Dialog
    {

        protected readonly Dialog RDialog;
        protected readonly ConversationState ConversationState ;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private ConcurrentDictionary<string,string> _unassignedTicketSubscribers;

        public ZomBotDice(ConversationState conversationState, UserState userState,
            ConcurrentDictionary<string, ConversationReference> ConversationReferences,
            ConcurrentDictionary<string,string> UnassignedTicketSubscribers, T rootDialog, ILogger<ZomBotDice<T>> logger)
        {
            ConversationState = conversationState;
            RDialog = rootDialog;
            UserState = userState;
            Logger = logger;
            _conversationReferences = ConversationReferences;
            _unassignedTicketSubscribers = UnassignedTicketSubscribers;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            if(conversationReference?.User?.Name != null)
            {
                _conversationReferences.AddOrUpdate(conversationReference.User.Name, conversationReference, (key, newValue) => conversationReference);
            }
            
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            AddConversationReference(turnContext.Activity as Activity);

            var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            if (String.IsNullOrEmpty(turnContext.Activity.Text))
            {
                //AddNoteOrResolveDialog addNoteDialog = new AddNoteOrResolveDialog("add-note", Logger, null);

                try
                {
                    var json = JsonConvert.SerializeObject(turnContext.Activity.Value);
                    //Assign the serialized value to the turn context's activity
                    turnContext.Activity.Text = json;
                }
                catch
                { }

            }

            if (turnContext.Activity.Text.ToUpper().Equals("CANCEL"))
            {
                await turnContext.SendWithRetry(MessageFactory.Text(IdiomGenerator.TaskCancelled()), cancellationToken);
                await ConversationState .ClearStateAsync(turnContext);
            }
            //else
            //{
            await RDialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            //}
        }
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {

                    await turnContext.SendWithRetry(MessageFactory.Text(Models.IdiomGenerator.Greeting(turnContext.Activity.From.Name)), cancellationToken);
                    await RDialog.RunAsync(turnContext, ConversationState .CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

                }
            }
        }

        //private async Task SendValueToDialogAsync(
        //    ITurnContext turnContext,
        //    CancellationToken cancellationToken)
        //{
        //    // Serialize value
        //    var json = JsonConvert.SerializeObject(turnContext.Activity.Value);
        //    // Assign the serialized value to the turn context's activity
        //    turnContext.Activity.Text = json;
        //    // Create a dialog context
        //    var dc = await _dialogSet.CreateContextAsync(
        //        turnContext, cancellationToken);
        //    // Continue the dialog with the modified activity
        //    await dc.ContinueDialogAsync(cancellationToken);
        //}
    }
}
