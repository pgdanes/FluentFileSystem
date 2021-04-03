using FluentFileSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var paths = new List<string>
            {
                @"C:\temp\",
                @"C:\source\"
            };

            var files = new FileQuery()
                .InPath(@"C:\temp\")
                .WithExtension("sql")
                .Find()
                .ToList();

            var files2 = Directory.EnumerateFiles(@"C:\temp\", @"*.sql", SearchOption.AllDirectories);
        }
    }
}
