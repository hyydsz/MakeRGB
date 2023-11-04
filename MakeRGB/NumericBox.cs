using System.Windows;
using System.Windows.Controls;

namespace MakeRGB
{
    [TemplatePart(Name = PART_NumberBox, Type = typeof(TextBox))]
    public class NumericBox : Control
    {
        private const string PART_NumberBox = "PART_NumberBox";
        private TextBox number_input;
        static NumericBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericBox), new FrameworkPropertyMetadata(typeof(NumericBox)));
        }

        public override void OnApplyTemplate()
        {
            number_input = GetTemplateChild("PART_NumberBox") as TextBox;
            number_input.TextChanged += Number_input_TextChanged;
            number_input.MouseWheel += Number_input_MouseWheel;
            number_input.PreviewKeyDown += Number_input_PreviewKeyDown;

            OnValueChange(Value);

            base.OnApplyTemplate();
        }

        private void Number_input_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!number_input.IsKeyboardFocused) {
                return;
            }

            switch (e.Key)
            {
                case System.Windows.Input.Key.Up:
                    ContinueChangeValue(true);
                    break;

                case System.Windows.Input.Key.Down:
                    ContinueChangeValue(false);
                    break;
            }
        }

        private void Number_input_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ContinueChangeValue(e.Delta >= 0);
        }

        private void OnValueChange(int newValue)
        {
            Value = newValue;

            if (OnNumberChanged != null)
            {
                OnNumberChanged(this, newValue);
            }

            string text = newValue.ToString();
            number_input.Text = text;   
        }
        private void ContinueChangeValue(bool Iscontinue)
        {
            var number = number_input.Text;
            int a = int.Parse(number);
            a += Iscontinue ? 1 : -1;

            OnValueChange(Clamp(a, Minimum, Maximum));
        }

        private void Number_input_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnNewValueEnter(number_input.Text);
        }

        private void OnNewValueEnter(string newValue)
        {
            if (number_input == null) return;

            if (int.TryParse(newValue, out int sb))
            {
                OnValueChange(Clamp(sb, Minimum, Maximum));
            }
            else
            {
                OnValueChange(0);
            }
        }

        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;

            return value;
        }

        public delegate void NumberChangedEventHandler(object sender, int newValue);
        public event NumberChangedEventHandler OnNumberChanged;

        public int Value
        {
            get {
                return (int)GetValue(ValueProperty); 
            }

            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
          DependencyProperty.Register("Value", typeof(int), typeof(NumericBox), new FrameworkPropertyMetadata(0, OnPropertyChange));

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
          DependencyProperty.Register("Maximum", typeof(int), typeof(NumericBox), new FrameworkPropertyMetadata(0, OnPropertyChange));

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
          DependencyProperty.Register("Minimum", typeof(int), typeof(NumericBox), new FrameworkPropertyMetadata(0, OnPropertyChange));

        private static void OnPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            NumericBox numeric = (NumericBox)sender;
            numeric.OnNewValueEnter(numeric.Value.ToString());
        }
    }
}
