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
            this.Table = new BidirectionalDictionary<int, LZWNode>();
            this.ClearTable();
        }

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

            var maxExclusive = 1 << this.MinimumCodeLength;

            for (int i = 0; i < maxExclusive; i++)
            {
                this.Table.Add(i, new LZWNode((byte)i));
            }

            this.ClearCode = maxExclusive + 0;
            this.EoiCode = maxExclusive + 1;
            this.NextCode = maxExclusive + 2;
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
            if (this.Table.TryGetA(node, out var prevKey) == false)
            {
                var key = this.NextCode++;
                this.Table.Add(key, node);
                return key;
            }
            else
            {
                return prevKey;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Table Key of Inserted value, -1 mean 'Require More Values'</returns>
        public int Encode(int value)
        {
            var lastKey = this.LastCode;

            if (value <= -1)
            {
                this.LastCode = -1;
                this.NextCode++;
                this.EncodeBuilder = new LZWNode();
                return lastKey;
            }
            else
            {
                var byteValue = (byte)value;
                var builder = this.EncodeBuilder;
                builder.Add(byteValue);

                if (this.Table.TryGetA(builder, out var key) == true)
                {
                    this.LastCode = key;
                    return -1;
                }
                else
                {
                    this.InsertToTable(builder);
                    this.EncodeBuilder = new LZWNode(byteValue);

                    this.LastCode = value;
                    return lastKey;
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
                var builder = new LZWNode();

                if (lastKey > -1)
                {
                    builder.AddRange(table[lastKey].Values);
                }

                if (table.ContainsA(code) == true)
                {
                    if (lastKey > -1)
                    {
                        builder.Add(table[code].Values[0]);
                        this.InsertToTable(builder);
                    }

                    this.LastCode = code;
                    return code;
                }
                else
                {
                    builder.Add(table[lastKey].Values[0]);
                    var key = this.InsertToTable(builder);
                    this.LastCode = key;
                    return key;
                }

            }

        }

    }

}
