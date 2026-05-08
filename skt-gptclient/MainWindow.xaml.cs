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

        private static readonly string[] DefaultModels =
        {
            "gpt-5.5",
            "gpt-5.4",
            "gpt-5.4-mini",
            "gpt-5.4-nano",
            "gpt-4.1"
        };

        private const string SettingsFilePath = @".\settings";
        private const string DefaultEndpoint = "https://api.openai.com/v1/responses";

        string PreviewInput = ""; // 時間差で数秒前と変更が無いかを確認する
        string PreviewPreviewInput = ""; // 時間差で数秒前と変更が無いかを確認する
        string apiKey = "";
        string apiEndpoint = DefaultEndpoint;
        string selectedModel = DefaultModels[0];
        string selectedTopic = DefaultTopics[0];
        JsonObject settingJson = new JsonObject();
        List<string> customModels = new List<string>();
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
            MainGridRowDifinition1.Height = GridLength.Auto;
            MainGrid.RowDefinitions.Add(MainGridRowDifinition1);
            RowDefinition MainGridRowDifinition2 = new RowDefinition();
            MainGridRowDifinition2.Height = new GridLength(7.5, GridUnitType.Star);
            MainGrid.RowDefinitions.Add(MainGridRowDifinition2);
            RowDefinition MainGridRowDifinition3 = new RowDefinition();
            MainGridRowDifinition3.Height = GridLength.Auto;
            MainGrid.RowDefinitions.Add(MainGridRowDifinition3);

            Menu MainMenu = new Menu();
            MainGrid.Children.Add(MainMenu);
            Grid.SetRow(MainMenu, 0);
            Grid.SetColumnSpan(MainMenu, 2);

            MenuItem SettingsMenuItem = new MenuItem();
            SettingsMenuItem.Header = "_Settings";
            MainMenu.Items.Add(SettingsMenuItem);

            MenuItem ApiSettingsMenuItem = new MenuItem();
            ApiSettingsMenuItem.Header = "API _Settings...";
            SettingsMenuItem.Items.Add(ApiSettingsMenuItem);

            Content = MainGrid;

            Grid HeaderGrid = new Grid();
            HeaderGrid.Margin = new Thickness(0, 0, 4, 0);
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            HeaderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            MainGrid.Children.Add(HeaderGrid);
            Grid.SetRow(HeaderGrid, 1);

            void ApiSettingsMenuItem_Click(object sender, RoutedEventArgs e)
            {
                ShowApiSettingsDialog();
            }

            ApiSettingsMenuItem.Click += ApiSettingsMenuItem_Click;

            ComboBox ModelComboBox = new ComboBox();
            PopulateModelComboBox(ModelComboBox);
            ModelComboBox.Margin = new Thickness(0, 0, 8, 0);
            HeaderGrid.Children.Add(ModelComboBox);
            Grid.SetColumn(ModelComboBox, 0); Grid.SetRow(ModelComboBox, 0);

            Button DeleteModelButton = new Button();
            DeleteModelButton.Content = "Delete model";
            DeleteModelButton.Visibility = Visibility.Collapsed;
            DeleteModelButton.MinWidth = 88;
            HeaderGrid.Children.Add(DeleteModelButton);
            Grid.SetColumn(DeleteModelButton, 1); Grid.SetRow(DeleteModelButton, 0);

            TextBox FreeFormModelTextBlock = new TextBox();
            FreeFormModelTextBlock.Text = "";
            FreeFormModelTextBlock.Visibility = Visibility.Collapsed;
            FreeFormModelTextBlock.Margin = new Thickness(0, 0, 8, 0);
            HeaderGrid.Children.Add(FreeFormModelTextBlock);
            Grid.SetColumn(FreeFormModelTextBlock, 0); Grid.SetRow(FreeFormModelTextBlock, 1);

            Button AddModelButton = new Button();
            AddModelButton.Content = "Add model";
            AddModelButton.Visibility = Visibility.Collapsed;
            AddModelButton.MinWidth = 88;
            HeaderGrid.Children.Add(AddModelButton);
            Grid.SetColumn(AddModelButton, 1); Grid.SetRow(AddModelButton, 1);

            ComboBox TopicComboBox = new ComboBox();
            PopulateTopicComboBox(TopicComboBox);
            TopicComboBox.Margin = new Thickness(0, 0, 8, 0);
            HeaderGrid.Children.Add(TopicComboBox);
            Grid.SetColumn(TopicComboBox, 0); Grid.SetRow(TopicComboBox, 2);

            TextBox FreeFormTopicTextBlock = new TextBox();
            FreeFormTopicTextBlock.Text = "";
            FreeFormTopicTextBlock.Visibility = Visibility.Collapsed;
            FreeFormTopicTextBlock.Margin = new Thickness(0, 0, 8, 0);
            HeaderGrid.Children.Add(FreeFormTopicTextBlock);
            Grid.SetColumn(FreeFormTopicTextBlock, 0); Grid.SetRow(FreeFormTopicTextBlock, 3);

            Button AddTopicButton = new Button();
            AddTopicButton.Content = "Add topic";
            AddTopicButton.Visibility = Visibility.Collapsed;
            AddTopicButton.MinWidth = 88;
            HeaderGrid.Children.Add(AddTopicButton);
            Grid.SetColumn(AddTopicButton, 1); Grid.SetRow(AddTopicButton, 3);

            Button DeleteTopicButton = new Button();
            DeleteTopicButton.Content = "Delete topic";
            DeleteTopicButton.Visibility = Visibility.Collapsed;
            DeleteTopicButton.MinWidth = 88;
            HeaderGrid.Children.Add(DeleteTopicButton);
            Grid.SetColumn(DeleteTopicButton, 1); Grid.SetRow(DeleteTopicButton, 2);

            void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ToggleFreeModelPanelVisibility(ModelComboBox, FreeFormModelTextBlock, AddModelButton);
                ToggleDeleteModelButtonVisibility(ModelComboBox, DeleteModelButton);
                selectedModel = GetSelectedComboBoxItem(ModelComboBox);
                SaveSettings();
            }

            ModelComboBox.SelectionChanged += ModelComboBox_SelectionChanged;

            void AddModelButton_Click(object sender, RoutedEventArgs e)
            {
                string newModel = FreeFormModelTextBlock.Text.Trim();
                if (string.IsNullOrWhiteSpace(newModel))
                {
                    MessageBox.Show("Please enter a model before adding it.");
                    return;
                }

                AddCustomModel(ModelComboBox, newModel);
                ComboBoxItem? addedModelItem = FindModelComboBoxItem(ModelComboBox, newModel);
                if (addedModelItem != null)
                {
                    ModelComboBox.SelectedItem = addedModelItem;
                }
                FreeFormModelTextBlock.Text = "";
            }

            AddModelButton.Click += AddModelButton_Click;

            void DeleteModelButton_Click(object sender, RoutedEventArgs e)
            {
                string selectedModel = GetSelectedComboBoxItem(ModelComboBox);
                if (!customModels.Contains(selectedModel))
                {
                    return;
                }

                RemoveCustomModel(ModelComboBox, selectedModel);
                ModelComboBox.SelectedIndex = 0;
            }

            DeleteModelButton.Click += DeleteModelButton_Click;

            void TopicComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ToggleFreeTopicPanelVisibility(TopicComboBox, FreeFormTopicTextBlock, AddTopicButton);
                ToggleDeleteTopicButtonVisibility(TopicComboBox, DeleteTopicButton);
                selectedTopic = GetSelectedComboBoxItem(TopicComboBox);
                SaveSettings();
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

            RestoreModelSelection(ModelComboBox, FreeFormModelTextBlock);
            RestoreTopicSelection(TopicComboBox, FreeFormTopicTextBlock);

            TextBox InputTextBox = new TextBox();
            InputTextBox.AcceptsReturn = true;
            InputTextBox.TextWrapping = TextWrapping.Wrap;
            InputTextBox.Text = "";
            MainGrid.Children.Add(InputTextBox);
            Grid.SetColumn(InputTextBox, 0); Grid.SetRow(InputTextBox, 2);

            TextBox OutputTextBox = new TextBox();
            OutputTextBox.IsReadOnly = true;
            OutputTextBox.TextWrapping = TextWrapping.Wrap;
            OutputTextBox.Text = "Please enter your ChatGPT API key." +
                "\nThen enter your questions or inquiries on the left side.";
            MainGrid.Children.Add(OutputTextBox);
            Grid.SetColumn(OutputTextBox, 1); Grid.SetRow(OutputTextBox, 2);

            async void InputTextBox_TextChanged(object sender, RoutedEventArgs e)
            {
                string model = GetSelectedModel(ModelComboBox, FreeFormModelTextBlock);
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
                        await RequestResponseAsync(model, topic, input, OutputTextBox);
                    }));
                })).Start();
            };
            InputTextBox.TextChanged += InputTextBox_TextChanged;

            async void InputTextBox_Paste(object sender, DataObjectPastingEventArgs e)
            {
                string model = GetSelectedModel(ModelComboBox, FreeFormModelTextBlock);
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
                        await RequestResponseAsync(model, topic, input, OutputTextBox);
                    }));
                })).Start();
            };
            DataObject.AddPastingHandler(InputTextBox, InputTextBox_Paste);

            ProgressBar.Height = 10;
            MainGrid.Children.Add(ProgressBar);
            Grid.SetColumnSpan(ProgressBar, 2); Grid.SetRow(ProgressBar, 3);
        }

        private string GetSelectedComboBoxItem(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem comboBoxItem)
            {
                return comboBoxItem.Content?.ToString() ?? "";
            }

            return "";
        }

        private string GetSelectedModel(ComboBox modelComboBox, TextBox freeFormModelTextBlock)
        {
            string selectedModel = GetSelectedComboBoxItem(modelComboBox);
            if (selectedModel == "Custom model")
            {
                return freeFormModelTextBlock.Text.Trim();
            }

            return selectedModel;
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

        private async Task RequestResponseAsync(string model, string topic, string input, TextBox outputTextBox)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                SaveHistory(input);
                return;
            }

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
                ConfigureAuthenticationHeaders(client, apiKey);

                HttpResponseMessage httpResponse = await client.PostAsync(apiEndpoint, content);
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

                SaveSettings();
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
            apiEndpoint = settingJson["endpoint"]?.ToString() ?? DefaultEndpoint;
            selectedModel = settingJson["selected_model"]?.ToString() ?? DefaultModels[0];
            selectedTopic = settingJson["selected_topic"]?.ToString() ?? DefaultTopics[0];

            JsonArray? savedModels = settingJson["models"]?.AsArray();
            if (savedModels != null)
            {
                foreach (JsonNode? savedModel in savedModels)
                {
                    string model = savedModel?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(model) && !customModels.Contains(model))
                    {
                        customModels.Add(model);
                    }
                }
            }

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

        private void PopulateModelComboBox(ComboBox modelComboBox)
        {
            modelComboBox.Items.Clear();

            foreach (string defaultModel in DefaultModels)
            {
                modelComboBox.Items.Add(new ComboBoxItem() { Content = defaultModel });
            }

            foreach (string customModel in customModels)
            {
                modelComboBox.Items.Add(new ComboBoxItem() { Content = customModel });
            }

            modelComboBox.Items.Add(new ComboBoxItem() { Content = "Custom model" });
        }

        private void RestoreModelSelection(ComboBox modelComboBox, TextBox freeFormModelTextBlock)
        {
            ComboBoxItem? selectedModelItem = FindModelComboBoxItem(modelComboBox, selectedModel);
            if (selectedModelItem != null)
            {
                modelComboBox.SelectedItem = selectedModelItem;
                return;
            }

            if (!string.IsNullOrWhiteSpace(selectedModel))
            {
                ComboBoxItem? customModelItem = FindModelComboBoxItem(modelComboBox, "Custom model");
                if (customModelItem != null)
                {
                    modelComboBox.SelectedItem = customModelItem;
                    freeFormModelTextBlock.Text = selectedModel;
                    return;
                }
            }

            modelComboBox.SelectedIndex = 0;
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

        private void RestoreTopicSelection(ComboBox topicComboBox, TextBox freeFormTopicTextBlock)
        {
            ComboBoxItem? selectedTopicItem = FindTopicComboBoxItem(topicComboBox, selectedTopic);
            if (selectedTopicItem != null)
            {
                topicComboBox.SelectedItem = selectedTopicItem;
                return;
            }

            if (!string.IsNullOrWhiteSpace(selectedTopic))
            {
                ComboBoxItem? freeTopicItem = FindTopicComboBoxItem(topicComboBox, "Free topic");
                if (freeTopicItem != null)
                {
                    topicComboBox.SelectedItem = freeTopicItem;
                    freeFormTopicTextBlock.Text = selectedTopic;
                    return;
                }
            }

            topicComboBox.SelectedIndex = 0;
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

            freeFormTopicTextBox.Visibility = Visibility.Collapsed;
            addTopicButton.Visibility = Visibility.Collapsed;
        }

        private void ToggleFreeModelPanelVisibility(ComboBox modelComboBox, TextBox freeFormModelTextBox, Button addModelButton)
        {
            if (GetSelectedComboBoxItem(modelComboBox) == "Custom model")
            {
                freeFormModelTextBox.Visibility = Visibility.Visible;
                addModelButton.Visibility = Visibility.Visible;
                return;
            }

            freeFormModelTextBox.Visibility = Visibility.Collapsed;
            addModelButton.Visibility = Visibility.Collapsed;
        }

        private void ToggleDeleteModelButtonVisibility(ComboBox modelComboBox, Button deleteModelButton)
        {
            string selectedModel = GetSelectedComboBoxItem(modelComboBox);
            if (customModels.Contains(selectedModel))
            {
                deleteModelButton.Visibility = Visibility.Visible;
                return;
            }

            deleteModelButton.Visibility = Visibility.Collapsed;
        }

        private void AddCustomModel(ComboBox modelComboBox, string model)
        {
            if (DefaultModels.Contains(model) || customModels.Contains(model))
            {
                return;
            }

            customModels.Add(model);

            ComboBoxItem? customModelItem = FindModelComboBoxItem(modelComboBox, "Custom model");
            int insertIndex = customModelItem == null ? -1 : modelComboBox.Items.IndexOf(customModelItem);
            if (insertIndex < 0)
            {
                insertIndex = modelComboBox.Items.Count;
            }

            modelComboBox.Items.Insert(insertIndex, new ComboBoxItem() { Content = model });
            SaveSettings();
        }

        private void RemoveCustomModel(ComboBox modelComboBox, string model)
        {
            if (!customModels.Remove(model))
            {
                return;
            }

            SaveSettings();

            ComboBoxItem? modelItem = FindModelComboBoxItem(modelComboBox, model);
            if (modelItem != null)
            {
                modelComboBox.Items.Remove(modelItem);
            }
        }

        private ComboBoxItem? FindModelComboBoxItem(ComboBox modelComboBox, string model)
        {
            foreach (object item in modelComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content?.ToString() == model)
                {
                    return comboBoxItem;
                }
            }

            return null;
        }

        private void ToggleDeleteTopicButtonVisibility(ComboBox topicComboBox, Button deleteTopicButton)
        {
            string selectedTopic = GetSelectedComboBoxItem(topicComboBox);
            if (customTopics.Contains(selectedTopic))
            {
                deleteTopicButton.Visibility = Visibility.Visible;
                return;
            }

            deleteTopicButton.Visibility = Visibility.Collapsed;
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

        private void SaveSettings(string? newApiKey = null, string? newEndpoint = null)
        {
            if (newApiKey != null)
            {
                apiKey = newApiKey;
            }

            if (newEndpoint != null)
            {
                apiEndpoint = newEndpoint;
            }

            JsonArray topicsJsonArray = new JsonArray();
            foreach (string topic in customTopics)
            {
                topicsJsonArray.Add(topic);
            }

            JsonArray modelsJsonArray = new JsonArray();
            foreach (string model in customModels)
            {
                modelsJsonArray.Add(model);
            }

            settingJson["key"] = apiKey;
            settingJson["endpoint"] = apiEndpoint;
            settingJson["selected_model"] = selectedModel;
            settingJson["selected_topic"] = selectedTopic;
            settingJson["models"] = modelsJsonArray;
            settingJson["topics"] = topicsJsonArray;

            File.WriteAllText(SettingsFilePath, settingJson.ToJsonString(new JsonSerializerOptions()));
        }

        private void ShowApiSettingsDialog()
        {
            Window dialog = new Window();
            dialog.Title = "API Settings";
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ResizeMode = ResizeMode.NoResize;
            dialog.SizeToContent = SizeToContent.WidthAndHeight;

            Grid dialogGrid = new Grid();
            dialogGrid.Margin = new Thickness(12);
            dialogGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            dialogGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            dialogGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            dialogGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            dialogGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            dialogGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(320) });
            dialog.Content = dialogGrid;

            TextBlock apiKeyLabel = new TextBlock() { Text = "API Key *", Margin = new Thickness(0, 0, 12, 8), VerticalAlignment = VerticalAlignment.Center };
            dialogGrid.Children.Add(apiKeyLabel);
            Grid.SetColumn(apiKeyLabel, 0); Grid.SetRow(apiKeyLabel, 0);

            PasswordBox apiKeyEditor = new PasswordBox() { Password = apiKey, Margin = new Thickness(0, 0, 0, 8) };
            dialogGrid.Children.Add(apiKeyEditor);
            Grid.SetColumn(apiKeyEditor, 1); Grid.SetRow(apiKeyEditor, 0);

            TextBlock endpointLabel = new TextBlock() { Text = "Endpoint", Margin = new Thickness(0, 0, 12, 8), VerticalAlignment = VerticalAlignment.Center };
            dialogGrid.Children.Add(endpointLabel);
            Grid.SetColumn(endpointLabel, 0); Grid.SetRow(endpointLabel, 1);

            TextBox endpointEditor = new TextBox() { Text = apiEndpoint, Margin = new Thickness(0, 0, 0, 8) };
            dialogGrid.Children.Add(endpointEditor);
            Grid.SetColumn(endpointEditor, 1); Grid.SetRow(endpointEditor, 1);

            TextBlock noteTextBlock = new TextBlock()
            {
                Text = "Enter the full request URL. Leave this blank to use the default OpenAI endpoint: https://api.openai.com/v1/responses",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            };
            dialogGrid.Children.Add(noteTextBlock);
            Grid.SetColumn(noteTextBlock, 0); Grid.SetRow(noteTextBlock, 2);
            Grid.SetColumnSpan(noteTextBlock, 2);

            StackPanel buttonPanel = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            dialogGrid.Children.Add(buttonPanel);
            Grid.SetColumn(buttonPanel, 0); Grid.SetRow(buttonPanel, 3);
            Grid.SetColumnSpan(buttonPanel, 2);

            Button saveButton = new Button() { Content = "Save", MinWidth = 88, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            buttonPanel.Children.Add(saveButton);

            Button cancelButton = new Button() { Content = "Cancel", MinWidth = 88, IsCancel = true };
            buttonPanel.Children.Add(cancelButton);

            void SaveButton_Click(object sender, RoutedEventArgs e)
            {
                string newEndpoint = endpointEditor.Text.Trim();
                if (!IsValidEndpoint(newEndpoint))
                {
                    MessageBox.Show(dialog, "Please enter a valid absolute http or https endpoint.", "Invalid Endpoint");
                    return;
                }

                SaveSettings(apiKeyEditor.Password, newEndpoint);
                dialog.DialogResult = true;
                dialog.Close();
            }

            saveButton.Click += SaveButton_Click;
            dialog.ShowDialog();
        }

        private bool IsValidEndpoint(string endpoint)
        {
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? endpointUri))
            {
                return false;
            }

            return endpointUri.Scheme == Uri.UriSchemeHttp || endpointUri.Scheme == Uri.UriSchemeHttps;
        }

        private void ConfigureAuthenticationHeaders(HttpClient client, string credential)
        {
            if (IsAzureOpenAiEndpoint())
            {
                client.DefaultRequestHeaders.Remove("api-key");
                client.DefaultRequestHeaders.Add("api-key", credential);
                client.DefaultRequestHeaders.Authorization = null;
                return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credential);
        }

        private bool IsAzureOpenAiEndpoint()
        {
            if (!Uri.TryCreate(apiEndpoint, UriKind.Absolute, out Uri? endpointUri))
            {
                return false;
            }

            return endpointUri.Host.EndsWith(".openai.azure.com", StringComparison.OrdinalIgnoreCase);
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
