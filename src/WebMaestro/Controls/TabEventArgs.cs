using System;
using System.ComponentModel;

namespace WebMaestro.Controls
{
    public class TabItemEventArgs : EventArgs
    {
        public CloseableTabItem TabItem { get; private set; }

        public TabItemEventArgs(CloseableTabItem item)
        {
            TabItem = item;
        }
    }
    public class NewTabItemEventArgs : EventArgs
    {
        /// <summary>
        ///     The object to be used as the Content for the new TabItem
        /// </summary>
        public object Content { get; set; }
    }

    public class TabItemCancelEventArgs : CancelEventArgs
    {
        public CloseableTabItem TabItem { get; private set; }

        public TabItemCancelEventArgs(CloseableTabItem item)
        {
            TabItem = item;
        }
    }
}
