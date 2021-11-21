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

using PhotoSauce.MagicScaler;
using System.Collections.Generic;
using System.Drawing;

namespace MovieBarCodeGenerator.Core;

public class MagicScalerBarGenerator : IBarGenerator
{
    public bool Average { get; }
    public InterpolationSettings Interpolation { get; }

    public MagicScalerBarGenerator(
        string displayName,
        string fileNameSuffix = "",
        bool average = false,
        InterpolationSettings? interpolation = null)
    {
        _displayName = displayName;
        FileNameSuffix = fileNameSuffix;
        Average = average;
        Interpolation = interpolation ?? InterpolationSettings.Average;
    }

    private static Dictionary<InterpolationSettings, string> _defaultInterpolations = new Dictionary<InterpolationSettings, string>
    {
        [InterpolationSettings.Average] = nameof(InterpolationSettings.Average),
        [InterpolationSettings.CatmullRom] = nameof(InterpolationSettings.CatmullRom),
        [InterpolationSettings.Cubic] = nameof(InterpolationSettings.Cubic),
        [InterpolationSettings.CubicSmoother] = nameof(InterpolationSettings.CubicSmoother),
        [InterpolationSettings.Hermite] = nameof(InterpolationSettings.Hermite),
        [InterpolationSettings.Lanczos] = nameof(InterpolationSettings.Lanczos),
        [InterpolationSettings.Linear] = nameof(InterpolationSettings.Linear),
        [InterpolationSettings.Mitchell] = nameof(InterpolationSettings.Mitchell),
        [InterpolationSettings.NearestNeighbor] = nameof(InterpolationSettings.NearestNeighbor),
        [InterpolationSettings.Quadratic] = nameof(InterpolationSettings.Quadratic),
        [InterpolationSettings.Spline36] = nameof(InterpolationSettings.Spline36),
    };
    private static string GetInterpolationName(InterpolationSettings interpolation)
        => _defaultInterpolations.TryGetValue(interpolation, out var result) ? result : "Unknown";

    public string Name =>
        $"Magic"
        + $"-Average={Average}"
        + $"-Interpolation={(GetInterpolationName(Interpolation))}";
    private string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }


    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
    {
        ProcessImageSettings GetSettingsBase()
            => new ProcessImageSettings
            {
                ResizeMode = CropScaleMode.Stretch,
                Sharpen = false,
                Interpolation = Interpolation,
                SaveFormat = FileFormat.Bmp,
                OrientationMode = OrientationMode.Ignore,
                Width = barWidth,
                Height = barHeight,
            };

        var processImageSettings = GetSettingsBase();

        if (Average)
        {
            processImageSettings.Height = 1;
        }

        var resizedStream = new MemoryStream();
        using (var pipeline = MagicImageProcessor.BuildPipeline(source, processImageSettings))
        {
            pipeline.WriteOutput(resizedStream);
        }

        if (Average)
        {
            var secondPassSettings = GetSettingsBase();
            resizedStream.Position = 0;
            using (var pipeline = MagicImageProcessor.BuildPipeline(resizedStream, secondPassSettings))
            {
                var secondPassStream = new MemoryStream();
                pipeline.WriteOutput(secondPassStream);
                var image = Image.FromStream(secondPassStream, true, false);
                return image;
            }
        }
        else
        {
            var image = Image.FromStream(resizedStream, true, false);
            return image;
        }
    }
}
