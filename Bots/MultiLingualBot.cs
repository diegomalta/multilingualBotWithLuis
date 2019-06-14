// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace MultiLingualBot
{
    public class MultiLingualBot : ActivityHandler
    {
        private const string WelcomeText = @"Say whatever you want to get started.";

        private readonly UserState _userState;

        public MultiLingualBot(UserState userState)
        {
            _userState = userState ?? throw new NullReferenceException(nameof(userState));
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //if (IsLanguageChangeRequested(turnContext.Activity.Text))
            //{
            //    var currentLang = turnContext.Activity.Text.ToLower();
            //    var lang = currentLang == EnglishEnglish || currentLang == SpanishEnglish ? EnglishEnglish : EnglishSpanish;

            //    // If the user requested a language change through the suggested actions with values "es" or "en",
            //    // simply change the user's language preference in the user state.
            //    // The translation middleware will catch this setting and translate both ways to the user's
            //    // selected language.
            //    // If Spanish was selected by the user, the reply below will actually be shown in spanish to the user.
            //    await _languagePreference.SetAsync(turnContext, lang, cancellationToken);
            //    var reply = ((Activity)turnContext.Activity).CreateReply($"Your current language code is: {lang}");

            //    await turnContext.SendActivityAsync(reply, cancellationToken);

            //    // Save the user profile updates into the user state.
            //    await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            //}
            //else
            //{
            //    // Show the user the possible options for language. If the user chooses a different language
            //    // than the default, then the translation middleware will pick it up from the user state and
            //    // translate messages both ways, i.e. user to bot and bot to user.
            //    var reply = ((Activity)turnContext.Activity).CreateReply("Choose your language:");
            //    reply.SuggestedActions = new SuggestedActions()
            //    {
            //        Actions = new List<CardAction>()
            //            {
            //                new CardAction() { Title = "Español", Type = ActionTypes.PostBack, Value = EnglishSpanish },
            //                new CardAction() { Title = "English", Type = ActionTypes.PostBack, Value = EnglishEnglish },
            //            },
            //    };

            //    await turnContext.SendActivityAsync(reply, cancellationToken);
            //}
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeCard = CreateAdaptiveCardAttachment();
                    var response = CreateResponse(turnContext.Activity, welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply(WelcomeText), cancellationToken);
                }
            }
        }

        private static Activity CreateResponse(IActivity activity, Attachment attachment)
        {
            var response = ((Activity)activity).CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        private static Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", "Cards", "welcomeCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
    }
}
