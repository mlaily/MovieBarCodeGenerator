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

using System.Threading;

namespace MovieBarCodeGenerator.Core;

/// <summary>
/// A data stream for a single bitmap image.
/// </summary>
public class BitmapStream : Stream
{
    public static bool TryCreate(BinaryReader reader, out BitmapStream bitmapStream, CancellationToken cancellationToken)
    {
        try
        {
            // https://en.wikipedia.org/wiki/BMP_file_format
            var magicNumber = reader.ReadBytes(2);
            if (magicNumber.Length != 2)
            {
                bitmapStream = null;
                return false;
            }

            if (magicNumber[0] != 0x42 || magicNumber[1] != 0x4D)
            {
                throw new InvalidDataException();
            }

            var bmpSizeBytes = reader.ReadBytes(4);
            var bmpSize = BitConverter.ToInt32(bmpSizeBytes, 0);

            var remainingDataLength = bmpSize - bmpSizeBytes.Length - magicNumber.Length;
            var remainingData = reader.ReadBytes(remainingDataLength);

            var ms = new MemoryStream();
            ms.Write(magicNumber, 0, magicNumber.Length);
            ms.Write(bmpSizeBytes, 0, bmpSizeBytes.Length);
            ms.Write(remainingData, 0, remainingData.Length);
            ms.Position = 0;

            bitmapStream = new BitmapStream(ms);
            return true;
        }
        catch (Exception)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new OperationCanceledException();
            }
            else
            {
                throw;
            }
        }
    }

    private readonly MemoryStream _dataStream;
    public BitmapStream(MemoryStream dataStream)
    {
        _dataStream = dataStream;
    }

    public override bool CanRead => _dataStream.CanRead;
    public override bool CanSeek => _dataStream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _dataStream.Length;
    public override long Position
    {
        get => _dataStream.Position;
        set => _dataStream.Position = value;
    }

    public override void Flush() => _dataStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _dataStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _dataStream.Seek(offset, origin);

    public override void SetLength(long value) => _dataStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _dataStream.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dataStream.Dispose();
        }

        base.Dispose(disposing);
    }
}
