namespace AsteCore.FileEntity
{
    public abstract class FileBase
    {
        public string Path { get; init; }
        public string Name { get; init; }

        public string FullPath => System.IO.Path.Combine(Path, Name);

        public FileBase(string path, string name)
        {
            Path = path;
            Name = name;
        }
        public FileBase()
        {
            Path = string.Empty;
            Name = string.Empty;

        }

       
    }
}
