using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using MultiLingualBot.Models;
using MultiLingualBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiLingualBot.Dialogs
{
    public class LenguageDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        #endregion  
        public LenguageDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(LenguageDialog)}.mainFlow", waterfallSteps));
            AddDialog(new ChoicePrompt($"{nameof(LenguageDialog)}.language"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(LenguageDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync($"{nameof(LenguageDialog)}.language",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Oh también hablo Español!, Por favor confirma el idioma de tu preferencia:"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Español", "English"}),
                }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Language))
            {
                // Set the name
                userProfile.Language = ((FoundChoice)stepContext.Result).Value == "Español" ? "es" : "en" ;

                // Save any state changes that might have occured during the turn.
                await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }

            if (userProfile.Language == "en")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Okay, let's continue our conversation in english"), cancellationToken);
            }
            else if (userProfile.Language == "es")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Muy bien, Continuemos en Español"), cancellationToken);
            }
            
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
