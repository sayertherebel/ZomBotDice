using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScribeBotV4;

namespace ZomBotDice.Models
{
    public static class IdiomGenerator
    {

        private static string _taskCompletedAffirmative()
        {
            var selector = new Random();
            var options = new[] {
                "All done 👍",
                "It is done. 🥳",
                "I have done it. 🐱‍🚀",
                "I've taken care of it. ✅",
                "All done! 🤓"
             };
            return options[selector.Next(0, options.Length)];
        }

        public static string TaskCompletedAffirmative => _taskCompletedAffirmative();


        public static string GetHelpMessage(string username)
        {
            string helpMessage = @"Hey <b>{0}</b>, I'm <b>ScribeBot.</b> You can ask me to: <br /><br />
<ul>
    <li><b>Show my tickets</b> to get a list of your assigned tickets </li>
    <li><b>Show IR123456</b> to display a ticket as an interactive card</li>
    <li><b>Resolve IR123456</b> to resolve a ticket</li>
    <li> <b>Add note IR123456</b> or,</li>
    <li> <b>Add note ""note body"" to IR123456</b> to add a note to a ticket </li>    
    <li> <b>Set first response IR123456</b> to set the first response flag of a ticket</li>
    <li> <b>Set await customer for IR123456</b> to set the status of a ticket to 'Await Customer'</li>
    <li><b>Place IR123456 on hold for a week</b> or,</li>
    <li><b>Place IR123456 on hold for 4 days</b> or,</li>
    <li><b>Place IR123456 on hold until 20th October 2019</b> to place a ticket on hold.</li>
    <li><b>Reassign IR123456 to Joe Bloggs</b> to reassign a ticket (partial names accepted.)</li>
</ul>
I understand some natural language so your syntax does not need to be exact 😎<br /><br /> Version: {1}";

            return String.Format(helpMessage, username, typeof(ZomBotDice.Controllers.BotController).Assembly.GetName().Version);

        }

        public static string Tip()
        {
            var selector = new Random();
            var options = new[] {
                "You can add a note in a number of ways. You can ask me to <b>Show IR123456</b> and within the Incident card, expand out the Actions and use the text box there.",
                "You can add a note in a number of ways. You can simply ask me to <b>Add note</b> and I'll then prompt for the IR number and note body.",
                @"You can add a note in a number of ways. You can ask me to <b>Add note ""notebody"" to IR123456</b> assuming I can find the ticket with the given reference 😀"
            };

            return options[selector.Next(0, options.Length)];
        }

        public static string TaskCancelled()
        {
            var selector = new Random();
            var options = new[] {
                "OK, let me know if there's anything else I can help with.",
                "No trouble, let me know if there's anything else I can help with."
            };

            return options[selector.Next(0, options.Length)];
        }

        public static string AcknowledgeThanks()
        {
            var selector = new Random();
            var options = new[] {
                "I'm here to help 🐱‍🏍",
                "You are very welcome 🤠",
                "No trouble 😊",
                "No trouble, let me know if there's anything else I can help with. 🤓"
            };

            return options[selector.Next(0, options.Length)];
        }

        public static string TomSpecial()
        {
            var selector = new Random();
            var options = new[] {
                "Fuck off Tom.",
                "Haven't you given up on IT yet?",
                "How about you fix the fucking WiFi.",
                "Come back when you can pronounce router.",
                "Where's Chris when you need him?",
                "Get a fucking haircut.",
                "IT Engineers are like buses. There's always one that's breaking down and smells of tramp's piss."
            };

            return options[selector.Next(0, options.Length)];
        }


        public static string Greeting(string name)
        {
            var selector = new Random();
            var options = new[] {
                "Hi {0} 😀 Type 'new game' or 'join game' to play"
            };

            return String.Format(options[selector.Next(0, options.Length)], name);
        }

        public static string SubscriptionAdded(string name)
        {
            var selector = new Random();
            var options = new[] {
                "I've subscribed you to the unassigned tickets notification {0}."
            };

            return String.Format(options[selector.Next(0, options.Length)], name);
        }

        public static string SubscriptionRemoved(string name)
        {
            var selector = new Random();
            var options = new[] {
                "I've unsubscribed you from the unassigned tickets notification {0}."
            };

            return String.Format(options[selector.Next(0, options.Length)], name);
        }

        public static string RequestResolutionNote()
        {
            var selector = new Random();
            var options = new[] {
                "Sure, what Resolution Note should I add? 📝",
                "No trouble, what should I put as the Resolution Note? 📋",
                "No trouble, what Resolution Note should I add? ✏",
                "Sure, what should I put as the Resolution Note? 🖋"
            };

            return options[selector.Next(0, options.Length)];
        }

        public static string RequestNoteBody()
        {
            var selector = new Random();
            var options = new[] {
                "Sure, what should the Note say? 📝",
                "No trouble, what should I put in the Note? 📋",
                "No trouble, what Note should I add? ✏",
                "Sure, what should I put as the Note? 🖋"
            };

            return options[selector.Next(0, options.Length)];
        }

        public static string RequestAssignee()
        {
            var selector = new Random();
            var options = new[] {
                "Sure, who should the ticket be reassigned to? 🤷‍♂️",
                "No trouble, who should the ticket be reassigned to? 🤷‍♀️",
                "No trouble, who's the lucky person? 🙋‍♂️",
                "Sure, who's the lucky person? 🙋‍♀️"
            };

            return options[selector.Next(0, options.Length)];
        }

        public static string RequestOnHoldDate()
        {
            var selector = new Random();
            var options = new[] {
                "Sure, what should the new old date be? 📝"
            };

            return options[selector.Next(0, options.Length)];
        }

        public static string RequestIRNumber()
        {
            var selector = new Random();
            var options = new[] {
                "What's the ticket IR number? 🤓",
                "What's the IR number of the ticket to be updated? 🔢"
            };

            return options[selector.Next(0, options.Length)];
        }
    }
}