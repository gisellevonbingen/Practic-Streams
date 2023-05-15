using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.Collections;

namespace Streams.LZW
{
    public class LZWProcessor
    {
        public BidirectionalDictionary<int, LZWNode> Table { get; }
        private LZWNode EncodeBuilder;
        public int LastKey { get; private set; } = -1;

        public LZWProcessor() : this(0)
        {

        }

        public LZWProcessor(int extendsKeyOffset)
        {
            this.Table = new BidirectionalDictionary<int, LZWNode>();
            this.EncodeBuilder = new LZWNode();

            for (int i = byte.MinValue; i <= byte.MaxValue; i++)
            {
                this.Table.Add(i, new LZWNode((byte)i));
            }

            this.NextKey = this.Table.Count + extendsKeyOffset;
        }

        public virtual int NextKey { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Table Key of Inserted values</returns>
        public int InsertToTable(LZWNode node)
        {
            var key = this.NextKey++;

            if (this.Table.TryGetA(node, out var prevKey) == true)
            {
                throw new LZWException($"Already inserted node as key : {prevKey}, insertingKey : {key}");
            }

            this.Table.Add(key, node);
            return key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Table Key of Inserted value, -1 mean 'Require More Values'</returns>
        public int Encode(int value)
        {
            var lastKey = this.LastKey;

            if (value <= -1)
            {
                this.LastKey = -1;
                return lastKey;
            }
            else
            {
                var byteValue = (byte)value;
                var builder = this.EncodeBuilder;
                builder.Add(byteValue);

                if (this.Table.TryGetA(builder, out var key) == true)
                {
                    this.LastKey = key;
                    return -1;
                }
                else
                {
                    this.InsertToTable(builder);
                    this.EncodeBuilder = new LZWNode(byteValue);

                    this.LastKey = value;
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
                var lastKey = this.LastKey;
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

                    this.LastKey = code;
                    return code;
                }
                else
                {
                    builder.Add(table[lastKey].Values[0]);
                    var key = this.InsertToTable(builder);
                    this.LastKey = key;
                    return key;
                }

            }

        }

    }

}
