using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public class ArrayMirror : ObjectMirror, IEnumerable {

		public int[] lengths;
		public int[] lower_bounds;
		public int rank;

		internal ArrayMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		internal ArrayMirror (VirtualMachine vm, long id, TypeMirror type, AppDomainMirror domain) : base (vm, id, type, domain) {
		}

		public int Length {
			get {
				GetLengths ();

				int length = lengths [0];

				for (int i = 1; i < Rank; i++) {
					length *= lengths [i];
				}

				return length;
			}
		}

		public int Rank {
			get {
				GetLengths ();

				return rank;
			}
		}

		public int GetLength (int dimension) {
			GetLengths ();

			if (dimension < 0 || dimension >= Rank)
				throw new ArgumentOutOfRangeException ("dimension");

			return lengths [dimension];
		}

		public int GetLowerBound (int dimension) {
			GetLengths ();

			if (dimension < 0 || dimension >= Rank)
				throw new ArgumentOutOfRangeException ("dimension");

			return lower_bounds [dimension];
		}

		void GetLengths () {
			if (lengths == null)
				lengths = vm.conn.Array_GetLength (id, out this.rank, out this.lower_bounds);
		}

		public Value this [int index] {
			get {
				// FIXME: Multiple dimensions
				if (index < 0 || index > Length - 1)
					throw new IndexOutOfRangeException ();
				return vm.DecodeValue (vm.conn.Array_GetValues (id, index, 1) [0]);
			}
			set {
				// FIXME: Multiple dimensions
				if (index < 0 || index > Length - 1)
					throw new IndexOutOfRangeException ();
				vm.conn.Array_SetValues (id, index, new ValueImpl [] { vm.EncodeValue (value) });
			}
		}

		public IList<Value> GetValues (int index, int length) {
			// FIXME: Multiple dimensions
				if (index < 0 || index > Length - length)
					throw new IndexOutOfRangeException ();
			return vm.DecodeValues (vm.conn.Array_GetValues (id, index, length));
		}

		public void SetValues (int index, Value[] values) {
			if (values == null)
				throw new ArgumentNullException ("values");
			// FIXME: Multiple dimensions
			if (index < 0 || index > Length - values.Length)
				throw new IndexOutOfRangeException ();
			vm.conn.Array_SetValues (id, index, vm.EncodeValues (values));
		}

		public void SetByteValues (byte [] bytes)
		{
			if (bytes != null && bytes.Length != Length) {
				throw new IndexOutOfRangeException ();
			}
			vm.conn.ByteArray_SetValues (id, bytes);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new SimpleEnumerator (this);
		}

		internal class SimpleEnumerator : IEnumerator, ICloneable
		{
			ArrayMirror arr;
			int pos, length;

			public SimpleEnumerator (ArrayMirror arr)
			{
				this.arr = arr;
				this.pos = -1;
				this.length = arr.Length;
			}

			public object Current {
				get {
					if (pos < 0 )
						throw new InvalidOperationException ("Enumeration has not started.");
					if  (pos >= length)
						throw new InvalidOperationException ("Enumeration has already ended");
					return arr [pos];
				}
			}

			public bool MoveNext()
			{
				if (pos < length)
					pos++;
				if(pos < length)
					return true;
				else
					return false;
			}

			public void Reset()
			{
				pos = -1;
			}

			public object Clone ()
			{
				return MemberwiseClone ();
			}
		}
	}
}
