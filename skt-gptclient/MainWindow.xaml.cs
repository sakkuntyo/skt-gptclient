using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace skt_gptclient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///


    public partial class MainWindow : Window
    {
        string PreviewInput = ""; // 時間差で数秒前と変更が無いかを確認する
        string PreviewPreviewInput = ""; // 時間差で数秒前と変更が無いかを確認する
        string apiKey = "";
        JsonNode settingJson;
        ProgressBar ProgressBar = new ProgressBar();


        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(@".\settings"))
            {
                settingJson = JsonObject.Parse(File.ReadAllText(@".\settings"));
                if (settingJson["key"] != null)
                {
                    apiKey = settingJson["key"].ToString();
                }
            }
            this.Title = "gptclient";

            Grid MainGrid = new Grid();
            //cl0row0
            ColumnDefinition MainGridColumnDifinition0 = new ColumnDefinition();
            MainGridColumnDifinition0.Width = new GridLength(5, GridUnitType.Star);
            MainGrid.ColumnDefinitions.Add(MainGridColumnDifinition0);
            ColumnDefinition MainGridColumnDifinition1 = new ColumnDefinition();
            MainGridColumnDifinition1.Width = new GridLength(5, GridUnitType.Star);
            MainGrid.ColumnDefinitions.Add(MainGridColumnDifinition1);
            RowDefinition MainGridRowDifinition0 = new RowDefinition();
            MainGridRowDifinition0.Height = GridLength.Auto;
            MainGrid.RowDefinitions.Add(MainGridRowDifinition0);
            RowDefinition MainGridRowDifinition1 = new RowDefinition();
            MainGridRowDifinition1.Height = new GridLength(7.5, GridUnitType.Star);
            MainGrid.RowDefinitions.Add(MainGridRowDifinition1);
            RowDefinition MainGridRowDifinition2 = new RowDefinition();
            MainGridRowDifinition2.Height = GridLength.Auto;
            MainGrid.RowDefinitions.Add(MainGridRowDifinition2);

            Content = MainGrid;

            //HeaderStackPanel
            StackPanel HeaderStackPanel = new StackPanel();
            MainGrid.Children.Add(HeaderStackPanel);
            Grid.SetRow(HeaderStackPanel, 0);

            TextBlock ChatGPTAPIKey = new TextBlock();
            ChatGPTAPIKey.Text = "ChatGPT API Key";
            HeaderStackPanel.Children.Add(ChatGPTAPIKey);
            Grid.SetColumn(ChatGPTAPIKey, 0); Grid.SetRow(ChatGPTAPIKey, 0);
            PasswordBox ChatGPTAPIKeyPWBOX = new PasswordBox() { Password = apiKey };
            HeaderStackPanel.Children.Add(ChatGPTAPIKeyPWBOX);

            ComboBox ModelComboBox = new ComboBox();
            ModelComboBox.Items.Add(new ComboBoxItem() { Content = "gpt-4o" });
            ModelComboBox.Items.Add(new ComboBoxItem() { Content = "gpt-4-turbo" });
            ModelComboBox.Items.Add(new ComboBoxItem() { Content = "gpt-4-vision-preview" });
            ModelComboBox.Items.Add(new ComboBoxItem() { Content = "gpt-3.5-turbo", });
            ModelComboBox.SelectedIndex = 0;
            HeaderStackPanel.Children.Add(ModelComboBox);

            ComboBox TopicComboBox = new ComboBox();
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "Translate to English" });
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "Translate to Chinese" });
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "Translate to Japanese" });
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "Free topic" });
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "-" });
            TopicComboBox.SelectedIndex = 0;
            HeaderStackPanel.Children.Add(TopicComboBox);

            TextBox FreeFormTopicTextBlock = new TextBox();
            FreeFormTopicTextBlock.Text = "Free topic";
            FreeFormTopicTextBlock.Visibility = Visibility.Hidden;
            HeaderStackPanel.Children.Add(FreeFormTopicTextBlock);

            void TopicComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
                if (((ComboBox)sender).SelectedItem.ToString().Split("System.Windows.Controls.ComboBoxItem: ")[1] == "Free topic")
                {
                    FreeFormTopicTextBlock.Visibility = Visibility.Visible;
                }
                else {
                    FreeFormTopicTextBlock.Visibility = Visibility.Hidden;
                }
            }

            TopicComboBox.SelectionChanged += TopicComboBox_SelectionChanged;

            TextBox InputTextBox = new TextBox();
            InputTextBox.AcceptsReturn = true;
            InputTextBox.TextWrapping = TextWrapping.Wrap;
            InputTextBox.Text = "";
            MainGrid.Children.Add(InputTextBox);
            Grid.SetColumn(InputTextBox, 0); Grid.SetRow(InputTextBox, 1);

            TextBox OutputTextBox = new TextBox();
            OutputTextBox.IsReadOnly = true;
            OutputTextBox.TextWrapping = TextWrapping.Wrap;
            OutputTextBox.Text = "Please enter your ChatGPT API key." +
                "\nThen enter your questions or inquiries on the left side.";
            MainGrid.Children.Add(OutputTextBox);
            Grid.SetColumn(OutputTextBox, 1); Grid.SetRow(OutputTextBox, 1);

            async void InputTextBox_TextChanged(object sender, RoutedEventArgs e)
            {

                string Model = "";
                string Topic = "";
                string Input = "";
                Model = ModelComboBox.SelectedItem.ToString().Split("System.Windows.Controls.ComboBoxItem: ")[1];
                if (TopicComboBox.SelectedItem.ToString().Split("System.Windows.Controls.ComboBoxItem: ")[1] == "Free topic")
                {
                    Topic = FreeFormTopicTextBlock.Text;
                }
                else
                {
                    Topic = TopicComboBox.SelectedItem.ToString().Split("System.Windows.Controls.ComboBoxItem: ")[1];
                }
                if (TopicComboBox.SelectedItem.ToString().Split("System.Windows.Controls.ComboBoxItem: ")[1] == "-")
                {
                    Topic = "";
                }
                Input = InputTextBox.Text;

                new Thread(new ThreadStart(async () =>
                {
                    Thread.Sleep(1000);
                    this.Dispatcher.Invoke((Action)(async () =>
                    {
                        if (InputTextBox.Text != PreviewInput && InputTextBox.Text != PreviewPreviewInput) {
                            SaveHistory(Input);
                                return;
                        }
                        using (HttpClient client = new HttpClient())
                        {
                            ProgressBar.IsIndeterminate = true;
                            Input = Input.Replace("\n", "\\n"); // リクエスト用に修正
                            Input = Input.Replace("\r", "\\r"); // リクエスト用に修正
                            Input = Input.Replace("\"", "\\\""); // リクエスト用に修正
//                            MessageBox.Show($"{{\"model\":\"{Model}\",\"messages\": [{{ \"role\":\"user\",\"content\":\"{Topic}\\n{Input}\"}}],\"temperature\":0.7}}");
                            string requestBody = $"{{\"model\":\"{Model}\",\"messages\": [{{ \"role\":\"user\",\"content\":\"{Topic}\\n{Input}\"}}],\"temperature\":0.7}}";
                            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChatGPTAPIKeyPWBOX.Password);
                            HttpResponseMessage httpResponse = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                            var responseContentString = await httpResponse.Content.ReadAsStringAsync();
                            var responseJsonNode = JsonObject.Parse(responseContentString);
                            if (responseJsonNode["error"] != null)
                            {
                                if (responseJsonNode["error"]["type"].ToString() == "invalid_request_error")
                                {
                                    Debug.WriteLine("invalid_request_error");
                                    if (responseJsonNode["error"]["message"].ToString().Contains("Incorrect API key provided:") || responseJsonNode["error"]["message"].ToString().Contains("You didn't provide an API key."))
                                    {
                                        MessageBox.Show("The ChatGPT API key is either incorrect or has not been entered." + "\n" + "Error: " + responseJsonNode["error"]["message"].ToString());
                                    }
                                    else {
                                        MessageBox.Show("Error: " + responseJsonNode["error"]["message"].ToString());
                                    }
                                }
                                SaveHistory(Input);
                                return;
                            }
                            OutputTextBox.Text = responseJsonNode["choices"][0]["message"]["content"].ToString();
                            File.WriteAllText(@".\settings", $"{{\"key\": \"{ChatGPTAPIKeyPWBOX.Password}\"}}");
                            SaveHistory(Input);
                        }
                    }));
                })).Start();
            };
            InputTextBox.TextChanged += InputTextBox_TextChanged;

            ProgressBar.Height = 10;
            MainGrid.Children.Add(ProgressBar);
            Grid.SetColumn(ProgressBar,2);Grid.SetRow(ProgressBar, 2);
        }

        private void SaveHistory(string nowInput)
        {
            PreviewPreviewInput = PreviewInput;
            PreviewInput = nowInput;
            ProgressBar.IsIndeterminate = false;
        }

        private void TopicComboBox_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}