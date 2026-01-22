using AsteCore.FileEntity.Interfaces;
using System.Collections;
using System.Text.Json.Nodes;

namespace AsteCore.FileEntity
{
    public class MultyFileSet<T> : FileBase, IFileCollection<T>, IChangeTracking, IFileSave, IFileLoading
    {
        private List<FileObject<T>> _addedItems = [];
        private List<FileObject<T>> _removedItems = [];

        private readonly List<FileObject<T>> _values = [];
        private readonly Func<T, string> _fileName;


        public IEnumerable<IFileObject<T>> Objects { get => _values; }

        public MultyFileSet(Func<T, string> fileName, string path, string? nameDirectory = null)
        {
            _fileName = fileName;
            Path = path;
            Name = nameDirectory ?? typeof(T).Name + "s";
        }

        public int Count => _values.Count;

        public bool IsReadOnly => false;

        public bool IsDirty { get; set; } = false;

       

        void ICollection<T>.Add(T item)
        {
            Add(item);
            MarkDirty();
        }
        public IFileObject<T>? Add(T item)
        {
            FileObject<T> fileObj = new(FullPath, _fileName.Invoke(item));
            _values.Add(fileObj);
            _addedItems.Add(fileObj);
            MarkDirty();
            return fileObj;
        }
        public bool Remove(T item)
        {
            var obj = _values.FirstOrDefault(v => EqualityComparer<T>.Default.Equals(v.Value, item));
            if (obj == null) return false;

            _values.Remove(obj);
            _removedItems.Add(obj);
            MarkDirty();
            return true;
        }
        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(T item)
        {
            return _values.Contains(item);
        }

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

        public IFileObject<T>? GetObject(Func<T, bool> predicate)
        {
            return _values.FirstOrDefault(o => o.Value != null && predicate(o.Value));
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void MarkDirty() => IsDirty = true;
        public void AcceptChanges() => IsDirty = false;

        public void Save()
        {
            if (!IsDirty) return;

            DirectoryInfo info = Directory.CreateDirectory(FullPath);

            if(_addedItems.Count > 0)
            {
                foreach(var item in _addedItems)
                {
                    item.Save();
                }
            }
            if (_removedItems.Count > 0)
            {
                foreach (var item in _removedItems)
                {
                    File.Delete(item.FullPath);
                }
            }

            foreach (var item in _values.Where(v => v.IsDirty))
            {
                item.Save();
                item.AcceptChanges();
            }

            AcceptChanges();
        }

        public async Task SaveAsync()
        {

            if (!IsDirty) return;

            Directory.CreateDirectory(FullPath);

            if (_addedItems.Count > 0)
            {
                foreach (var item in _addedItems)
                {
                    await item.SaveAsync();
                }
                _addedItems.Clear();
            }
            if (_removedItems.Count > 0)
            {
                foreach (var item in _removedItems)
                {
                    File.Delete(item.FullPath);
                }
                _removedItems.Clear();
            }

            foreach (var item in _values.Where(v => v.IsDirty))
            {
                await item.SaveAsync();
                item.AcceptChanges();
            }
            AcceptChanges();
        }

        public void Load()
        {
            _values.Clear();
            _addedItems.Clear();
            _removedItems.Clear();

            if (!Directory.Exists(FullPath))
                return;

            foreach (var file in Directory.EnumerateFiles(FullPath, "*.json"))
            {
                var name = System.IO.Path.GetFileName(file);

                var fileObject = new FileObject<T>(FullPath, name);

                try
                {
                    fileObject.Load();
                    _values.Add(fileObject);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            AcceptChanges();
        }

        public async Task LoadAsync()
        {
            _values.Clear();
            _addedItems.Clear();
            _removedItems.Clear();

            if (!Directory.Exists(FullPath))
                return;

            foreach (var file in Directory.EnumerateFiles(FullPath, "*.json"))
            {
                var name = System.IO.Path.GetFileName(file);

                var fileObject = new FileObject<T>(FullPath, name);

                try
                {
                    await fileObject.LoadAsync();
                    _values.Add(fileObject);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            AcceptChanges();
        }
    }
}
