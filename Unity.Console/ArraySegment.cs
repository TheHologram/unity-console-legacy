using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Unity.Console
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct ArraySegment : IList, ICollection, IEnumerable
    {
        public ArraySegment(Array array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
            }
            if (offset > array.Length)
            {
                throw new ArgumentException("out of bounds");
            }
            if (array.Length - offset < count)
            {
                throw new ArgumentException("out of bounds", "offset");
            }
            Array = array;
            Offset = offset;
            Count = count;
        }

        public ArraySegment(Array array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Array = array;
            Offset = 0;
            Count = array.Length;
        }

        public Array Array { get; }

        public int Offset { get; }

        public void CopyTo(Array array, int index)
        {
            Array.CopyTo(array, index+Offset);
        }

        public int Count { get; }
        public object SyncRoot
        {
            get { return this.Array.SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return this.Array.IsSynchronized; }
        }

        public override bool Equals(object obj)
        {
            return obj is ArraySegment && Equals((ArraySegment) obj);
        }

        public bool Equals(ArraySegment obj)
        {
            return (Array == obj.Array) && (Offset == obj.Offset) && (Count == obj.Count);
        }

        public override int GetHashCode()
        {
            return this.Array.GetHashCode() ^ Offset ^ Count;
        }

        public IEnumerator GetEnumerator()
        {
            return this.Array.GetEnumerator();
        }

        public static bool operator ==(ArraySegment a, ArraySegment b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ArraySegment a, ArraySegment b)
        {
            return !a.Equals(b);
        }

        public int Add(object value)
        {
            return ~0;
        }

        public bool Contains(object value)
        {
            return 0 <= System.Array.IndexOf(this.Array, value, this.Offset, this.Count);           
        }

        public void Clear()
        {
        }

        public int IndexOf(object value)
        {
            int idx = System.Array.IndexOf(this.Array, value, this.Offset, this.Count);
            if (idx >= 0) return idx - Offset;
            return idx;
        }

        public void Insert(int index, object value)
        {
            
        }

        public void Remove(object value)
        {
            
        }

        public void RemoveAt(int index)
        {
            
        }

        public object this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
                return Array.GetValue(index+Offset);
            }
            set
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
                Array.SetValue(value, index+Offset);
            }
        }

        public bool IsReadOnly => true;

        public bool IsFixedSize => true;
    }
}