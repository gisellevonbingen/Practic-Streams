using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.IO
{
    public abstract class InternalStream : WrappedStream
    {
        protected bool ReadingMode { get; }

        protected long StartBasePosition { get; }
        private long _InternalPosition = 0L;

        public InternalStream(Stream baseStream, bool readingMode) : this(baseStream, readingMode, false)
        {

        }

        public InternalStream(Stream baseStream, bool readingMode, bool leaveOpen) : base(baseStream, leaveOpen)
        {
            this.ReadingMode = readingMode;

            if (this.CanSeek == true)
            {
                this.StartBasePosition = baseStream.Position;
            }
            else
            {
                this.StartBasePosition = -1L;
            }

        }

        public override bool CanRead => base.CanRead && this.ReadingMode == true;

        public override bool CanWrite => base.CanWrite && this.ReadingMode == false;

        public override bool CanSeek => base.CanSeek;

        protected long InternalPosition
        {
            get => this._InternalPosition;
            private set
            {
                if (this.CanSeek == false)
                {
                    this._InternalPosition = value;
                }
                else if (0 <= value && value <= this.Length)
                {
                    this._InternalPosition = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

            }

        }

        public override long Position
        {
            get => this.InternalPosition;
            set
            {
                if (this.CanSeek == false)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    this.InternalPosition = value;
                    this.BaseStream.Position = this.StartBasePosition + value;
                }

            }

        }

        public override long Length => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readingCount;

            if (this.CanSeek == true)
            {
                readingCount = (int)Math.Min(this.Length - this.Position, count);
            }
            else
            {
                readingCount = count;
            }

            if (readingCount > 0)
            {
                var readLength = base.Read(buffer, offset, readingCount);
                this.InternalPosition += readingCount;
                return readLength;
            }
            else
            {
                return 0;
            }

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int writingCount;

            if (this.CanSeek == true)
            {
                writingCount = (int)Math.Min(this.Length - this.Position, count);
            }
            else
            {
                writingCount = count;
            }

            if (writingCount > 0)
            {
                base.Write(buffer, offset, writingCount);
                this.InternalPosition += writingCount;
            }

        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this.CanSeek == false)
            {
                throw new NotSupportedException();
            }
            else if (origin == SeekOrigin.Begin)
            {
                this.Position = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                this.Position += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                this.Position = this.Length - offset;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(origin));
            }

            return this.Position;
        }

    }

}
