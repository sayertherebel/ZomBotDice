using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZomBotDice.Models
{
    // POCO class for back channel intents
    public class ScribeAction
    {
        public string ID;
        public string action;
        public string comment;
        public string onholddate;
        public string msgid;
        public string newValue;
    }
}
