using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.IO
{
    public class LengthOnlyStream : Stream
    {
        private long _Position = 0L;
        private long _Length = 0L;

        public LengthOnlyStream()
        {

        }

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Position { get => this._Position; set => this._Position = Math.Max(value, 0); }

        public override long Length => this._Length;

        public override void SetLength(long value)
        {
            this._Length = Math.Max(value, 0L);
            this.Position = Math.Max(this.Position, value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.Position += count;
            this._Length = Math.Max(this.Position, this.Length);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
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

        public override void Flush()
        {

        }

    }

}
