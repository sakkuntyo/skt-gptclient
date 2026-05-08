using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
        private static readonly string[] DefaultTopics =
        {
            "Translate to English",
            "Translate to Chinese",
            "Translate to Japanese"
        };

        private static readonly string[] SupportedModels =
        {
            "gpt-5.5",
            "gpt-5.4",
            "gpt-5.4-mini",
            "gpt-5.4-nano",
            "gpt-4.1"
        };

        private const string SettingsFilePath = @".\settings";

        string PreviewInput = ""; // 時間差で数秒前と変更が無いかを確認する
        string PreviewPreviewInput = ""; // 時間差で数秒前と変更が無いかを確認する
        string apiKey = "";
        JsonObject settingJson = new JsonObject();
        List<string> customTopics = new List<string>();
        ProgressBar ProgressBar = new ProgressBar();


        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
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

            Grid HeaderGrid = new Grid();
            HeaderGrid.Margin = new Thickness(0, 0, 4, 0);
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            MainGrid.Children.Add(HeaderGrid);
            Grid.SetRow(HeaderGrid, 0);

            TextBlock ChatGPTAPIKey = new TextBlock();
            ChatGPTAPIKey.Text = "ChatGPT API Key";
            HeaderGrid.Children.Add(ChatGPTAPIKey);
            Grid.SetColumn(ChatGPTAPIKey, 0); Grid.SetRow(ChatGPTAPIKey, 0);
            PasswordBox ChatGPTAPIKeyPWBOX = new PasswordBox() { Password = apiKey };
            HeaderGrid.Children.Add(ChatGPTAPIKeyPWBOX);
            Grid.SetColumn(ChatGPTAPIKeyPWBOX, 0); Grid.SetRow(ChatGPTAPIKeyPWBOX, 1);
            Grid.SetColumnSpan(ChatGPTAPIKeyPWBOX, 2);

            ComboBox ModelComboBox = new ComboBox();
            foreach (string supportedModel in SupportedModels)
            {
                ModelComboBox.Items.Add(new ComboBoxItem() { Content = supportedModel });
            }
            ModelComboBox.SelectedIndex = 0;
            HeaderGrid.Children.Add(ModelComboBox);
            Grid.SetColumn(ModelComboBox, 0); Grid.SetRow(ModelComboBox, 2);
            Grid.SetColumnSpan(ModelComboBox, 2);

            ComboBox TopicComboBox = new ComboBox();
            PopulateTopicComboBox(TopicComboBox);
            TopicComboBox.SelectedIndex = 0;
            TopicComboBox.Margin = new Thickness(0, 0, 8, 0);
            HeaderGrid.Children.Add(TopicComboBox);
            Grid.SetColumn(TopicComboBox, 0); Grid.SetRow(TopicComboBox, 3);

            TextBox FreeFormTopicTextBlock = new TextBox();
            FreeFormTopicTextBlock.Text = "";
            FreeFormTopicTextBlock.Visibility = Visibility.Hidden;
            FreeFormTopicTextBlock.Margin = new Thickness(0, 0, 8, 0);
            HeaderGrid.Children.Add(FreeFormTopicTextBlock);
            Grid.SetColumn(FreeFormTopicTextBlock, 0); Grid.SetRow(FreeFormTopicTextBlock, 4);

            Button AddTopicButton = new Button();
            AddTopicButton.Content = "Add topic";
            AddTopicButton.Visibility = Visibility.Hidden;
            AddTopicButton.MinWidth = 88;
            HeaderGrid.Children.Add(AddTopicButton);
            Grid.SetColumn(AddTopicButton, 1); Grid.SetRow(AddTopicButton, 4);

            Button DeleteTopicButton = new Button();
            DeleteTopicButton.Content = "Delete topic";
            DeleteTopicButton.Visibility = Visibility.Hidden;
            DeleteTopicButton.MinWidth = 88;
            HeaderGrid.Children.Add(DeleteTopicButton);
            Grid.SetColumn(DeleteTopicButton, 1); Grid.SetRow(DeleteTopicButton, 3);

            void TopicComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ToggleFreeTopicPanelVisibility(TopicComboBox, FreeFormTopicTextBlock, AddTopicButton);
                ToggleDeleteTopicButtonVisibility(TopicComboBox, DeleteTopicButton);
            }

            TopicComboBox.SelectionChanged += TopicComboBox_SelectionChanged;

            void AddTopicButton_Click(object sender, RoutedEventArgs e)
            {
                string newTopic = FreeFormTopicTextBlock.Text.Trim();
                if (string.IsNullOrWhiteSpace(newTopic))
                {
                    MessageBox.Show("Please enter a topic before adding it.");
                    return;
                }

                AddCustomTopic(TopicComboBox, newTopic);
                ComboBoxItem? addedTopicItem = FindTopicComboBoxItem(TopicComboBox, newTopic);
                if (addedTopicItem != null)
                {
                    TopicComboBox.SelectedItem = addedTopicItem;
                }
                FreeFormTopicTextBlock.Text = "";
            }

            AddTopicButton.Click += AddTopicButton_Click;

            void DeleteTopicButton_Click(object sender, RoutedEventArgs e)
            {
                string selectedTopic = GetSelectedComboBoxItem(TopicComboBox);
                if (!customTopics.Contains(selectedTopic))
                {
                    return;
                }

                RemoveCustomTopic(TopicComboBox, selectedTopic);
                TopicComboBox.SelectedIndex = 0;
            }

            DeleteTopicButton.Click += DeleteTopicButton_Click;

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
                string model = GetSelectedComboBoxItem(ModelComboBox);
                string topic = GetSelectedTopic(TopicComboBox, FreeFormTopicTextBlock);
                string input = InputTextBox.Text;

                new Thread(new ThreadStart(async () =>
                {
                    Thread.Sleep(1000);
                    this.Dispatcher.Invoke((Action)(async () =>
                    {
                        if (InputTextBox.Text != PreviewInput && InputTextBox.Text != PreviewPreviewInput)
                        {
                            SaveHistory(input);
                            return;
                        }
                        await RequestResponseAsync(model, topic, input, ChatGPTAPIKeyPWBOX, OutputTextBox);
                    }));
                })).Start();
            };
            InputTextBox.TextChanged += InputTextBox_TextChanged;

            async void InputTextBox_Paste(object sender, DataObjectPastingEventArgs e)
            {
                string model = GetSelectedComboBoxItem(ModelComboBox);
                string topic = GetSelectedTopic(TopicComboBox, FreeFormTopicTextBlock);
                string input = "";
                if (e.DataObject.GetDataPresent(typeof(string)))
                {
                    // ペーストされたデータが文字列として取得できる場合
                    // ペーストされたテキストを取得します。
                    input = (string?)e.DataObject.GetData(typeof(string)) ?? string.Empty;
                }

                new Thread(new ThreadStart(async () =>
                {
                    this.Dispatcher.Invoke((Action)(async () =>
                    {
                        await RequestResponseAsync(model, topic, input, ChatGPTAPIKeyPWBOX, OutputTextBox);
                    }));
                })).Start();
            };
            DataObject.AddPastingHandler(InputTextBox, InputTextBox_Paste);

            ProgressBar.Height = 10;
            MainGrid.Children.Add(ProgressBar);
            Grid.SetColumn(ProgressBar, 2); Grid.SetRow(ProgressBar, 2);
        }

        private string GetSelectedComboBoxItem(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem comboBoxItem)
            {
                return comboBoxItem.Content?.ToString() ?? "";
            }

            return "";
        }

        private string GetSelectedTopic(ComboBox topicComboBox, TextBox freeFormTopicTextBlock)
        {
            string selectedTopic = GetSelectedComboBoxItem(topicComboBox);
            if (selectedTopic == "Free topic")
            {
                return freeFormTopicTextBlock.Text;
            }

            if (selectedTopic == "-")
            {
                return "";
            }

            return selectedTopic;
        }

        private async Task RequestResponseAsync(string model, string topic, string input, PasswordBox chatGptApiKeyPwBox, TextBox outputTextBox)
        {
            using (HttpClient client = new HttpClient())
            {
                ProgressBar.IsIndeterminate = true;

                JsonObject requestBody = new JsonObject
                {
                    ["model"] = model,
                    ["input"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["role"] = "user",
                            ["content"] = new JsonArray
                            {
                                new JsonObject
                                {
                                    ["type"] = "input_text",
                                    ["text"] = BuildPrompt(topic, input)
                                }
                            }
                        }
                    }
                };

                var content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", chatGptApiKeyPwBox.Password);

                HttpResponseMessage httpResponse = await client.PostAsync("https://api.openai.com/v1/responses", content);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();
                var responseJsonNode = JsonNode.Parse(responseContentString);

                if (responseJsonNode?["error"] != null)
                {
                    ShowApiError(responseJsonNode["error"]);
                    SaveHistory(input);
                    return;
                }

                string outputText = ExtractOutputText(responseJsonNode);
                if (string.IsNullOrWhiteSpace(outputText))
                {
                    MessageBox.Show("The model response did not include text output.");
                    SaveHistory(input);
                    return;
                }

                outputTextBox.Text = outputText;

                SaveSettings(chatGptApiKeyPwBox.Password);
                SaveHistory(input);
            }
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return;
            }

            JsonNode? loadedSettings = JsonNode.Parse(File.ReadAllText(SettingsFilePath));
            if (loadedSettings is not JsonObject loadedSettingsObject)
            {
                return;
            }

            settingJson = loadedSettingsObject;
            apiKey = settingJson["key"]?.ToString() ?? "";

            JsonArray? savedTopics = settingJson["topics"]?.AsArray();
            if (savedTopics == null)
            {
                return;
            }

            foreach (JsonNode? savedTopic in savedTopics)
            {
                string topic = savedTopic?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(topic) && !customTopics.Contains(topic))
                {
                    customTopics.Add(topic);
                }
            }
        }

        private void PopulateTopicComboBox(ComboBox topicComboBox)
        {
            topicComboBox.Items.Clear();

            foreach (string defaultTopic in DefaultTopics)
            {
                topicComboBox.Items.Add(new ComboBoxItem() { Content = defaultTopic });
            }

            foreach (string customTopic in customTopics)
            {
                topicComboBox.Items.Add(new ComboBoxItem() { Content = customTopic });
            }

            topicComboBox.Items.Add(new ComboBoxItem() { Content = "Free topic" });
            topicComboBox.Items.Add(new ComboBoxItem() { Content = "-" });
        }

        private string BuildPrompt(string topic, string input)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return input;
            }

            return topic + "\n" + input;
        }

        private void ToggleFreeTopicPanelVisibility(ComboBox topicComboBox, TextBox freeFormTopicTextBox, Button addTopicButton)
        {
            if (GetSelectedComboBoxItem(topicComboBox) == "Free topic")
            {
                freeFormTopicTextBox.Visibility = Visibility.Visible;
                addTopicButton.Visibility = Visibility.Visible;
                return;
            }

            freeFormTopicTextBox.Visibility = Visibility.Hidden;
            addTopicButton.Visibility = Visibility.Hidden;
        }

        private void ToggleDeleteTopicButtonVisibility(ComboBox topicComboBox, Button deleteTopicButton)
        {
            string selectedTopic = GetSelectedComboBoxItem(topicComboBox);
            if (customTopics.Contains(selectedTopic))
            {
                deleteTopicButton.Visibility = Visibility.Visible;
                return;
            }

            deleteTopicButton.Visibility = Visibility.Hidden;
        }

        private void AddCustomTopic(ComboBox topicComboBox, string topic)
        {
            if (DefaultTopics.Contains(topic) || customTopics.Contains(topic))
            {
                return;
            }

            customTopics.Add(topic);

            ComboBoxItem? freeTopicItem = FindTopicComboBoxItem(topicComboBox, "Free topic");
            int insertIndex = freeTopicItem == null ? -1 : topicComboBox.Items.IndexOf(freeTopicItem);
            if (insertIndex < 0)
            {
                insertIndex = topicComboBox.Items.Count;
            }

            topicComboBox.Items.Insert(insertIndex, new ComboBoxItem() { Content = topic });
            SaveSettings();
        }

        private void RemoveCustomTopic(ComboBox topicComboBox, string topic)
        {
            if (!customTopics.Remove(topic))
            {
                return;
            }

            SaveSettings();

            ComboBoxItem? topicItem = FindTopicComboBoxItem(topicComboBox, topic);
            if (topicItem != null)
            {
                topicComboBox.Items.Remove(topicItem);
            }
        }

        private ComboBoxItem? FindTopicComboBoxItem(ComboBox topicComboBox, string topic)
        {
            foreach (object item in topicComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content?.ToString() == topic)
                {
                    return comboBoxItem;
                }
            }

            return null;
        }

        private void SaveSettings(string? newApiKey = null)
        {
            if (newApiKey != null)
            {
                apiKey = newApiKey;
            }

            JsonArray topicsJsonArray = new JsonArray();
            foreach (string topic in customTopics)
            {
                topicsJsonArray.Add(topic);
            }

            settingJson["key"] = apiKey;
            settingJson["topics"] = topicsJsonArray;

            File.WriteAllText(SettingsFilePath, settingJson.ToJsonString(new JsonSerializerOptions()));
        }

        private void ShowApiError(JsonNode? errorNode)
        {
            string errorType = errorNode?["type"]?.ToString() ?? "";
            string errorMessage = errorNode?["message"]?.ToString() ?? "Unknown API error.";

            if (errorType == "invalid_request_error" &&
                (errorMessage.Contains("Incorrect API key provided:") || errorMessage.Contains("You didn't provide an API key.")))
            {
                MessageBox.Show("The ChatGPT API key is either incorrect or has not been entered." + "\n" + "Error: " + errorMessage);
                return;
            }

            MessageBox.Show("Error: " + errorMessage);
        }

        private string ExtractOutputText(JsonNode? responseJsonNode)
        {
            List<string> outputTexts = new List<string>();
            JsonArray? outputArray = responseJsonNode?["output"]?.AsArray();
            if (outputArray == null)
            {
                return "";
            }

            foreach (JsonNode? outputItem in outputArray)
            {
                if (outputItem?["type"]?.ToString() != "message")
                {
                    continue;
                }

                JsonArray? contentArray = outputItem["content"]?.AsArray();
                if (contentArray == null)
                {
                    continue;
                }

                foreach (JsonNode? contentItem in contentArray)
                {
                    if (contentItem?["type"]?.ToString() == "output_text")
                    {
                        string? text = contentItem["text"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            outputTexts.Add(text);
                        }
                    }
                }
            }

            return string.Join(Environment.NewLine, outputTexts);
        }

        private void SaveHistory(string nowInput)
        {
            PreviewPreviewInput = PreviewInput;
            PreviewInput = nowInput;
            ProgressBar.IsIndeterminate = false;
        }
    }
}
