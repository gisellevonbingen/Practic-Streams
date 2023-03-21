using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.Collections
{
    public class BidirectionalDictionary<A, B> : ICollection<Tuple<A, B>>
    {
        private readonly Dictionary<A, B> A2B;
        private readonly Dictionary<B, A> B2A;

        public IEqualityComparer<A> AComparer { get; }
        public IEqualityComparer<B> BComparer { get; }

        public BidirectionalDictionary() : this(null, null)
        {

        }

        public BidirectionalDictionary(IEqualityComparer<A> aComparer, IEqualityComparer<B> bComparer)
        {
            this.AComparer = aComparer ?? EqualityComparer<A>.Default;
            this.BComparer = bComparer ?? EqualityComparer<B>.Default;

            this.A2B = new Dictionary<A, B>(this.AComparer);
            this.B2A = new Dictionary<B, A>(this.BComparer);
        }

        public ReadOnlyDictionary<A, B> AtoB => new(this.A2B);

        public ReadOnlyDictionary<B, A> BtoA => new(this.B2A);

        public int Count => this.A2B.Count;

        public bool IsReadOnly => false;

        public void Add(Tuple<A, B> item) => this.Add(item.Item1, item.Item2);

        public void Add(A a, B b)
        {
            if (this.A2B.ContainsKey(a) == true)
            {
                throw new ArgumentException("A already added");
            }
            else if (this.B2A.ContainsKey(b) == true)
            {
                throw new ArgumentException("B already added");
            }
            else
            {
                this.A2B.Add(a, b);
                this.B2A.Add(b, a);
            }

        }

        public bool Contains(A a, B b) => this.TryGetB(a, out var rb) && this.BComparer.Equals(b, rb) && this.TryGetA(b, out var ra) && this.AComparer.Equals(a, ra);

        public bool Contains(Tuple<A, B> item) => this.Contains(item.Item1, item.Item2);

        public bool ContainsA(A a) => this.A2B.ContainsKey(a);

        public bool ContainsB(B b) => this.B2A.ContainsKey(b);

        public B this[A a]
        {
            get => this.GetB(a);
        }

        public A this[B b]
        {
            get => this.GetA(b);
        }

        public B GetB(A a) => this.A2B[a];

        public A GetA(B b) => this.B2A[b];

        public bool TryGetB(A a, out B b) => this.A2B.TryGetValue(a, out b);

        public bool TryGetA(B b, out A a) => this.B2A.TryGetValue(b, out a);

        public bool RemoveByA(A a)
        {
            if (this.TryGetB(a, out var b) == true)
            {
                this.A2B.Remove(a);
                this.B2A.Remove(b);
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool RemoveByB(B b)
        {
            if (this.TryGetA(b, out var a) == true)
            {
                this.A2B.Remove(a);
                this.B2A.Remove(b);
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool Remove(Tuple<A, B> item) => this.Remove(item.Item1, item.Item2);

        public bool Remove(A a, B b)
        {
            if (this.TryGetB(a, out var rb) == true && this.BComparer.Equals(b, rb) == true)
            {
                if (this.TryGetA(rb, out var ra) == true && this.AComparer.Equals(a, ra) == true)
                {
                    this.A2B.Remove(a);
                    this.B2A.Remove(b);
                    return true;
                }

            }

            return false;
        }

        public IEnumerator<Tuple<A, B>> GetEnumerator()
        {
            foreach (var pair in this.A2B)
            {
                yield return Tuple.Create(pair.Key, pair.Value);
            }

        }

        IEnumerator IEnumerable.GetEnumerator() => this.A2B.GetEnumerator();

        public void Clear()
        {
            this.A2B.Clear();
            this.B2A.Clear();
        }

        public void CopyTo(Tuple<A, B>[] destinationArray, int destinationIndex)
        {
            var sourceArray = this.ToArray();
            Array.Copy(sourceArray, 0, destinationArray, destinationIndex, sourceArray.Length);
        }

    }

}
