using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WebMaestro.Controls
{
    public class CloseableTabItem : TabItem
    {
        static CloseableTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CloseableTabItem), new FrameworkPropertyMetadata(typeof(CloseableTabItem)));
        }

        /// <summary>
        /// Provides a place to display an Icon on the Header and on the DropDown Context Menu
        /// </summary>
        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(CloseableTabItem), new UIPropertyMetadata(null));

        /// <summary>
        /// Allow the Header to be Deleted by the end user
        /// </summary>
        public bool AllowDelete
        {
            get { return (bool)GetValue(AllowDeleteProperty); }
            set { SetValue(AllowDeleteProperty, value); }
        }
        public static readonly DependencyProperty AllowDeleteProperty = DependencyProperty.Register("AllowDelete", typeof(bool), typeof(CloseableTabItem), new UIPropertyMetadata(true));

        /// <summary>
        /// OnApplyTemplate override
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // wire up the CloseButton's Click event if the button exists
            ButtonBase button = this.Template.FindName("PART_CloseButton", this) as ButtonBase;
            if (button != null)
            {
                button.Click += delegate
                {
                    // get the parent tabcontrol
                    CloseableTabControl tc = Helper.FindParentControl<CloseableTabControl>(this);
                    if (tc == null) return;

                    // remove this tabitem from the parent tabcontrol
                    if (tc.IsUsingItemsSource)
                    {
                        if (tc.ItemsSource is IList list)
                        {
                            list.Remove(this.Content);
                        }
                    }
                    else
                    {
                        tc.Items.Remove(this);
                    }
                };
            }
        }

        /// <summary>
        ///     Used by the TabPanel for sizing
        /// </summary>
        internal Dimension Dimension { get; set; }

        /// <summary>
        /// OnMouseEnter, Create and Display a Tooltip
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            //TODO: Fix tooltip
            //this.ToolTip = Helper.CloneElement(Header);
            e.Handled = true;
        }

        /// <summary>
        /// OnMouseLeave, remove the tooltip
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            this.ToolTip = null;
            e.Handled = true;
        }
    }
}
