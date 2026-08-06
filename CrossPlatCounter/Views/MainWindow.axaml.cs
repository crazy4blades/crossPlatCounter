using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace CrossPlatCounter.Views
{
    public partial class MainWindow : Window
    {
        private int p1LP = 8000;
        private int p2LP = 8000;

        private Stack<(int p1, int p2)> lpHistory = new();
        private ExternalLPWindow _externalLPWindow;

        private string _logText = "";
        private LogWindow _logWindow;

        public MainWindow()
        {
            InitializeComponent();

            // LP Buttons
            P1Damage.Click += (_, _) => UpdateLP(P1Input, P1LPLabel, ref p1LP, -1);
            P1Heal.Click += (_, _) => UpdateLP(P1Input, P1LPLabel, ref p1LP, +1);
            P1Half.Click += (_, _) => HalfLP(ref p1LP, P1LPLabel, "Player 1");

            P2Damage.Click += (_, _) => UpdateLP(P2Input, P2LPLabel, ref p2LP, -1);
            P2Heal.Click += (_, _) => UpdateLP(P2Input, P2LPLabel, ref p2LP, +1);
            P2Half.Click += (_, _) => HalfLP(ref p2LP, P2LPLabel, "Player 2");

            // Undo / Reset
            UndoButton.Click += (_, _) => Undo();
            ResetButton.Click += (_, _) => ResetDuel();

            // LOG buttons open LogWindow
            P1Log.Click += (_, _) => OpenLogWindow();
            P2Log.Click += (_, _) => OpenLogWindow();

            // Numpads
            BuildNumpad(P1NumpadGrid, P1Input);
            BuildNumpad(P2NumpadGrid, P2Input);

            DiceButton.Click += (_, _) =>
            {
                var win = new DiceWindow();
                win.LogCallback = AppendLog;   // <-- send results to LogWindow
                win.Show();
            };
            CoinButton.Click += (_, _) =>
            {
                var win = new CoinWindow();
                win.LogCallback = AppendLog;   // send results to LogWindow
                win.Show();
            };

            // LP Window
            LPWindowButton.Click += (_, _) => OpenLPWindow();
        }

        private void OpenLogWindow()
        {
            if (_logWindow == null)
            {
                _logWindow = new LogWindow();
                _logWindow.Closed += (_, _) => _logWindow = null;
                _logWindow.Show();
            }

            _logWindow.UpdateLog(_logText);
        }

        private void AppendLog(string text)
        {
            _logText += text + Environment.NewLine;

            if (_logWindow != null)
                _logWindow.UpdateLog(_logText);
        }

        private void OpenLPWindow()
        {
            if (_externalLPWindow == null)
            {
                _externalLPWindow = new ExternalLPWindow();
                _externalLPWindow.Show();
                _externalLPWindow.AnimateStartupLP(p1LP, p2LP);
            }
            else
            {
                _externalLPWindow.UpdateLP(p1LP, p2LP);
            }

            _externalLPWindow.UpdateNames(P1Name.Text, P2Name.Text);
        }

        private void BuildNumpad(Grid grid, TextBox target)
        {
            void Add(string text, int row, int col)
            {
                var btn = new Button
                {
                    Content = text,
                    FontSize = 22,
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    Margin = new Thickness(5)
                };

                btn.Click += (_, _) =>
                {
                    target.Text += text;
                };

                grid.Children.Add(btn);
                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, col);
            }

            Add("1", 0, 0);
            Add("2", 0, 1);
            Add("3", 0, 2);

            Add("4", 1, 0);
            Add("5", 1, 1);
            Add("6", 1, 2);

            Add("7", 2, 0);
            Add("8", 2, 1);
            Add("9", 2, 2);

            Add("0", 3, 0);
            Add("00", 3, 1);
            Add("000", 3, 2);
        }

        private void UpdateLP(TextBox input, TextBlock label, ref int lp, int direction)
        {
            if (int.TryParse(input.Text, out int amount) && amount > 0)
            {
                lpHistory.Push((p1LP, p2LP));

                int oldLP = lp;
                lp += amount * direction;

                input.Text = "";
                AnimateLP(label, oldLP, lp);

                AppendLog($"{(label == P1LPLabel ? "Player 1" : "Player 2")} {(direction < 0 ? "lost" : "gained")} {amount} LP → {lp}");

                _externalLPWindow?.UpdateLP(p1LP, p2LP);

                if (lp <= 0)
                {
                    lp = 0;
                    label.Text = "0";
                    DeclareWinner(label == P1LPLabel ? "Player 2" : "Player 1");
                }
            }
        }

        private void HalfLP(ref int lp, TextBlock label, string player)
        {
            lpHistory.Push((p1LP, p2LP));
            int old = lp;
            lp /= 2;

            AnimateLP(label, old, lp);
            AppendLog($"{player} halved LP → {lp}");

            _externalLPWindow?.UpdateLP(p1LP, p2LP);
        }

        private void AnimateLP(TextBlock label, int oldValue, int newValue)
        {
            int duration = 400;
            int steps = 30;
            int stepTime = duration / steps;

            int diff = newValue - oldValue;
            int currentStep = 0;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(stepTime) };

            timer.Tick += (_, _) =>
            {
                currentStep++;
                double t = (double)currentStep / steps;
                int animatedValue = oldValue + (int)(diff * t);

                label.Text = animatedValue.ToString();

                if (currentStep >= steps)
                {
                    label.Text = newValue.ToString();
                    timer.Stop();
                }
            };

            timer.Start();
        }

        private void Undo()
        {
            if (lpHistory.Count == 0)
            {
                AppendLog("Nothing to undo.");
                return;
            }

            var previous = lpHistory.Pop();

            AnimateLP(P1LPLabel, p1LP, previous.p1);
            AnimateLP(P2LPLabel, p2LP, previous.p2);

            p1LP = previous.p1;
            p2LP = previous.p2;

            AppendLog($"Undo → P1={p1LP}, P2={p2LP}");

            _externalLPWindow?.UpdateLP(p1LP, p2LP);
        }

        private void ResetDuel()
        {
            p1LP = 8000;
            p2LP = 8000;

            P1LPLabel.Text = "8000";
            P2LPLabel.Text = "8000";

            lpHistory.Clear();

            AppendLog("Duel reset.");

            _externalLPWindow?.UpdateLP(p1LP, p2LP);
        }

        private void DeclareWinner(string winner)
        {
            AppendLog($"{winner} WINS THE DUEL!");
            _externalLPWindow?.UpdateLP(p1LP, p2LP);
        }
    }
}
