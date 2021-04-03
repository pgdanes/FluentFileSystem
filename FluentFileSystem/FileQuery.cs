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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileSystem">The file system, if left null the query object will be made using the default file system.</param>
        public FileQuery(IFileSystem fileSystem = null)
        {
            fileSystem ??= new FileSystem();
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Add paths to find files within.
        /// </summary>
        /// <param name="paths">List of paths to search in.</param>
        public FileQuery InPaths(IEnumerable<string> paths)
        {
            _searchPaths.ForEach(path =>
            {
                _fileSystem.Directory.Exists(path);
            });

            _searchPaths.AddRange(paths.Distinct().ToList());
            return this;
        }

        /// <summary>
        /// Add path to find files within.
        /// </summary>
        /// <param name="path">Path to search in.</param>
        public FileQuery InPath(string path)
        {
            return InPaths(new List<string>
            {
                path
            });
        }

        /// <summary>
        /// Add a filter to match the file'ss extension to a provided string. 
        /// </summary>
        /// <param name="extension">The extension name.</param>
        /// <remarks>
        /// Will accept extension specified with or without preceding full stop,
        /// i.e. ".txt" and "txt" are both acceptable.
        /// </remarks>
        public FileQuery WithExtension(string extension)
        {
            return WithExtension(new List<string>
            {
                extension
            });
        }

        /// <summary>
        /// Add a filter where the file's extension matches a provided list.
        /// </summary>
        /// <param name="extensions">The extensions.</param>
        /// <remarks>
        /// Will accept extension specified with or without preceding full stop,
        /// i.e. ".txt" and "txt" are both acceptable.
        /// </remarks>
        public FileQuery WithExtension(IEnumerable<string> extensions)
        {
            var extensionsWithDot = extensions.Select(e => e.StartsWith(".") ? e : $".{e}");
            _filters.Add(filePath => extensionsWithDot.Any(extension => Path.GetExtension(filePath).ToLower().Equals(extension)));
            return this;
        }

        /// <summary>
        /// Add a filter where the file name matches a provided regex pattern.
        /// </summary>
        /// <param name="expression">The regex expression to match on.</param>
        /// <remarks>
        /// Expression matches on full file name including extension.
        /// For filtering on extension solely <see cref="WithExtension(string)"/>
        /// </remarks>
        public FileQuery Matching(Regex expression)
        {
            _filters.Add(filePath => expression.IsMatch(_fileSystem.Path.GetFileName(filePath)));
            return this;
        }

        /// <summary>
        /// Negates all specified filters.
        /// </summary>
        /// <remarks>
        /// All filters are evaluated on a terminal method e.g. <see cref="Find"/>,
        /// therefore this method is not required to occur after other filter methods to take affect.
        /// </remarks>
        public FileQuery Not()
        {
            _negateFilters = true;
            return this;
        }

        /// <summary>
        /// Add a filter where the file's last write time satisfies a provided function.
        /// </summary>
        /// <param name="modifiedTimeTestFunction">The function used to test the last accessed time.</param>
        public FileQuery WhereLastWriteTime(Func<DateTime, bool> modifiedTimeTestFunction)
        {
            _filters.Add(filePath => modifiedTimeTestFunction(_fileSystem.File.GetLastWriteTime(filePath)));
            return this;
        }

        /// <summary>
        /// Add a filter where the file's last accessed time satisfies a provided function.
        /// </summary>
        /// <param name="accessedTimeTestFunction">The function used to test the last accessed time.</param>
        public FileQuery WhereLastAccessedTime(Func<DateTime, bool> accessedTimeTestFunction)
        {
            _filters.Add(filePath => accessedTimeTestFunction(_fileSystem.File.GetLastAccessTime(filePath)));
            return this;
        }

        /// <summary>
        /// Add a filter where the file's last accessed time satisfies a provided function.
        /// </summary>
        /// <param name="createdTimeTestFunction">The function used to test the creation time.</param>
        public FileQuery WhereCreationTime(Func<DateTime, bool> createdTimeTestFunction)
        {
            _filters.Add(filePath => createdTimeTestFunction(_fileSystem.File.GetCreationTime(filePath)));
            return this;
        }

        /// <summary>
        /// Add a filter where the file's size satisfies a provided function.
        /// </summary>
        /// <param name="sizeTestFunction">The function used to test the file size.</param>
        public FileQuery WhereSize(Func<long, bool> sizeTestFunction)
        {
            _filters.Add(filePath => sizeTestFunction(_fileSystem.FileInfo.FromFileName(filePath).Length));
            return this;
        }

        /// <summary>
        /// Specify a maximum sub-directory depth that files will be searched for in.
        /// </summary>
        /// <param name="depthLimit">The depth limit.</param>
        public FileQuery WithMaxDepthOf(int depthLimit)
        {
            _depthLimit = depthLimit;
            return this;
        }

        /// <summary>
        /// Find all files that match all the provided filters.
        /// </summary>
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