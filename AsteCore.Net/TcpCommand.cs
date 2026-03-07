using System.Text.Json;

namespace AsteCore.Net
{
    public class TcpCommand
    {
        public byte TypeCommand { get; set; }
        public JsonElement RawData { get; set; } // Данные в сыром виде

        public string? TypeData { get; set; }

#pragma warning disable CS8618
        public TcpCommand() { }
#pragma warning restore CS8618
        public TcpCommand(Enum typeCommand, object? data = null)
        {
            TypeCommand = Convert.ToByte(typeCommand);

            if (data != null)
            {
                TypeData = data.GetType().AssemblyQualifiedName;
                RawData = JsonSerializer.SerializeToElement(data);
            }
        }


        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static TcpCommand Deserialize(string json)
        {
            return JsonSerializer.Deserialize<TcpCommand>(json)!;
        }

        // Метод для получения типизированных данных
        public T GetData<T>()
        {
            return RawData.Deserialize<T>()!;
        }
        // Метод для проверки типа данных
        public bool IsData<T>()
        {
            try
            {
                return RawData.Deserialize<T>() != null;
            }
            catch
            {
                return false;
            }
        }
    }

    public class TcpCommand<T>(Enum typeCommand, T data) : TcpCommand(typeCommand, data!);
}
