using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Pluralsight_bot.Services;
using Pluralsight_bot.Models;

namespace Pluralsight_bot.Dailogs
{
    public class GreetingDialog : ComponentDialog
    {

        #region Variables
        private readonly StateService _botStateService;
        #endregion

        #region Constructors
        public GreetingDialog(string dialogId, StateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }
        #endregion

        private void InitializeWaterfallDialog()
        {
            //Create Waterfall Steps
            var waterFallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            //Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterFallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            //Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }
    
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfle = await _botStateService.UserProfileAcessor.GetAsync(stepContext.Context, () => new UserProfile());

            if (string.IsNullOrEmpty(userProfle.Name))
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name", new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is your name?")
                }, cancellationToken);
                    
            } else
            {
                return await stepContext.NextAsync(null,cancellationToken);
            }

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAcessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                //Set the user name gathered before
                userProfile.Name = (string)stepContext.Result;

                //Save the state changes that might have occured during the turn
                await _botStateService.UserProfileAcessor.SetAsync(stepContext.Context, userProfile);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Hi {0}. How can I help you today?", userProfile.Name)), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    
    }
}
