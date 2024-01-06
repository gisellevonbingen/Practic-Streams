using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.IO
{
    public class BitStream : WrappedByteStream
    {
        public static int GetBitShift(bool isLittleEndian, int bits, int position)
        {
            if (0 > position || position > bits - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }
            else if (isLittleEndian == true)
            {
                return position;
            }
            else
            {
                return bits - 1 - position;
            }

        }

        private int ReadingByte = 0;
        private int ReadingPosition = -1;
        private int ReadingLength = 0;
        private int WritingByte = 0;
        private int WritingPosition = 0;

        public bool IsLittleEndian { get; private set; }
        public long InBits { get; protected set; }
        public long InBytes { get; protected set; }
        public long OutBits { get; protected set; }
        public long OutBytes { get; protected set; }

        public BitStream(Stream baseStream, bool isLittleEndian) : this(baseStream, isLittleEndian, false)
        {

        }

        public BitStream(Stream baseStream, bool isLittleEndian, bool leaveOpen) : base(baseStream, leaveOpen)
        {
            this.IsLittleEndian = isLittleEndian;
        }

        protected virtual int ReadEncodedByte(out int length)
        {
            length = 8;
            return this.ReadEncodedByte();
        }

        protected virtual int ReadEncodedByte()
        {
            return this.BaseStream.ReadByte();
        }

        protected virtual bool TryWriteEncodedByte(byte value, int position, bool disposing)
        {
            if (position == 8 | disposing == true)
            {
                this.BaseStream.WriteByte(value);
                return true;
            }
            else
            {
                return false;
            }

        }

        public int GetBitShift(int positionInByte) => GetBitShift(this.IsLittleEndian, 8, positionInByte);

        public int ReadBit()
        {
            if (this.ReadingPosition == -1 || this.ReadingPosition == this.ReadingLength)
            {
                this.ReadingByte = this.ReadEncodedByte(out var length);
                this.ReadingPosition = 0;
                this.ReadingLength = length;

                if (this.ReadingByte > -1)
                {
                    this.InBytes++;
                }

            }

            if (this.ReadingByte == -1)
            {
                return -1;
            }

            var shift = this.GetBitShift(this.ReadingPosition);
            var bitMask = 1 << shift;
            var bit = (this.ReadingByte & bitMask) >> shift;
            this.ReadingPosition++;
            this.InBits++;

            return bit;
        }

        public override int ReadByte()
        {
            var value = 0;

            for (var i = 0; i < 8; i++)
            {
                var bit = this.ReadBit();

                if (bit == -1)
                {
                    return bit;
                }

                var shift = this.GetBitShift(i);
                value |= bit << shift;
            }

            return value;
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

        public void WriteBit(bool bit) => this.WriteBit(Convert.ToInt32(bit));

        public void WriteBit(int bit)
        {
            var shift = this.GetBitShift(this.WritingPosition);
            this.WritingByte |= bit << shift;
            this.WritingPosition++;
            this.OutBits++;

            if (this.TryWriteEncodedByte((byte)this.WritingByte, this.WritingPosition, false) == true)
            {
                this.WritingByte = 0;
                this.WritingPosition = 0;
                this.OutBytes++;
            }

        }

        public override void WriteByte(byte value)
        {
            for (var i = 0; i < 8; i++)
            {
                var shift = this.GetBitShift(i);
                var bitMask = 1 << shift;
                var bit = (value & bitMask) >> shift;
                this.WriteBit(bit);
            }

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                this.WriteByte(buffer[offset + i]);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (this.WritingPosition > 0)
            {
                this.TryWriteEncodedByte((byte)this.WritingByte, this.WritingPosition, true);
                this.WritingPosition = 0;
            }

            base.Dispose(disposing);
        }

        public override bool CanSeek => false;

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    }

}
