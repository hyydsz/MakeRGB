using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace MakeRGB
{
    public partial class donate : Window
    {
        public Random random;
        public List<Path> paths;
        public Timer timer;

        public donate()
        {
            InitializeComponent();

            timer = new Timer(TimerCallback, null, 0, 800);
            random = new Random((int) DateTime.Now.Ticks);
            paths = new List<Path>();

            for (int i = 0; i < random.Next(1, 7); i++)
            {
                int length = random.Next(20, 40);

                Path path = new Path()
                {
                    Width = length,
                    Height = length,

                    Fill = new SolidColorBrush(Color.FromRgb(255, 100, 100)),

                    Stretch = Stretch.Fill,

                    VerticalAlignment = VerticalAlignment.Bottom,

                    Effect = new DropShadowEffect()
                    {
                        Color = Color.FromArgb(1, 180, 180, 180),
                        BlurRadius = 20,
                        ShadowDepth = 10
                    },

                    Data = Geometry.Parse("M8.727 3C7.091 3 6.001 4.65 6.001 4.65S4.909 3 3.273 3C1.636 3 0 4.1 0 6.3 0 9.6 6 14 6 14s6-4.4 6-7.7C12 4.1 10.364 3 8.727 3z")
                };

                paths.Add(path);

                lover_list.Children.Add(path);
            }

            paths.ForEach(path =>
            {
                path.RenderTransform = new TranslateTransform()
                {
                    X = random.Next(-300, 300),
                    Y = random.Next(-300, 300)
                };
            });
        }

        private void TimerCallback(object o)
        {
            Dispatcher.Invoke(() =>
            {
                paths.ForEach(path =>
                {

                    double random_X = random.Next(-300, 300);
                    double random_Y = random.Next(-300, 300);
                    double smoothingFactor = 0.4;

                    TranslateTransform translate = path.RenderTransform as TranslateTransform;

                    random_X = translate.X + (smoothingFactor * (random_X - translate.X));
                    random_Y = translate.Y + (smoothingFactor * (random_Y - translate.Y));

                    DoubleAnimation Xanimation = new DoubleAnimation
                    {
                        From = translate.X,
                        To = random_X,
                        Duration = TimeSpan.FromSeconds(1)
                    };

                    DoubleAnimation Yanimation = new DoubleAnimation
                    {
                        From = translate.Y,
                        To = random_Y,
                        Duration = TimeSpan.FromSeconds(3)
                    };

                    translate.BeginAnimation(TranslateTransform.XProperty, Xanimation);
                    translate.BeginAnimation(TranslateTransform.YProperty, Yanimation);
                });
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            timer.Dispose();

            base.OnClosing(e);
        }
    }
}
