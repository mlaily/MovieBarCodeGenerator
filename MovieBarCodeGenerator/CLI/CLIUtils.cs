using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.CLI
{
    public static class CLIUtils
    {
        private class WildCardInput
        {
            public string PathPartWithoutWildcards { get; set; }
            public string FilePattern { get; set; }
        }
        /// <summary>
        /// the accepted input can be:
        /// - a simple file path
        /// - a directory path
        /// - a file pattern
        /// - a directory path followed by a file pattern
        /// - an url
        /// </summary>
        /// <returns>A list of existing file path matching the input.</returns>
        public static IEnumerable<string> GetExpandedAndValidatedFilePaths(IFileSystemService fileSystemService, IEnumerable<string> rawUserInputs, bool recursive)
        {
            if (rawUserInputs == null)
            {
                return Enumerable.Empty<string>();
            }

            var allInputFiles = new List<WildCardInput>();
            // At this point, the input files list might contain wildcards needing to be expanded.
            // To do that, we either keep just the file path,
            // or, if the path contains a wildcard, we split the directory path and the file pattern:
            foreach (var item in rawUserInputs)
            {
                string rawInputWithoutWildCards = item;
                string inputPattern = "*";
                if (item.Contains('*') || item.Contains('?'))
                {
                    rawInputWithoutWildCards = fileSystemService.GetDirectoryName(item);
                    if (rawInputWithoutWildCards == "") // No directory name. The input is a simple file pattern
                    {
                        rawInputWithoutWildCards = ".";
                    }
                    inputPattern = fileSystemService.GetFileName(item);
                }
                allInputFiles.Add(new WildCardInput { PathPartWithoutWildcards = rawInputWithoutWildCards, FilePattern = inputPattern });
            }

            IEnumerable<string> LazyEnumeration()
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var input in allInputFiles)
                {
                    // The only way to know whether a path is a directory or a file is to test for its existence
                    if (fileSystemService.DirectoryExists(input.PathPartWithoutWildcards))
                    {
                        foreach (var file in fileSystemService.EnumerateDirectoryFiles(input.PathPartWithoutWildcards, input.FilePattern, searchOption))
                        {
                            yield return file;
                        }
                    }
                    else if (fileSystemService.FileExists(input.PathPartWithoutWildcards))
                    {
                        yield return input.PathPartWithoutWildcards;
                    }
                    else 
                    {
                        // FFmpeg is able to handle more than file and directory paths (urls...)
                        yield return input.PathPartWithoutWildcards;
                    }
                }
            }

            return LazyEnumeration();
        }
    }
}
