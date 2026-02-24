using System.Windows;
using System.Windows.Shell;

namespace WebMaestro.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AddHeader.xaml
    /// </summary>
    public partial class AddHeader : Window
    {
        public AddHeader()
        {
            InitializeComponent();

            WindowChrome.SetWindowChrome(
                this,
                new WindowChrome
                {
                    CaptionHeight = 28,
                    CornerRadius = new CornerRadius(12),
                    GlassFrameThickness = new Thickness(-1),
                    ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                    UseAeroCaptionButtons = true,
                    NonClientFrameEdges = SystemParameters.HighContrast ? NonClientFrameEdges.None :
                        NonClientFrameEdges.Right | NonClientFrameEdges.Bottom | NonClientFrameEdges.Left
                }
            );
        }
    }
}
