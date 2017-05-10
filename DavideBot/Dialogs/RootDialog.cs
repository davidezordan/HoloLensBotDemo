using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace DavideBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity != null)
            {
                await ProcessLuis(activity, context);
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task ProcessLuis(Activity activity, IDialogContext context)
        {
            if (string.IsNullOrEmpty(activity.Text))
            {
                return;
            }

            var errorResult = "Try something else, please.";

            var stLuis = await LUISDavideBot.ParseUserInput(activity.Text);
            // Get the stateClient to get/set Bot Data
            //StateClient _stateClient = activity.GetStateClient();
            //BotData _botData = _stateClient.BotState.GetUserData(activity.ChannelId, activity.Conversation.Id);

            
            var intent = stLuis?.topScoringIntent?.intent;
            if (!string.IsNullOrEmpty(intent)
                //&& stLuis.entities != null && stLuis.entities.Length >= 1 &&
                //!string.IsNullOrEmpty(stLuis.entities[0].entity)
                )
            {
                switch (intent)
                {
                    case "FavouriteTechnologiesIntent":
                        await context.PostAsync("My favourite technologies are Azure, Mixed Reality and Xamarin!");
                        break;
                    case "FavouriteColorIntent":
                        await context.PostAsync("My favourite color is Blue!");
                        break;
                    case "WhatsYourNameIntent":
                        await context.PostAsync("My name is Davide, of course :)");
                        break;
                    case "HelloIntent":
                        await context.PostAsync(@"Hi there! Davide here :) This is my personal Bot. Try asking 'What are your favourite technologies?'");
                        return;
                    case "GoodByeIntent":
                        await context.PostAsync("Thanks for the chat! See you soon :)");
                        return;
                    case "None":
                        await context.PostAsync(errorResult);
                        break;
                    default:
                        await context.PostAsync(errorResult);
                        break;
                }
            }
            return;
        }
    }
}