using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.IO
{
    public class WrappedStream : Stream
    {
        protected virtual Stream BaseStream { get; }
        protected bool LeaveOpen { get; }

        public WrappedStream(Stream baseStream) : this(baseStream, false)
        {

        }

        public WrappedStream(Stream baseStream, bool leaveOpen)
        {
            this.BaseStream = baseStream;
            this.LeaveOpen = leaveOpen;
        }

        public override bool CanRead => this.BaseStream.CanRead;

        public override bool CanWrite => this.BaseStream.CanWrite;

        public override bool CanSeek => this.BaseStream.CanSeek;

        public override long Position
        {
            get => this.BaseStream.Position;
            set => this.BaseStream.Position = value;
        }

        public override long Length => this.BaseStream.Length;

        public override void SetLength(long value) => this.BaseStream.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count) => this.BaseStream.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count) => this.BaseStream.Write(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => this.BaseStream.Seek(offset, origin);

        public override void Flush() => this.BaseStream.Flush();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.LeaveOpen == false)
            {
                this.BaseStream.Dispose();
            }

        }

    }

}
