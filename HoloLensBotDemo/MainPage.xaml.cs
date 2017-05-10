using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HoloLensBotDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TextStatus.Text = "Listening....";
            
            // Create an instance of SpeechRecognizer.
            var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();

            // Compile the dictation grammar by default.
            await speechRecognizer.CompileConstraintsAsync();

            // Start recognition.
            Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

            TextCommand.Text = speechRecognitionResult.Text;

            await SendToBot(TextCommand.Text);
        }

        private async Task SendToBot(string inputText)
        {
            if (!string.IsNullOrEmpty(inputText))
            {
                var directLine = new DirectLineClient("<Direct Line Secret Key here>");
                var conversation = await directLine.Conversations.StartConversationWithHttpMessagesAsync();
                var convId = conversation.Body.ConversationId;
                var httpMessages = await directLine.Conversations.GetActivitiesWithHttpMessagesAsync(convId);

                var command = inputText;
                TextStatus.Text = "Sending text to Bot....";
                var postMessage = await directLine.Conversations.PostActivityWithHttpMessagesAsync(convId,
                    new Activity()
                    {
                        Type = "message",
                        From = new ChannelAccount()
                        {
                            Id = "Davide"
                        },
                        Text = command
                    });

                var result = await directLine.Conversations.GetActivitiesAsync(convId);
                if (result.Activities.Count > 0)
                {
                    var firstOrDefault = result
                        .Activities
                        .FirstOrDefault(a => a.From != null
                        && a.From.Name != null
                        && a.From.Name.Equals("Davide Personal Bot"));
                    if (firstOrDefault != null)
                    {
                        TextOutputBot.Text = "Bot response: " + firstOrDefault.Text;
                        TextStatus.Text = string.Empty;
                    }
                }
            }
        }
    }
}
