using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.IO
{
    public class SiphonBlock : IDisposable
    {
        public Stream BaseStream { get; }
        public long Origin { get; }
        public long Length { get; }

        private readonly MemoryStream Memory;

        public SiphonStream SiphonStream { get; }

        public static SiphonBlock ByLength(Stream baseStream, long length)
        {
            if (baseStream.CanSeek == false)
            {
                return ByLength(baseStream, 0, length);
            }
            else
            {
                var origin = baseStream.Position;
                return ByLength(baseStream, origin, length);
            }

        }

        private static int GetReadBufferSize(Stream stream, long length)
        {
            length = Math.Min(length, 81920);

            if (stream.CanSeek == false)
            {
                return (int)length;
            }
            else
            {
                var remain = stream.GetRemain();

                if (remain > 0)
                {
                    return (int)Math.Min(length, remain);
                }
                else
                {
                    return 1;
                }

            }

        }

        public static SiphonBlock ByLength(Stream baseStream, long blockOrigin, long length)
        {
            var bufferSize = GetReadBufferSize(baseStream, length);
            return ByLength(baseStream, blockOrigin, length, bufferSize);
        }

        public static SiphonBlock ByLength(Stream baseStream, long blockOrigin, long length, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            return ByLength(baseStream, blockOrigin, length, buffer);
        }

        public static SiphonBlock ByLength(Stream baseStream, long blockOrigin, long length, byte[] buffer)
        {
            if (baseStream.CanSeek == false)
            {
                var position = 0;
                var memory = new MemoryStream();

                while (true)
                {
                    var readCount = (int)Math.Min(length - position, buffer.Length);
                    var reading = baseStream.Read(buffer, 0, readCount);

                    if (reading <= 0)
                    {
                        break;
                    }
                    else
                    {
                        position += reading;
                        memory.Write(buffer, 0, reading);
                    }

                }

                memory.Position = 0L;
                var siphon = new SiphonStream(memory, length, true, true);
                return new SiphonBlock(baseStream, siphon, blockOrigin, memory);
            }
            else
            {
                var siphon = new SiphonStream(baseStream, length, true, true);
                return new SiphonBlock(baseStream, siphon, blockOrigin);
            }

        }

        public static SiphonBlock ByRemain(Stream baseStream)
        {
            if (baseStream.CanSeek == false)
            {
                return ByRemain(baseStream, 0);
            }
            else
            {
                var origin = baseStream.Position;
                return ByRemain(baseStream, origin);
            }

        }

        public static SiphonBlock ByRemain(Stream baseStream, long blockOrigin)
        {
            if (baseStream.CanSeek == false)
            {
                var memory = new MemoryStream();
                baseStream.CopyTo(memory);
                memory.Position = 0L;
                var siphon = new SiphonStream(memory, memory.Length, true, true);
                return new SiphonBlock(baseStream, siphon, blockOrigin, memory);
            }
            else
            {
                var length = baseStream.GetRemain();
                var siphon = new SiphonStream(baseStream, length, true, true);
                return new SiphonBlock(baseStream, siphon, blockOrigin);
            }

        }

        private SiphonBlock(Stream baseStream, SiphonStream siphonStream, long origin)
        {
            this.BaseStream = baseStream;
            this.Origin = origin;
            this.Length = siphonStream.Length;

            this.Memory = null;
            this.SiphonStream = siphonStream;
        }

        private SiphonBlock(Stream baseStream, SiphonStream siphonStream, long origin, MemoryStream memoryStream) : this(baseStream, siphonStream, origin)
        {
            this.Memory = memoryStream;
        }

        public void SetBasePosition(long positionFromOrigin)
        {
            this.SiphonStream.Position = positionFromOrigin - this.Origin;
        }

        public long BaseEndPosition => this.Origin + this.Length;

        protected virtual void Dispose(bool disposing)
        {
            this.SiphonStream.Dispose();

            if (this.Memory != null)
            {
                this.Memory.Dispose();
            }

            if (this.BaseStream.CanSeek == true)
            {
                this.BaseStream.Position = this.BaseEndPosition;
            }

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        ~SiphonBlock()
        {
            this.Dispose(false);
        }

    }

}
