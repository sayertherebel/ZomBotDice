using AdaptiveCards;
using AdaptiveCards.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;


namespace ZomBotDice.Helpers
{
    public static class AdaptiveCardBuilders
    {

        public static async Task<AdaptiveCard> FromResult(ZomBotDice.Models.RollResult result)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ZomBotDice.Cards.zombiecard.json";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    AdaptiveCardTemplate template = new AdaptiveCardTemplate(json);

                    var viewObject = new
                    {
                        dice1image = $"{result.dice[0].rolledState}{result.dice[0].colour}",
                        dice2image = $"{result.dice[1].rolledState}{result.dice[1].colour}",
                        dice3image = $"{result.dice[2].rolledState}{result.dice[2].colour}",
                        brainsthisround = result.brainsAdded,
                        shotgunsthisround = result.shotguns,
                        bankedbrains = result.brainsBanked,
                        title = result.playerdisplayname
                    };

                    string expandedTemplate = template.Expand(viewObject);
                    AdaptiveCardParseResult cresult = AdaptiveCard.FromJson(expandedTemplate);

                    // Get card from result
                    AdaptiveCard card = cresult.Card;
                    return card;

                }
            }
            
            
        }

        public static async Task<AdaptiveCard> TitleCard()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ZomBotDice.Cards.zombietitle.json";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    AdaptiveCardParseResult cresult = AdaptiveCard.FromJson(json);

                    // Get card from result
                    AdaptiveCard card = cresult.Card;
                    return card;

                }
            }


        }

        //        public static async Task<AdaptiveCard> IncidentACFromVMIncident(VMIncident vmIncident, string msgid, string user)
        //        {

        //            var adaptiveCard = new AdaptiveCard("1.0");
        //            //Title
        //            var titleText = new AdaptiveTextBlock() { Weight = AdaptiveTextWeight.Bolder, Text = vmIncident.title, Spacing = AdaptiveSpacing.None };
        //            var titleIRNumber = new AdaptiveTextBlock() { Weight = AdaptiveTextWeight.Bolder, Text = vmIncident.id, Spacing = AdaptiveSpacing.None, Color = AdaptiveTextColor.Accent };


        //            var titleColums = new AdaptiveColumnSet()
        //            {

        //                Columns = new List<AdaptiveColumn>()
        //                        {
        //                            new AdaptiveColumn()
        //                            {
        //                                Spacing = AdaptiveSpacing.Padding,
        //                                Items = {titleText },
        //                                Width = AdaptiveColumnWidth.Stretch
        //                            },
        //                            new AdaptiveColumn()
        //                            {
        //                                Spacing = AdaptiveSpacing.Padding,
        //                                Items = { titleIRNumber },
        //                                Width = AdaptiveColumnWidth.Auto                            }
        //                        }
        //            };

        //            //var titleBannerContainer = new AdaptiveContainer()
        //            //{
        //            //    Style = AdaptiveContainerStyle.Emphasis,
        //            //    Bleed = true,
        //            //    Items = {new AdaptiveColumnSet()
        //            //        {

        //            //            Columns = new List<AdaptiveColumn>()
        //            //            {
        //            //                new AdaptiveColumn()
        //            //                {
        //            //                    Spacing = AdaptiveSpacing.Padding,
        //            //                    Items = {titleText },
        //            //                    Width = AdaptiveColumnWidth.Stretch
        //            //                },
        //            //                new AdaptiveColumn()
        //            //                {
        //            //                    Spacing = AdaptiveSpacing.Padding,
        //            //                    Items = { titleIRNumber },
        //            //                    Width = AdaptiveColumnWidth.Auto                            }
        //            //            }
        //            //        }
        //            //    }
        //            //};

        //            adaptiveCard.Body.Add(titleColums);

        //            //Image roundel

        //            //var heroImage = new AdaptiveImage() { Style = AdaptiveImageStyle.Person, Url = new System.Uri("https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg"), Size = AdaptiveImageSize.Small };
        //            var heroImage = new AdaptiveImage() { Style = AdaptiveImageStyle.Person, Url = new System.Uri(String.Format("https://smscribeapi.azurewebsites.net/SMUser/GetProfileImage/?name={0}&APIKey=87ab0f17-32da-4597-8aa4-e41cd750dd55", HttpUtility.UrlEncode(vmIncident.assignedToEmail))), Size = AdaptiveImageSize.Small };

        //            // Identities factset

        //            var identities = new AdaptiveFactSet();
        //            identities.Spacing = AdaptiveSpacing.Padding;

        //            string assignedToDisplayName = "";

        //            if (vmIncident.assignedTo != null)
        //            {
        //                if (vmIncident.assignedTo.Contains('@'))
        //                {
        //                    assignedToDisplayName = vmIncident.assignedTo.Split('@')[0].Replace(".", " ");
        //                }
        //                else
        //                {
        //                    assignedToDisplayName = vmIncident.assignedTo;
        //                }
        //            }

        //            identities.Facts = new List<AdaptiveFact>() { new AdaptiveFact() { Title = "Assigned to:", Value = assignedToDisplayName }, new AdaptiveFact() { Title = "Customer:", Value = vmIncident.affectedUser } };
        //            AdaptiveTextBlock description = null;
        //            // Description Textblock
        //            if (vmIncident.description.Length > 400)
        //            {
        //                description = new AdaptiveTextBlock() { Text = vmIncident.description, Wrap = true, Size = AdaptiveTextSize.Small, Spacing = AdaptiveSpacing.Small };
        //            }
        //            else
        //            {
        //                description = new AdaptiveTextBlock() { Text = vmIncident.description, Wrap = true };
        //            }


        //            // Details factset

        //            var details1 = new AdaptiveFactSet();
        //            details1.Facts = new List<AdaptiveFact>() { new AdaptiveFact() { Title = "Status:", Value = vmIncident.status } };
        //            if (!String.IsNullOrEmpty(vmIncident.firstResponseDate) && vmIncident.status.Equals("On Hold"))
        //            {
        //                details1.Facts.Add(new AdaptiveFact() { Title = "On Hold Date:", Value = vmIncident.onHoldDate });
        //            }

        //            //var details2 = new AdaptiveFactSet();
        //            details1.Facts.Add(new AdaptiveFact() { Title = "First Response:", Value = vmIncident.firstResponseDate });
        //            // Add Comment Action


        //            if ((!vmIncident.status.Equals("Resolved") && (!vmIncident.status.Equals("Closed"))))
        //            {

        //                var actions = new AdaptiveShowCardAction
        //                {
        //                    Type = "Action.ShowCard",
        //                    Title = "Actions",
        //                    Card = new AdaptiveCard("1.0")
        //                    {
        //                        Body = {
        //                        new AdaptiveTextInput() { Id = "comment", IsMultiline = true } },
        //                        Actions = {
        //                                new AdaptiveSubmitAction() {
        //                                    Title = "+Note",
        //                                    DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"addnote\", \"msgId\" : \"" + msgid+ "\"}")
        //                                },
        //                                new AdaptiveSubmitAction {
        //                                    Title = "!Resolve",
        //                                    DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"resolve\", \"msgId\" : \"" + msgid+ "\"}")
        //                                },
        //                                new AdaptiveSubmitAction {
        //                                    Title = ">Reassign",
        //                                    DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"reassign\", \"msgId\" : \"" + msgid+ "\"}")
        //                                }

        //                            }
        //                    }

        //                };

        //                if (vmIncident.firstResponseDate.Equals("(null)"))
        //                {
        //                    actions.Card.Actions.Add(
        //                        new AdaptiveSubmitAction
        //                        {
        //                            Title = "+Set First Response",
        //                            DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"sfr\", \"msgId\" : \"" + msgid + "\"}")
        //                        }
        //                    );
        //                }

        //                if (!vmIncident.status.Equals("Awaiting Customer"))

        //                {
        //                    actions.Card.Actions.Add(
        //                        new AdaptiveSubmitAction
        //                        {
        //                            Title = "!Set Await Customer",
        //                            DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"ac\", \"msgId\" : \"" + msgid + "\"}")
        //                        }
        //                    );
        //                }

        //                if (!vmIncident.status.Equals("Active"))
        //                {
        //                    {
        //                        actions.Card.Actions.Add(
        //                            new AdaptiveSubmitAction
        //                            {
        //                                Title = "!Set Active",
        //                                DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"sa\", \"msgId\" : \"" + msgid + "\"}")
        //                            }
        //                        );
        //                    }
        //                }

        //#if DEBUG

        //                if (!assignedToDisplayName.ToUpper().Equals(user.ToUpper()))
        //                {
        //                    actions.Card.Actions.Add(
        //                        new AdaptiveSubmitAction
        //                        {
        //                            Title = "!Assign to Me",
        //                            DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"reassign\", \"msgId\" : \"" + msgid + "\", \"newValue\" : \"" + "jamie.sayer@silversands.co.uk" + "\"}")
        //                        }
        //                    );
        //                }

        //#else
        //                if(!assignedToDisplayName.ToUpper().Equals(user.ToUpper()))
        //                {
        //                    //if(!user!.Contains("@"))
        //                    //{
        //                    //    user = user.Replace(" ", "@") + ("@silversands.co.uk");
        //                    //}
        //                    actions.Card.Actions.Add(
        //                        new AdaptiveSubmitAction
        //                        {
        //                            Title = "!Assign to Me",
        //                            DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"reassign\", \"msgId\" : \"" + msgid + "\", \"newValue\" : \"" + user + "\"}")
        //                        }
        //                    );
        //                }
        //#endif
        //                adaptiveCard.Actions.Add(actions);

        //            }

        //            string pattern = @"\n";

        //            Regex rPattern = new Regex(pattern);
        //            string sOut = rPattern.Replace(vmIncident.lastnote, "\n>");

        //            string lastnote = "";

        //            if (sOut.Length > 2)
        //            {
        //                lastnote = String.Format("Added by **{0}** at **{1}** \r\n\r\n {2}", vmIncident.lastNoteAddedBy, vmIncident.lastNoteDate, vmIncident.lastnote);
        //            }
        //            else
        //            {
        //                lastnote = String.Format("There aren't any notes on ticket **{0}** yet.", vmIncident.id);
        //            }

        //            var lastNoteSection = new AdaptiveShowCardAction
        //            {
        //                Type = "Action.ShowCard",
        //                Title = "Last Note",
        //                Card = new AdaptiveCard("1.0")
        //                {
        //                    Body = {
        //                            new AdaptiveTextBlock() { Id = "lastcomment", Text = lastnote, Wrap = true }
        //                        }
        //                }

        //            };

        //            var notesSection = new AdaptiveShowCardAction
        //            {
        //                Type = "Action.ShowCard",
        //                Title = (vmIncident.notes.Count > 2) ? "Last 3 Notes" : "Notes",
        //                Card = new AdaptiveCard("1.0")
        //            };

        //            bool isFirstNote = true;
        //            int noteCount = 1;
        //            if (vmIncident.notes.Count == 0)
        //            {
        //                notesSection.Card.Body.Add(new AdaptiveTextBlock { Text = "No notes on this ticket yet." });
        //            }

        //            foreach (VMNote note in vmIncident.notes)
        //            {
        //                notesSection.Card.Body.Add(await ACNoteColumn(note, noteCount, isFirstNote));
        //                isFirstNote = false;
        //                noteCount++;
        //            }

        //            adaptiveCard.Actions.Add(notesSection);

        //            adaptiveCard.Actions.Add(
        //                        new AdaptiveSubmitAction
        //                        {
        //                            Title = "🔁",
        //                            DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"show\", \"msgId\" : \"" + msgid + "\", \"newValue\" : \"" + user + "\"}")
        //                        }
        //                    );

        //            //var ResolveAction = new AdaptiveSubmitAction { Title = "!Resolve", DataJson = "{\"msteams\": {\"type\":\"messageBack\", \"displayText\":\"\",\"text\":\"Resolve IR123456\",\"value\":\"Resolve!\"}}" };

        //            // Build ColumnSets

        //            var columns1 = new AdaptiveColumnSet()
        //            {

        //                Columns = new List<AdaptiveColumn>()
        //                        {
        //                            new AdaptiveColumn()
        //                            {
        //                                Spacing = AdaptiveSpacing.Padding,
        //                                Items = {heroImage },
        //                                Width = AdaptiveColumnWidth.Auto
        //                            },
        //                            new AdaptiveColumn()
        //                            {
        //                                Spacing = AdaptiveSpacing.Padding,
        //                                Items = { identities },
        //                                Width = AdaptiveColumnWidth.Stretch
        //                            }
        //                        }
        //            };

        //            //var columns2 = new AdaptiveColumnSet()
        //            //{

        //            //    Columns = new List<AdaptiveColumn>()
        //            //            {
        //            //                new AdaptiveColumn()
        //            //                {
        //            //                    Spacing = AdaptiveSpacing.Padding,
        //            //                    Items = {details1 },
        //            //                    Width = AdaptiveColumnWidth.Auto
        //            //                },
        //            //                new AdaptiveColumn()
        //            //                {
        //            //                    Spacing = AdaptiveSpacing.Padding,
        //            //                    Items = { details2 },
        //            //                    Width = AdaptiveColumnWidth.Auto
        //            //                }
        //            //            }
        //            //};

        //            // Actions


        //            // Build card

        //            adaptiveCard.Body.Add(columns1);
        //            adaptiveCard.Body.Add(description);
        //            adaptiveCard.Body.Add(details1);

        //            return adaptiveCard;


        //            //var card = new ThumbnailCard()
        //            //{
        //            //    Title = vmincident.title,
        //            //    Subtitle = (String.Format("{0} - {1} - Assigned To : {2}", vmincident.id, vmincident.status.Replace("IncidentStatusEnum.", ""), vmincident.assignedTo)),
        //            //    Text = vmincident.description
        //            //};

        //            //CardAction resolveAction = new CardAction() { Type = ActionTypes.MessageBack, Title = "!Resolve", Value = String.Format("Resolve {0}", vmincident.id), Text = String.Format("Resolve {0}", vmincident.id) };
        //            //CardAction addNoteAction = new CardAction() { Type = ActionTypes.MessageBack, Title = "+Note", Value = String.Format("Add Note {0}", vmincident.id), Text = String.Format("Add Note {0}", vmincident.id) };
        //            //CardAction setFirstResponseAction = new CardAction() { Type = ActionTypes.MessageBack, Title = "+FR", Value = String.Format("Set first response {0}", vmincident.id), Text = String.Format("Set first response {0}", vmincident.id) };
        //            //CardAction setACAction = new CardAction() { Type = ActionTypes.MessageBack, Title = "+AC", Value = String.Format("Set await customer {0}", vmincident.id), Text = String.Format("Set await customer {0}", vmincident.id) };
        //            //adaptiveCard.Buttons = new List<CardAction> { resolveAction, addNoteAction, setFirstResponseAction, setACAction };
        //        }

        //        public static async Task<AdaptiveContainer> ACNoteColumn(VMNote note, int index, bool isFirstNote = false)
        //        {

        //            return new AdaptiveContainer
        //            {
        //                Items = {
        //                    new AdaptiveColumnSet
        //                            {
        //                                Columns =
        //                                {
        //                                    new AdaptiveColumn
        //                                    {
        //                                        Items =
        //                                        {
        //                                            new AdaptiveTextBlock
        //                                            {
        //                                                Text = note.addedBy,
        //                                                Wrap = true
        //                                            }
        //                                        }
        //                                    },
        //                                    new AdaptiveColumn
        //                                    {
        //                                        Items =
        //                                        {
        //                                            new AdaptiveTextBlock
        //                                            {
        //                                                Text = note.dateAdded
        //                                            }
        //                                        }
        //                                    },
        //                                    new AdaptiveColumn
        //                                    {
        //                                        Id = "ChevronDown" + index,
        //                                        Spacing = AdaptiveSpacing.Small,
        //                                        VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
        //                                        Items =
        //                                        {
        //                                            new AdaptiveImage
        //                                            {
        //                                                Url = new Uri("https://adaptivecards.io/content/down.png"),
        //                                                PixelWidth = 20,
        //                                                AltText = "Collapsed",
        //                                                SelectAction = new AdaptiveToggleVisibilityAction
        //                                                {
        //                                                    Title = "collapse",
        //                                                    TargetElements =
        //                                                    {
        //                                                        new AdaptiveTargetElement
        //                                                        {
        //                                                            ElementId = "Note" + index
        //                                                        },
        //                                                        new AdaptiveTargetElement
        //                                                        {
        //                                                            ElementId = "ChevronDown" + index
        //                                                        },
        //                                                        new AdaptiveTargetElement
        //                                                        {
        //                                                            ElementId = "ChevronUp" + index
        //                                                        }
        //                                                    }
        //                                                }

        //                                            }
        //                                        }
        //                                    },
        //                                    new AdaptiveColumn
        //                                    {
        //                                        Id = "ChevronUp" + index,
        //                                        Spacing = AdaptiveSpacing.Small,
        //                                        VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
        //                                        Items =
        //                                        {
        //                                            new AdaptiveImage
        //                                            {
        //                                                Url = new Uri("https://adaptivecards.io/content/up.png"),
        //                                                PixelWidth = 20,
        //                                                AltText = "Collapsed",
        //                                                IsVisible = false,
        //                                                SelectAction = new AdaptiveToggleVisibilityAction
        //                                                {
        //                                                    Title = "collapse",
        //                                                    TargetElements =
        //                                                    {
        //                                                        new AdaptiveTargetElement
        //                                                        {
        //                                                            ElementId = "Note" + index
        //                                                        },
        //                                                        new AdaptiveTargetElement
        //                                                        {
        //                                                            ElementId = "ChevronDown" + index
        //                                                        },
        //                                                        new AdaptiveTargetElement
        //                                                        {
        //                                                            ElementId = "ChevronUp" + index
        //                                                        }
        //                                                    }
        //                                                }

        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            },
        //                            new AdaptiveContainer
        //                            {
        //                                IsVisible = isFirstNote,
        //                                Id = "Note" + index,
        //                                Items =
        //                                {
        //                                    new AdaptiveTextBlock
        //                                    {
        //                                        Text  = note.body,
        //                                        IsSubtle = true,
        //                                        Wrap = true
        //                                    }
        //                                }

        //                            }
        //                        }
        //            };

        //        }

        //        public static async Task<AdaptiveCard> IncidentListACFromVMIncidentArray(VMIncident[] incidents)
        //        {
        //            var adaptiveCard = new AdaptiveCard("1.0");
        //            //Title
        //            var irTitle = new AdaptiveTextBlock() { Weight = AdaptiveTextWeight.Bolder, Text = "IRNumber", Color = AdaptiveTextColor.Accent };
        //            var titleTitle = new AdaptiveTextBlock() { Weight = AdaptiveTextWeight.Bolder, Text = "Title" };

        //            var titleBannerContainer = new AdaptiveContainer()
        //            {
        //                Style = AdaptiveContainerStyle.Emphasis,
        //                Bleed = true,
        //                Items = {new AdaptiveColumnSet()
        //                    {

        //                        Columns = new List<AdaptiveColumn>()
        //                        {
        //                            new AdaptiveColumn()
        //                            {
        //                                Spacing = AdaptiveSpacing.Large,
        //                                Items = {irTitle },
        //                                Width = AdaptiveColumnWidth.Auto
        //                            },
        //                          new AdaptiveColumn()
        //                            {
        //                                Spacing = AdaptiveSpacing.Large,
        //                                Items = {titleTitle },
        //                                Width = AdaptiveColumnWidth.Stretch
        //                            },
        //                          new AdaptiveColumn()
        //                            {
        //                                Spacing = AdaptiveSpacing.Large,
        //                                Items = { },
        //                                Width = AdaptiveColumnWidth.Auto
        //                            }
        //                        }
        //                    }
        //                }
        //            };

        //            //adaptiveCard.Body.Add(titleBannerContainer);

        //            foreach (VMIncident vmIncident in incidents)
        //            {
        //                var incidentIR = new AdaptiveTextBlock() { Text = String.Format("{0} - ({1})", vmIncident.id, vmIncident.status), Color = AdaptiveTextColor.Accent };
        //                var incidentTitle = new AdaptiveTextBlock() { Text = vmIncident.title };

        //                var incidentRow = new AdaptiveColumnSet()
        //                {
        //                    Columns = new List<AdaptiveColumn>()
        //                    {
        //                        new AdaptiveColumn()
        //                        {
        //                            Spacing = AdaptiveSpacing.Large,
        //                            Items = {
        //                                incidentIR,
        //                                incidentTitle,
        //                                new AdaptiveActionSet() {
        //                                Actions =
        //                                {
        //                                    new AdaptiveSubmitAction()
        //                                    {
        //                                        Title = "Show",
        //                                        DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"show\"}")
        //                                    }
        //                                }
        //                                }
        //                            },
        //                            Width = AdaptiveColumnWidth.Stretch
        //                        },
        //                        //new AdaptiveColumn()
        //                        //{
        //                        //    Spacing = AdaptiveSpacing.Large,
        //                        //    Items = {incidentTitle },
        //                        //    Width = AdaptiveColumnWidth.Stretch
        //                        //},
        //                        //new AdaptiveColumn()
        //                        //{
        //                        //    Spacing = AdaptiveSpacing.Large,
        //                        //    Items = { new AdaptiveActionSet() {
        //                        //        Actions =
        //                        //        {
        //                        //            new AdaptiveSubmitAction()
        //                        //            {
        //                        //                Title = "Show",
        //                        //                DataJson = ("{\"ID\" : \"" + vmIncident.id + "\", \"action\" : \"show\"}")
        //                        //            }
        //                        //        }
        //                        //    }
        //                        //    },
        //                        //    Width = AdaptiveColumnWidth.Auto
        //                        //}

        //                }
        //                };

        //                adaptiveCard.Body.Add(incidentRow);
        //            }

        //            return adaptiveCard;

        //        }
    }
}


