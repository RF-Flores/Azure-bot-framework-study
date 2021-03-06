using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Pluralsight_bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pluralsight_bot.Services
{
    public class StateService
    {
        #region Variables
        // State variables
        public UserState UserState { get; }
        public ConversationState ConversationState { get; }

        //ID's
        public static string UserProfileId { get; } = $"{nameof(StateService)}.UserProfile";
        public static string ConversationDataId { get; } = $"{nameof(StateService)}.ConversationData";
        public static string DialogStateId { get; } = $"{nameof(StateService)}.DialogState";

        //Accessors
        public IStatePropertyAccessor<UserProfile> UserProfileAcessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAcessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
        #endregion

        public StateService(UserState userState, ConversationState conversationState) 
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));

            InitializeAccessors();
        }

        public void InitializeAccessors()
        {
            //Initialize ConversationData
            ConversationDataAcessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);

            //Initialize UserState
            UserProfileAcessor = UserState.CreateProperty<UserProfile>(UserProfileId);

            //Intialize DialogState
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);
        }

    }
}
