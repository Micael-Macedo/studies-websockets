using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Diagnostics;

namespace socket_worker.Services
{
    public class WorkerService : BackgroundService
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly string _url = "http://localhost:5000/ws/";
        private WebSocket _webClient;
        private WebSocket _biometricClient;


        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(_url);
            listener.Start();
            _logger.LogInformation($"Servidor WebSocket rodando em {_url}");

            while (!stoppingToken.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _logger.LogInformation("Cliente conectado!");
                    _ = Task.Run(() => HandleWebSocketAsync(wsContext.WebSocket, stoppingToken));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }

            listener.Stop();
        }

        private async Task HandleWebSocketAsync(WebSocket webSocket, CancellationToken stoppingToken)
        {
            try
            {
                while (webSocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var buffer = new byte[10 * 1024 * 1024];
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexão encerrada pelo cliente", stoppingToken);
                        _logger.LogInformation("Conexão fechada pelo cliente.");
                        return;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count).Replace("\"", "");
                    _logger.LogInformation("Mensagem recebida: " + message);

                    if (message.StartsWith("register:"))
                    {
                        var clientType = message.Split(":")[1];
                        var clientId = Guid.NewGuid().ToString();

                        if (clientType == "web_client")
                            _webClient = webSocket;
                        if (clientType == "biometric_client")
                            _biometricClient = webSocket;
                    }

                    if (message.StartsWith("requestImage"))
                    {
                        OpenBiometricClient();
                    }

                    if (message.StartsWith("image:"))
                    {
                        var imageBase64 = message.Split(":")[1];
                        _logger.LogInformation("Imagem recebida do WPF.");

                        if (_webClient != null)
                        {
                            var responseMessage = new { Text = $"Polegar Direito - {DateTime.Now.ToString()}", Image = String.Concat("data:image/jpeg;base64,", imageBase64) };
                            var responseBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(responseMessage));
                            await _webClient.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, stoppingToken);
                            _logger.LogInformation($"Mensagem enviada para o WebClient: {responseMessage}");
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                //Console.WriteLine($"🚨 Erro no WebSocket: {ex.InnerException}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"🔥 Erro inesperado: {ex.Message}");
            }
        }


        private async void OpenBiometricClient()
        {
            string path = @"C:\Users\gabriel.pontes\Documents\Docs\studies-websocket\biometric-client\bin\Debug\net8.0-windows\biometric-client.exe";
            ProcessStartInfo psi = new ProcessStartInfo(path)
            {
                UseShellExecute = false
            };
            Process.Start(psi);
        }
    }
}
