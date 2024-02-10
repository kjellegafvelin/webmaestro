using System.Windows.Input;

namespace WebMaestro.Commands
{

    // Check out https://kent-boogaart.com/blog/multikeygesture for handling multiple key gestures


    internal class WindowCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(WindowCommands), new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) });

        public static readonly RoutedUICommand Save = new RoutedUICommand("Save to File", "Save", typeof(WindowCommands),
            new InputGestureCollection() {
                    new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S")
            });

        public static readonly RoutedUICommand SaveRequest = new RoutedUICommand("Save Request to File", "SaveRequest", typeof(WindowCommands));

        public static readonly RoutedUICommand SaveResponse = new RoutedUICommand("Save Response to File", "SaveResponse", typeof(WindowCommands));

        public static readonly RoutedUICommand FormatDocument = new RoutedUICommand("Format document", "FormatDocument", typeof(WindowCommands));

        public static readonly RoutedUICommand SaveToCollection = new RoutedUICommand("Save to Collection", "SaveToCollection", typeof(WindowCommands));

        public static readonly RoutedUICommand SendRequest = new RoutedUICommand("Send Request", "SendRequest", typeof(WindowCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.F5, ModifierKeys.None, "F5")
            });

        public static readonly RoutedUICommand CancelRequest = new RoutedUICommand("Cancel Request", "CancelRequest", typeof(WindowCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.F5, ModifierKeys.Control, "Ctrl+F5")
            });

        public static readonly RoutedUICommand StartServer = new RoutedUICommand("Start Server", "StartServer", typeof(WindowCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.F6, ModifierKeys.None, "F6")
            });

        public static readonly RoutedUICommand StopServer = new RoutedUICommand("Stop Server", "StopServer", typeof(WindowCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.F6, ModifierKeys.Control, "Ctrl+F6")
            });

        public static readonly RoutedUICommand DuplicateHeader = new RoutedUICommand("Duplicate", "DuplicateHeader", typeof(WindowCommands));

        public static readonly RoutedUICommand RemoveHistoryItem = new RoutedUICommand("Remove", "RemoveHistoryItem", typeof(WindowCommands));

        public static readonly RoutedUICommand EditComment = new RoutedUICommand("Edit Comment", "EditComment", typeof(WindowCommands));

    }
}
