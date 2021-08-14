using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pluralsight_bot.Models
{
    public class ConversationData
    {
        // Track wether we have already asked the user's name
        public bool PromptedUserForName { get; set; } = false;

    }
}
