using AsteCore.FileEntity.Interfaces;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsteCore.FileEntity
{
    public class FileSet<T> : FileBase, IFileCollection<T>, IEnumerable<T>, IEnumerable, IChangeTracking, IFileLoading, IFileSave
    {
        public IEnumerable<T>? DefaultValue { get; }

        private readonly List<FileObject<T>> _values;
        public IEnumerable<IFileObject<T>> Objects =>_values;
        public FileSet(string filePath, string? name = null)
        {
            Name = name ?? typeof(T).Name + ".json";
            _values = [];
            Path = filePath;
        }
        public FileSet(IEnumerable<T> defaultValues, string filePath, string? name = null)
        {
            DefaultValue = defaultValues;
            Name = Tool.NormalizeName( name ,typeof(T).Name + ".json");
            _values = [];
            Path = filePath;
        }

        public bool IsDirty { get; set; }

        public int Count => _values.Count;

        public bool IsReadOnly => false;

        void ICollection<T>.Add(T item)
        {
            Add(item);
            MarkDirty();
        }

        public IFileObject<T> Add(T item)
        {
            var fileObject = new FileObject<T>(item);
            _values.Add(fileObject);
            MarkDirty();
            return fileObject;
        }

        public void Clear()
        {
            _values.Clear();
            MarkDirty();
        }

        public bool Contains(T item) => _values.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            var valuesAsT = _values.ConvertAll(fo => fo.Value);
            valuesAsT.CopyTo(array, arrayIndex);
        }

#pragma warning disable CS8613
        public IEnumerator<T?> GetEnumerator()
#pragma warning restore CS8613
        {
            foreach (var fileObject in _values)
            {
                yield return fileObject.Value;
            }
        }

        public bool Remove(T item)
        {
            MarkDirty();
            return _values.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void MarkDirty() => IsDirty = true;
        public void AcceptChanges() => IsDirty = false;


        public void UseDefault()
        {
            _values.Clear();
            if (DefaultValue != null)
            {
                _values.AddRange(DefaultValue.Select(v => new FileObject<T>(v)));
            }
            MarkDirty();
        }

        public string Serializable()
        {
            var values = _values.Select(fo => fo.Value)
                           .Where(v => v != null)
                           .ToList();

            return JsonSerializer.Serialize(values,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        public void Load()
        {
            try
            {
                string json = File.ReadAllText(FullPath);
                Deserialize(json);
            }
            catch (System.IO.FileNotFoundException)
            {
                UseDefault();
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task LoadAsync()
        {
            try
            {
                string json = await File.ReadAllTextAsync(FullPath);
                Deserialize(json);
            }
            catch (System.IO.FileNotFoundException)
            {
                UseDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Deserialize(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Игнорировать регистр
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Игнорировать null
                };
                var values = JsonSerializer.Deserialize<List<T>>(json, options);
                _values.Clear();

                if (values != null)
                {
                    _values.AddRange(values.Select(v => new FileObject<T>(v)));
                }
                else
                {
                    UseDefault();
                }
            }
            catch(Exception)
            {
                throw;
            }
            
        }


        public IFileObject<T>? GetObject(Func<T, bool> predicate)
        {
            return _values.FirstOrDefault(fo => fo.Value != null && predicate(fo.Value));
        }

        public void Save()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FullPath)!);

            if (IsDirty)
            {
                string json = Serializable();
                File.WriteAllText(FullPath, json);
                AcceptChanges();
            }
            else
            {
                var changes = _values.Where(fo => fo.IsDirty);
                if (!changes.Any()) return;
                string json = Serializable();
                File.WriteAllText(FullPath, json);
            }
           
        }

        public async Task SaveAsync()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FullPath)!);

            if (IsDirty)
            {
                string json = Serializable();
                await File.WriteAllTextAsync(FullPath, json);
                AcceptChanges();
                foreach (var fo in _values)
                    fo.AcceptChanges();
            }
            else
            {
                var changes = _values.Where(fo => fo.IsDirty);
                if (!changes.Any()) return;
                string json = Serializable();
                await File.WriteAllTextAsync(FullPath, json);
                foreach (var fo in _values)
                    fo.AcceptChanges();
            }
        }
    }
}
