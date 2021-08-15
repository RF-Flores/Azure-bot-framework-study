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
    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        private readonly string _mainDialogNameOf = nameof(MainDialog);
        #endregion

        #region Constructors
        public MainDialog(StateService stateService)
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
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Name Dialogs
            AddDialog(new GreetingDialog(_mainDialogNameOf + ".greeting", _stateService));
            AddDialog(new BugReportDialog(_mainDialogNameOf + ".bugReport", _stateService));
            AddDialog(new WaterfallDialog(_mainDialogNameOf + ".mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = _mainDialogNameOf + ".mainFlow";
        }

        #region WaterFall steps
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "hi").Success)
            {
                return await stepContext.BeginDialogAsync(_mainDialogNameOf + ".greeting", null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(_mainDialogNameOf + ".bugReport", null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        #endregion

        #endregion
    }
}
