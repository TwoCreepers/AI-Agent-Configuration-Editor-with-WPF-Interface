using Microsoft.Win32;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AIAgentConfigurationEditorwithWPFInterface
{
    public class VirtualPetConfig
    {
#pragma warning disable IDE1006 // 命名样式
        public string record_temp_dir { get; set; }
        public string speak_temp_dir { get; set; }
        public string call_path { get; set; }
        public string music_dir { get; set; }
        public string program_dir { get; set; }
        public string name { get; set; }
        public string gif { get; set; }
        public string role_system { get; set; }
        public string mods { get; set; }
        public string tts_location { get; set; }
        public bool control_mijia { get; set; }
        public string tts_model { get; set; }
        public double volume { get; set; }
        public int sizes { get; set; }
        public int set_mindb { get; set; }
        public int wait_time { get; set; }
        public int chat_random { get; set; }
        public int v_chat_random { get; set; }
        public string user_token { get; set; }
        public string[] awake { get; set; }
        public string[] listen { get; set; }
        public string[] think { get; set; }
        public string[] click { get; set; }

        public VirtualPetConfig()
        {
            // 为字符串类型属性分配空字符串
            record_temp_dir = string.Empty;
            speak_temp_dir = string.Empty;
            call_path = string.Empty;
            music_dir = string.Empty;
            program_dir = string.Empty;
            name = string.Empty;
            gif = string.Empty;
            role_system = string.Empty;
            mods = string.Empty;
            tts_location = string.Empty;
            tts_model = string.Empty;
            user_token = string.Empty;

            // 为布尔类型属性分配false
            control_mijia = false;

            // 为数值类型属性分配默认值
            volume = 1.0f; // 假设默认音量为最大音量的100%
            sizes = 0;
            set_mindb = 0;
            wait_time = 0;
            chat_random = 0;
            v_chat_random = 0;

            // 为字符串数组属性分配空数组
            awake = [];
            listen = [];
            think = [];
            click = [];
        }
#pragma warning restore IDE1006 // 命名样式
    }
    public class TTSConfig
    {
#pragma warning disable IDE1006 // 命名样式
        public string deflaut_character { get; set; }
        public Dictionary<string, string[]> characters_and_emotions { get; set; }
#pragma warning restore IDE1006 // 命名样式
        public TTSConfig()
        {
            deflaut_character = string.Empty;
            characters_and_emotions = [];
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool m_是否存在未保存的更改 = false;
        private FileInfo m_configFile = new("./setting.json");
        private VirtualPetConfig? virtualPetConfig = null;
        private readonly JsonSerializerOptions m_jsonOptions = new()
        {
            WriteIndented = true, // 设置为格式化输出
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 允许不转义 Unicode 字符
        };
        public MainWindow()
        {
            InitializeComponent();
            消除改动();
        }

        private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) =>
            // 只允许输入数字
            e.Handled = !int.TryParse(e.Text, out _);
        private void MyTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
            有改动();
        private void MyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            有改动();
        private void MySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            有改动();
        private void MyCheckBox_Click(object sender, RoutedEventArgs e) =>
             有改动();


        private void 有改动()
        {
            m_是否存在未保存的更改 = true;
            this.Title = this.Title.TrimEnd('*') + '*';
        }
        private void 消除改动()
        {
            m_是否存在未保存的更改 = false;
            this.Title = this.Title.TrimEnd('*');
        }
        private async Task UpdataDataToUI(FileInfo configFile)
        {
            if (virtualPetConfig == null)
            {
                return;
            }
            this.名称.Text = virtualPetConfig.name;
            this.AI人设.Text = virtualPetConfig.role_system;
            this.音量.Value = (virtualPetConfig.volume * 100);
            this.大小.Value = virtualPetConfig.sizes;
            this.录音阈值.Text = virtualPetConfig.set_mindb.ToString();
            this.等待停止录音时间.Text = virtualPetConfig.wait_time.ToString();
            this.聊天增强概率.Text = virtualPetConfig.chat_random.ToString();
            this.主动聊天概率.Text = virtualPetConfig.v_chat_random.ToString();
            this.角色外观.Text = virtualPetConfig.gif.ToString();
            this.语言模型.Text = virtualPetConfig.tts_model;

            if (virtualPetConfig.tts_location == "server")
            {
                this.语言模型位置.SelectedIndex = 0;
            }
            else if (virtualPetConfig.tts_location == "local")
            {
                this.语言模型位置.SelectedIndex = 1;
            }

            if (virtualPetConfig.mods == "free")
            {
                this.回答模式.SelectedIndex = 0;
            }
            else if (virtualPetConfig.mods == "call")
            {
                this.回答模式.SelectedIndex = 1;
            }

            this.是否启用统筹控制智能家居.IsChecked = virtualPetConfig.control_mijia;

            this.角色外观.Items.Clear();
            List<DirectoryInfo> 角色外观列表 = [];
            await Task.Run(() =>
            {
                Stack<DirectoryInfo> 待遍历目录 = new(configFile.Directory?.GetDirectories() ?? []);

                while (待遍历目录.Count > 0)
                {
                    var 当前目录 = 待遍历目录.Pop();
                    var 子目录 = 当前目录.GetDirectories();

                    // 检查是否存在"normal"和"other"目录
                    bool 存在normal = 子目录.Any(d => d.Name.Equals("normal", StringComparison.OrdinalIgnoreCase));
                    bool 存在other = 子目录.Any(d => d.Name.Equals("other", StringComparison.OrdinalIgnoreCase));

                    if (存在normal && 存在other)
                    {
                        角色外观列表.Add(当前目录);
                    }
                    else
                    {
                        // 将所有子目录加入到栈中，以便继续遍历
                        foreach (var dir in 子目录)
                        {
                            待遍历目录.Push(dir);
                        }
                    }
                }
            });
            foreach (var item in 角色外观列表)
            {
                this.角色外观.Items.Add(item);
            }
            this.角色外观.SelectedItem = virtualPetConfig.gif;

            this.语言模型.Items.Clear();
            var ttsConfig = JsonSerializer.Deserialize<TTSConfig>(File.ReadAllText(System.IO.Path.Combine(configFile.Directory?.ToString() ?? "./", "tools", "GPT-SoVITS-Inference", "trained", "character_info.json")));
            if (ttsConfig != null)
            {
                foreach (var item in ttsConfig.characters_and_emotions)
                {
                    this.语言模型.Items.Add(item.Key);
                }
            }
            this.语言模型.SelectedItem = virtualPetConfig.tts_model;

            this.唤醒文本.Text = string.Join('，', virtualPetConfig.awake);
            this.监听文本.Text = string.Join('，', virtualPetConfig.listen);
            this.思考文本.Text = string.Join('，', virtualPetConfig.think);
            this.点击文本.Text = string.Join('，', virtualPetConfig.click);
        }
        private void UpdataUIToData()
        {
            if (virtualPetConfig == null)
            {
                return;
            }
            virtualPetConfig.name = this.名称.Text;
            virtualPetConfig.role_system = this.AI人设.Text;
            virtualPetConfig.volume = (this.音量.Value / 100);
            virtualPetConfig.sizes = (int)this.大小.Value;
            virtualPetConfig.set_mindb = int.Parse(this.录音阈值.Text);
            virtualPetConfig.wait_time = int.Parse(this.等待停止录音时间.Text);
            virtualPetConfig.chat_random = int.Parse(this.聊天增强概率.Text);
            virtualPetConfig.v_chat_random = int.Parse(this.主动聊天概率.Text);

            virtualPetConfig.gif = this.角色外观.Text;
            virtualPetConfig.tts_model = this.语言模型.Text;

            if (this.语言模型位置.SelectedIndex == 0)
            {
                virtualPetConfig.tts_location = "server";
            }
            else if (this.语言模型位置.SelectedIndex == 1)
            {
                virtualPetConfig.tts_location = "local";
            }

            if (this.回答模式.SelectedIndex == 0)
            {
                virtualPetConfig.mods = "free";
            }
            else if (this.回答模式.SelectedIndex == 1)
            {
                virtualPetConfig.mods = "call";
            }

            virtualPetConfig.control_mijia = this.是否启用统筹控制智能家居.IsChecked ?? false;

            virtualPetConfig.awake = this.唤醒文本.Text.Split('，');
            virtualPetConfig.listen = this.监听文本.Text.Split('，');
            virtualPetConfig.think = this.思考文本.Text.Split('，');
            virtualPetConfig.click = this.点击文本.Text.Split('，');
        }
        private void Save(FileInfo file)
        {
            UpdataUIToData();
            System.IO.File.WriteAllText(file.FullName, JsonSerializer.Serialize(virtualPetConfig, m_jsonOptions));
            消除改动();
        }
        private async void OpenConfigFile(FileInfo configFile)
        {
            var config = File.ReadAllText(configFile.FullName);
            m_configFile = configFile;
            virtualPetConfig = JsonSerializer.Deserialize<VirtualPetConfig>(config);
            await UpdataDataToUI(configFile); // 防止UI长时间未响应
            消除改动();
        }

        private void OpenFileCommand(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "json文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                FilterIndex = 1,
                FileName = "setting"
            };
            var result = openFileDialog.ShowDialog();
            result ??= false;
            if (!result.Value)
            {
                return;
            }
            OpenConfigFile(new FileInfo(openFileDialog.FileName));
        }
        private void SaveFileCommand(object sender, RoutedEventArgs e)
        {
            Save(m_configFile);
        }
        private void SaveAsFileCommand(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "json文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                FilterIndex = 1,
                FileName = "setting"
            };
            var result = saveFileDialog.ShowDialog();
            result ??= false;
            if (!result.Value)
            {
                return;
            }
            FileInfo filePath = new(saveFileDialog.FileName);
            if (string.IsNullOrEmpty(filePath.Extension))
                filePath = new(System.IO.Path.ChangeExtension(filePath.FullName, ".json"));
            Save(filePath);
        }
        private void ExitCommand(object sender, RoutedEventArgs e)
        {
            if (!m_是否存在未保存的更改)
            {
                Application.Current.Shutdown();
                return;
            }
            var result = MessageBox.Show(
            "你的更改尚未保存，是否保存？",
            "提示",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning
            );
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Application.Current.Shutdown();
                    break;
                case MessageBoxResult.No:
                    Application.Current.Shutdown();
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }
    }
}