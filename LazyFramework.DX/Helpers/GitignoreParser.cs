using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LazyFramework.Models
{
    public static class GitignoreParser
    {
        public static IEnumerable<string> GeneratePaths(string input, string rootPath)
        {
            // Split the input string by newlines
            var patterns = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var regexPatterns = new List<string>();

            foreach (var pattern in patterns)
            {
                var trimmedPattern = pattern.Trim();
                if (string.IsNullOrWhiteSpace(trimmedPattern) || trimmedPattern.StartsWith("#"))
                    continue; // Ignore empty lines and comments

                // Convert .gitignore patterns to regex
                var regexPattern = ConvertToRegex(trimmedPattern);
                regexPatterns.Add(regexPattern);
            }

            return FindMatchingPaths(regexPatterns, rootPath);
        }

        private static string ConvertToRegex(string pattern)
        {
            var regex = Regex.Escape(pattern)
                             .Replace("\\*", ".*")     // * matches any number of characters
                             .Replace("\\?", ".")      // ? matches a single character
                             .Replace("/.*", "(?:/.*)?$"); // Match directories

            if (!pattern.StartsWith("/") && !pattern.Contains("/"))
            {
                // Add match for any subdirectory if the pattern is relative
                regex = $"(^|.*/)({regex})";
            }
            else
            {
                regex = $"^{regex}";
            }

            return regex;
        }

        private static IEnumerable<string> FindMatchingPaths(List<string> regexPatterns, string rootPath)
        {
            var matchingPaths = new List<string>();
            var allFiles = Directory.EnumerateFileSystemEntries(rootPath, "*", SearchOption.AllDirectories);

            foreach (var path in allFiles)
            {
                foreach (var regexPattern in regexPatterns)
                {
                    if (Regex.IsMatch(path, regexPattern))
                    {
                        matchingPaths.Add(path);
                        break; // Avoid adding duplicates
                    }
                }
            }

            return matchingPaths;
        }
    }
}
