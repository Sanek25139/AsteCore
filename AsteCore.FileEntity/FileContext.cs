using AsteCore.FileEntity.Interfaces;

namespace AsteCore.FileEntity
{
    public abstract class FileContext
    {
        protected Dictionary<Type,object> FileSets { get; set; } = [];
        protected Dictionary<Type,object> FileObjects { get; set; } = [];

        protected Dictionary<Type, object> MultyFileSets { get; set; } = [];

        public void Create()
        {
            Registration();
            Load();
        }
        public async Task CreateAsync()
        {
            Registration();
            await LoadAsync();
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


        protected void RegisterMultySet<T>(Func<T,string> fileName, string filePath, string? nameDirectory = null)
        {
            var set = new MultyFileSet<T>(fileName, filePath, nameDirectory);
            FileSets.Add(typeof(T), set);
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
        public IFileObject<T> SetObject<T>()
        {
            if (FileSets.TryGetValue(typeof(T), out var set))
                return (IFileObject<T>)set;

            throw new ArgumentException("Unregistered type");
        }
        public void SaveChanges()
        {
            var files = new List<object>( FileSets.Values).Concat( FileObjects.Values).Concat(MultyFileSets.Values);

            foreach (var file in files)
            {
                
                if(file is not IChangeTracking tracking || file is not IFileSave fileSave)
                    continue;

                try
                {
                    fileSave.Save();
                    tracking.AcceptChanges();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public async Task SaveChangesAsync()
        {
            var files = new List<object>(FileSets.Values).Concat(FileObjects.Values).Concat(MultyFileSets.Values);

            foreach (var file in files)
            {
                if (file is not IChangeTracking tracking || file is not IFileSave fileSave)
                    continue;

                try
                {
                    await fileSave.SaveAsync();
                    tracking.AcceptChanges();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Load()
        {
            var files = new List<object>(FileSets.Values).Concat(FileObjects.Values).Concat(MultyFileSets.Values);

            foreach (var file in files)
            {
                if (file is not IFileLoading fileLoad)
                    continue;
                try
                {
                    fileLoad.Load();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public async Task LoadAsync()
        {
            var files = new List<object>(FileSets.Values).Concat(FileObjects.Values).Concat(MultyFileSets.Values);

            foreach (var file in files)
            {
                if (file is not IFileLoading fileLoad)
                    continue;

                try
                {
                    await fileLoad.LoadAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


    }
}
