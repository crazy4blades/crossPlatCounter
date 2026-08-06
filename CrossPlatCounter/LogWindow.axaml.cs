using Avalonia.Controls;

namespace CrossPlatCounter.Views
{
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        public void UpdateLog(string text)
        {
            LogDisplay.Text = text;
            LogDisplay.CaretIndex = LogDisplay.Text.Length;
        }
    }
}
