using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace CrossPlatCounter.Views
{
    public partial class CoinWindow : Window
    {
        private readonly Random rng = new Random();
        private List<Image> coinImages = new();

        public string SelectedPlayer { get; private set; }
        public string[] Results { get; private set; }

        public Action<string>? LogCallback { get; set; }

        public CoinWindow()
        {
            InitializeComponent();

            FlipButton.Click += (_, _) =>
            {
                SelectedPlayer = (PlayerChooser.SelectedItem as ComboBoxItem)?.Content.ToString();
                StartCoinAnimation();
            };
        }

        private void StartCoinAnimation()
        {
            CoinPanel.Children.Clear();
            coinImages.Clear();

            int count = (int)CoinCount.Value;

            for (int i = 0; i < count; i++)
            {
                var img = new Image
                {
                    Width = 100,
                    Height = 100,
                    Source = LoadCoinImage(true)
                };

                coinImages.Add(img);
                CoinPanel.Children.Add(img);
            }

            int frames = 20;
            int interval = 50;
            int current = 0;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(interval) };

            timer.Tick += (_, _) =>
            {
                current++;

                foreach (var img in coinImages)
                {
                    bool heads = rng.Next(0, 2) == 1;
                    img.Source = LoadCoinImage(heads);
                }

                if (current >= frames)
                {
                    timer.Stop();
                    ShowFinalCoins();
                }
            };

            timer.Start();
        }

        private void ShowFinalCoins()
        {
            Results = new string[coinImages.Count];

            for (int i = 0; i < coinImages.Count; i++)
            {
                bool heads = rng.Next(0, 2) == 1;
                coinImages[i].Source = LoadCoinImage(heads);
                Results[i] = heads ? "HEADS" : "TAILS";
            }

            // ⭐ SINGLE CLEAN LOG ENTRY ⭐
            LogCallback?.Invoke($"{SelectedPlayer} flipped: {string.Join(", ", Results)}");
        }

        private Bitmap LoadCoinImage(bool heads)
        {
            return new Bitmap($"Assets/Coins/{(heads ? "coin_heads.png" : "coin_tails.png")}");
        }
    }
}
