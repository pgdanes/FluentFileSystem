using System;
using System.IO.Abstractions.TestingHelpers;

namespace FluentFileSystem.Tests
{
    public static class MockFileDataExtensions
    {
        public static MockFileData WithLastWriteTime(this MockFileData mockFileData, DateTime lastWriteTime)
        {
            mockFileData.LastWriteTime = lastWriteTime;
            return mockFileData;
        }

        public static MockFileData WithLastAccessTime(this MockFileData mockFileData, DateTime lastAccessTime)
        {
            mockFileData.LastAccessTime = lastAccessTime;
            return mockFileData;
        }

        public static MockFileData WithCreationTime(this MockFileData mockFileData, DateTime creationTime)
        {
            mockFileData.CreationTime = creationTime;
            return mockFileData;
        }
    }
}