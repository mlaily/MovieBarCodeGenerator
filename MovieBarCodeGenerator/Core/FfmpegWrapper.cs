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

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MovieBarCodeGenerator.Core;

public class FfmpegWrapper
{
    public string FfmpegExecutablePath { get; }

    public FfmpegWrapper(string ffmpegExecutablePath)
    {
        FfmpegExecutablePath = ffmpegExecutablePath;
    }

    private Process StartFfmpegInstance(string args, bool redirectError = false)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = FfmpegExecutablePath,
                Arguments = args,
                // Warning: if a standard stread is redirected but is not read,
                // its buffer might fill up and block the whole process.
                // Only redirect a standard stream if you read it!
                RedirectStandardOutput = true,
                RedirectStandardError = redirectError,
                StandardErrorEncoding = redirectError ? Encoding.UTF8 : null, // Setting encoding crashes if not redirected. Default is null.
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            return process;
        }
        catch (Exception ex)
        {
            throw new Exception("Error while starting FFmpeg", ex);
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            process.Kill();
        }
        catch
        {
            // The call may fail if the process already exited, and we don't want to crash for that...
        }
    }

    public MediaInfo GetMediaInfo(string inputPath, CancellationToken cancellationToken, Action<string> log = null)
    {
        log?.Invoke("Getting input duration from FFmpeg...");

        var args = $"-i \"{inputPath}\"";

        var process = StartFfmpegInstance(args, redirectError: true);

        using (cancellationToken.Register(() => TryKill(process)))
        {
            var output = process.StandardError.ReadToEnd();

            foreach (var item in output.Split(new[] { '\n' }))
            {
                // mimic the behaviour of ErrorDataReceived and BeginErrorReadLine:
                log?.Invoke(item);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var result = new MediaInfo();

            {
                var match = Regex.Match(output, @"Duration: (.*?),");
                if (match.Success && TryParseHourBasedTimeSpan(match.Groups[1].Value, out var parsedDuration))
                {
                    TryParseHourBasedTimeSpan("30:22:07.51", out var result2);
                    result.Duration = parsedDuration;
                }
                else
                {
                    throw new FormatException("Could not parse media duration.");
                }
            }
            {
                // bt2020 seems to be the color primary used by all HDR formats...
                // (We used to just detect 10bits videos here, but it's not enough. A video can be 10bits and still be bt709 and thus SDR)
                // TODO: maybe add bt2100 if/when ffmpeg supports it?
                var match = Regex.Match(output, @"Video: .*?bt2020");
                result.IsHDR = match.Success;
            }

            return result;
        }
    }

    /// <summary>
    /// <see cref="TimeSpan.Parse(string)"/> throws on values with too many hours like `30:22:07.51`,
    /// so we need to do it ourselves...
    /// </summary>
    private bool TryParseHourBasedTimeSpan(string value, out TimeSpan result)
    {
        var match = Regex.Match(value, @"(?<hours>\d+):(?<minutes>\d+):(?<seconds>\d+)\.(?<centiseconds>\d+)");
        if (match.Success
            && int.TryParse(match.Groups["hours"].Value, out var hours)
            && int.TryParse(match.Groups["minutes"].Value, out var minutes)
            && int.TryParse(match.Groups["seconds"].Value, out var seconds)
            && int.TryParse(match.Groups["centiseconds"].Value, out var centiseconds))
        {
            result = new TimeSpan(days: 0, hours: hours, minutes: minutes, seconds: seconds, milliseconds: centiseconds * 10);
            return true;
        }

        result = default;
        return false;
    }

    public IEnumerable<BitmapStream> GetImagesFromMedia(string inputPath, int frameCount, CancellationToken cancellationToken, Action<string> log = null, bool autoToneMapHDR = true)
    {
        var mediaInfo = GetMediaInfo(inputPath, cancellationToken, log);

        log?.Invoke("Reading images from FFmpeg...");

        var fps = frameCount / mediaInfo.Duration.TotalSeconds;
        var fpsFilter = $"fps={fps.ToInvariantString()}";

        // Note: tone mapping algorithms have been tested (*cough* *cough* on the Interstellar movie only =°)
        // and compared to the SDR reference barcode, hable seems to give the closest result.
        // Warning: this is very slow!
        // https://web.archive.org/web/20190722004804/https://stevens.li/guides/video/converting-hdr-to-sdr-with-ffmpeg/
        var hdrToSdrFilter = "zscale=transfer=linear:npl=100,format=gbrpf32le,zscale=primaries=bt709,tonemap=tonemap=hable,zscale=transfer=bt709:matrix=bt709:range=tv,format=yuv420p";

        var vfilters = new List<string> { fpsFilter };

        if (mediaInfo.IsHDR && autoToneMapHDR)
        {
            log?.Invoke("HDR video detected. Adding tone mapping filter.");
            vfilters.Add(hdrToSdrFilter);
        }

        // Output a raw stream of bitmap images taken at the specified frequency
        var args = $"-i \"{inputPath}\" -vf \"{string.Join(",", vfilters)}\" -c:v bmp -f rawvideo -an -";

        log?.Invoke($"FFmpeg arguments: {args}");

        var process = StartFfmpegInstance(args, redirectError: log != null);

        if (log != null)
        {
            process.ErrorDataReceived += (s, e) => log(e.Data);
            process.BeginErrorReadLine();
        }

        IEnumerable<BitmapStream> GetLazyStream()
        {
            using (cancellationToken.Register(() => TryKill(process)))
            using (var reader = new BinaryReader(process.StandardOutput.BaseStream))
            {
                while (BitmapStream.TryCreate(reader, out var bitmapStream, cancellationToken))
                {
                    yield return bitmapStream;
                }
            }
        }

        return GetLazyStream();
    }
}

public class MediaInfo
{
    public TimeSpan Duration { get; internal set; }
    public bool IsHDR { get; internal set; }
}
