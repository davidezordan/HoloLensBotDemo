using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HoloLensBotDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SpeechSynthesizer synthesizer;
        private SpeechRecognizer recognizer;

        public MainPage()
        {
            this.InitializeComponent();

            InitializeSpeech();
        }

        private async void InitializeSpeech()
        {
            synthesizer = new SpeechSynthesizer();
            recognizer = new SpeechRecognizer();

            media.MediaEnded += Media_MediaEnded;
            recognizer.StateChanged += Recognizer_StateChanged;

            // Compile the dictation grammar by default.
            await recognizer.CompileConstraintsAsync();
        }

        private void Recognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            if (args.State == SpeechRecognizerState.Idle)
            {
                SetTextStatus(string.Empty);
            }

            if (args.State == SpeechRecognizerState.Capturing)
            {
                SetTextStatus("Listening....");
            }
        }

        private async void SetTextStatus(string text)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TextStatus.Text = text;
            });
        }

        private void Media_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StartRecognitionButton_Click(null, null);
        }

        private async void StartRecognitionButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {           
            if (recognizer.State == SpeechRecognizerState.Idle)
            {
                // Start recognition.
                SpeechRecognitionResult speechRecognitionResult = await recognizer.RecognizeWithUIAsync();

                TextCommand.Text = speechRecognitionResult.Text;

                await SendToBot(TextCommand.Text);
            }
        }

        private async Task SendToBot(string inputText)
        {
            if (!string.IsNullOrEmpty(inputText))
            {
                var directLine = new DirectLineClient("<Direct Line key here>");
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
                    var botResponse = result
                        .Activities
                        .LastOrDefault(a => a.From != null && a.From.Name != null && a.From.Name.Equals("Davide Personal Bot"));
                    if (botResponse != null && !string.IsNullOrEmpty(botResponse.Text))
                    {
                        var response = botResponse.Text;

                        TextOutputBot.Text = "Bot response: " + response;
                        TextStatus.Text = string.Empty;

                        Speech(response);
                    }
                }

                TextStatus.Text = string.Empty;
            }
        }

        private async void Speech(string text)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                media.Stop();
            }
            else
            {
                try
                {
                    // Create a stream from the text. This will be played using a media element.
                    SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

                    // Set the source and start playing the synthesized audio stream.
                    media.AutoPlay = true;
                    media.SetSource(synthesisStream, synthesisStream.ContentType);
                    media.Play();
                }
                catch (System.IO.FileNotFoundException)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
                    await messageDialog.ShowAsync();
                }
                catch (Exception)
                {
                    media.AutoPlay = false;
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to synthesize text");
                    await messageDialog.ShowAsync();
                }
            }
        }
    }
}
