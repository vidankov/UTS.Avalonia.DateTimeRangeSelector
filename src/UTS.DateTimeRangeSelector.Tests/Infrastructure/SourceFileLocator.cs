using System.Reflection;

namespace UTS.DateTimeRangeSelector.Tests.Infrastructure;

/// <summary>
/// Locates library source files relative to the test assembly for FileSystem-scoped proof tests.
/// </summary>
internal static class SourceFileLocator
{
    internal static string? FindSourceFile(string relativeFromSrc)
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
        while (dir != null)
        {
            var srcDir = Path.Combine(dir.FullName, "src");
            if (Directory.Exists(srcDir))
            {
                var full = Path.Combine(srcDir, relativeFromSrc.Replace('/', Path.DirectorySeparatorChar));
                return File.Exists(full) ? full : null;
            }

            dir = dir.Parent;
        }

        return null;
    }

    internal static string RequireSourceFile(string relativeFromSrc)
    {
        var path = FindSourceFile(relativeFromSrc);
        if (path is null)
        {
            Assert.Skip($"Source file not found: src/{relativeFromSrc}");
        }

        return path!;
    }
}
