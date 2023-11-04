using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MakeRGB
{
    public partial class MainWindow : Window
    {
        public static List<Saved> savedList = new List<Saved>();
        public static List<LED_Pos> led_positions = new List<LED_Pos>();
        public static Action ActionUpdateLedCount;

        private int width = 0;
        private int height = 0;

        private bool busy = false;

        private byte[] image_buffer;

        public MainWindow()
        {
            InitializeComponent();

            ActionUpdateLedCount = () => LED_Count.Text = $"灯珠: {led_positions.Count}个";
        }

        private void OnDevicePropertyChange(object sender, int newValue)
        {
            if ((Device_Width.Value == width &&
                Device_Height.Value == height) || busy) return;

            width = Device_Width.Value;
            height = Device_Height.Value;

            if (width == 0 || height == 0)
            {
                Rich_Box.Document.PageWidth = 0;

                led_positions.Clear();
                Rich_Box.Document.Blocks.Clear();

                return;
            }

            CreateLedTable();
            ActionUpdateLedCount();
        }

        private void CreateLedTable(List<LED_Pos> led_list = null)
        {
            led_positions.Clear();
            Rich_Box.Document.Blocks.Clear();

            // 创建一个列
            StackPanel colume = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                RenderTransform = getRenderScale()
            };

            // 创建一个顶部位置栏
            StackPanel pos = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            pos.Children.Add(new Border() { Width = 40 });

            // 遍历长度设置位置栏长度
            for (int x = 0; x < width; x++)
            {
                pos.Children.Add(CreatePosTextblock(x.ToString()));
            }

            colume.Children.Add(pos);

            // 遍历高度然后把ToggleButton直接放到表格里
            for (int y = 0; y < height; y++)
            {
                StackPanel LED = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };

                LED.Children.Add(CreatePosTextblock(y.ToString()));

                for (int x = 0; x < width; x++)
                {
                    LEDButton LED_Button = new LEDButton()
                    {
                        led_pos = new LED_Pos()
                        {
                            x = x,
                            y = y,
                            index = -1
                        },

                        AllowDrop = true
                    };

                    if (led_list != null)
                    {
                        var led = led_list.FirstOrDefault(item => item.x == x && item.y == y);
                        if (led != default(LED_Pos))
                        {
                            // 创建一个复制
                            LED_Pos led_copy = led;
                            led_copy.button = LED_Button;

                            // 将按钮实例传入
                            led_positions.Add(led_copy);

                            // 将按钮设置为按下
                            LED_Button.SetButton(led.index);
                        }
                    }

                    LED.Children.Add(LED_Button);
                }

                colume.Children.Add(LED);
            }

            led_positions = led_positions.OrderBy(item => item.index).ToList();

            Rich_Box.Document.PageWidth = (width + 2) * 40;
            Rich_Box.Document.Blocks.Add(new BlockUIContainer(colume));
        }

        private TextBlock CreatePosTextblock(string text)
        {
            return new TextBlock()
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 18,

                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                Width = 40,
                Height = 40,

                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
            };
        }

        private bool TestInput()
        {
            if (Product_Name.Text == string.Empty ||
                Display_Name.Text == string.Empty ||
                Brand.Text == string.Empty)
            {
                if (Product_Name.Text == string.Empty)
                {
                    Product_Name.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }

                if (Display_Name.Text == string.Empty)
                {
                    Display_Name.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }

                if (Brand.Text == string.Empty)
                {
                    Brand.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }

                return false;
            }

            Product_Name.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            Display_Name.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            Brand.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));

            return true;
        }

        private void Generate(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!TestInput())
                {
                    MessageBox.Show("信息填写不完整", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "请选择保存地址";
                saveFileDialog.InitialDirectory = "C:\\desktop";
                saveFileDialog.Filter = "Json|*.json";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = Display_Name.Text;

                if (saveFileDialog.ShowDialog().Value)
                {
                    busy = true;

                    string path = saveFileDialog.FileName;

                    JObject json = new JObject()
                    {
                        new JProperty("ProductName", Product_Name.Text),
                        new JProperty("DisplayName", Display_Name.Text),
                        new JProperty("Brand", Brand.Text),
                        new JProperty("Type", "custom"),
                        new JProperty("LedCount", led_positions.Count),
                        new JProperty("Width", width),
                        new JProperty("Height", height)
                    };

                    JArray LedMapping = new JArray();
                    JArray LedCoordinates = new JArray();
                    JArray LedNames = new JArray();

                    // 不放心所以加上一个排序
                    led_positions = led_positions.OrderBy(item => item.index).ToList();

                    for (int i = 0; i < led_positions.Count; i++)
                    {
                        LED_Pos item = led_positions[i];

                        LedMapping.Add(i);
                        LedCoordinates.Add(new JArray(item.x, item.y));
                        LedNames.Add($"Led{i + 1}");
                    }

                    json.Add(new JProperty("LedMapping", LedMapping));
                    json.Add(new JProperty("LedCoordinates", LedCoordinates));
                    json.Add(new JProperty("LedNames", LedNames));

                    if (image_buffer == null)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Title = "请选择一个图片";
                        openFileDialog.InitialDirectory = "C:\\desktop";
                        openFileDialog.Filter = "Image|*.png";
                        openFileDialog.RestoreDirectory = true;

                        if (openFileDialog.ShowDialog().Value)
                        {
                            string image_path = openFileDialog.FileName;

                            if (File.Exists(image_path))
                            {
                                byte[] data = File.ReadAllBytes(image_path);

                                json.Add("Image", Convert.ToBase64String(data));
                            }
                        }
                    }
                    else
                    {
                        json.Add("Image", Convert.ToBase64String(image_buffer));
                    }

                    File.WriteAllText(path, json.ToString());

                    if (MessageBox.Show("文件生成成功,是否要跳转到文件目录", "成功", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        Process.Start("Explorer", "/select," + path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现错误 \n" + ex.Message, "失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            busy = false;
        }

        private void ClearAll(object sender, RoutedEventArgs e)
        {
            led_positions.ForEach(item => item.button.ResetButton());
            led_positions.Clear();

            Product_Name.Text = string.Empty;
            Display_Name.Text= string.Empty;
            Brand.Text = string.Empty;

            Device_Width.Value = 0;
            Device_Height.Value = 0;

            Png_Show.Source = new BitmapImage();

            image_buffer = null;

            ActionUpdateLedCount();
        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "C:\\desktop";
            openFileDialog.Filter = "Json|*.json";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog().Value)
            {
                OpenFile(openFileDialog.FileName);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (busy)
                return;

            if (e.Data.GetData("FileDrop") is string[] path)
            {
                switch (Path.GetExtension(path[0]))
                {
                    case ".json":
                        OpenFile(path[0]);
                        break;

                    case ".png":
                        OpenPng(path[0], "png");
                        break;

                    case ".txt":
                        OpenPng(path[0], "txt");
                        break;

                    default:
                        MessageBox.Show("您正在拖入非json文件", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private void OpenPng(string path, string format)
        {
            switch (format)
            {
                case "png":
                    image_buffer = File.ReadAllBytes(path);

                    break;

                case "txt":
                    string base64_data = File.ReadAllText(path);
                    image_buffer = Convert.FromBase64String(base64_data);

                    break;
            }

            Png_Show.Source = bytesToImage(image_buffer);
        }

        private void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    busy = true;

                    string text = File.ReadAllText(path);
                    JObject json = JObject.Parse(text);

                    if (json.ContainsKey("ProductName"))
                    {
                        Product_Name.Text = json["ProductName"].ToString();
                    }
                    else Product_Name.Text = string.Empty;

                    if (json.ContainsKey("DisplayName"))
                    {
                        Display_Name.Text = json["DisplayName"].ToString();
                    }
                    else Display_Name.Text = string.Empty;

                    if (json.ContainsKey("Brand"))
                    {
                        Brand.Text = json["Brand"].ToString();
                    }
                    else Brand.Text = string.Empty;

                    width = (int)json["Width"];
                    height = (int)json["Height"];

                    Device_Width.Value = width;
                    Device_Height.Value = height;

                    List<LED_Pos> leds = new List<LED_Pos>();
                    foreach (JToken jToken in json["LedCoordinates"].ToArray())
                    {
                        LED_Pos pos = new LED_Pos();
                        pos.x = (int)jToken[0];
                        pos.y = (int)jToken[1];
                        pos.index = leds.Count;

                        leds.Add(pos);
                    }

                    CreateLedTable(leds);
                    ActionUpdateLedCount();

                    // 如果没有Image就清除画布
                    if (json.ContainsKey("Image"))
                    {
                        image_buffer = Convert.FromBase64String(json["Image"].ToString());
                        Png_Show.Source = bytesToImage(image_buffer);
                    }
                    else
                    {
                        Png_Show.Source = new BitmapImage();
                    }
                }
                catch
                {
                    MessageBox.Show("读取Json文件失败", "失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                busy = false;
            }
        }

        private BitmapImage bytesToImage(byte[] bytes)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        private void DownloadImage(object sender, MouseButtonEventArgs e)
        {
            if (image_buffer == null) return;

            var img = e.OriginalSource as Image;
            if (img == null) return;

            string tempPath = Path.GetTempPath();

            tempPath = Path.Combine(tempPath, "MakeRGB");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            string image_path = Path.Combine(tempPath, Product_Name.Text + ".png");

            File.WriteAllBytes(image_path, image_buffer);

            DataObject dataObject = new DataObject(DataFormats.FileDrop, new string[] { image_path });
            DragDrop.DoDragDrop(img, dataObject, DragDropEffects.Move);

            Directory.Delete(tempPath, true);
        }

        private void ScaleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Rich_Box == null) return;

            foreach (var stack in Rich_Box.Document.Blocks.OfType<BlockUIContainer>().Select(ui => ui.Child).OfType<StackPanel>())
            {
                stack.RenderTransform = getRenderScale();
            }
        }

        private ScaleTransform getRenderScale()
        {
            double value = ScaleSlider.Value / 100;
            return new ScaleTransform()
            {
                ScaleX = value,
                ScaleY = value,
            };
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            if (hyperlink.NavigateUri.ToString() == "donate")
            {
                donate donate = new donate();
                donate.Show();

                return;
            }

            Process.Start(new ProcessStartInfo(hyperlink.NavigateUri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }

        private void SaveToList(object sender, RoutedEventArgs e)
        {
            if (!TestInput())
            {
                MessageBox.Show("信息填写不完整", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (savedList.FirstOrDefault(item => item.DisplayName == Display_Name.Text) == default(Saved))
            {
                Saved saved = new Saved();
                saved.DeviceName = Product_Name.Text;
                saved.DisplayName = Display_Name.Text;
                saved.Brand = Brand.Text;

                saved.width = width;
                saved.height = height;

                saved.image_buffer = image_buffer;

                saved.led_positions = new List<LED_Pos>();
                led_positions.ForEach(item => saved.led_positions.Add(item));

                ComboBoxItem list = new ComboBoxItem();
                list.Content = saved.DisplayName;

                SaveBox.Items.Add(list);

                savedList.Add(saved);

                MessageBox.Show("保存完成", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("存在相同项", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteToList(object sender, RoutedEventArgs e)
        {
            if (SaveBox.SelectedIndex == -1)
                return;

            savedList.RemoveAt(SaveBox.SelectedIndex);
            SaveBox.Items.RemoveAt(SaveBox.SelectedIndex);
        }

        private void SaveBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveBox.SelectedIndex == -1)
                return;

            busy = true;

            Saved saved = savedList[SaveBox.SelectedIndex];

            Product_Name.Text = saved.DeviceName;
            Display_Name.Text = saved.DisplayName;
            Brand.Text = saved.Brand;

            width = saved.width;
            height = saved.height;

            Device_Width.Value = width;
            Device_Height.Value = height;

            image_buffer = saved.image_buffer;

            if (image_buffer != null)
                Png_Show.Source = bytesToImage(image_buffer);

            CreateLedTable(saved.led_positions);
            ActionUpdateLedCount();

            busy = false;
        }
    }

    public class LED_Pos
    {
        public int index;
        public int x;
        public int y;
        public LEDButton button;
    }

    public class Saved
    {
        public string DeviceName;
        public string DisplayName; // 检测
        public string Brand;

        public int width;
        public int height;

        public byte[] image_buffer;

        public List<LED_Pos> led_positions;
    }
}
