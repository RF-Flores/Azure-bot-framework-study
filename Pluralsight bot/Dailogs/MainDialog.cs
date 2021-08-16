using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Pluralsight_bot.Services;
using Pluralsight_bot.Models;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Text.RegularExpressions;
using Pluralsight_bot.Helpers.PluralsightBot.Helpers;

namespace Pluralsight_bot.Dailogs
{
    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        private readonly string _mainDialogNameOf = nameof(MainDialog);
        private readonly BotServices _botService;
        #endregion

        #region Constructors
        public MainDialog(StateService stateService, BotServices botServices)
        {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
            _botService = botServices ?? throw new ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }
        #endregion

        #region Methods
        private void InitializeWaterfallDialog()
        {
            //Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Name Dialogs
            AddDialog(new GreetingDialog(_mainDialogNameOf + ".greeting", _stateService));
            AddDialog(new BugReportDialog(_mainDialogNameOf + ".bugReport", _stateService));
            AddDialog(new BugTypeDialog(_mainDialogNameOf + ".bugType", _botService));
            AddDialog(new WaterfallDialog(_mainDialogNameOf + ".mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = _mainDialogNameOf + ".mainFlow";
        }

        #region WaterFall steps
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                //First, we uuse the dispatch model to determine which cognitive server (LUIS or QnA) to use.
                var recognizerResult = await _botService.Dispatch.RecognizeAsync<LuisModel>(stepContext.Context, cancellationToken);

                //Top intent tell us which cognitive service to use.
                var topIntent = recognizerResult.TopIntent();

                switch (topIntent.intent)
                {
                    case LuisModel.Intent.GreetingIntent:
                        return await stepContext.BeginDialogAsync(_mainDialogNameOf + ".greeting", null, cancellationToken);
                    // For the bug report:
                    // First LUIS recognized all the elements from the chat, if its a bug report, bug type, phone, callback time
                    // Second it populates the user profile
                    // Third, call the bug report dialog in which if any field  is not populated, the bot will prompt the user for the details
                    // Finally it shows the summary to the user
                    case LuisModel.Intent.NewBugReportIntent:
                        var userProfile = new UserProfile();
                        var bugReport = recognizerResult.Entities.BugReport_ML?.FirstOrDefault();
                        if (bugReport != null)
                        {
                            var description = bugReport.Description?.FirstOrDefault();
                            if (description != null)
                            {
                                //Retrieve Description Text
                                userProfile.Description = bugReport._instance.Description?.FirstOrDefault() != null ? bugReport._instance.Description.FirstOrDefault().Text: userProfile.Description;

                                //Retrieve Bug Text
                                var bugOuter = description.Bug?.FirstOrDefault();
                                if (bugOuter != null)
                                {
                                    userProfile.Bug = bugOuter?.FirstOrDefault() != null ? bugOuter?.FirstOrDefault().ToString() : userProfile.Bug;
                                }
                            }

                            // Retrieve Phone Number Text
                            userProfile.PhoneNumber = bugReport.PhoneNumber?.FirstOrDefault() != null ? bugReport.PhoneNumber?.FirstOrDefault() : userProfile.PhoneNumber;

                            //Retrieve Callback Time
                            userProfile.CallbackTime = bugReport.CallbackTime?.FirstOrDefault() != null ? AiRecognizer.RecognizeDateTime(bugReport.CallbackTime?.FirstOrDefault(), out string rawString) : userProfile.CallbackTime;
                        }

                        return await stepContext.BeginDialogAsync(_mainDialogNameOf + ".bugReport", userProfile, cancellationToken);
                    case LuisModel.Intent.QueryBugTypeIntent:
                        return await stepContext.BeginDialogAsync(_mainDialogNameOf + ".bugType", null, cancellationToken);
                    default:
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry, I don't know what you mean."), cancellationToken);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        #endregion

        #endregion
    }
}
