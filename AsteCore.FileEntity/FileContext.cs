using AsteCore.FileEntity.Interfaces;

namespace AsteCore.FileEntity
{
    public abstract class FileContext
    {
        private bool _uploaded;

        protected Dictionary<Type,object> FileSets { get; set; } = [];
        protected Dictionary<Type,object> FileObjects { get; set; } = [];
        protected Dictionary<Type, object> MultiFileSets { get; set; } = [];

        public void Create()
        {
            if (_uploaded) return;

            Registration();
            Load();

            _uploaded = true;
        }
        public async Task CreateAsync()
        {
            if (_uploaded) return;

            Registration();
            await LoadAsync();
            _uploaded = true;
        }

        /// <summary>
        /// Регестрация FileSet через метод Register<T>
        /// </summary>
        protected abstract void Registration();

        protected void RegisterSet<T>(string filePath, string? name = null)
        {
            var set = new FileSet<T>(filePath,name);
            FileSets.Add(typeof(T), set);
        }
        protected void RegisterSet<T>(IEnumerable<T> defaultValue, string filePath, string? name = null)
        {
            var set = new FileSet<T>(defaultValue, filePath, name);
            FileSets.Add(typeof(T), set);
        }


        protected void RegisterMultySet<T>(string filePath, string? nameDirectory = null, Func<T, string>? fileName = null)
        {
            var set = new MultiFileSet<T>(filePath, nameDirectory, fileName);
            MultiFileSets.Add(typeof(T), set);
        }
        protected void RegisterMultySet<T>(IEnumerable<T> defaultValues, string filePath, string? nameDirectory = null, Func<T, string>? fileName = null)
        {
            var set = new MultiFileSet<T>(defaultValues,filePath, nameDirectory, fileName);
            MultiFileSets.Add(typeof(T), set);
        }

        protected void RegisterObject<T>(string filePath,string? name = null)
        {
            var set = new FileObject<T>(filePath, name);
            FileSets.Add(typeof(T), set);
        } 
        protected void RegisterObject<T>(T defaultValue, string filePath, string? name = null)
        {
            var set = new FileObject<T>(defaultValue, filePath, name);
            FileSets.Add(typeof(T), set);
        }

        public IFileCollection<T> Set<T>()
        {
            if(FileSets.TryGetValue(typeof(T), out var set))
                return (IFileCollection<T>)set;

            throw new ArgumentException("Unregistered type");
        }
        public IFileCollection<T> MultiSet<T>()
        {
            if (MultiFileSets.TryGetValue(typeof(T), out var set))
                return (IFileCollection<T>)set;

            throw new ArgumentException("Unregistered type");
        }
        public IFileObject<T> SetObject<T>()
        {
            if (FileSets.TryGetValue(typeof(T), out var set))
                return (IFileObject<T>)set;

            throw new ArgumentException("Unregistered type");
        }

        private IEnumerable<object> GetFiles() => new List<object>(FileSets.Values).Concat(FileObjects.Values).Concat(MultiFileSets.Values);

        public void SaveChanges()
        {
            foreach (var file in GetFiles())
            {    
                if(file is not IChangeTracking tracking || file is not IFileSave fileSave)
                    continue;

                fileSave.Save();
                tracking.AcceptChanges();
            }
        }
        public async Task SaveChangesAsync()
        {
            foreach (var file in GetFiles())
            {
                if (file is not IChangeTracking tracking || file is not IFileSave fileSave)
                    continue;

                await fileSave.SaveAsync();
                tracking.AcceptChanges();
            }
        }

        private void Load()
        {
            foreach (var file in GetFiles())
            {
                if (file is not IFileLoading fileLoad)
                    continue;

                fileLoad.Load();
            }
        }
        private async Task LoadAsync()
        {
            foreach (var file in GetFiles())
            {
                if (file is not IFileLoading fileLoad)
                    continue;

                await fileLoad.LoadAsync();
            }
        }
    }
}
