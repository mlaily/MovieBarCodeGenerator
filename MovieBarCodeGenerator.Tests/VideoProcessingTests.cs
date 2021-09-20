using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieBarCodeGenerator.Core;
using NUnit.Framework;
using PhotoSauce.MagicScaler;

namespace MovieBarCodeGenerator.Tests
{
    [TestFixture]
    public class VideoProcessingTests
    {

        public string FfmpegExecutablePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../ffmpeg.exe");
        public string TestVideoFileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.mkv");
        public const int TestVideoWidth = 1280;
        public const int TestVideoHeight = 720;
        public const int TestVideoDuration = 10;

        private void CreateTestVideoIfNecessary()
        {
            if (!File.Exists(TestVideoFileName))
            {
                var commandArguments = $"-f lavfi -i testsrc=duration={TestVideoDuration}:size={TestVideoWidth}x{TestVideoHeight}:rate=30 {TestVideoFileName}";
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = FfmpegExecutablePath,
                    Arguments = commandArguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });
                process.WaitForExit(10000);
            }
        }

        [Test]
        public void FfFmpegExecutableExists()
        {
            Assert.IsTrue(File.Exists(FfmpegExecutablePath));
        }

        [Test]
        public void TestFileCanBeCreated()
        {
            CreateTestVideoIfNecessary();

            Assert.IsTrue(File.Exists(TestVideoFileName));
        }

        [Test]
        public void FfmpegWrapper_GetMediaDuration_Returns_Correct_Value()
        {
            CreateTestVideoIfNecessary();
            var ffmpegWrapper = new FfmpegWrapper(FfmpegExecutablePath);

            var mediaInfo = ffmpegWrapper.GetMediaInfo(TestVideoFileName, CancellationToken.None);

            Assert.AreEqual(TimeSpan.FromSeconds(TestVideoDuration), mediaInfo.Duration);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(10)]
        [TestCase(21)]
        public async Task FfmpegWrapper_GetImagesFromMedia_Returns_Expected_Values(int requestedFrameCount)
        {
            CreateTestVideoIfNecessary();
            var ffmpegWrapper = new FfmpegWrapper(FfmpegExecutablePath);

            var images = ffmpegWrapper.GetImagesFromMedia(
                TestVideoFileName,
                requestedFrameCount,
                CancellationToken.None)
                .ToList();

            CollectionAssert.AllItemsAreNotNull(images);
            Assert.AreEqual(requestedFrameCount, images.Count);
            foreach (var bitmapStream in images)
            {
                var imageInfo = await Task.Run(() => ImageFileInfo.Load(bitmapStream));
                Assert.AreEqual(TestVideoWidth, imageInfo.Frames[0].Width);
                Assert.AreEqual(TestVideoHeight, imageInfo.Frames[0].Height);
            }
        }

        // TODO: CLI tests (file patterns, various flags combinations...), parameters validation tests, and actual barcode creation tests.
    }
}
