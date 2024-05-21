using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace NexusNinjasApplication.Chatbox;
public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
        // Start a timer to update the percentage
        StartTimer();
    }

    private async void StartTimer()
    {
        // Simulate loading process
        for (int i = 0; i <= 100; i++)
        {
            // Update the percentage label
            PercentageLabel.Text = $"{i}%";

            // Simulate a delay
            await Task.Delay(50); // Adjust the delay time as needed
        }

        // Navigate to the main chat page after loading is complete
        await Navigation.PushAsync(new MainPage());
    }
}