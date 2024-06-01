using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.Collections;

namespace Streams.LZW
{
    /// <summary>
    /// A variation of the LZW algorithm that uses variable length codes
    /// </summary>
    public abstract class AbstractLZWProcessor
    {
        public BidirectionalDictionary<int, LZWNode> Table { get; }
        public int MinimumCodeLength { get; private set; }
        public int MaximumCodeLength { get; private set; }
        /// <summary>
        /// Exclusive
        /// </summary>
        public int MaximumCode { get; private set; }

        private LZWNode EncodeBuilder;
        public int ClearCode { get; private set; } = -1;
        public int EoiCode { get; private set; } = -1;
        public int LastCode { get; private set; } = -1;
        public int NextCode { get; private set; } = -1;
        public int CodeLength { get; private set; } = -1;

        public AbstractLZWProcessor(int minimumCodeLength, int maximumCodeLength)
        {
            this.MinimumCodeLength = minimumCodeLength;
            this.MaximumCodeLength = maximumCodeLength;
            this.MaximumCode = 1 << minimumCodeLength;
            this.Table = new BidirectionalDictionary<int, LZWNode>();
            this.ClearTable();

            this.ClearCode = this.MaximumCode + 0;
            this.EoiCode = this.MaximumCode + 1;
        }

        public virtual int GetCodeBitsUnclamped() => this.CodeLength + 1;

        public int GetCodeBits() => Math.Min(this.GetCodeBitsUnclamped(), this.MaximumCodeLength);

        public abstract int GetCodeLengthGrowThreashold(bool reading);

        public void GrowCodeLength(bool reading)
        {
            while (this.NextCode >= this.GetCodeLengthGrowThreashold(reading))
            {
                this.CodeLength++;
            }

        }

        public void ClearTable()
        {
            this.Table.Clear();
            this.EncodeBuilder = new LZWNode();

            for (var i = 0; i < this.MaximumCode; i++)
            {
                this.Table.Add(i, new LZWNode((byte)i));
            }

            this.NextCode = this.MaximumCode + 2;
            this.LastCode = -1;
            this.CodeLength = this.MinimumCodeLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Table Key of Inserted values</returns>
        public int InsertToTable(LZWNode node)
        {
            if (this.Table.TryGetA(node, out var prevCode) == false)
            {
                var code = this.NextCode++;
                this.Table.Add(code, node);
                return code;
            }
            else
            {
                return prevCode;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Table Key of Inserted value, -1 mean 'Require More Values'</returns>
        public int Encode(int value)
        {
            var lastCode = this.LastCode;

            if (value <= -1)
            {
                this.LastCode = -1;
                this.NextCode++;
                this.EncodeBuilder = new LZWNode();
                return lastCode;
            }
            else
            {
                var byteValue = (byte)value;
                var builder = this.EncodeBuilder;
                builder.Add(byteValue);

                if (this.Table.TryGetA(builder, out var code) == true)
                {
                    this.LastCode = code;
                    return -1;
                }
                else
                {
                    this.InsertToTable(builder);
                    this.EncodeBuilder = new LZWNode(byteValue);

                    this.LastCode = value;
                    return lastCode;
                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns>Table Key of decoded data, -1 mean 'End Of Decode'</returns>
        public int Decode(int code)
        {
            if (code <= -1)
            {
                return -1;
            }
            else
            {
                var table = this.Table;
                var lastKey = this.LastCode;
                var lastNode = lastKey > -1 ? table[lastKey] : null;
                var builder = new LZWNode();

                if (lastNode != null)
                {
                    builder.AddRange(lastNode.Values);
                }

                if (table.TryGetB(code, out var existing) == true)
                {
                    if (lastNode != null)
                    {
                        builder.Add(existing.Values[0]);
                        this.InsertToTable(builder);
                    }

                    this.LastCode = code;
                    return code;
                }
                else
                {
                    builder.Add(lastNode.Values[0]);
                    var newCode = this.InsertToTable(builder);
                    this.LastCode = newCode;
                    return newCode;
                }

            }

        }

    }

}
