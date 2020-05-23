using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZomBotDice.LUIS
{
    public class ZombieIntents : IRecognizerConvert
    {
        public string Text;
        public string AlteredText;
        public enum Intent
        {
            New_Game,
            Join_Game,
            Roll,
            Stick,
            Quit,
            Begin_Game,
            Debug,
            Whos_Turn,
            Scores,
            None
        };
        public Dictionary<Intent, IntentScore> Intents;

        public class _Entities
        {

            // Built-in entities
            

            public class _InstanceIncident
            {
                public InstanceData[] Incident;
            }

            public class IncidentClass
            {
                public string incident;
                public string freenotebody;
                public string assignee;
                public DateTimeSpec[] datetime;

                [JsonProperty("$instance")]
                public _InstanceIncident _instance;
            }
            public string[] incident;
            public string[] freenotebody;
            public string[] assignee;
            public string[] GameID;

            public DateTimeSpec[] datetime;

            //Instance

            public class _Instance
            {
                public InstanceData[] incident;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;

        }
        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; }

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<ZombieIntents>(JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
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
