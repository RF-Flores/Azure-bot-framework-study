using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Pluralsight_bot.Models;
using Pluralsight_bot.Services;

namespace Pluralsight_bot.Dailogs
{
    public class BugTypeDialog : ComponentDialog
    {
        #region Variables
        private readonly BotServices _botServices;
        private readonly string _bugTypeDialogNameOf = nameof(BugTypeDialog);
        #endregion

        #region Constructors
        public BugTypeDialog(string dialogId, BotServices botServices) : base(dialogId)
        {
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }
        #endregion

        #region Methods
        private void InitializeWaterfallDialog()
        {
            //Create the Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            //Add Named Dialogs
            AddDialog(new WaterfallDialog(_bugTypeDialogNameOf + ".mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = _bugTypeDialogNameOf + ".mainFlow";


        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancelationToken)
        {
            var result = await _botServices.Dispatch.RecognizeAsync<LuisModel>(stepContext.Context, cancelationToken);
            var value = string.Empty;
            var bugOuter = result.Entities.BugTypes_List?.FirstOrDefault();
            if(bugOuter != null)
            {
                value = bugOuter?.FirstOrDefault() != null ? bugOuter?.FirstOrDefault() : value;
            }
            if (Common.BugTypes.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Yes! {value} is a Bug Type!"), cancelationToken);
            } 
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"No that is not a bug type"), cancelationToken);
            }

            return await stepContext.NextAsync(null, cancelationToken);
        }
        #endregion

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
