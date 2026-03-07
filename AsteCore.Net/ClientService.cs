using AsteCore.Net.Signal;
using AsteCore.Service;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;


namespace AsteCore.Net
{
    public class ClientService(EventBus eventBus) : IDisposable
    {
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;

        private CancellationTokenSource? _receiveCts;

        private TcpClient? _tcpClient;
        public TcpClient? Client { get { return _tcpClient; } }

        

        public async Task<bool> Connect(string ip, int port, int timeout = 3000)
        {
            await Disconnect();

            var client = new TcpClient();
            _receiveCts = new CancellationTokenSource();

            var task = client.ConnectAsync(ip, port);

            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                if(!client.Connected) return false;

                await eventBus.InvokeAsync<ConnectServerEvent>(this);

                _tcpClient  = client;

                _stream = client.GetStream();
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

                _ = Task.Run(() => ReceiveMessagesAsync(_receiveCts.Token));
            }
            return client.Connected;
        }
        private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested &&
                       _tcpClient?.Connected == true &&
                       _reader != null)
                {
                    string? message = await _reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message))
                        break;
                    try
                    {
                        var command = JsonSerializer.Deserialize<TcpCommand>(message);
                        if (command == null) continue;
                        if(command.TypeData == null) continue;

                        var type = Type.GetType(command.TypeData);
                        if (type == null) continue;

                        var data = command.RawData.Deserialize(type);
                        if(data == null) continue;

                        await eventBus.InvokeAsync((dynamic)data);
                    }
                    catch
                    {

                    }
                    
                }
            }
            finally
            {

            }
        }

        public async Task<bool> RequestAsync(string command)
        {
            if (_tcpClient == null) return false;
            if (_writer == null) return false;
            if (!_tcpClient.Connected) return false;

            try
            {
                await _writer.WriteLineAsync(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RequestAsync<T>(TcpCommand<T> command)
        {
            return await RequestAsync(command.Serialize());
        }

        public async Task Disconnect()
        {
            if (_tcpClient == null) return;

            await eventBus.InvokeAsync<DisconnectServerEvent>(this);

            _receiveCts?.Cancel();

            _reader?.Close();
            _writer?.Close();
            _tcpClient?.Close();
            Dispose();
            _tcpClient = null;
            _reader = null;
            _writer = null;
            _stream = null;
            _tcpClient = null;
            _receiveCts = null;
        }

        public void Dispose()
        {
            _tcpClient?.Dispose();
            _stream?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }
}
