using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
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

        private MemoryStream _dataStream;
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
}
