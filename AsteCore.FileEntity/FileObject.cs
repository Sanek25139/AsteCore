using AsteCore.FileEntity.Interfaces;
using System.Text.Json;

namespace AsteCore.FileEntity
{
    public class FileObject<T> : FileBase, IChangeTracking, IFileObject<T>, IFileLoading, IFileSave, IEquatable<FileObject<T>>, IEquatable<T>,
                             IComparable<FileObject<T>>, IComparable<T> 
    {
        public bool IsDirty { get; set; }

        public T? DefaultValue { get; }

        private T? _value;
        public T? Value
        {
            get => _value;
            set
            {
                _value = value;
                MarkDirty();
            }
        }

      

        public FileObject(string filePath, string? name = null)
        {
            Name = Tool.NormalizeName(name, typeof(T).Name + ".json");
            Path = filePath;
            _value = default!;
        }
        public FileObject(T defaultValue, string filePath, string? name = null)
        {
            DefaultValue = defaultValue;
            Name = Tool.NormalizeName(name, typeof(T).Name + ".json");
            Path = filePath;
            _value = defaultValue;
            
        }
        public FileObject(T value)
        {
            _value = value;
            Path = string.Empty;
            Name = string.Empty;
        }
        public FileObject()
        {
            _value = default!;
            Path = string.Empty;
            Name = string.Empty;
        }
        public void MarkDirty() => IsDirty = true;
        public void AcceptChanges() => IsDirty = false;


        public bool GetIsSave() => IsDirty;

        public string Serializable()
        {
            Value ??= default;

            return JsonSerializer.Serialize(Value,
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

        public void Save()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FullPath)!);

            string json = Serializable();
            File.WriteAllText(FullPath, json);
        }

        public Task SaveAsync()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FullPath)!);

            string json = Serializable();
            return File.WriteAllTextAsync(FullPath, json);
        }

        private void Deserialize(string json)
        {
            try
            {
                var value = JsonSerializer.Deserialize<T>(json);
                if (value is not null)
                    _value = value;
                else
                    UseDefault();
            }
            catch
            {
                throw;
            }
        }
        public void UseDefault()
        {
            if (DefaultValue is not null)
            {
                _value = DefaultValue;
                MarkDirty();
            }
        }

        #region IEquatable реализации

        public bool Equals(FileObject<T>? other)
        {
            if (other is null) return false;
            return EqualityComparer<T?>.Default.Equals(_value, other._value);
        }

        public bool Equals(T? other)
        {
            return EqualityComparer<T?>.Default.Equals(_value, other);
        }

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                FileObject<T> fileObj => Equals(fileObj),
                T value => Equals(value),
                _ => false
            };
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        #endregion

        #region IComparable реализации

        public int CompareTo(FileObject<T>? other)
        {
            if (other is null) return 1;
            return CompareTo(other._value);
        }

        public int CompareTo(T? other)
        {
            // Если T реализует IComparable<T>
            if (_value is IComparable<T> comparable)
                return comparable.CompareTo(other);

            // Если T реализует IComparable
            if (_value is IComparable comparableBase)
            {
                if (other is null) return 1;
                return comparableBase.CompareTo(other);
            }

            // Общий случай
            return Comparer<T?>.Default.Compare(_value, other);
        }

      



        #endregion

        #region Операторы сравнения

        public static bool operator ==(FileObject<T>? left, FileObject<T>? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(FileObject<T>? left, FileObject<T>? right)
        {
            return !(left == right);
        }

        public static bool operator ==(FileObject<T>? left, T? right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(FileObject<T>? left, T? right)
        {
            return !(left == right);
        }

        public static bool operator ==(T? left, FileObject<T>? right)
        {
            return right?.Equals(left) ?? left is null;
        }

        public static bool operator !=(T? left, FileObject<T>? right)
        {
            return !(left == right);
        }

        // Операторы сравнения
        public static bool operator <(FileObject<T> left, FileObject<T> right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(FileObject<T> left, FileObject<T> right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(FileObject<T> left, FileObject<T> right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(FileObject<T> left, FileObject<T> right)
        {
            return left.CompareTo(right) >= 0;
        }

        #endregion

        #region Операторы преобразования

        public static explicit operator T?(FileObject<T> fileObject)
        {
            return fileObject.Value;
        }

        public static implicit operator FileObject<T>(T value)
        {
            return new FileObject<T>(value);
        }

        #endregion
    }
}