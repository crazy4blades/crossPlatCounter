using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace CrossPlatCounter.Views
{
    public partial class DiceWindow : Window
    {
        private readonly Random rng = new Random();
        private List<Image> diceImages = new();
        public Action<string>? LogCallback { get; set; }

        public string SelectedPlayer { get; private set; }
        public int[] Results { get; private set; }

        public DiceWindow()
        {
            InitializeComponent();

            RollButton.Click += (_, _) =>
            {
                SelectedPlayer = (PlayerChooser.SelectedItem as ComboBoxItem)?.Content.ToString();
                StartDiceAnimation();
            };
        }

        private void StartDiceAnimation()
        {
            DicePanel.Children.Clear();
            diceImages.Clear();

            int count = (int)DiceCount.Value;

            for (int i = 0; i < count; i++)
            {
                var img = new Image
                {
                    Width = 100,
                    Height = 100,
                    Source = LoadDiceImage(1)
                };

                diceImages.Add(img);
                DicePanel.Children.Add(img);
            }

            int frames = 20;
            int interval = 50;
            int current = 0;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(interval) };

            timer.Tick += (_, _) =>
            {
                current++;

                foreach (var img in diceImages)
                {
                    int roll = rng.Next(1, 7);
                    img.Source = LoadDiceImage(roll);
                }

                if (current >= frames)
                {
                    timer.Stop();
                    ShowFinalDice();
                }
            };

            timer.Start();
        }

        private void ShowFinalDice()
        {
            Results = new int[diceImages.Count];

            for (int i = 0; i < diceImages.Count; i++)
            {
                int roll = rng.Next(1, 7);
                diceImages[i].Source = LoadDiceImage(roll);
                Results[i] = roll;
            }
            LogCallback?.Invoke($"{SelectedPlayer} rolled: {string.Join(", ", Results)}");
        }

        private Bitmap LoadDiceImage(int value)
        {
            return new Bitmap($"Assets/Dice/dice{value}.png");
        }
    }
}
