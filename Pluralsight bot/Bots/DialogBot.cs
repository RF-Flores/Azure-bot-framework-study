using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Pluralsight_bot.Services;
using Pluralsight_bot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pluralsight_bot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        #region Variables
        protected readonly Dialog _dialog;
        protected readonly StateService _stateService;
        protected readonly ILogger _logger;
        #endregion

        #region Constructors
        public DialogBot(StateService botStateService, T dialog, ILogger<DialogBot<T>> logger)
        {
            _stateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _dialog = dialog ?? throw new System.ArgumentNullException(nameof(dialog));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }
        #endregion

        #region Public methods
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save my state changes that might have occured during the turn
            await _stateService.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _stateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            //Run the dialog with the new message Activity.
            await _dialog.Run(turnContext, _stateService.DialogStateAccessor, cancellationToken);
        }



        #endregion




    }
}