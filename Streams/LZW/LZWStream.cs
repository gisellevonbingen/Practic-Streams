using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

namespace Streams.LZW
{
    public class LZWStream : WrappedByteStream
    {
        protected readonly BitStream BaseBitStream;
        protected readonly CompressionMode Mode;

        public AbstractLZWProcessor Processor { get; private set; }

        protected int ReadingCode { get; private set; } = -1;
        protected IReadOnlyList<byte> ReadingData { get; private set; } = Array.Empty<byte>();
        protected int ReadingPosition { get; private set; } = 0;

        public LZWStream(BitStream baseStream, CompressionMode mode, AbstractLZWProcessor processor) : this(baseStream, mode, processor, false)
        {

        }

        public LZWStream(BitStream baseStream, CompressionMode mode, AbstractLZWProcessor processor, bool leaveOpen) : base(baseStream, leaveOpen)
        {
            this.BaseBitStream = baseStream;
            this.Mode = mode;
            this.Processor = processor;

            if (mode == CompressionMode.Compress)
            {
                this.WriteCode(processor.ClearCode);
            }

        }

        public override bool CanRead => this.BaseStream.CanRead && this.Mode == CompressionMode.Decompress;

        public override bool CanSeek => false;

        public override bool CanWrite => this.BaseStream.CanWrite && this.Mode == CompressionMode.Compress;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public void ClearReading()
        {
            this.ReadingCode = -1;
            this.ReadingPosition = 0;
            this.ReadingData = Array.Empty<byte>();
            this.Processor.ClearTable();
        }

        public virtual int GetCodeBitsUnclamped() => this.Processor.CodeLength + 1;

        public int GetCodeBits() => Math.Min(this.GetCodeBitsUnclamped(), Processor.MaximumCodeLength);

        public int GetBitShift(int bits, int position) => BitStream.GetBitShift(this.BaseBitStream.IsLittleEndian, bits, position);

        protected int ReadCode()
        {
            this.Processor.GrowCodeLength(true);
            var bits = this.GetCodeBits();
            var code = 0;

            for (var i = 0; i < bits; i++)
            {
                var b = this.BaseBitStream.ReadBit();

                if (b == -1)
                {
                    return this.Processor.EoiCode;
                }
                else
                {
                    code |= b << this.GetBitShift(bits, i);
                }

            }

            return code;
        }

        protected void WriteCode(int code)
        {
            this.Processor.GrowCodeLength(false);
            var bits = this.GetCodeBits();

            for (var i = 0; i < bits; i++)
            {
                var shift = this.GetBitShift(bits, i);
                var mask = 1 << shift;
                var bit = (code & mask) >> shift;
                this.BaseBitStream.WriteBit(bit);
            }

        }

        protected bool ReadData()
        {
            var code = this.ReadCode();

            if (code == this.Processor.EoiCode)
            {
                this.ReadingCode = code;
                return false;
            }
            else if (code == this.Processor.ClearCode)
            {
                this.Processor.ClearTable();
                var code2 = this.ReadCode();
                this.ReadingCode = this.Processor.Decode(code2);
                return code2 != this.Processor.EoiCode;
            }
            else
            {
                this.ReadingCode = this.Processor.Decode(code);
                return true;
            }

        }

        public override int ReadByte()
        {
            if (this.ReadingCode == this.Processor.EoiCode)
            {
                return -1;
            }
            else if (this.ReadingPosition >= this.ReadingData.Count)
            {
                if (this.ReadData() == false)
                {
                    return -1;
                }
                else
                {
                    this.ReadingPosition = 0;
                    this.ReadingData = this.Processor.Table[this.ReadingCode].Values;
                }

            }

            var data = this.ReadingData[this.ReadingPosition++];
            return data;
        }

        public override void WriteByte(byte value)
        {
            if (this.GetCodeBitsUnclamped() >= this.Processor.MaximumCodeLength)
            {
                this.WriteClearCode();
            }

            this.WriteData(value);
        }

        public void WriteClearCode()
        {
            this.WriteData(-1);
            this.WriteCode(this.Processor.ClearCode);
            this.Processor.ClearTable();
        }

        protected void WriteData(int value)
        {
            var code = this.Processor.Encode(value);

            if (code == -1)
            {
                return;
            }

            this.WriteCode(code);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.Mode == CompressionMode.Compress)
            {
                this.WriteData(-1);
                this.WriteCode(this.Processor.EoiCode);
            }

            base.Dispose(disposing);
        }

    }

}
