//
// Bit Array.cs
//
// Authors:
// Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

using System;

namespace System.Collections {
	[Serializable]
	public sealed class BitArray : ICollection, ICloneable {
		int [] _array;
		int _length;
		int _version = 0;

#region Constructors
		public BitArray (BitArray orig)
		{
			if (orig == null)
				throw new ArgumentNullException ("orig");

			_length = orig._length;
			_array = new int [(_length + 31) / 32];

			Array.Copy(orig._array, _array, _array.Length);
		}

		public BitArray (bool [] bits)
		{
			if (bits == null)
				throw new ArgumentNullException ("bits");
	    
			_length = bits.Length;
			_array = new int [(_length + 31) / 32];
			
			for (int i = 0; i < bits.Length; i++)
				this [i] = bits [i];
		}

		public BitArray (byte [] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");

			_length = bytes.Length * 8;
			_array = new int [(_length + 31) / 32];

			for (int i = 0; i < bytes.Length; i++)
				setByte (i, bytes [i]);
		}
		
		public BitArray (int [] words)
		{
			if (words == null)
				throw new ArgumentNullException ("words");
						
			int arrlen = words.Length;
			_length = arrlen*32;
			_array = new int [arrlen];
			Array.Copy (words, _array, arrlen);
		}
		
		public BitArray (int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			
			_length = capacity;
			_array = new int [(_length + 31) / 32];
		}

		public BitArray (int capacity, bool value) : this (capacity)
		{
			if (value) {
				for (int i = 0; i < _array.Length; i++)
				_array[i] = ~0;
			}
		}
		
		private BitArray (int [] array, int length)
		{
			_array = array;
			_length = length;
		}
#endregion
#region Utility Methods
		
		byte getByte (int byteIndex)
		{
			int index = byteIndex / 4;
			int shift = (byteIndex % 4) * 8;
			
			int theByte = _array [index] & (0xff << shift);
			
			return (byte)((theByte >> shift) & 0xff);
		}
		
		void setByte (int byteIndex, byte value)
		{
			int index = byteIndex / 4;
			int shift = (byteIndex % 4) * 8;
			
			// clear the byte
			_array [index] &= ~(0xff << shift);
			// or in the new byte
			_array [index] |= value << shift;
			
			_version++;
		}
		
		void checkOperand (BitArray operand)
		{
			if (operand == null)
				throw new ArgumentNullException ();
			if (operand._length != _length)
				throw new ArgumentException ();
		}
#endregion

		public int Count {
			get { return _length; }
		}
		
		public bool IsReadOnly {
			get { return false; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public bool this [int index] {
			get { return Get (index); }
			set { Set (index, value); }			
		}
		
		public int Length {
			get { return _length; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ();
				
				int newLen = value;
				if (_length != newLen) {
					int numints = (newLen + 31) / 32;
					int [] newArr = new int [numints];
					int copylen = (numints > _array.Length) ? _array.Length : numints;
					Array.Copy (_array, newArr, copylen);
					
					// set the internal state
					_array = newArr;
					_length = newLen;
					_version++;
				}
			}
		}
		
		public object SyncRoot {
			get { return this; }
		}

		public object Clone ()
		{
			// LAMESPEC: docs say shallow, MS makes deep.
			return new BitArray (this);
		}
		
		public void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			
			if (array.Rank != 1)
				throw new ArgumentException ("array", "Array rank must be 1");
			
			if (index >= array.Length)
				throw new ArgumentException ("index", "index is greater than array.Length");
			
			// in each case, check to make sure enough space in array
			
			if (array is bool []) {
				if (array.Length - index < _length)
					 throw new ArgumentException ();
				
				bool [] barray = (bool []) array;
				
				// Copy the bits into the array
				for (int i = 0; i < _length; i++)
					barray[index + i] = this [i];
				
			} else if (array is byte []) {
				int numbytes = (_length + 7) / 8;
				
				if ((array.Length - index) < numbytes)
					 throw new ArgumentException ();
				
				byte [] barray = (byte []) array;
				// Copy the bytes into the array
				for (int i = 0; i < numbytes; i++)
					barray [index + i] = getByte (i);
				
			} else if (array is int []) {
				
				Array.Copy (_array, 0, array, index, (_length + 31) / 32);
				
			} else {
				throw new ArgumentException ("array", "Unsupported type");
			}
		}

		public BitArray Not ()
		{
			int ints = (_length + 31) / 32;
			for (int i = 0; i < ints; i++)
				_array [i] = ~_array [i];
			
			_version++;
			return this;
		}
		
		public BitArray And (BitArray operand)
		{
			checkOperand (operand);
			
			int ints = (_length + 31) / 32;
			for (int i = 0; i < ints; i++)
				_array [i] &= operand._array [i];
			
			_version++;
			return this;
		}
		
		public BitArray Or (BitArray operand)
		{
			checkOperand (operand);

			int ints = (_length + 31) / 32;
			for (int i = 0; i < ints; i++)
				_array [i] |= operand._array [i];
			
			_version++;
			return this;
		}

		public BitArray Xor (BitArray operand)
		{
			checkOperand (operand);

			int ints = (_length + 31) / 32;
			for (int i = 0; i < ints; i++)
				_array [i] ^= operand._array [i];

			_version++;
			return this;
		}
		
		public bool Get (int index)
		{
			if (index < 0 || index >= _length)
				throw new ArgumentOutOfRangeException ();
			
			return (_array [index / 32] & (1 << (index % 32))) != 0;
		}
		
		public void Set (int index, bool value)
		{
			if (index < 0 || index >= _length)
				throw new ArgumentOutOfRangeException ();
			
			if (value)
				_array [index / 32] |=  (1 << (index % 32));
			else
				_array [index / 32] &= ~(1 << (index % 32));
		
			_version++;
		}
		
		public void SetAll (bool value)
		{
			if (value) {
				for (int i = 0; i < _array.Length; i++)
					_array[i] = ~0;
			}
			else
				Array.Clear (_array, 0, _array.Length);

			_version++;
		}

		public IEnumerator GetEnumerator ()
		{
			return new BitArrayEnumerator (this);
		}

		[Serializable]
		class BitArrayEnumerator : IEnumerator, ICloneable {
			BitArray _bitArray;
			bool _current;
			int _index, _max, _version;
			
			public object Clone () {
				return MemberwiseClone ();
			}
			    
			public BitArrayEnumerator (BitArray ba)
			{
				_index = -1;
				_bitArray = ba;
				_max = ba._length;
				_version = ba._version;
			}

			public object Current {
				get {
					if (_index == -1)
						throw new InvalidOperationException ("Enum not started");
					if (_index >= _bitArray.Count)
						throw new InvalidOperationException ("Enum Ended");
					
					return _current;
				}
			}

			public bool MoveNext ()
			{
				checkVersion ();

				if (_index < (_bitArray.Count - 1)) {
					_current = _bitArray [++_index];
					return true;
				}
				else
					_index = _bitArray.Count;
				
				return false;
			}

			public void Reset ()
			{
				checkVersion ();
				_index = -1;
			}
			
			void checkVersion ()
			{
				if (_version != _bitArray._version)
					throw new InvalidOperationException ();
			}
		}
	}
}