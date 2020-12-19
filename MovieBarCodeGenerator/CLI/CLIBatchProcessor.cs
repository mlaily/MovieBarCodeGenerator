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

using Mono.Options;
using MovieBarCodeGenerator.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.CLI
{
    public class CLIBatchProcessor
    {
        private BarCodeParametersValidator _barCodeParametersValidator = new BarCodeParametersValidator();
        private FfmpegWrapper _ffmpegWrapper = new FfmpegWrapper("ffmpeg.exe");
        private ImageStreamProcessor _imageProcessor = new ImageStreamProcessor();

        public void Process(string[] args)
        {
            var arguments = new RawArguments();
            var allRawInputs = new List<string>();

            var options = new OptionSet();

            options.Add("?|help",
                "Show this help message.",
                x => ShowHelp(options));

            options.Add("in=|input=",
                @"Accepted inputs:
- a file path
- a directory path
- a file pattern (simple '?' and '*' wildcards are accepted)
- a directory path followed by a file pattern
- an url
This parameter can be set multiple times.",
                x => allRawInputs.Add(x));

            options.Add("out=|output=",
                "Output file or directory. Default: current directory, same name as the input file.",
                x => arguments.RawOutput = x);

            options.Add("x|overwrite",
                "If set, existing files will be overwritten instead of being ignored.",
                x => arguments.Overwrite = true);

            options.Add("r|recursive",
                "If set, input is browsed recursively.",
                x => arguments.Recursive = true);

            options.Add("w=|width=",
                $"Width of the output image. Default: {RawArguments.DefaultWidth}",
                x => arguments.RawWidth = x);

            options.Add("h=|H=|height=",
                "Height of the output image. If this argument is not set, the input height will be used.",
                x => arguments.RawHeight = x);

            options.Add("b=|barwidth=|barWidth=",
                $"Width of each bar in the output image. Default: {RawArguments.DefaultBarWidth}",
                x => arguments.RawBarWidth = x);

            options.Add("s|smooth",
                "Also generate a smooth version of the output, suffixed with '_smoothed'.",
                x => arguments.Smooth = true);

            try
            {
                options.Parse(args);
            }
            catch (OptionException ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
                ShowHelp(options);
                return;
            }

            if (arguments.RawHeight == null)
            {
                arguments.UseInputHeight = true;
            }

            var fileSystemService = new FileSystemService();
            var expandedInputFileList = CLIUtils.GetExpandedAndValidatedFilePaths(fileSystemService, allRawInputs, arguments.Recursive).ToList();

            if (expandedInputFileList.Any())
            {
                foreach (var file in expandedInputFileList)
                {
                    arguments.RawInput = file; // FIXME: copy instead of changing in place...
                    DealWithOneInputFile(arguments);
                }
            }
            else
            {
                Console.WriteLine("No input.");
            }

            Console.WriteLine($"Exiting...");
        }

        private void DealWithOneInputFile(RawArguments arguments)
        {
            Console.WriteLine($"Processing file '{arguments.RawInput}':");

            CompleteBarCodeGenerationParameters parameters;
            try
            {
                parameters = _barCodeParametersValidator.GetValidatedParameters(
                    rawInputPath: arguments.RawInput,
                    rawOutputPath: arguments.RawOutput,
                    rawBarWidth: arguments.RawBarWidth,
                    rawImageWidth: arguments.RawWidth,
                    rawImageHeight: arguments.RawHeight,
                    useInputHeightForOutput: arguments.UseInputHeight,
                    generateSmoothVersion: arguments.Smooth,
                    // Choosing whether to overwrite or not is done after validating parameters, not here
                    shouldOverwriteOutput: x => true);
            }
            catch (ParameterValidationException ex)
            {
                Console.Error.WriteLine($"Invalid parameters: {ex.Message}");
                return;
            }

            if (File.Exists(parameters.OutputPath) && arguments.Overwrite == false)
            {
                // Check once before generating the image, and once just before saving.
                Console.WriteLine($"WARNING: skipped file {parameters.OutputPath} because it already exists.");
                return;
            }

            var barGenerator = new GdiBarGenerator();

            var result = _imageProcessor.CreateBarCode(
                parameters.InputPath,
                parameters.BarCode,
                _ffmpegWrapper,
                CancellationToken.None,
                null,
                x => Console.WriteLine(x),
                barGenerator).Single();

            try
            {
                if (File.Exists(parameters.OutputPath) && arguments.Overwrite == false)
                {
                    // Check once before generating the image, and once just before saving.
                    Console.WriteLine($"WARNING: skipped file {parameters.OutputPath} because it already exists.");
                }
                else
                {
                    result.Save(parameters.OutputPath);
                    Console.WriteLine($"File {parameters.OutputPath} saved successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to save the image: {ex}");
            }

            if (parameters.GenerateSmoothedOutput)
            {
                Bitmap smoothed;
                try
                {
                    smoothed = _imageProcessor.GetSmoothedCopy(result);

                    try
                    {
                        if (File.Exists(parameters.SmoothedOutputPath) && arguments.Overwrite == false)
                        {
                            Console.WriteLine($"WARNING: skipped file {parameters.SmoothedOutputPath} because it already exists.");
                        }
                        else
                        {
                            smoothed.Save(parameters.SmoothedOutputPath);
                            Console.WriteLine($"File {parameters.SmoothedOutputPath} saved successfully!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Unable to save the smoothed image: {ex}");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while creating the smoothed version of the barcode. Error: {ex}");
                }
            }
        }

        private static void ShowHelp(OptionSet options)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            Console.WriteLine($@"Movie BarCode Generator {executingAssembly.GetName().Version}

Generate bar codes from movies. (concatenate movie frames in one image)

You can provide one input file, or a full directory,
along with an output file or directory.
");

            options.WriteOptionDescriptions(Console.Out);
        }
    }

    class RawArguments
    {
        public const string DefaultWidth = "1000";
        public const string DefaultBarWidth = "1";
        public string RawInput { get; set; } = null;
        public string RawOutput { get; set; } = null;
        public bool Overwrite { get; set; } = false;
        public bool Recursive { get; set; } = false;
        public string RawWidth { get; set; } = DefaultWidth;
        public string RawHeight { get; set; } = null;
        public bool UseInputHeight { get; set; } = false;
        public string RawBarWidth { get; set; } = DefaultBarWidth;
        public bool Smooth { get; set; } = false;
    }
}
