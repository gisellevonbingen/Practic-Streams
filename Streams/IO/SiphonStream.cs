using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.IO
{
    public class SiphonStream : InternalStream
    {
        private readonly long _Length = 0L;

        public SiphonStream(Stream baseStream, long length, bool readingMode) : this(baseStream, length, readingMode, false)
        {

        }

        public SiphonStream(Stream baseStream, long length, bool readingMode, bool leaveOpen) : base(baseStream, readingMode, leaveOpen)
        {
            this._Length = length;
        }

        public override long Length => this._Length;

        public override void SetLength(long value) => throw new NotSupportedException();

    }

}
