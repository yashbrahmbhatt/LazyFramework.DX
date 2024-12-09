using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LazyFramework.DX.Models.Helpers
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

        private static string AppendDirectorySeparator(string path)
        {
            // Manually check if the path ends with the directory separator
            return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;
        }
    }

}