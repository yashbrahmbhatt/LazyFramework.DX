using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LazyFramework.DX.Helpers
{
    public static class PathExtensions
    {
        public static string GetRelativePath(string basePath, string targetPath)
        {
            var baseUri = new Uri(AppendDirectorySeparator(basePath));
            var targetUri = new Uri(targetPath);

            if (baseUri.Scheme != targetUri.Scheme)
            {
                throw new InvalidOperationException("URI schemes do not match.");
            }

            var relativeUri = baseUri.MakeRelativeUri(targetUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // Convert URI-style separators to Windows-style separators, if needed
            if (targetUri.Scheme == Uri.UriSchemeFile)
            {
                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                throw new ArgumentException("At least one path must be provided.", nameof(paths));

            string result = string.Empty;

            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                if (string.IsNullOrEmpty(result))
                {
                    result = path;
                }
                else
                {
                    // Ensure separators are consistent
                    if (result.EndsWith(PathSeparator) || path.StartsWith(PathSeparator))
                    {
                        result += path;
                    }
                    else
                    {
                        result += PathSeparator + path;
                    }
                }
            }

            return NormalizePath(result);
        }

        private static string NormalizePath(string path)
        {
            // Replace multiple separators with a single one and handle trailing slashes
            return path.Replace($"{PathSeparator}{PathSeparator}", PathSeparator)
                       .TrimEnd(PathSeparator[0]);
        }

        private const string PathSeparator = "\\"; // Change to '\\' for Windows-style paths

        private static string AppendDirectorySeparator(string path)
        {
            // Manually check if the path ends with the directory separator
            return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;
        }
    }

}