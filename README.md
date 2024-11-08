# Experiments with HoloLens, Bot Framework, LUIS and Speech Recognition

This article was originally published @ https://davidezordan.github.io/blog:<br />
[Experiments with HoloLens, Bot Framework and LUIS: adding text to speech](https://davidezordan.github.io/experiments-with-hololens-bot-framework-and-luis-adding-text-to-speech/)<br/>
[Experiments with HoloLens, Bot Framework, LUIS and Speech Recognition](https://davidezordan.github.io/experiments-with-hololens-bot-framework-luis-and-speech-recognition/)

<p style="text-align: justify;">Recently, I had the opportunity to use a HoloLens device for some personal training and building some simple demos.</p>
<p style="text-align: justify;"><img class="aligncenter wp-image-8156 size-medium" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLens-dev-kit-225x300.jpg" alt="" width="225" height="300" /></p>
<p style="text-align: justify;">One of the scenarios that I find very intriguing is the possibility of integrating Mixed Reality and Artificial Intelligence (AI) in order to create immersive experiences for the user.</p>
<p style="text-align: justify;">I decided to perform an experiment by integrating a Bot, Language Understanding Intelligent Services (LUIS), Speech Recognition and Mixed Reality via a Holographic 2D app.</p>
<p style="text-align: justify;">The idea was to create a sort of "digital assistant" of myself that can be contacted using Mixed Reality: the first implementation contains only basic interactions (answering questions like "What are your favourite technologies" or "What's your name") but these could be easily be expanded in the future with features like time management (via the Graph APIs) or tracking projects status, etc.</p>

<h1>Creating the LUIS application</h1>
<p style="text-align: justify;">To start, I created a new LUIS application in the portal with a list of intents that needed to be handled:</p>
<p style="text-align: justify;"><img class="aligncenter size-large wp-image-8131" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-LUIS-Intents-1024x652.png" alt="" width="660" height="420" /></p>
In the future, this could be further extended with extra capabilities.

<img class="aligncenter size-large wp-image-8137" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-LUIS-Intents-Utterances-1024x652.png" alt="" width="660" height="420" />

After defining the intents and utterances, I trained and published my LUIS app to Azure and copied the key and URL for usage in my Bot:

<img class="aligncenter size-large wp-image-8139" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-LUIS-Publish-App-1024x652.png" alt="" width="660" height="420" />
<h1>Creating the Bot</h1>
<p style="text-align: justify;">I proceeded with the creation of the Bot using <em>Microsoft Bot framework </em>downloading the <a href="http://aka.ms/bf-bc-vstemplate" target="_blank" rel="noopener noreferrer">Visual Studio template</a> and creating a new project:</p>
<p style="text-align: justify;"><img class="aligncenter size-large wp-image-8134" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-Create-new-Bot-Application-1024x710.png" alt="" width="660" height="458" /></p>
<p style="text-align: justify;">The Bot template already defined a dialog named <em>RootDialog</em> so I extended the generated project with the classes required for parsing the JSON from the LUIS endpoint:</p>

<pre title="LUIS classes" class="lang:default decode:true ">public class LUISDavideBot
{
    public static async Task&lt;DavideBotLUIS&gt; ParseUserInput(string strInput)
    {
            string strRet = string.Empty;
            string strEscaped = Uri.EscapeDataString(strInput);

            using (var client = new HttpClient())
            {
                string uri = "&lt;LUIS url here&gt;" + strEscaped;
                HttpResponseMessage msg = await client.GetAsync(uri);

                if (msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    var _Data = JsonConvert.DeserializeObject&lt;DavideBotLUIS&gt;(jsonResponse);
                    return _Data;
                }
            }

        return null;
    }
}


public class DavideBotLUIS
{
    public string query { get; set; }
    public Topscoringintent topScoringIntent { get; set; }
    public Intent[] intents { get; set; }
    public Entity[] entities { get; set; }
    public Dialog dialog { get; set; }
}

public class Topscoringintent
{
    public string intent { get; set; }
    public float score { get; set; }
    public Action[] actions { get; set; }
}

public class Action
{
    public bool triggered { get; set; }
    public string name { get; set; }
    public object[] parameters { get; set; }
}

public class Dialog
{
    public string contextId { get; set; }
    public string status { get; set; }
}

public class Intent
{
    public string intent { get; set; }
    public float score { get; set; }
    public Action1[] actions { get; set; }
}

public class Action1
{
    public bool triggered { get; set; }
    public string name { get; set; }
    public object[] parameters { get; set; }
}

public class Entity
{
    public string entity { get; set; }
    public string type { get; set; }
    public int startIndex { get; set; }
    public int endIndex { get; set; }
    public float score { get; set; }
}</pre>
And then processed the various LUIS intents in <em>RootDialog </em>(another option is the usage of the <strong>LuisDialog</strong> and <strong>LuisModel</strong> classes as explained <a href="https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-luis-dialogs" target="_blank" rel="noopener noreferrer">here</a>):
<pre title="RootDialog implementation" class="lang:default decode:true">[Serializable]
public class RootDialog : IDialog&lt;object&gt;
{
    public Task StartAsync(IDialogContext context)
    {
        context.Wait(MessageReceivedAsync);

        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(IDialogContext context, IAwaitable&lt;object&gt; result)
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
            
        var intent = stLuis?.topScoringIntent?.intent;
        if (!string.IsNullOrEmpty(intent))
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
}</pre>
Then, I tested the implementation using the <a href="https://emulator.botframework.com/" target="_blank" rel="noopener noreferrer">Bot Framework Emulator</a>:

<img class="aligncenter size-large wp-image-8140" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-Using-Bot-Framework-Emulator-1024x653.png" alt="" width="660" height="421" />

And created a new Bot definition in the framework portal.

After that, I published it to Azure with an updated <em>Web.config</em> with the generated Microsoft App ID and password:

<img class="aligncenter size-large wp-image-8143" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-Define-Bot-App-New-1024x652.png" alt="" width="660" height="420" />
<p style="text-align: left;">Since the final goal was the communication with an UWP HoloLens application, I enabled the <em>Diret Line </em>channel:</p>
<img class="aligncenter size-large wp-image-8133" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-Create-new-Bot-Application-Define-DirectLine-channel-1024x652.png" alt="" width="660" height="420" />
<h1>Creating the Holographic 2D app</h1>
<p style="text-align: left;">Windows 10 UWP apps are executed on the HoloLens device as <em>Holographic 2D apps</em> that can be pinned in the environment.</p>
I created a new project using the default Visual Studio Template:
<p style="text-align: left;"><img class="aligncenter size-large wp-image-8135" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-Create-new-UWP-project-1024x710.png" alt="" width="660" height="458" /></p>
And then added some simple text controls in XAML to receive the input and display the response from the Bot:
<pre title="XAML Page for the UWP app" class="lang:default decode:true ">&lt;Page
    x:Class="HoloLensBotDemo.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"&gt;

    &lt;Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}"&gt;
        &lt;Grid.ColumnDefinitions&gt;
            &lt;ColumnDefinition Width="10"/&gt;
            &lt;ColumnDefinition Width="Auto"/&gt;
            &lt;ColumnDefinition Width="10"/&gt;
            &lt;ColumnDefinition Width="*"/&gt;
            &lt;ColumnDefinition Width="10"/&gt;
        &lt;/Grid.ColumnDefinitions&gt;
        &lt;Grid.RowDefinitions&gt;
            &lt;RowDefinition Height="50"/&gt;
            &lt;RowDefinition Height="50"/&gt;
            &lt;RowDefinition Height="50"/&gt;
            &lt;RowDefinition Height="Auto"/&gt;
        &lt;/Grid.RowDefinitions&gt;
        &lt;TextBlock Text="Command received: " Grid.Column="1" VerticalAlignment="Center" /&gt;
        &lt;TextBox x:Name="TextCommand" Grid.Column="3" VerticalAlignment="Center"/&gt;

        &lt;Button Content="Start Recognition" Click="Button_Click" Grid.Row="1" Grid.Column="1" VerticalAlignment="Center" /&gt;

        &lt;TextBlock Text="Status: " Grid.Column="1" VerticalAlignment="Center" Grid.Row="2" /&gt;
        &lt;TextBlock x:Name="TextStatus" Grid.Column="3" VerticalAlignment="Center" Grid.Row="2"/&gt;

        &lt;TextBlock Text="Bot response: " Grid.Column="1" VerticalAlignment="Center" Grid.Row="3" /&gt;
        &lt;TextBlock x:Name="TextOutputBot" Foreground="Red" Grid.Column="3" 
                   VerticalAlignment="Center" Width="Auto" Height="Auto" Grid.Row="3"
                   TextWrapping="Wrap" /&gt;
    &lt;/Grid&gt;
&lt;/Page&gt;</pre>
I decided to use the <em>SpeechRecognizer</em> APIs for receiving the input via voice (another option could be the usage of <em>Cognitive Services</em>):
<pre title="Speech Recognition" class="lang:default decode:true ">private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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
}</pre>
<p style="text-align: left;">The <strong>SendToBot()</strong> method makes use of the Direct Line APIs which permit communication with the Bot using the channel previously defined:</p>

<pre title="Using Direct Line" class="lang:default decode:true ">private async Task SendToBot(string inputText)
{
    if (!string.IsNullOrEmpty(inputText))
    {
        var directLine = new DirectLineClient("&lt;Direct Line Secret Key here&gt;");
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
        if (result.Activities.Count &gt; 0)
        {
            var firstOrDefault = result
                .Activities
                .FirstOrDefault(a =&gt; a.From != null
                &amp;&amp; a.From.Name != null
                &amp;&amp; a.From.Name.Equals("Davide Personal Bot"));
            if (firstOrDefault != null)
            {
                TextOutputBot.Text = "Bot response: " + firstOrDefault.Text;
                TextStatus.Text = string.Empty;
            }
        }
    }
}</pre>
<p style="text-align: justify;">And then I got the app running on HoloLens and interfacing with a Bot using LUIS for language understanding and Speech recognition:</p>
<img class="aligncenter size-large wp-image-8132" src="https://davidezordan.github.io/wp-content/uploads/2017/05/HoloLensBot-App-running-1024x576.jpg" alt="" width="660" height="371" />
Happy coding!
