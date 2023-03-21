using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

namespace Streams.Huffman
{
    public abstract class AbstractHuffmanStream : BitStream
    {
        protected AbstractHuffmanStream(Stream baseStream) : base(baseStream)
        {

        }

        protected AbstractHuffmanStream(Stream baseStream, bool leaveOpen) : base(baseStream, leaveOpen)
        {

        }

        protected abstract Dictionary<byte, HuffmanCode> NextReadingCodes();

        protected abstract Dictionary<byte, HuffmanCode> NextWritingCodes();

        public override int ReadByte()
        {
            var rawCode = 0;
            var codes = this.NextReadingCodes();

            for (var i = 0; ; i++)
            {
                var bit = this.ReadBit();

                if (bit == -1)
                {
                    return bit;
                }

                rawCode = rawCode << 1 | bit;
                var length = i + 1;
                var looksMaxLength = 1;

                foreach (var pair in codes)
                {
                    var code = pair.Value;

                    if (code.Raw == rawCode && code.Length == length)
                    {
                        this.InBytes++;
                        return pair.Key;
                    }
                    else if (code.Length > looksMaxLength)
                    {
                        looksMaxLength = code.Length;
                    }

                }

                if (length >= looksMaxLength)
                {
                    throw new ArgumentException($"RawCode 0x{rawCode:X2} is not exist in HuffmanTable");
                }

            }

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var b = this.ReadByte();

                if (b == -1)
                {
                    return i;
                }

                buffer[offset + i] = (byte)b;
            }

            return count;
        }

        public override void WriteByte(byte value)
        {
            var codes = this.NextWritingCodes();

            if (codes.TryGetValue(value, out var code) == false)
            {
                throw new ArgumentException($"Byte 0x{value:X2} is not exist in HuffmanTable");
            }

            for (var i = 0; i < code.Length; i++)
            {
                var shift = code.Length - 1 - i;
                var bitMask = 1 << shift;
                var bit = (code.Raw & bitMask) >> shift;
                this.WriteBit(bit);
            }

            this.OutBytes++;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                this.WriteByte(buffer[offset + i]);
            }

        }

        public override bool CanRead => this.BaseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => this.BaseStream.CanWrite;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

    }

}
