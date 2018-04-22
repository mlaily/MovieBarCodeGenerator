# Movie BarCode Generator

A user friendly tool to generate movie barcodes.

More information here: [zerowidthjoiner.net/movie-barcode-generator](https://zerowidthjoiner.net/movie-barcode-generator)

![GUI](readme-images/gui.png)

## Example results

![Blade Runner 2049](readme-images/blade_runner_1000.jpg)

With the "Smooth bars" feature enabled:

![Blade Runner 2049 Smooth](readme-images/blade_runner_1000_smooth.png)

## About the code

This tool is mostly a wrapper around [FFmpeg](http://ffmpeg.org/).

A copy of ffmpeg.exe must be placed next to the main executable for it to work.

All the code in this repository is released under the GPL license. see [License.txt](License.txt) for more information.

The code has been written relatively quickly (a few hours), but should be of acceptable quality.

There might be buggy corner cases though.

For the old legacy version based on DirectShow and released in 2011, [see here](https://github.com/mlaily/MovieBarCodeGenerator-Legacy).
