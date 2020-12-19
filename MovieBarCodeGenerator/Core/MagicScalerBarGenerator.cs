using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public class MagicScalerBarGenerator : IBarGenerator
    {
        public Image GetBar(BitmapStream source, int barWidth, int barHeight)
        {
            var processImageSettings = new ProcessImageSettings
            {
                ResizeMode = CropScaleMode.Stretch,
                HybridMode = HybridScaleMode.Off,
                Sharpen = false,
                Interpolation = InterpolationSettings.Average,
                SaveFormat = FileFormat.Bmp,
                OrientationMode = OrientationMode.Ignore,
                Width = barWidth,
                Height = barHeight,
            };
            using (var pipeline = MagicImageProcessor.BuildPipeline(source, processImageSettings))
            {
                var resizedStream = new MemoryStream();
                pipeline.WriteOutput(resizedStream);
                var gdiImage = Image.FromStream(resizedStream, true, false);
                return gdiImage;
            }
        }
    }
}
