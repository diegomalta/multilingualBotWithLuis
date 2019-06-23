using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using MultiLingualBot.Dialogs.Spanish;
using MultiLingualBot.Models;
using MultiLingualBot.Services;
using MultiLingualBot.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MultiLingualBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly Translator _translator;
        private readonly BotServices _botServices;
        #endregion  


        public MainDialog(BotStateService botStateService, BotServices botServices, Translator translator) : base(nameof(MainDialog))
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _translator = translator ?? throw new System.ArgumentNullException(nameof(translator));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));

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

            // Add lenguage Dialog
            AddDialog(new LenguageDialog($"{nameof(MainDialog)}.lenguage", _botStateService));

            // Add Named Dialogs
            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _botStateService));
            AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugReport", _botStateService));

            // Add Spanish Named Dialogs
            AddDialog(new GreetingDialogSpanish($"{nameof(MainDialog)}.greetingSpanish", _botStateService));
            AddDialog(new RegisterAccountDialogSpanish($"{nameof(MainDialog)}.accountSpanish", _botStateService));

            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // save in user state lenguage preference of the user
            // Get the current profile object from user state.
            var userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (string.IsNullOrEmpty(userProfile.Language))
            {
                var lang = await _translator.DetectLanguageAsync(stepContext.Context.Activity.Text);

                // if we detect spanish we need to ask the user if we can continue with spanish
                if (lang == "es")
                {
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.lenguage", null, cancellationToken);
                }
                else
                {
                    userProfile.Language = "en";
                    // Save changes
                    await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
                }
            }

            if (userProfile.Language == "es")
            {
                var recognizerResult = await _botServices.DispatchES.RecognizeAsync(stepContext.Context, cancellationToken);

                // Top intent tell us which cognitive service to use.
                var topIntent = recognizerResult.GetTopScoringIntent();
                
                switch (topIntent.intent)
                {                   
                    case "CrearCuenta":
                        return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.accountSpanish", null, cancellationToken);
                    default:
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Perdon, no entiendo lo que me dices"), cancellationToken);
                        break;
                }

            }
            else
            {
                // this is the EN LUIS
                var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

                // Top intent tell us which cognitive service to use.
                var topIntent = recognizerResult.GetTopScoringIntent();

                switch (topIntent.intent)
                {
                    case "Greetings":
                        return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
                    case "NewBugReportIntent":
                        return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null, cancellationToken);
                    case "QueryBugTypeIntent":
                        return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugType", null, cancellationToken);
                    default:
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry I don't know what you mean."), cancellationToken);
                        break;
                }
            }            

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
