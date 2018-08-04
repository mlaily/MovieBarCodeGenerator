using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator
{
    public interface IFileSystemService
    {
        string GetDirectoryName(string path);
        string GetFileName(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        IEnumerable<string> EnumerateDirectoryFiles(string path, string searchPattern, SearchOption searchOption);

    }

    public class FileSystemService : IFileSystemService
    {
        public string GetDirectoryName(string path) => Path.GetDirectoryName(path);
        public string GetFileName(string path) => Path.GetFileName(path);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool FileExists(string path) => File.Exists(path);
        public IEnumerable<string> EnumerateDirectoryFiles(string path, string searchPattern, SearchOption searchOption)
            => Directory.EnumerateFiles(path, searchPattern, searchOption);
    }
}
