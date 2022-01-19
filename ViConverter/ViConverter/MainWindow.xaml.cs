using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Media;

namespace ViConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string workingDirectory = Environment.CurrentDirectory;
        string projectDirectory;
        string audioFile;
        string[] arrVoice = { "1", "2", "3", "4" };
        string[] arrSpeed = { "1","0.8", "0.9", "1.1", "1.2" };
        bool soundFinished = true;
        Thread ThreadUpdateUI;
        DataResponse dataResponse;
        string textPre;
        public MainWindow()
        {
            InitializeComponent();
            projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            audioFile = projectDirectory + @"\audio.wav";
            ThreadUpdateUI = new Thread(() => UpdateUI());
            ThreadUpdateUI.IsBackground = true;
            ThreadUpdateUI.Start();
        }
        private void UpdateUI()
        {
            textPre = "";
            while (true)
            {
                this.Dispatcher.Invoke(() =>
                {

                    if (_text.Text != textPre)
                    {
                        _kytu.Content = "Ký tự đã nhập: " + _text.Text.Length.ToString();
                        textPre = _text.Text;

                    }
                });
                Thread.Sleep(200);
            }
        }
        public async Task callApi(string input,string voice, string speed)
        {
            HttpClient client = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Headers.Add("apikey", "y9Ve7ae2x3Hg1Pbkf7ZcTEXTJRL1ndKE");
            httpRequestMessage.Method = HttpMethod.Post;
            httpRequestMessage.RequestUri = new Uri("https://api.zalo.ai/v1/tts/synthesize");
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("input", input));
            parameters.Add(new KeyValuePair<string, string>("speaker_id", voice));
            parameters.Add(new KeyValuePair<string, string>("encode_type", "0"));
            parameters.Add(new KeyValuePair<string, string>("speed", speed));
            var content = new FormUrlEncodedContent(parameters);
            httpRequestMessage.Content = content;
            var response = await client.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            var a = await response.Content.ReadAsStringAsync();
            dataResponse = JsonConvert.DeserializeObject<DataResponse>(a);
        }

        //private void _stop_Click(object sender, RoutedEventArgs e)
        //{
        //    SoundPlayer player = new SoundPlayer(audioFile);
        //    player.Stop();
        //    _stop.IsEnabled = false;
        //}

        private async void _run_Click(object sender, RoutedEventArgs e)
        {
            string voice="";
            string speed="";
            string input;
            input = textPre;
            int speed_id=_speed.SelectedIndex;
            int voice_id = _voice.SelectedIndex;
            speed = arrSpeed[speed_id];
            voice = arrVoice[voice_id];
            
  
            
            await callApi(input,voice,speed);
                //_stop.IsEnabled = true;
            

            if (dataResponse.error_code == "0")
            {

                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        Thread.Sleep(1000);
                        webClient.DownloadFile(dataResponse.data.url, audioFile);

                    }
                    catch (WebException) { };
                }
                
                if (File.Exists(audioFile))
                {
                    SoundPlayer player = new SoundPlayer(audioFile);

                    if (!soundFinished)
                    {
                        
                    }
                    if (soundFinished)
                    {
                        soundFinished = false;
                        await Task.Factory.StartNew(() =>
                        {
                            player.PlaySync();
                            soundFinished = true;
                            File.Delete(audioFile);
                        });
                        
                    }
                }

            }
            else
            {
                MessageBox.Show("Error");
            }

        }
    }
}
