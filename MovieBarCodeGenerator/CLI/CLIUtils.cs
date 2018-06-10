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
        /// - a list of all the above separated by the '|' character
        /// </summary>
        /// <returns>A list of existing file path matching the input.</returns>
        public static IEnumerable<string> GetExpandedAndValidatedFilePaths(IEnumerable<string> rawUserInputs, bool recursive)
        {
            if (rawUserInputs == null)
            {
                return Enumerable.Empty<string>();
            }

            List<WildCardInput> allInputFiles
                = new List<WildCardInput>();
            // At this point, the input files list might contain wildcards needing to be expanded.
            // To do that, we either keep just the file path,
            // or, if the path contains a wildcard, we split the directory path and the file pattern:
            foreach (var item in rawUserInputs)
            {
                string rawInputWithoutWildCards = item;
                string inputPattern = "*";
                if (item.Contains('*') || item.Contains('?'))
                {
                    rawInputWithoutWildCards = Path.GetDirectoryName(item);
                    if (rawInputWithoutWildCards == "") // No directory name. The input is a simple file pattern
                    {
                        rawInputWithoutWildCards = ".";
                    }
                    inputPattern = Path.GetFileName(item);
                }
                allInputFiles.Add(new WildCardInput { PathPartWithoutWildcards = rawInputWithoutWildCards, FilePattern = inputPattern });
            }

            IEnumerable<string> LazyEnumeration()
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var input in allInputFiles)
                {
                    // The only way to know whether a path is a directory or a file is to test for its existence
                    if (Directory.Exists(input.PathPartWithoutWildcards))
                    {
                        foreach (var file in Directory.EnumerateFiles(input.PathPartWithoutWildcards, input.FilePattern, searchOption))
                        {
                            yield return file;
                        }
                    }
                    else if (File.Exists(input.PathPartWithoutWildcards))
                    {
                        yield return input.PathPartWithoutWildcards;
                    }
                }
            }

            return LazyEnumeration();
        }
    }
}
