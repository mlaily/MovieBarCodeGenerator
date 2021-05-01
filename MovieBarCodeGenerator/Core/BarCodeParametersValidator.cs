//Copyright 2011-2021 Melvyn Laily
//https://zerowidthjoiner.net

//This file is part of MovieBarCodeGenerator.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public class BarCodeParametersValidator
    {
        public BarCodeParameters GetValidatedParameters(
            string rawInputPath,
            string rawBaseOutputPath,
            string rawBarWidth,
            string rawImageWidth,
            string rawImageHeight,
            Func<IReadOnlyCollection<string>, bool> shouldOverwriteOutputPaths,
            IEnumerable<IBarGenerator> barGenerators)
        {
            var inputPath = rawInputPath.Trim(new[] { '"' });

            (string Path, bool AlreadyExists) ValidateOutputPath(string initialPath)
            {
                string path = initialPath;
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = $"{GetSafeFileNameWithoutExtension(inputPath)}.png";
                }

                if (!Path.HasExtension(path))
                {
                    path += ".png";
                }

                if (path.Any(x => Path.GetInvalidPathChars().Contains(x)))
                {
                    throw new ParameterValidationException("The output path is invalid.");
                }

                return (path, File.Exists(path));
            }

            var baseOutputPath = ValidateOutputPath(rawBaseOutputPath?.Trim(new[] { '"' })).Path;

            var outputPaths = from generator in barGenerators
                              let name = $"{GetSafeFileNameWithoutExtension(baseOutputPath)}{generator.FileNameSuffix}{Path.GetExtension(baseOutputPath)}"
                              let path = Path.Combine(Path.GetDirectoryName(baseOutputPath), name)
                              select new { generator, ValidatedPath = ValidateOutputPath(path) };

            if (outputPaths.Any(x => x.ValidatedPath.AlreadyExists)
                && shouldOverwriteOutputPaths(outputPaths.Where(x => x.ValidatedPath.AlreadyExists).Select(x => x.ValidatedPath.Path).ToList()) == false)
            {
                throw new OperationCanceledException("At least one output file already exists.");
            }

            if (!int.TryParse(rawBarWidth, out var barWidth) || barWidth <= 0)
            {
                throw new ParameterValidationException("Invalid bar width.");
            }

            if (!int.TryParse(rawImageWidth, out var imageWidth) || imageWidth <= 0)
            {
                throw new ParameterValidationException("Invalid output width.");
            }

            int? imageHeight = null;
            if (int.TryParse(rawImageHeight, out var nonNullableImageHeight) && nonNullableImageHeight > 0)
            {
                imageHeight = nonNullableImageHeight;
            }
            else
            {
                throw new ParameterValidationException("Invalid output height.");
            }

            return new BarCodeParameters
            {
                InputPath = inputPath,
                GeneratorOutputPaths = outputPaths.ToDictionary(x => x.generator, x => x.ValidatedPath.Path),
                BarWidth = barWidth,
                Width = imageWidth,
                Height = imageHeight,
            };
        }

        private string GetSafeFileNameWithoutExtension(string input)
        {
            try
            {
                return Path.GetFileNameWithoutExtension(input);
            }
            catch
            {
                // TODO: this implementation could largely be improved...
                return "output";
            }
        }
    }

    public class ParameterValidationException : Exception
    {
        public ParameterValidationException(string message) : base(message) { }
    }
}
