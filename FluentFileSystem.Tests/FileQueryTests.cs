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
        public void WithExtension_NoFilesWithExtensionExists_ReturnsNoFiles()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { @"C:\1.etc", MockFileData.NullObject },
                { @"C:\2.etc", MockFileData.NullObject },
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
        public void WhereLastWriteTime_FilesWithLastWriteTimeSatisfyingLambda_ReturnsAllFilesMatching()
        {
            var dateTimeSatisfyingLambda = DateTime.Now - TimeSpan.FromDays(2);
            var filesSatisfyingLambda = new Dictionary<string, MockFileData>()
            {
                { @"C:\1.txt", new MockFileData("").WithLastWriteTime(dateTimeSatisfyingLambda) }, 
                { @"C:\2.txt", new MockFileData("").WithLastWriteTime(dateTimeSatisfyingLambda) }
            };

            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(filesSatisfyingLambda));
            mockFileSystem.AddFile(@"C:\3.txt", new MockFileData("").WithLastWriteTime(DateTime.Now));

            var files = new FileQuery(mockFileSystem)
                .InPath(@"C:\")
                .WhereLastWriteTime(t => t <= DateTime.Now - TimeSpan.FromDays(1))
                .Find();

            Assert.That(files, Is.EqualTo(filesSatisfyingLambda.Keys));
        }

        [Test]
        public void WhereLastAccessTime_FilesWithLastAccessTimeSatisfyingLambda_ReturnsAllFilesMatching()
        {
            var dateTimeSatisfyingLambda = DateTime.Now - TimeSpan.FromDays(2);
            var filesSatisfyingLambda = new Dictionary<string, MockFileData>()
            {
                { @"C:\1.txt", new MockFileData("").WithLastAccessTime(dateTimeSatisfyingLambda) }, 
                { @"C:\2.txt", new MockFileData("").WithLastAccessTime(dateTimeSatisfyingLambda) }
            };

            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(filesSatisfyingLambda));
            mockFileSystem.AddFile(@"C:\3.txt", new MockFileData("").WithLastAccessTime(DateTime.Now));

            var files = new FileQuery(mockFileSystem)
                .InPath(@"C:\")
                .WhereLastAccessedTime(t => t <= DateTime.Now - TimeSpan.FromDays(1))
                .Find();

            Assert.That(files, Is.EqualTo(filesSatisfyingLambda.Keys));
        }

        [Test]
        public void WhereLastCreationTime_FilesWithCreationTimeSatisfyingLambda_ReturnsAllFilesMatching()
        {
            var dateTimeSatisfyingLambda = DateTime.Now - TimeSpan.FromDays(2);
            var filesSatisfyingLambda = new Dictionary<string, MockFileData>()
            {
                { @"C:\1.txt", new MockFileData("").WithCreationTime(dateTimeSatisfyingLambda) }, 
                { @"C:\2.txt", new MockFileData("").WithCreationTime(dateTimeSatisfyingLambda) }
            };

            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(filesSatisfyingLambda));
            mockFileSystem.AddFile(@"C:\3.txt", new MockFileData("").WithCreationTime(DateTime.Now));

            var files = new FileQuery(mockFileSystem)
                .InPath(@"C:\")
                .WhereCreationTime(t => t <= DateTime.Now - TimeSpan.FromDays(1))
                .Find();

            Assert.That(files, Is.EqualTo(filesSatisfyingLambda.Keys));
        }

        [Test]
        public void WhereSize_FilesWithSizeSatisfyingLambda_ReturnsAllFilesMatching()
        {
            var filesSatisfyingLambda = new Dictionary<string, MockFileData>()
            {
                { @"C:\1.txt", new MockFileData("1") },
                { @"C:\2.txt", new MockFileData("2") },
            };

            var mockFileSystem = new MockFileSystem(filesSatisfyingLambda);
            mockFileSystem.AddFile(@"C:\3.txt", MockFileData.NullObject);

            var files = new FileQuery(mockFileSystem)
                .InPath(@"C:\")
                .WhereSize(s => s == 1)
                .Find();

            Assert.That(files, Is.EqualTo(filesSatisfyingLambda.Keys));
        }
    }
}
