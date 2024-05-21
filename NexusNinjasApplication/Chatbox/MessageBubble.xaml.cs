using Microsoft.Maui.Controls;
using NexusNinjasApplication;
using NexusNinjasApplication.Chatbox;

namespace NexusNinjasMobileApp
{
    public partial class MessageBubble : ContentView
    {
        // Maximum width of the message bubble
        private const double MaxBubbleWidth = 300;

        public MessageBubble()
        {

            InitializeComponent();
            BindingContext = new MeeageViewModel(); // Set the BindingContext to the MessageViewModel
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            double maxWidth = GetMessageBubbleMaxWidth();
            MessageFrame.WidthRequest = maxWidth;

            // Call the animation method when the message bubble size changes
            AnimateMessageBubble();
        }

        private double GetMessageBubbleMaxWidth()
        {
            var viewModel = (MessageViewModel)BindingContext;
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width;

            // Calculate the width based on the length of the message text
            var labelSizeRequest = MessageTextLabel.Measure(screenWidth, double.PositiveInfinity);
            double labelWidth = labelSizeRequest.Request.Width;

            // Add some padding or margin if needed
            double maxBubbleWidth = labelWidth + 27; // Adjusted margin to 20
                                                     // Subtract a small margin to prevent the last letter from going to a new line
            maxBubbleWidth -= 5; // Adjust margin as needed

            // Ensure the width does not exceed the maximum allowed width
            return Math.Min(maxBubbleWidth, MaxBubbleWidth);
        }

        // Define the animation method
        private async void AnimateMessageBubble()
        {
            // Add your animation logic here
            // For example, you can use Xamarin.Forms animations like FadeTo, TranslateTo, or RotateTo
            await MessageFrame.FadeTo(1, 1000); // Example animation: Fade in the message bubble over 1 second
        }
    }
}