## Examples

You create query using the `FileQuery` constructor.

_Finds files in "C:\backups\" and any sub-directories._
```csharp
new FileQuery()
    .InPath(@"C:\backups\")
    .Find()
```

By default it will search through sub-directories recursively, this can be controlled via `.WithMaxDepthOf()`.

_Finds files in the root directory of "C:\backups\"_
```csharp
new FileQuery()
    .InPath(@"C:\backups\")
    .WithMaxDepthOf(0) // 0 will only search the root path
    .Find()
```

Common file information is surfaced methods that take function parameters.

_Finds files older than a week._
```csharp
new FileQuery()
    .InPath(@"C:\backups\")
    .WhereLastWriteTime(time => time < DateTime.Now.AddDays(-7))
    .Find()
```