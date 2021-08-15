﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
namespace Pluralsight_bot.Models
{
    public partial class LuisModel : IRecognizerConvert
    {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("alteredText")]
        public string AlteredText;

        public enum Intent
        {
            GreetingIntent,
            NewBugReportIntent,
            None,
            QueryBugTypeIntent
        };
        [JsonProperty("intents")]
        public Dictionary<Intent, IntentScore> Intents;

        public class _Entities
        {
            // Built-in entities
            public DateTimeSpec[] datetime;
            public string[] phonenumber;

            // Lists
            public string[][] BugTypes_List;


            // Composites
            public class _InstanceDescription
            {
                public InstanceData[] Bug;
            }
            public class DescriptionClass
            {
                public string[] Bug;
                [JsonProperty("$instance")]
                public _InstanceDescription _instance;
            }
            public DescriptionClass[] Description;

            public class _InstanceBugReport_ML
            {
                public InstanceData[] CallbackTime;
                public InstanceData[] PhoneNumber;
                public InstanceData[] Description;
            }
            public class BugReport_MLClass
            {
                public string[] CallbackTime;
                public string[] PhoneNumber;
                public DescriptionClass[] Description;
                [JsonProperty("$instance")]
                public _InstanceBugReport_ML _instance;
            }
            public BugReport_MLClass[] BugReport_ML;

            // Instance
            public class _Instance
            {
                public InstanceData[] Bug;
                public InstanceData[] BugReport_ML;
                public InstanceData[] BugTypes_List;
                public InstanceData[] CallbackTime;
                public InstanceData[] Description;
                public InstanceData[] PhoneNumber;
                public InstanceData[] datetime;
                public InstanceData[] phonenumber;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;
        }
        [JsonProperty("entities")]
        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; }

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<LuisModel>(
                JsonConvert.SerializeObject(
                    result,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Error = OnError }
                )
            );
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        private static void OnError(object sender, ErrorEventArgs args)
        {
            // If needed, put your custom error logic here
            Console.WriteLine(args.ErrorContext.Error.Message);
            args.ErrorContext.Handled = true;
        }

        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}