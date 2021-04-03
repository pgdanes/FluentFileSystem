using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace FluentFileSystem
{
    public class FileQuery
    {
        readonly IFileSystem _fileSystem;
        readonly List<string> _searchPaths = new List<string>();
        readonly List<Func<string, bool>> _filters = new List<Func<string, bool>>();
        int _depthLimit = int.MaxValue;
        bool _negateFilters = false;

        public FileQuery(IFileSystem fileSystem = null)
        {
            fileSystem ??= new FileSystem();
            _fileSystem = fileSystem;
        }

        public FileQuery InPaths(IEnumerable<string> paths)
        {
            _searchPaths.ForEach(path =>
            {
                _fileSystem.Directory.Exists(path);
            });

            _searchPaths.AddRange(paths.Distinct().ToList());
            return this;
        }

        public FileQuery InPath(string path)
        {
            return InPaths(new List<string>
            {
                path
            });
        }

        public FileQuery WithExtension(string extension)
        {
            return WithExtension(new List<string>
            {
                extension
            });
        }

        public FileQuery WithExtension(IEnumerable<string> extensions)
        {
            var extensionsWithDot = extensions.Select(e => e.StartsWith(".") ? e : $".{e}");
            _filters.Add(filePath => extensionsWithDot.Any(extension => Path.GetExtension(filePath).ToLower().Equals(extension)));
            return this;
        }

        public FileQuery Matching(Regex expression)
        {
            _filters.Add(filePath => expression.IsMatch(_fileSystem.Path.GetFileName(filePath)));
            return this;
        }

        public FileQuery Not()
        {
            _negateFilters = true;
            return this;
        }

        public FileQuery WhereLastWriteTime(Func<DateTime, bool> modifiedTestFunction)
        {
            _filters.Add(filePath => modifiedTestFunction(_fileSystem.File.GetLastWriteTime(filePath)));
            return this;
        }

        public FileQuery WhereLastAccessedTime(Func<DateTime, bool> accessedTestFunction)
        {
            _filters.Add(filePath => accessedTestFunction(_fileSystem.File.GetLastAccessTime(filePath)));
            return this;
        }

        public FileQuery WhereCreationTime(Func<DateTime, bool> createdTestFunction)
        {
            _filters.Add(filePath => createdTestFunction(_fileSystem.File.GetCreationTime(filePath)));
            return this;
        }

        public FileQuery WhereSize(Func<long, bool> sizeTestFunction)
        {
            _filters.Add(filePath => sizeTestFunction(_fileSystem.FileInfo.FromFileName(filePath).Length));
            return this;
        }

        public FileQuery WithMaxDepthOf(int depthLimit)
        {
            _depthLimit = depthLimit;
            return this;
        }

        public IEnumerable<string> Find()
        {
            return _searchPaths.SelectMany(path => Find(path, _depthLimit, 0)).ToList();
        }

        IEnumerable<string> Find(string rootPath, int depthLimit, int depth)
        {
            var allFiles = new List<string>();

            var files = _fileSystem.Directory
                .EnumerateFiles(rootPath)
                .Where(filePath => _filters.All(filter => _negateFilters ? !filter(filePath) : filter(filePath)));

            allFiles.AddRange(files);

            var folders = _fileSystem.Directory.EnumerateDirectories(rootPath);

            if (depth < depthLimit)
            {
                foreach (var folder in folders)
                {
                    allFiles.AddRange(Find(folder, depthLimit, depth + 1));
                }
            }

            return allFiles;
        }
    }
}