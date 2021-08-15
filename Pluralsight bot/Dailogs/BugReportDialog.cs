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

namespace Pluralsight_bot.Dailogs
{
    public class BugReportDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        private readonly string _bugReportDialogNameOf = nameof(BugReportDialog);
        #endregion

        #region Constructors
        public BugReportDialog(string dialogId, StateService stateService) : base(dialogId) 
        {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));

            InitializeWaterfallDialog();
        }
        #endregion

        #region Methods
        private void InitializeWaterfallDialog()
        {
            //Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                DescriptionStepAsync,
                CallbackTimeStepAsync,
                PhoneNumberStepAsync,
                BugStepAsync,
                SummaryStepAsync
            };


            // Add Named Dialogs
            AddDialog(new WaterfallDialog(_bugReportDialogNameOf + ".mainFlow", waterfallSteps));
            AddDialog(new TextPrompt(_bugReportDialogNameOf + ".description"));
            AddDialog(new DateTimePrompt(_bugReportDialogNameOf + ".callbackTime", CallbackTimeValidatorAsync));
            AddDialog(new TextPrompt(_bugReportDialogNameOf + ".phoneNumber", PhoneNumberValidatorAsync));
            AddDialog(new ChoicePrompt(_bugReportDialogNameOf + ".bug"));

            //Set the starting Dialog
            InitialDialogId = _bugReportDialogNameOf + ".mainFlow";
        }

        #region Waterfall Steps
        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options;

            if (string.IsNullOrEmpty(userProfile.Description))
            {
                return await stepContext.PromptAsync(_bugReportDialogNameOf + ".description",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Enter a description for your report")
                    }, 
                    cancellationToken);
            }

            return await stepContext.NextAsync(userProfile.Description, cancellationToken);
        }

        private async Task<DialogTurnResult> CallbackTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options;

            stepContext.Values["description"] = (string)stepContext.Result;

            if (userProfile.CallbackTime == null)
            {
                return await stepContext.PromptAsync(_bugReportDialogNameOf + ".callbackTime",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please enter in a callback time"),
                        RetryPrompt = MessageFactory.Text("The value entered must be between the hours of 9 am and 5 pm.")
                    }, cancellationToken);
            }

            return await stepContext.NextAsync(userProfile.CallbackTime, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options;
            if (stepContext.Result is DateTime time)
            {
                stepContext.Values["callbackTime"] = time;
            }
            else
            {
                var result = (List<DateTimeResolution>)stepContext.Result;
                stepContext.Values["callbackTime"] = result?.FirstOrDefault() != null ? Convert.ToDateTime(result?.FirstOrDefault().Value) : null;
            }

            if (string.IsNullOrEmpty(userProfile.PhoneNumber))
            {
                return await stepContext.PromptAsync(_bugReportDialogNameOf + ".phoneNumber",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please enter in a phone number that we can call you back at"),
                        RetryPrompt = MessageFactory.Text("Please enter a valid phone number")
                    }, cancellationToken);
            }

            return await stepContext.NextAsync(userProfile.PhoneNumber, cancellationToken);
        }

        private async Task<DialogTurnResult> BugStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Options;

            stepContext.Values["phoneNumber"] = (string)stepContext.Result;

            if (string.IsNullOrEmpty(userProfile.Bug))
            {
                return await stepContext.PromptAsync(_bugReportDialogNameOf + ".bug",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please enter the type of bug."),
                        Choices = ChoiceFactory.ToChoices(new List<string> { "Security", "Crash", "Power", "Performance", "Usability", "Serious Bug", "Other" })
                    },cancellationToken);
            }

            return await stepContext.NextAsync(userProfile.Bug, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
        {
            if (stepContext.Result is string bug)
            {
                stepContext.Values["bug"] = bug;
            }
            else
            {
                stepContext.Values["bug"] = ((FoundChoice)stepContext.Result).Value;
            }

            // Get the current profile object from user state.
            var userProfile = await _stateService.UserProfileAcessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // Save all the data inside the user profile
            userProfile.Description = (string)stepContext.Values["description"];
            userProfile.CallbackTime = (DateTime)stepContext.Values["callbackTime"];
            userProfile.PhoneNumber = (string)stepContext.Values["phoneNumber"];
            userProfile.Bug = (string)stepContext.Values["bug"];

            //Show the summary to user
            var stepContextAnswersList = new String[]
            {
                userProfile.Description,//0
                userProfile.CallbackTime.ToString("HH,mm"),//1
                userProfile.PhoneNumber,//2
                userProfile.Bug,//3
                Environment.NewLine//4
            };

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format(
                "Here is a summary of your bug report: {4}Description: {0}{4}Callback Time: {1}{4}Phone Number: {2}{4}Bug: {3}"
                , stepContextAnswersList)), cancellationToken);

            //Save data in userState
            await _stateService.UserProfileAcessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

            //WaterfallStep always finishes with the end of the Waterfall or with another dialog
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion

        #region Validators
        private Task<bool> CallbackTimeValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if(promptContext.Recognized.Succeeded)
            {
                var resolution = promptContext.Recognized.Value.First();
                DateTime selectedDate = Convert.ToDateTime(resolution.Value);
                TimeSpan start = new TimeSpan(9, 0, 0); //9AM
                TimeSpan end = new TimeSpan(17, 0, 0); //5PM
                if((selectedDate.TimeOfDay >= start) && (selectedDate.TimeOfDay <= end))
                {
                    valid = true;
                }
            }
            return Task.FromResult(valid);
        }

        private Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellation)
        {
            var valid = false;

            if(promptContext.Recognized.Succeeded)
            {
                valid = Regex.Match(promptContext.Recognized.Value, @"^((\+\d{1,3}(-| )?\(?\d\)?(-| )?\d{1,5})|(\(?\d{2,6}\)?))(-| )?(\d{3,4})(-| )?(\d{4})(( x| ext)\d{1,5}){0,1}$").Success;
            }
            return Task.FromResult(valid);
        }
        #endregion

        #endregion
    }
}
