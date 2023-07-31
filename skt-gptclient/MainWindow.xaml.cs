using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
            ChatGPTAPIKey.Text = "ChatGPTAPIキー";
            HeaderStackPanel.Children.Add(ChatGPTAPIKey);
            Grid.SetColumn(ChatGPTAPIKey, 0); Grid.SetRow(ChatGPTAPIKey, 0);
            PasswordBox ChatGPTAPIKeyPWBOX = new PasswordBox();
            HeaderStackPanel.Children.Add(ChatGPTAPIKeyPWBOX);


            ComboBox ModelComboBox = new ComboBox();
            ModelComboBox.Items.Add(new ComboBoxItem() { Content = "gpt-3.5-turbo-0301",  });
            ModelComboBox.Items.Add(new ComboBoxItem() { Content = "gpt-4-0613" });
            ModelComboBox.SelectedIndex = 0;
            HeaderStackPanel.Children.Add(ModelComboBox);

            ComboBox TopicComboBox = new ComboBox();
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "以下を英語に翻訳してください" });
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "以下を日本語に翻訳してください" });
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "自由入力" });
            TopicComboBox.SelectedIndex = 0;
            HeaderStackPanel.Children.Add(TopicComboBox);

            TextBox FreeFormTopicTextBlock = new TextBox();
            FreeFormTopicTextBlock.Text = "自由入力のお題";
            FreeFormTopicTextBlock.Visibility = Visibility.Hidden;
            HeaderStackPanel.Children.Add(FreeFormTopicTextBlock);

            void TopicComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
                if (((ComboBox)sender).SelectedItem.ToString().Split(" ")[1] == "自由入力")
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
            InputTextBox.Text = "入力";           
            MainGrid.Children.Add(InputTextBox);
            Grid.SetColumn(InputTextBox, 0); Grid.SetRow(InputTextBox, 1);

            TextBox OutputTextBox = new TextBox();
            OutputTextBox.IsReadOnly = true;
            OutputTextBox.Text = "出力";
            MainGrid.Children.Add(OutputTextBox);
            Grid.SetColumn(OutputTextBox, 1); Grid.SetRow(OutputTextBox, 1);

            //FooterStackPanel
            StackPanel FooterStackPanel = new StackPanel();
            MainGrid.Children.Add(FooterStackPanel);
            Grid.SetColumn(FooterStackPanel, 2); Grid.SetRow(FooterStackPanel, 2);

            Button ExecButton = new Button();
            ExecButton.Content = "実行";
            async void ExecButton_TouchEnter(object sender, RoutedEventArgs e)
            {
                MessageBox.Show(ChatGPTAPIKeyPWBOX.Password);
                string Model = "";
                string Topic = "";
                string Input = "";
                Model = ModelComboBox.SelectedItem.ToString().Split(" ")[1];
                if (TopicComboBox.SelectedItem.ToString().Split(" ")[1] == "自由入力") {
                    Topic = FreeFormTopicTextBlock.Text;
                }
                else
                {
                    Topic = TopicComboBox.SelectedItem.ToString().Split(" ")[1];
                }
                Input = InputTextBox.Text;

                using (HttpClient client = new HttpClient())
                {
                    string requestBody = $"{{\"model\":\"{Model}\",\"messages\": [{{ \"role\":\"user\",\"content\":\"{Topic}\\n{Input}\"}}],\"temperature\":0.7}}";
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChatGPTAPIKeyPWBOX.Password);
                    HttpResponseMessage httpResponse = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    var responseContentString = await httpResponse.Content.ReadAsStringAsync();
                    var responseJsonObject = JsonObject.Parse(responseContentString);
                    OutputTextBox.Text = responseJsonObject["choices"][0]["message"]["content"].ToString();
                    //TextChangedイベント駆動にしたかったけどなんだか挙動がおかしかった
                };
            }

            ExecButton.Click += ExecButton_TouchEnter;
            FooterStackPanel.Children.Add(ExecButton);
        }

        private void TopicComboBox_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
