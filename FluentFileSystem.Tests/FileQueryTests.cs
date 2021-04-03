using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace FluentFileSystem.Tests
{
    [TestFixture]
    class FileQueryTests 
    {
        [Test]
        public void InPath_WithFilesInPath_ReturnsAllFilesInPath()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { @"C:\test\1.txt", MockFileData.NullObject },
                { @"C:\test\2.txt", MockFileData.NullObject },
                { @"C:\test\3.txt", MockFileData.NullObject }
            });

            var files = new FileQuery(mockFileSystem)
                .InPath(@"C:\test\")
                .Find();

            Assert.That(files, Is.EqualTo(mockFileSystem.AllFiles.ToList()));
        }

        [Test]
        public void InPaths_WithFilesInPaths_ReturnsAllFilesInAllPaths()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { @"C:\test\1.txt", MockFileData.NullObject },
                { @"C:\test2\1.txt", MockFileData.NullObject },
            });

            var files = new FileQuery(mockFileSystem)
                .InPaths(new List<string>{
                    @"C:\test\",
                    @"C:\test2\"
                })
                .Find();

            Assert.That(files, Is.EqualTo(mockFileSystem.AllFiles.ToList()));
        }

        [Test]
        public void InPaths_PathDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { @"C:\test\1.txt", MockFileData.NullObject },
                { @"C:\test2\1.txt", MockFileData.NullObject },
            });

            Assert.Throws<DirectoryNotFoundException>(() =>
            {
                var files = new FileQuery(mockFileSystem)
                    .InPaths(new List<string>
                    {
                        @"C:\test\",
                        @"C:\test3\"
                    })
                    .Find();
            });
        }

        [Test]
        public void WithExtension_FilesWithExtensionExists_ReturnsAllFilesWithMatchingExtension()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { @"C:\1.txt", MockFileData.NullObject },
                { @"C:\2.txt", MockFileData.NullObject },
                { @"C:\3.etc", MockFileData.NullObject }
            });

            var files = new FileQuery(mockFileSystem)
                .InPath(@"C:\")
                .WithExtension(".txt")
                .Find();

            Assert.That(files, Is.EqualTo(mockFileSystem.AllFiles.Where(f => f.EndsWith(".txt")).ToList()));
        }

        [Test]
        public void WithExtension_ExtensionProvidedWithoutDot_ReturnsAllFilesWithMatchingExtension()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { @"C:\1.txt", MockFileData.NullObject },
                { @"C:\2.txt", MockFileData.NullObject },
                { @"C:\3.etc", MockFileData.NullObject }
            });

            var files = new FileQuery(mockFileSystem)
                .InPath(@"C:\")
                .WithExtension("txt")
                .Find();

            Assert.That(files, Is.EqualTo(mockFileSystem.AllFiles.Where(f => f.EndsWith(".txt")).ToList()));
        }

        [Test]
        public void WhereSize_FileWithSizeSatisfyingLambda_ReturnsAllFilesMatching()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { @"C:\1.txt", new MockFileData("1")},
                { @"C:\2.txt", MockFileData.NullObject },
                { @"C:\3.etc", MockFileData.NullObject }
            });

            var files = new FileQuery(mockFileSystem)
                .InPath(@"C:\")
                .WhereSize(size => size == 1)
                .Find();

            var fileInfo = mockFileSystem.FileInfo.FromFileName(@"C:\1.txt");
            Assert.That(files, Is.EqualTo(mockFileSystem.AllFiles.Where(f => mockFileSystem.FileInfo.FromFileName(f).Length == 1)));
        }
    }
}
