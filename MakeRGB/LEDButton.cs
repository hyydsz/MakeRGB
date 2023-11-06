using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MakeRGB
{
    public class LEDButton : Control
    {
        public ToggleButton led_toggle;
        public TextBlock led_index;

        public LED_Pos led_pos;

        private Point startPoint;

        static LEDButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LEDButton), new FrameworkPropertyMetadata(typeof(LEDButton)));
        }

        public override void OnApplyTemplate()
        {
            led_toggle = GetTemplateChild("PART_ClickBox") as ToggleButton;
            led_index = GetTemplateChild("PART_LedIndex") as TextBlock;
            led_toggle.Click += Led_toggle_Click;

            led_pos.button = this;

            if (!IsEnabled)
                return;

            if (led_pos.index != -1)
            {
                led_toggle.IsChecked = true;
                led_index.Text = led_pos.index.ToString();
            }

            this.PreviewMouseDown += (sender, e) =>
            {
                if (led_toggle.IsChecked.Value)
                {
                    if (e.ChangedButton == MouseButton.Left)
                    {
                        startPoint = e.GetPosition(this);
                    }
                }
            };

            this.PreviewMouseMove += (sender, e) =>
            {
                if (led_toggle.IsChecked.Value)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Point currentPoint = e.GetPosition(this);
                        Vector offset = startPoint - currentPoint;

                        if (offset.Length > SystemParameters.MinimumHorizontalDragDistance || offset.Length > SystemParameters.MinimumVerticalDragDistance)
                        {
                            DragDrop.DoDragDrop(this, led_pos, DragDropEffects.Move);
                        }
                    }
                }
            };

            this.DragOver += (sender, e) =>
            {
                e.Handled = true;
            };

            this.Drop += (sender, e) =>
            {
                if (!led_toggle.IsChecked.Value)
                {
                    if (e.Data.GetData(typeof(LED_Pos)) is LED_Pos pos)
                    {
                        if (pos.index == -1 || pos.index >= MainWindow.led_positions.Count)
                            return;

                        led_toggle.IsChecked = true;
                        led_index.Text = pos.index.ToString();

                        led_pos.index = pos.index;

                        MainWindow.led_positions[pos.index] = led_pos;

                        pos.button.ResetButton();
                    }
                }
            };

            base.OnApplyTemplate();
        }

        public void SetButton(int index)
        {
            led_pos.index = index;

            if (led_toggle != null)
            {
                led_toggle.IsChecked = true;
                led_index.Text = index.ToString();
            }
        }

        public void ResetButton()
        {
            led_toggle.IsChecked = false;
            led_index.Text = string.Empty;

            led_pos.index = -1;
        }

        private void Led_toggle_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEnabled) return;

            if (led_toggle.IsChecked.Value)
            {
                led_pos.index = MainWindow.led_positions.Count;
                led_index.Text = led_pos.index.ToString();  

                MainWindow.led_positions.Add(led_pos);
                MainWindow.ActionUpdateLedCount();
            }
            else
            {
                if (led_pos.index != MainWindow.led_positions.Count - 1)
                {
                    led_toggle.IsChecked = true;
                    return;
                }

                MainWindow.led_positions.RemoveAt(led_pos.index);
                MainWindow.ActionUpdateLedCount();

                led_pos.index = -1;
                led_index.Text = string.Empty;
            }
        }
    }
}
