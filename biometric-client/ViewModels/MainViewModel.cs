using biometric_client.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace biometric_client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {

        private const string WebSocketUrl = "ws://localhost:5000/ws/";
        private ClientWebSocket _client;

        private string _imageSource;

        public string ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    NotifyPropertyChanged(nameof(ImageSource));
                }
            }
        }

        public Visibility Logo {  get; set; }
        public Visibility FingerView {  get; set; }

        public RelayCommand ScanCommand {  get; set; }

        private string basePath = AppContext.BaseDirectory;

        public MainViewModel()
        {
            _client = new ClientWebSocket();
            ScanCommand = new RelayCommand(Scan);
            ConnectToWebSocket();
        }

        private async void Scan(object obj)
        {
            try
            {
                UpdateUI();

                for (int i = 0; i < 15; i++)
                {
                    LoadImage($"/Captura/{i + 1}.png");
                    Console.Beep();
                    await Task.Delay(100);
                }

                Console.Beep(800, 200);
                Console.Beep(1000, 200);
                Console.Beep(1200, 200);
                Console.Beep(1600, 300);

                if (_client.State == WebSocketState.Open)
                {
                    var imagemBase64 =  await CompressedImageBase64Async($@"{basePath}\Captura\15.png");
                    string response = String.Concat("image:", imagemBase64);
                    await _client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)), WebSocketMessageType.Text, true, CancellationToken.None);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void LoadImage(string imagePath)
        {
            ImageSource = imagePath;
        }

        private async void ConnectToWebSocket()
        {
            try
            {
                _client = new ClientWebSocket();
                _client.Options.SetBuffer(64 * 1024 * 1024, 64 * 1024 * 1024);
                await _client.ConnectAsync(new Uri(WebSocketUrl), CancellationToken.None);
                await RegisterClient();
                Console.WriteLine("Conectado ao WebSocket!");
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao conectar: " + ex.Message);
            }
        }

        private async Task RegisterClient()
        {
            if (_client.State == WebSocketState.Open)
            {
                await _client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("register:biometric_client")), WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine("Mensagem enviada!");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];
            while (_client.State == WebSocketState.Open)
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    MessageBox.Show("Mensagem recebida: " + message);
                }
            }
        }

        private async Task<string> CompressedImageBase64Async(string imagePath)
        {
            var compressedImage = ResizeAndCompressImage(imagePath, 300,400,10);
            string base64Image = Convert.ToBase64String(compressedImage);
            return base64Image;
        }


        public byte[] ResizeAndCompressImage(string imagePath, int maxWidth, int maxHeight, long quality)
        {
            using (var originalImage = new Bitmap(imagePath))
            {
                int newWidth = originalImage.Width;
                int newHeight = originalImage.Height;

                if (newWidth > maxWidth || newHeight > maxHeight)
                {
                    float aspectRatio = (float)originalImage.Width / originalImage.Height;
                    if (aspectRatio > 1)
                    {
                        newWidth = maxWidth;
                        newHeight = (int)(maxWidth / aspectRatio);
                    }
                    else
                    {
                        newHeight = maxHeight;
                        newWidth = (int)(maxHeight * aspectRatio);
                    }
                }

                using (var resizedImage = new Bitmap(originalImage, new System.Drawing.Size(newWidth, newHeight)))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var encoder = GetEncoder(ImageFormat.Jpeg);
                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                        resizedImage.Save(memoryStream, encoder, encoderParameters);

                        return memoryStream.ToArray();
                    }
                }
            }
        }

        private byte[] CompressImage(Bitmap image, long quality)
        {
            using (var memoryStream = new MemoryStream())
            {
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                var jpegCodec = GetEncoderInfo("image/jpeg");
                image.Save(memoryStream, jpegCodec, encoderParameters);

                return memoryStream.ToArray();
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var encoders = ImageCodecInfo.GetImageEncoders();
            foreach (var encoder in encoders)
            {
                if (encoder.MimeType == mimeType)
                {
                    return encoder;
                }
            }

            return null;
        }

        public static string GetExeDirectory()
        {
            string exePath = Assembly.GetEntryAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);

            return exeDirectory;
        }

        public void UpdateUI()
        {
            Logo = Logo == Visibility.Visible  ? Visibility.Hidden : Visibility.Visible;
            FingerView = FingerView == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
