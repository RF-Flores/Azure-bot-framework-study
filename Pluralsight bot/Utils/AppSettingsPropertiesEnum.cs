using System.ComponentModel;

namespace Pluralsight_bot.Utils
{
    //This enum includes all of the properties that should be present in appsettings.json file
    public enum AppSettingsPropertiesEnum
    {
        [Description("AzureBlobStorageAccount")]
        AzureBlobStorageAccount,
        [Description("LuisAPIHostName")]
        LuisAPIHostName,
        [Description("LuisAPIKey")]
        LuisAPIKey,
        [Description("LuisAppId")]
        LuisAppId,
        [Description("LuisSlot")]
        LuisSlot
    }
}