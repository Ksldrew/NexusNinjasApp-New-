
using Microsoft.Maui.Controls;

namespace NexusNinjasApplication.Chatbox;

    public class MessageViewModel
{
    public required string MessageText { get; set; }
    public required Color BubbleColor { get; set; }
    public LayoutOptions HorizontalAlignment { get; set; }
}