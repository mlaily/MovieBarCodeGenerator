//Copyright 2011-2018 Melvyn Laily
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
        public CompleteBarCodeGenerationParameters GetValidatedParameters(
            string rawInputPath,
            string rawOutputPath,
            string rawBarWidth,
            string rawImageWidth,
            string rawImageHeight,
            bool useInputHeightForOutput,
            bool generateSmoothVersion,
            Func<string, bool> shouldOverwriteOutput)
        {
            var inputPath = rawInputPath.Trim(new[] { '"' });

            var outputPath = rawOutputPath?.Trim(new[] { '"' });

            void ValidateOutputPath(ref string path)
            {
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

                if (File.Exists(path) && shouldOverwriteOutput(path) == false)
                {
                    throw new OperationCanceledException();
                }
            }

            ValidateOutputPath(ref outputPath);

            string smoothedOutputPath = null;
            if (generateSmoothVersion)
            {
                var name = $"{GetSafeFileNameWithoutExtension(outputPath)}_smoothed{Path.GetExtension(outputPath)}";
                smoothedOutputPath = Path.Combine(Path.GetDirectoryName(outputPath), name);
                ValidateOutputPath(ref smoothedOutputPath);
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
            if (!useInputHeightForOutput)
            {
                if (int.TryParse(rawImageHeight, out var nonNullableImageHeight) && nonNullableImageHeight > 0)
                {
                    imageHeight = nonNullableImageHeight;
                }
                else
                {
                    throw new ParameterValidationException("Invalid output height.");
                }
            }

            var barcodeParameters = new BarCodeParameters()
            {
                BarWidth = barWidth,
                Width = imageWidth,
                Height = imageHeight,
            };

            return new CompleteBarCodeGenerationParameters
            {
                BarCode = barcodeParameters,
                InputPath = inputPath,
                OutputPath = outputPath,
                SmoothedOutputPath = smoothedOutputPath,
                GenerateSmoothedOutput = generateSmoothVersion,
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
