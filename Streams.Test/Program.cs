using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Streams.Base64;
using Streams.Huffman;
using Streams.IO;
using Streams.LZW;

namespace Streams
{
    public static class Program
    {
        public static void Main()
        {
            var readTest = new MemoryStream(new byte[] { 0x00, 0x51, 0xFC });
            var readStream = new BitStream(readTest, true);
            var writeTest = new MemoryStream();
            var writeStream = new BitStream(writeTest, true);

            for (var i = 0; ;i++)
            {
                var bit = readStream.ReadBit();
                Console.WriteLine($"{i} - {bit}");
                writeStream.WriteBit(bit);

                if (bit == -1)
                {
                    return;
                }

            }

            var texts = new List<string>()
            {
                "Hello, World!\r\n안녕하세요!",
                "BABAABAAA",
                "AAAAAAABBCCCDEEEEFFFFFFGHIIJ",
                "000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000,000.000"
            };

            foreach (var text in texts)
            {
                Test(text);
            }

        }

        private static void Test(string text)
        {
            var testers = new List<CompressTester>
            {
                new Base64CompressTester(),
                new DeflateCompressTester(),
                new HuffmanCompressTester(),
                new LZWCompressTester()
            };

            foreach (var tester in testers)
            {
                Test(tester, text);
            }

        }

        public static void Test(CompressTester tester, string text)
        {
            var encoding = Encoding.Default;
            byte[] original = encoding.GetBytes(text);
            byte[] compressed = null;
            byte[] decompressed = null;

            Console.WriteLine();
            Console.WriteLine($"==================== {tester.Name} ====================");
            Console.WriteLine();
            Console.WriteLine($"===== Original =====");
            Console.WriteLine($"String\t\t: {text}");
            Console.WriteLine($"Bytes\t\t: {BitConverter.ToString(original)}");

            tester.PrepareCompress(original);

            using (var input = new MemoryStream(original))
            {
                using var output = new MemoryStream();
                tester.Compress(input, output);
                compressed = output.ToArray();
            }

            Console.WriteLine();
            Console.WriteLine($"===== Compressed =====");
            Console.WriteLine($"String\t\t: {encoding.GetString(compressed)}");
            Console.WriteLine($"Bytes\t\t: {BitConverter.ToString(compressed)}");

            tester.PrepareDecompress(compressed);

            using (var input = new MemoryStream(compressed))
            {
                using var output = new MemoryStream();
                tester.Decompress(input, output);
                decompressed = output.ToArray();
            }

            Console.WriteLine();
            Console.WriteLine($"===== Decompressed =====");
            Console.WriteLine($"String\t\t: {encoding.GetString(decompressed)}");
            Console.WriteLine($"Bytes\t\t: {BitConverter.ToString(decompressed)}");

            Console.WriteLine();
            Console.WriteLine($"===== Result =====");
            Console.WriteLine($"Length\t: {original.Length} => {compressed.Length} ({(compressed.Length / (original.Length / 100.0D)):F2}%)");
            Console.WriteLine($"Test\t\t: {original.SequenceEqual(decompressed)}");
        }

        public abstract class CompressTester
        {
            public abstract string Name { get; }

            public virtual void PrepareCompress(byte[] original) { }

            public virtual void PrepareDecompress(byte[] compressed) { }

            public abstract void Compress(Stream input, Stream output);

            public abstract void Decompress(Stream input, Stream output);
        }

        public class Base64CompressTester : CompressTester
        {
            public override string Name => "Base64";

            public override void Compress(Stream input, Stream output)
            {
                using var bs = new Base64Stream(output, true);
                input.CopyTo(bs);
            }

            public override void Decompress(Stream input, Stream output)
            {
                using var bs = new Base64Stream(input, true);
                bs.CopyTo(output);
            }

        }

        public class DeflateCompressTester : CompressTester
        {
            public override string Name => "Deflate";

            public override void Compress(Stream input, Stream output)
            {
                using var bs = new DeflateStream(output, CompressionLevel.Optimal);
                input.CopyTo(bs);
            }

            public override void Decompress(Stream input, Stream output)
            {
                using var bs = new DeflateStream(input, CompressionMode.Decompress);
                bs.CopyTo(output);
            }

        }

        public class HuffmanCompressTester : CompressTester
        {
            public override string Name => "Huffman";

            public HuffmanNode<byte> RootNode { get; set; }

            public override void PrepareCompress(byte[] original)
            {
                base.PrepareCompress(original);

                var rootNode = this.RootNode = HuffmanNode<byte>.CreateRootNode(original);
                var nodeMap = rootNode.ToCodeTable();

                Console.WriteLine();
                Console.WriteLine("Nodes By Element");
                Console.WriteLine();

                foreach (var pair in nodeMap.OrderBy(p => p.Key))
                {
                    Console.WriteLine($"    0x{pair.Key:X2} : {pair.Value}");
                }

                Console.WriteLine();
                Console.WriteLine("Nodes By Table");

                var sombolTable = rootNode.ToSimbolTable();

                for (var depth = 0; depth < sombolTable.Length; depth++)
                {
                    var simbols = sombolTable[depth];

                    Console.WriteLine();
                    Console.WriteLine($"   Code Length : {depth + 1}");
                    Console.WriteLine($"   Simbols : {string.Join(", ", simbols.Select(x => $"0x{x:X2}"))}");
                }

            }

            public override void Compress(Stream input, Stream output)
            {
                using var bs = new HuffmanStream(output, this.RootNode);
                input.CopyTo(bs);
            }

            public override void Decompress(Stream input, Stream output)
            {
                using var bs = new HuffmanStream(input, this.RootNode);
                bs.CopyTo(output);
            }

        }

        public class LZWCompressTester : CompressTester
        {
            public override string Name => "LZW";

            public override void Compress(Stream input, Stream output)
            {
                using var bs = new LZWStream(new BitStream(output, false), CompressionMode.Compress, new TestLZWPRocessor());
                input.CopyTo(bs);
            }

            public override void Decompress(Stream input, Stream output)
            {
                using var bs = new LZWStream(new BitStream(input, false), CompressionMode.Decompress, new TestLZWPRocessor());
                bs.CopyTo(output);
            }

            public class TestLZWPRocessor : AbstractLZWProcessor
            {
                public TestLZWPRocessor() : base(8, 12)
                {

                }

                public override int GetCodeLengthGrowThreashold(bool reading) => (int)Math.Pow(2, this.CodeLength + 2);
            }

        }

    }

}

