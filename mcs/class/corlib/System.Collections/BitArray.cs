using System;

namespace System.Collections
{
  // do we really need to specify IEnumerable since ICollection extends it?
  [Serializable]
  public sealed class BitArray : ICollection, IEnumerable, ICloneable
  {
    private Int32[] m_array;
    private int m_length;
    private int m_modCount = 0;
    
    private static void clearJunk(Int32[] arr, int numbits)
    {
      int numjunkbits = 32 - (numbits%32);
      UInt32 mask = (~0U >> numjunkbits);
      arr[arr.Length - 1] &= (int)mask;
    }

    private static int bitsToInts(int bits)
    {
      int retval = bits/32;
      if (bits % 32 != 0)
	retval++;

      return retval;
    }

    private static int bitsToBytes(int bits)
    {
      int retval = bits/8;
      if (bits % 8 != 0)
	retval++;

      return retval;
    }


    private void setBit(int bitIndex, bool value)
    {
      int index = bitIndex/32;
      int shift = bitIndex%32;

      Int32 theBit = 1 << shift;

      if(value)
	m_array[index] |= theBit;
      else
	m_array[index] &= ~theBit;
      
      m_modCount++;
    }

    private bool getBit(int bitIndex)
    {
      int index = bitIndex/32;
      int shift = bitIndex%32;

      Int32 theBit = m_array[index] & (1 << shift);

      return (theBit == 0) ? false : true;
    }

    private byte getByte(int byteIndex)
    {
      int index = byteIndex/4;
      int shift = (byteIndex%4)*8;

      Int32 theByte = m_array[index] & (0xff << shift);

      return (byte)((theByte >> shift)&0xff);
    }

    private void setByte(int byteIndex, byte value)
    {
      int index = byteIndex/4;
      int shift = (byteIndex%4)*8;

      Int32 orig = m_array[index];

      // clear the byte
      orig &= ~(0xff << shift);
      // or in the new byte
      orig |= value << shift;
      
      m_array[index] = orig;

      m_modCount++;
    }

    /* --- Constructors --- */
    public BitArray(BitArray orig)
    {
      m_length = orig.m_length;
      
      int numInts = bitsToInts(m_length);
      m_array = new Int32[numInts];
      Array.Copy(orig.m_array, m_array, numInts);
    }

    public BitArray(bool[] bits)
    {
      m_length = bits.Length;

      int numInts = bitsToInts(m_length);
      m_array = new Int32[numInts];
      for (int i=0; i < bits.Length; i++)
	setBit(i, bits[i]);
    }

    public BitArray(byte[] bytes)
    {
      m_length = bytes.Length * 8;

      m_array = new Int32[bitsToInts(m_length)];
      for (int i=0; i < bytes.Length; i++)
	setByte(i, bytes[i]);
    }

    public BitArray(int capacity)
    {
      m_length = capacity;
      m_array = new Int32[bitsToInts(m_length)];
    }
    
    public BitArray(int[] words)
    {
      int arrlen = words.Length;
      m_length = arrlen*32;
      m_array = new Int32[arrlen];
      Array.Copy(words, m_array, arrlen);
    }
    
    public BitArray(int capacity, bool value) : this(capacity)
    {
      if (value)
      {
	// FIXME:  Maybe you can create an array pre filled?
	for (int i = 0; i < m_array.Length; i++)
	  m_array[i] = ~0;
      }
    }

    private BitArray(Int32 [] array, int length)
    {
      m_array = array;
      m_length = length;
    }
    

    /* --- Public properties --- */
    public int Count
    {
      get
      {
	return m_length;
      }
    }

    public bool IsReadOnly
    {
      get
      {
	return false;
      }
    }

    public bool IsSynchronized
    {
      get
      {
	return false;
      }
    }

    public bool this[int index]
    {
      get
      {
        return Get(index);
      }
      set
      {
        Set(index, value);
      }

    }

    public int Length
    {
      get
      {
	return m_length;
      }
      set
      {
	if (value < 0)
	  throw new ArgumentOutOfRangeException();

	int newLen = value;
	if (m_length != newLen)
	{
	  int numints = bitsToInts(newLen);
	  Int32 [] newArr = new Int32[numints];
	  int copylen = (numints > m_array.Length ? m_array.Length : numints);
	  Array.Copy(m_array, newArr, copylen);
	  
	  // clear out the junk bits at the end:
	  clearJunk(newArr, newLen);

	  // set the internal state
	  m_array = newArr;
	  m_length = newLen;
	  m_modCount++;
	}
      }
    }

    public object SyncRoot
    {
      get
      {
	return this;
      }
    }

    /* --- Public methods --- */
    public BitArray And(BitArray operand)
    {
      if (operand == null)
        throw new ArgumentNullException();
      if (operand.m_length != m_length)
        throw new ArgumentException();
      
      Int32 [] newarr = new Int32[m_array.Length];
      for (int i=0; i < m_array.Length; i++)
        newarr[i] = m_array[i] & operand.m_array[i];

      return new BitArray(newarr, m_length);
    }

    public object Clone()
    {
      // FIXME: according to the doc, this should be a shallow copy.
      // But the MS implementation seems to do a deep copy.
      return new BitArray((Int32 [])m_array.Clone(), m_length);
    }

    public void CopyTo(Array array, int index)
    {
      if (array == null)
	throw new ArgumentNullException("array");
      if (index < 0)
	throw new ArgumentOutOfRangeException("index");
      
      if (array.Rank != 1)
	      throw new ArgumentException ("Array rank must be 1", "array");

      if (index >= array.Length)
	throw new ArgumentException("index is greater than array.Length", "index");

      // in each case, check to make sure enough space in array

      if (array is bool[])
      {
	if (index + m_length >= array.Length)
	  throw new ArgumentException();

	bool [] barray = (bool []) array;

	// Copy the bits into the array
	for (int i = 0; i < m_length; i++)
	  barray[index + i] = getBit(i);
      }
      else if (array is byte[])
      {
	int numbytes = bitsToBytes(m_length);
	if (index + numbytes >= array.Length)
	  throw new ArgumentException();

	byte [] barray = (byte []) array;
	// Copy the bytes into the array
	for (int i = 0; i < numbytes; i++)
	  barray[index + i] = getByte(i);
      }
      else if (array is int[])
      {
	int numints = bitsToInts(m_length);
	if (index + numints >= array.Length)
	  throw new ArgumentException();
	Array.Copy(m_array, 0, array, index, numints);
      }
      else
      {
	throw new ArgumentException("Unsupported type", "array");
      }
    }

      
    /*
     * All this code for nothing... Apparently, The MS BitArray doesn't
     * override Equals!
     *public override bool Equals(object obj)
    {
      // If it's not a BitArray, then it can't be equal to us.
      if (!(obj is BitArray))
	return false;

      // If the references are equal, then clearly the instances are equal
      if (this == obj)
	return true;

      // If its length is different, than it can't be equal.
      BitArray b = (BitArray) obj;
      if (m_length != b.m_length)
	return false;


      // Now compare the bits.
      // This is a little tricky, because if length doesn't divide 32,
      // then we shouldn't compare the unused bits in the last element 
      // of m_array.

      // Compare all full ints.  If any differ, then we are not equal.
      int numints = m_length/32;
      for (int i = 0; i < numints; i++)
      {
	if (b.m_array[i] != m_array[i])
	  return false;
      }
      
      // Compare the left over bits (if any)
      int extrabits = m_length%32;
      if (extrabits != 0)
      {
	// our mask is the "extrabits" least significant bits set to 1.
	UInt32 comparemask = ~0U >> (32 - extrabits);

	// numints is rounded down, so it's safe to use as an index here,
	// as long as extrabits > 0.
	if ((b.m_array[numints] & comparemask) 
	    != (m_array[numints] & comparemask))
	  return false;
      }
      
      // We passed through all the above, so we are equal.
      return true;

    }
    *  End comment out of Equals()
    */

    public bool Get(int index)
    {
      if (index < 0 || index >= m_length)
        throw new ArgumentOutOfRangeException();
      return getBit(index);
    }

    public IEnumerator GetEnumerator()
    {      
      return new BitArrayEnumerator(this);
    }

    /*
     *  Since MS doesn't appear to override Equals/GetHashCode, we don't.
     *public override int GetHashCode()
    {
      // We could make this a constant time function 
      // by just picking a constant number of bits, spread out
      // evenly across the entire array.  For now, this will suffice.

      int retval = m_length;

      // Add in each array element, except for the leftover bits.
      int numints = m_length/32;
      for (int i = 0; i < numints; i++)
	retval += (int)m_array[i];

      // That's enough.  Adding in the leftover bits is tiring.

      return retval;
    }
    * End comment out of GetHashCode()
    */

    public BitArray Not()
    {
      Int32 [] newarr = new Int32[m_array.Length];
      for (int i=0; i < m_array.Length; i++)
        newarr[i] = ~m_array[i];

      return new BitArray(newarr, m_length);
    }

    public BitArray Or(BitArray operand)
    {
      if (operand == null)
        throw new ArgumentNullException();
      if (operand.m_length != m_length)
        throw new ArgumentException();
      
      Int32 [] newarr = new Int32[m_array.Length];
      for (int i=0; i < m_array.Length; i++)
        newarr[i] = m_array[i] | operand.m_array[i];

      return new BitArray(newarr, m_length);
    }

    public void Set(int index, bool value)
    {
      if (index < 0 || index >= m_length)
        throw new ArgumentOutOfRangeException();
      setBit(index, value);
    }

    public void SetAll(bool value)
    {
      if (value)
      {
	for (int i = 0; i < m_array.Length; i++)
	  m_array[i] = ~0;
	
	// clear out the junk bits that we might have set
	clearJunk(m_array, m_length);
      }
      else
	Array.Clear(m_array, 0, m_array.Length);


      m_modCount++;
    }
    
    public BitArray Xor(BitArray operand)
    {
      if (operand == null)
        throw new ArgumentNullException();
      if (operand.m_length != m_length)
        throw new ArgumentException();
      
      Int32 [] newarr = new Int32[m_array.Length];
      for (int i=0; i < m_array.Length; i++)
        newarr[i] = m_array[i] ^ operand.m_array[i];

      return new BitArray(newarr, m_length);
    }

    class BitArrayEnumerator : IEnumerator
    {
      BitArray m_bitArray;
      private bool m_current;
      private int m_index;
      private int m_max;
      private int m_modCount;
     
      public BitArrayEnumerator(BitArray ba)
      {
	m_index = -1;
	m_bitArray = ba;
	m_max = ba.m_length;
	m_modCount = ba.m_modCount;
      }
      
      public object Current
      {
	get
	{
	  if (m_index < 0 || m_index >= m_max)
	    throw new InvalidOperationException();
	  return m_current;
	}
      }
      
      public bool MoveNext()
      {
	if (m_modCount != m_bitArray.m_modCount)
	  throw new InvalidOperationException();
	
	if (m_index + 1 >= m_max)
	  return false;

	m_index++;
	m_current = m_bitArray[m_index];
	return true;
      }
      
      public void Reset()
      {
	if (m_modCount != m_bitArray.m_modCount)
	  throw new InvalidOperationException();
	m_index = -1;
      }
    }
  }
}





