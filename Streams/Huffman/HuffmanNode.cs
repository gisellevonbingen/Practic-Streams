using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.Huffman
{
    public class HuffmanNode<T> : IEnumerable<HuffmanNode<T>>
    {
        public static HuffmanNode<T> FromSimbolTable(T[][] table)
        {
            var prevDepthNodes = new List<HuffmanNode<T>>();

            for (var depth = table.Length - 1; depth > -1; depth--)
            {
                var nodes = table[depth].Select(v => new HuffmanNode<T>(v)).ToList();
                nodes.AddRange(prevDepthNodes);

                var carry = new List<HuffmanNode<T>>();

                for (var i = 0; i < nodes.Count; i += 2)
                {
                    var left = nodes[i];
                    var right = (i + 1 < nodes.Count) ? nodes[i + 1] : null;
                    var node = new HuffmanNode<T>(left, right);
                    carry.Add(node);
                }

                prevDepthNodes.Clear();
                prevDepthNodes.AddRange(carry);
            }

            if (prevDepthNodes.Count == 1)
            {
                return prevDepthNodes[0];
            }
            else if (prevDepthNodes.Count == 2)
            {
                return new HuffmanNode<T>(prevDepthNodes[0], prevDepthNodes[1]);
            }
            else
            {
                throw new ArgumentException("Invalid HuffmanTable", nameof(table));
            }

        }

        public static HuffmanNode<T> CreateRootNode(IEnumerable<T> collection)
        {
            var counts = new Dictionary<HuffmanNode<T>, int>();
            var nodes = new List<HuffmanNode<T>>();

            foreach (var (count, value) in collection.GroupBy(c => c).Select(g => (Count: g.Count(), Value: g.Key)).OrderByDescending(n => n.Count))
            {
                var node = new HuffmanNode<T>(value);
                counts[node] = count;
                nodes.Add(node);
            }

            if (nodes.Count > 1)
            {
                while (nodes.Count > 1)
                {
                    var index1 = nodes.Count - 1;
                    var node1 = nodes[index1];
                    nodes.RemoveAt(index1);
                    var count1 = counts[node1];
                    counts.Remove(node1);

                    var index2 = nodes.Count - 1;
                    var node2 = nodes[index2];
                    nodes.RemoveAt(index2);
                    var count2 = counts[node2];
                    counts.Remove(node2);

                    var node = new HuffmanNode<T>(node1, node2);
                    var count = count1 + count2;
                    var insertIndex = 0;

                    for (insertIndex = nodes.Count; insertIndex > 0; insertIndex--)
                    {
                        if (count <= counts[nodes[insertIndex - 1]])
                        {
                            break;
                        }

                    }

                    if (insertIndex == -1)
                    {
                        nodes.Add(node);
                    }
                    else
                    {
                        nodes.Insert(insertIndex, node);
                    }

                    counts[node] = count;
                }

                return nodes.First();
            }
            else
            {
                return new HuffmanNode<T>(nodes.First(), default);
            }

        }

        public T Simbol { get; }
        public HuffmanNode<T> Left { get; }
        public HuffmanNode<T> Right { get; }
        public bool HasChildren { get; }
        public int Depth => (this.Max(n => n?.Depth) ?? 0) + 1;

        public HuffmanNode(T simbol)
        {
            this.Simbol = simbol;
        }

        public HuffmanNode(HuffmanNode<T> left, HuffmanNode<T> right)
        {
            this.Simbol = default;
            this.Left = left;
            this.Right = right;
            this.HasChildren = true;
        }

        public T[][] ToSimbolTable()
        {
            var simbolTable = new List<T[]>();
            var nextNodes = new List<HuffmanNode<T>>(this);

            while (nextNodes.Count > 0)
            {
                var nodes = new List<HuffmanNode<T>>(nextNodes);
                nextNodes.Clear();

                var simbols = new List<T>();

                foreach (var node in nodes)
                {
                    if (node.HasChildren == true)
                    {
                        nextNodes.AddRange(node);
                    }
                    else
                    {
                        simbols.Add(node.Simbol);
                    }

                }

                simbolTable.Add(simbols.ToArray());
            }

            return simbolTable.ToArray();
        }

        public Dictionary<T, HuffmanCode> ToCodeTable() => this.ToCodeTable(default);

        public Dictionary<T, HuffmanCode> ToCodeTable(HuffmanCode code)
        {
            var map = new Dictionary<T, HuffmanCode>();

            if (this.HasChildren == true)
            {
                var children = Enumerable.ToArray(this);

                for (var i = 0; i < children.Length; i++)
                {
                    foreach (var pair in children[i].ToCodeTable(new HuffmanCode(code, i)))
                    {
                        map[pair.Key] = pair.Value;
                    }

                }

            }
            else
            {
                map[this.Simbol] = code;
            }

            return map;
        }

        public override int GetHashCode() => this.ToString().GetHashCode();

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (this.HasChildren == true)
            {
                builder.Append($"[{string.Join(", ", this)}]");
            }
            else
            {
                builder.Append($"{this.Simbol}");
            }

            return builder.ToString();
        }

        public IEnumerator<HuffmanNode<T>> GetEnumerator()
        {
            if (this.Left != null)
            {
                yield return this.Left;
            }

            if (this.Right != null)
            {
                yield return this.Right;
            }

        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

}
