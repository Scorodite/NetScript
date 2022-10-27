using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NetScript.Interpretation
{
    /// <summary>
    /// Stream that reads data from byte array
    /// </summary>
    public class ByteArrayReader : Stream
    {
        private byte[] Source { get; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => Source.Length;
        public override long Position { get; set; }

        public ByteArrayReader(byte[] source)
        {
            Source = source;
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotImplementedException();

        public override void SetLength(long value) =>
            throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotImplementedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            for (int i = offset; i < offset + count; i++)
            {
                if (Position >= Length)
                {
                    return read;
                }
                buffer[i] = Source[Position++];
                read++;
            }
            return read;
        }

        public override int ReadByte()
        {
            if (Position >= Length)
            {
                return -1;
            }
            return Source[Position++];
        }
    }
}
