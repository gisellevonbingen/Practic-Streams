using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.Huffman
{
    public struct HuffmanCode : IEquatable<HuffmanCode>, IComparable<HuffmanCode>
    {
        public int Raw { get; set; }
        public int Length { get; set; }

        public HuffmanCode(HuffmanCode parent, int bit)
        {
            this.Raw = parent.Raw << 1 | bit;
            this.Length = parent.Length + 1;
        }

        public override bool Equals(object obj) => obj is HuffmanCode other && this.Equals(other);

        public bool Equals(HuffmanCode other) => this.Raw == other.Raw && this.Length == other.Length;

        public override int GetHashCode() => HashCode.Combine(this.Raw, this.Length);

        public override string ToString() => Convert.ToString(this.Raw, 2).PadLeft(this.Length, '0');

        public int CompareTo(HuffmanCode other) => this.ToString().CompareTo(other.ToString());

        public static bool operator ==(HuffmanCode left, HuffmanCode right) => left.Equals(right);

        public static bool operator !=(HuffmanCode left, HuffmanCode right) => !(left == right);

    }

}
