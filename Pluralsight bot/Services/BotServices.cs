
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Pluralsight_bot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pluralsight_bot.Services
{
    public class BotServices
    {
        public BotServices(IConfiguration configuration)
        {
            //Read the setting for cognitive services (LUIS,QnA) from the appsettings.json
            var luisApplication = new LuisApplication(
                configuration[AppSettingsPropertiesEnum.LuisAppId.ToString()],
                configuration[AppSettingsPropertiesEnum.LuisAPIKey.ToString()],
                $"https://{configuration["LuisAPIHostname"]}.api.cognitive.microsoft.com");

            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            {
                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true,
                    Slot = configuration[AppSettingsPropertiesEnum.LuisSlot.ToString()]
                }
            };

            Dispatch = new LuisRecognizer(recognizerOptions);
        }

        public LuisRecognizer Dispatch { get; private set; }
    }
}
