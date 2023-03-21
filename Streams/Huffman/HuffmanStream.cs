using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.Huffman
{
    public class HuffmanStream : AbstractHuffmanStream
    {
        private readonly Dictionary<byte, HuffmanCode> Caches;

        public HuffmanStream(Stream baseStream, HuffmanNode<byte> rootNode) : this(baseStream, rootNode, false)
        {

        }

        public HuffmanStream(Stream baseStream, HuffmanNode<byte> rootNode, bool leaveOpen) : base(baseStream, leaveOpen)
        {
            this.Caches = rootNode.ToCodeTable();
        }

        protected override Dictionary<byte, HuffmanCode> NextReadingCodes() => this.Caches;

        protected override Dictionary<byte, HuffmanCode> NextWritingCodes() => this.Caches;

    }

}
