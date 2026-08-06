using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace CrossPlatCounter.Views
{
    public partial class ExternalLPWindow : Window
    {
        private string savedP1Name = "Player 1";
        private string savedP2Name = "Player 2";

        public ExternalLPWindow()
        {
            InitializeComponent();
        }

        public void UpdateNames(string p1Name, string p2Name)
        {
            savedP1Name = p1Name;
            savedP2Name = p2Name;

            P1NameLabel.Text = p1Name;
            P2NameLabel.Text = p2Name;
        }

        public void UpdateLP(int p1, int p2)
        {
            // Winner screen active?
            if (RootGrid.Children.Count == 1 &&
                RootGrid.Children[0] is TextBlock winnerLabel)
            {
                if (p1 == 8000 && p2 == 8000)
                {
                    ResetOverlay(p1, p2);
                    return;
                }
            }

            int oldP1 = int.Parse(P1LPLabel.Text);
            int oldP2 = int.Parse(P2LPLabel.Text);

            AnimateLP(P1LPLabel, oldP1, p1);
            AnimateLP(P2LPLabel, oldP2, p2);

            if (p1 <= 0)
                ShowWinner(savedP2Name);

            if (p2 <= 0)
                ShowWinner(savedP1Name);
        }

        public void AnimateStartupLP(int p1Current, int p2Current)
        {
            P1LPLabel.Text = "0";
            P2LPLabel.Text = "0";

            AnimateLP(P1LPLabel, 0, p1Current);
            AnimateLP(P2LPLabel, 0, p2Current);
        }

        private void AnimateLP(TextBlock label, int oldValue, int newValue)
        {
            int duration = 600;
            int steps = 60;
            int stepTime = duration / steps;

            int diff = newValue - oldValue;
            int currentStep = 0;

            // Color change
            if (diff > 0)
                label.Foreground = Brushes.Lime;
            else if (diff < 0)
                label.Foreground = Brushes.Red;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(stepTime)
            };

            timer.Tick += async (_, _) =>
            {
                currentStep++;

                double t = (double)currentStep / steps;
                int animatedValue = oldValue + (int)(diff * t);

                label.Text = animatedValue.ToString();

                if (currentStep >= steps)
                {
                    label.Text = newValue.ToString();
                    timer.Stop();

                    await ShakeLabel(label);

                    label.Foreground = Brushes.White;
                }
            };

            timer.Start();
        }

        private async Task ShakeLabel(TextBlock label)
        {
            int shakeAmount = 6;
            int shakeTimes = 8;

            for (int i = 0; i < shakeTimes; i++)
            {
                label.Margin = new Thickness(
                    (i % 2 == 0) ? shakeAmount : 0,
                    0,
                    (i % 2 == 0) ? 0 : shakeAmount,
                    0
                );

                await Task.Delay(20);
            }

            label.Margin = new Thickness(0);
        }

        private async void ShowWinner(string winner)
        {
            await Task.Delay(1000);

            RootGrid.Children.Clear();

            var winnerLabel = new TextBlock
            {
                Text = $"{winner} WINS!",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Brushes.Gold,
                FontSize = 32
            };

            RootGrid.Children.Add(winnerLabel);
        }

        public void ResetOverlay(int p1Current, int p2Current)
        {
            RootGrid.Children.Clear();

            // Rebuild layout
            RootGrid.RowDefinitions.Clear();
            RootGrid.ColumnDefinitions.Clear();

            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            P1NameLabel = new TextBlock
            {
                Text = savedP1Name,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 24
            };

            P2NameLabel = new TextBlock
            {
                Text = savedP2Name,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 24
            };

            P1LPLabel = new TextBlock
            {
                Text = "0",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 32
            };

            P2LPLabel = new TextBlock
            {
                Text = "0",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 32
            };

            RootGrid.Children.Add(P1NameLabel);
            Grid.SetRow(P1NameLabel, 0);
            Grid.SetColumn(P1NameLabel, 0);

            RootGrid.Children.Add(P2NameLabel);
            Grid.SetRow(P2NameLabel, 0);
            Grid.SetColumn(P2NameLabel, 1);

            RootGrid.Children.Add(P1LPLabel);
            Grid.SetRow(P1LPLabel, 1);
            Grid.SetColumn(P1LPLabel, 0);

            RootGrid.Children.Add(P2LPLabel);
            Grid.SetRow(P2LPLabel, 1);
            Grid.SetColumn(P2LPLabel, 1);

            AnimateLP(P1LPLabel, 0, p1Current);
            AnimateLP(P2LPLabel, 0, p2Current);
        }
    }
}
