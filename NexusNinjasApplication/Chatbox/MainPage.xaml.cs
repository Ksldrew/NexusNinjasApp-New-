using NexusNinjasApplication.Chatbox;
using NexusNinjasMobileApp;
using SearchMachine;


namespace NexusNinjasApplication
{
    public partial class MainPage : ContentPage
    {
        private readonly JsonFileHelper jsonFileHelper;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            jsonFileHelper = new JsonFileHelper(); // Initialize the JsonFileHelper
        }

        // Event handler for the send message button click
        private async void SendMessage_Clicked(object sender, EventArgs e)
        {
            string message = MessageEntry.Text;
            if (!string.IsNullOrEmpty(message))
            {
                // Display the user's message in blue
                DisplayMessage(message, Colors.Blue);

                // Instantiate the ApiService with the required parameters
                var service = new ApiService("your_api_key_here", jsonFileHelper); // Provide your API key here

                // Call the GenerateContent method asynchronously and await the result
                var result = await service.GenerateContent(message);

                // Display the AI response in green
                DisplayMessage(result, Colors.Green);

                // Clear the message entry field
                MessageEntry.Text = "";
            }
        }

        // Display a message bubble with the specified text and color
        private void DisplayMessage(string message, Color color)
        {
            var messageBubble = new MessageBubble
            {
                BindingContext = new MessageViewModel { MessageText = message, BubbleColor = color, HorizontalAlignment = LayoutOptions.End }
            };
            MessageContainer.Children.Add(messageBubble);
        }

        // Event handler for the Completed event of the MessageEntry Editor
        private void OnMessageEntryCompleted(object sender, EventArgs e)
        {
            SendMessage_Clicked(sender, e);  // Call send message when enter is pressed
        }
    }
}








