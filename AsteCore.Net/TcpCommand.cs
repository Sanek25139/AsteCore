using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteCore.Net
{
    public class TcpCommand
    {
        public object TypeCommand { get; set; }
        public string Type {  get; set; }
        public string TypeData { get; set; } // Название типа вместо Type
        public JToken Data { get; set; } // Данные в сыром виде

#pragma warning disable CS8618
        public TcpCommand() { }

        public TcpCommand(object typeCommand)
        {
            TypeCommand = typeCommand;
            Type = typeCommand.GetType().FullName!;
        }
#pragma warning restore CS8618

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All // Сохраняет информацию о типах
            });
        }

        public static TcpCommand Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<TcpCommand>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            })!;
        }
        // Метод для получения типизированных данных
        public T GetData<T>()
        {
            return Data.ToObject<T>()!;
        }
        // Метод для проверки типа данных
        public bool IsData<T>() 
        {
            try
            {
                return Data.ToObject<T>() != null;
            }
            catch
            {
                return false;
            }
        }
    }

    public class TcpCommand<TJD> : TcpCommand
    {
        public new TJD Data
        {
            get => base.GetData<TJD>();
            set
            {
                base.Data = JToken.FromObject(value!);
                base.TypeData = typeof(TJD).AssemblyQualifiedName!;
            }
        }

        public TcpCommand(object typeCommand, TJD data) : base(typeCommand) 
        {
            Data = data;
            TypeData = typeof(TJD).ToString();
        }

        public static new TcpCommand<TJD> Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<TcpCommand<TJD>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            })!;
        }
    }
}
