using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using MultiLingualBot.Models;
using MultiLingualBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiLingualBot.Dialogs.Spanish
{
    public class RegisterAccountDialogSpanish : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        #endregion  

        public RegisterAccountDialogSpanish(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                UserNameStepAsync,
                SSNStepAsync,
                DOBStepAsync,
                SummaryStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(RegisterAccountDialogSpanish)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(RegisterAccountDialogSpanish)}.name"));
            AddDialog(new TextPrompt($"{nameof(RegisterAccountDialogSpanish)}.ssn"));
            AddDialog(new DateTimePrompt($"{nameof(RegisterAccountDialogSpanish)}.dob"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(RegisterAccountDialogSpanish)}.mainFlow";
        }

        private async Task<DialogTurnResult> UserNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(RegisterAccountDialogSpanish)}.name",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Seguro, Captura tu nombre completo")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> SSNStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(RegisterAccountDialogSpanish)}.ssn",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Captura tu SSN:")
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> DOBStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ssn"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(RegisterAccountDialogSpanish)}.dob",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your Date of Birth"),
                    RetryPrompt = MessageFactory.Text("incorret."),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["dob"] = Convert.ToDateTime(((List<DateTimeResolution>)stepContext.Result).FirstOrDefault().Value);

            // Get the current profile object from user state.
            var userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // Save all of the data inside the user profile
            userProfile.Name = (string)stepContext.Values["name"];
            userProfile.ssn = (string)stepContext.Values["ssn"];
            userProfile.dob = (DateTime)stepContext.Values["dob"];

            // Show the summary to the user
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Revisa tus datos:"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Nombre: {0}", userProfile.Name)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("SSN: {0}", userProfile.ssn)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Fecha de Nacimiento: {0}", userProfile.dob.ToString())), cancellationToken);
         

            // Save data in userstate
            await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
