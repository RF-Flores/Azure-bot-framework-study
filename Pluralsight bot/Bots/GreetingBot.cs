using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Pluralsight_bot.Models;
using Pluralsight_bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pluralsight_bot.Bots
{
    public class GreetingBot : ActivityHandler
    {

        #region Variables
        private readonly StateService _stateService;

        #endregion

        #region Constructors
        public GreetingBot(StateService stateService)
        {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        }
        #endregion

        private async Task GetName(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAcessor.GetAsync(turnContext, () => new UserProfile());
            ConversationData conversationData = await _stateService.ConversationDataAcessor.GetAsync(turnContext, () => new ConversationData());
            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(String.Format("Hi {0}. How can I help you today?", userProfile.Name)), cancellationToken);
            }
            else
            {
                if (conversationData.PromptedUserForName)
                {
                    //Set the name to what the user provided
                    userProfile.Name = turnContext.Activity.Text.Trim();

                    //Acknowledge we got their name.
                    await turnContext.SendActivityAsync(MessageFactory.Text(String.Format("Hi {0}. How can I help you today?", userProfile.Name)), cancellationToken);

                    //Reset the flag to allow the bot to cycle again.
                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    //Prompt the user for their name.
                    await turnContext.SendActivityAsync(MessageFactory.Text($"What is your name?"), cancellationToken);

                    //Set the flag to true, so we dont prompt in the next trun
                    conversationData.PromptedUserForName = true;
                }
                await _stateService.UserProfileAcessor.SetAsync(turnContext, userProfile);
                await _stateService.ConversationDataAcessor.SetAsync(turnContext, conversationData);

                await _stateService.UserState.SaveChangesAsync(turnContext);
                await _stateService.ConversationState.SaveChangesAsync(turnContext);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetName(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetName(turnContext, cancellationToken);
                }
            }
        }


    }
}
