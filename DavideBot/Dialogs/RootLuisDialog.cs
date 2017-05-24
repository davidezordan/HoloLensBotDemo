using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Luis;

namespace DavideBot.Dialogs
{
    [LuisModel("YourModelId", "YourSubscriptionKey")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            var errorResult = "Try something else, please.";
            await context.PostAsync(errorResult);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("FavouriteTechnologiesIntent")]
        public async Task FavouriteTechnologiesIntent(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("My favourite technologies are Azure, Mixed Reality and Xamarin!");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("FavouriteColorIntent")]
        public async Task FavouriteColorIntent(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("My favourite color is Blue!");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("WhatsYourNameIntent")]
        public async Task WhatsYourNameIntent(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("My name is Davide, of course :)");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("HelloIntent")]
        public async Task HelloIntent(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync(@"Hi there! Davide here :) This is my personal Bot. Try asking 'What are your favourite technologies?'");
            context.Wait(this.MessageReceived);
        }
    }
}