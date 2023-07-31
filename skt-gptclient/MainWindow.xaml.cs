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

            StackPanel MainStackPanel = new StackPanel();
            Content = MainStackPanel;
            
            ComboBox ModelComboBox = new ComboBox();
            ModelComboBox.Items.Add(new ComboBoxItem() { Content = "gpt-3.5-turbo-0301",  });
            ModelComboBox.Items.Add(new ComboBoxItem() { Content = "gpt-4-0613" });
            ModelComboBox.SelectedIndex = 0;
            MainStackPanel.Children.Add(ModelComboBox);

            ComboBox TopicComboBox = new ComboBox();
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "以下を英語に翻訳してください" });
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "以下を日本語に翻訳してください" });
            TopicComboBox.Items.Add(new ComboBoxItem() { Content = "自由入力" });
            TopicComboBox.SelectedIndex = 0;
            MainStackPanel.Children.Add(TopicComboBox);

            TextBox FreeFormTopicTextBlock = new TextBox();
            FreeFormTopicTextBlock.Text = "自由入力のお題を打つところ";
            FreeFormTopicTextBlock.Visibility = Visibility.Hidden;
            MainStackPanel.Children.Add(FreeFormTopicTextBlock);

            void TopicComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
                if (((ComboBox)sender).SelectedItem.ToString().Split(" ")[1] == "自由入力")
                {
                    FreeFormTopicTextBlock.Visibility = Visibility.Visible;
                }
                else {
                    FreeFormTopicTextBlock.Visibility = Visibility.Hidden;
                }
                MessageBox.Show(((ComboBox)sender).SelectedItem.ToString().Split(" ")[1]);
            }

            TopicComboBox.SelectionChanged += TopicComboBox_SelectionChanged;
            
            TextBox InputTextBox = new TextBox();
            InputTextBox.Text = "入力を打つ所";
            MainStackPanel.Children.Add(InputTextBox);

            TextBox OutputTextBox = new TextBox();
            OutputTextBox.IsReadOnly = true;
            OutputTextBox.Text = "出力が記載される所";
            MainStackPanel.Children.Add(OutputTextBox);

            Button ExecButton = new Button();
            ExecButton.Content = "実行";
            async void ExecButton_TouchEnter(object sender, RoutedEventArgs e)
            {
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
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");
                    HttpResponseMessage httpResponse = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    var responseContentString = await httpResponse.Content.ReadAsStringAsync();
                    var responseJsonObject = JsonObject.Parse(responseContentString);
                    OutputTextBox.Text = responseJsonObject["choices"][0]["message"]["content"].ToString();
                };
            }

            ExecButton.Click += ExecButton_TouchEnter;
            MainStackPanel.Children.Add(ExecButton);
        }

        private void TopicComboBox_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
