using System;
using ArrayList					= System.Collections.ArrayList;

//using CharFormatter				= antlr.CharFormatter;

namespace antlr.collections.impl
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: BitSet.cs,v 1.1 2003/04/22 05:01:35 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	/*A BitSet to replace java.util.BitSet.
	* Primary differences are that most set operators return new sets
	* as opposed to oring and anding "in place".  Further, a number of
	* operations were added.  I cannot contain a BitSet because there
	* is no way to access the internal bits (which I need for speed)
	* and, because it is final, I cannot subclass to add functionality.
	* Consider defining set degree.  Without access to the bits, I must
	* call a method n times to test the ith bit...ack!
	*
	* Also seems like or() from util is wrong when size of incoming set is bigger
	* than this.bits.length.
	*
	* @author Terence Parr
	* @author <br><a href="mailto:pete@yamuna.demon.co.uk">Pete Wells</a>
	*/

	public class BitSet : ICloneable
	{
		protected internal const int BITS = 64; // number of bits / long
		protected internal const int NIBBLE = 4;
		protected internal const int LOG_BITS = 6; // 2^6 == 64
		
		/*We will often need to do a mod operator (i mod nbits).  Its
		* turns out that, for powers of two, this mod operation is
		* same as (i & (nbits-1)).  Since mod is slow, we use a
		* precomputed mod mask to do the mod instead.
		*/
		protected internal static readonly int MOD_MASK = BITS - 1;
		
		/*The actual data bits */
		protected internal long[] dataBits;
		
		/*Construct a bitset of size one word (64 bits) */
		public BitSet() : this(BITS)
		{
		}
		
		/*Construction from a static array of longs */
		public BitSet(long[] bits_)
		{
			dataBits = bits_;
		}
		
		/*Construct a bitset given the size
		* @param nbits The size of the bitset in bits
		*/
		public BitSet(int nbits)
		{
			dataBits = new long[((nbits - 1) >> LOG_BITS) + 1];
		}
		
		/*or this element into this set (grow as necessary to accommodate) */
		public virtual void  add(int el)
		{
			int n = wordNumber(el);
			if (n >= dataBits.Length)
			{
				growToInclude(el);
			}
			dataBits[n] |= bitMask(el);
		}
		
		public virtual BitSet and(BitSet a)
		{
			BitSet s = (BitSet) this.Clone();
			s.andInPlace(a);
			return s;
		}
		
		public virtual void  andInPlace(BitSet a)
		{
			int min = (int) (Math.Min(dataBits.Length, a.dataBits.Length));
			 for (int i = min - 1; i >= 0; i--)
			{
				dataBits[i] &= a.dataBits[i];
			}
			// clear all bits in this not present in a (if this bigger than a).
			 for (int i = min; i < dataBits.Length; i++)
			{
				dataBits[i] = 0;
			}
		}
		
		private static long bitMask(int bitNumber)
		{
			int bitPosition = bitNumber & MOD_MASK; // bitNumber mod BITS
			return 1L << bitPosition;
		}
		
		public virtual void  clear()
		{
			 for (int i = dataBits.Length - 1; i >= 0; i--)
			{
				dataBits[i] = 0;
			}
		}
		
		public virtual void  clear(int el)
		{
			int n = wordNumber(el);
			if (n >= dataBits.Length)
			{
				// grow as necessary to accommodate
				growToInclude(el);
			}
			dataBits[n] &= ~ bitMask(el);
		}
		
		public virtual object Clone()
		{
			BitSet s;
			try
			{
				s = new BitSet();
				s.dataBits = new long[dataBits.Length];
				Array.Copy(dataBits, 0, s.dataBits, 0, dataBits.Length);
			}
			catch //(System.Exception e)
			{
				throw new System.ApplicationException();
			}
			return s;
		}
		
		public virtual int degree()
		{
			int deg = 0;
			 for (int i = dataBits.Length - 1; i >= 0; i--)
			{
				long word = dataBits[i];
				if (word != 0L)
				{
					 for (int bit = BITS - 1; bit >= 0; bit--)
					{
						if ((word & (1L << bit)) != 0)
						{
							deg++;
						}
					}
				}
			}
			return deg;
		}
		
		override public int GetHashCode()
		{
			return dataBits.GetHashCode();
		}

		/*code "inherited" from java.util.BitSet */
		override public bool Equals(object obj)
		{
			if ((obj != null) && (obj is BitSet))
			{
				BitSet bset = (BitSet) obj;
				
				int n = (int) (System.Math.Min(dataBits.Length, bset.dataBits.Length));
				 for (int i = n; i-- > 0; )
				{
					if (dataBits[i] != bset.dataBits[i])
					{
						return false;
					}
				}
				if (dataBits.Length > n)
				{
					 for (int i = (int) (dataBits.Length); i-- > n; )
					{
						if (dataBits[i] != 0)
						{
							return false;
						}
					}
				}
				else if (bset.dataBits.Length > n)
				{
					 for (int i = (int) (bset.dataBits.Length); i-- > n; )
					{
						if (bset.dataBits[i] != 0)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}
		
		/*
		* Grows the set to a larger number of bits.
		* @param bit element that must fit in set
		*/
		public virtual void  growToInclude(int bit)
		{
			int newSize = (int) (System.Math.Max(dataBits.Length << 1, numWordsToHold(bit)));
			long[] newbits = new long[newSize];
			Array.Copy(dataBits, 0, newbits, 0, dataBits.Length);
			dataBits = newbits;
		}
		
		public virtual bool member(int el)
		{
			int n = wordNumber(el);
			if (n >= dataBits.Length)
				return false;
			return (dataBits[n] & bitMask(el)) != 0;
		}
		
		public virtual bool nil()
		{
			 for (int i = dataBits.Length - 1; i >= 0; i--)
			{
				if (dataBits[i] != 0)
					return false;
			}
			return true;
		}
		
		public virtual BitSet not()
		{
			BitSet s = (BitSet) this.Clone();
			s.notInPlace();
			return s;
		}
		
		public virtual void  notInPlace()
		{
			 for (int i = dataBits.Length - 1; i >= 0; i--)
			{
				dataBits[i] = ~ dataBits[i];
			}
		}
		
		/*complement bits in the range 0..maxBit. */
		public virtual void  notInPlace(int maxBit)
		{
			notInPlace(0, maxBit);
		}
		
		/*complement bits in the range minBit..maxBit.*/
		public virtual void  notInPlace(int minBit, int maxBit)
		{
			// make sure that we have room for maxBit
			growToInclude(maxBit);
			 for (int i = minBit; i <= maxBit; i++)
			{
				int n = wordNumber(i);
				dataBits[n] ^= bitMask(i);
			}
		}
		
		private int numWordsToHold(int el)
		{
			return (el >> LOG_BITS) + 1;
		}
		
		public static BitSet of(int el)
		{
			BitSet s = new BitSet(el + 1);
			s.add(el);
			return s;
		}
		
		/*return this | a in a new set */
		public virtual BitSet or(BitSet a)
		{
			BitSet s = (BitSet) this.Clone();
			s.orInPlace(a);
			return s;
		}
		
		public virtual void  orInPlace(BitSet a)
		{
			// If this is smaller than a, grow this first
			if (a.dataBits.Length > dataBits.Length)
			{
				setSize((int) (a.dataBits.Length));
			}
			int min = (int) (System.Math.Min(dataBits.Length, a.dataBits.Length));
			 for (int i = min - 1; i >= 0; i--)
			{
				dataBits[i] |= a.dataBits[i];
			}
		}
		
		// remove this element from this set
		public virtual void  remove(int el)
		{
			int n = wordNumber(el);
			if (n >= dataBits.Length)
			{
				growToInclude(el);
			}
			dataBits[n] &= ~ bitMask(el);
		}
		
		/*
		* Sets the size of a set.
		* @param nwords how many words the new set should be
		*/
		private void  setSize(int nwords)
		{
			long[] newbits = new long[nwords];
			int n = (int) (System.Math.Min(nwords, dataBits.Length));
			Array.Copy(dataBits, 0, newbits, 0, n);
			dataBits = newbits;
		}
		
		public virtual int size()
		{
			return dataBits.Length << LOG_BITS; // num words * bits per word
		}
		
		/*return how much space is being used by the dataBits array not
		*  how many actually have member bits on.
		*/
		public virtual int lengthInLongWords()
		{
			return dataBits.Length;
		}
		
		/*Is this contained within a? */
		public virtual bool subset(BitSet a)
		{
			if (a == null) //(a == null || !(a is BitSet))
				return false;
			return this.and(a).Equals(this);
		}
		
		/*Subtract the elements of 'a' from 'this' in-place.
		* Basically, just turn off all bits of 'this' that are in 'a'.
		*/
		public virtual void  subtractInPlace(BitSet a)
		{
			if (a == null)
				return ;
			// for all words of 'a', turn off corresponding bits of 'this'
			 for (int i = 0; i < dataBits.Length && i < a.dataBits.Length; i++)
			{
				dataBits[i] &= ~ a.dataBits[i];
			}
		}
		
		public virtual int[] toArray()
		{
			int[] elems = new int[degree()];
			int en = 0;
			 for (int i = 0; i < (dataBits.Length << LOG_BITS); i++)
			{
				if (member(i))
				{
					elems[en++] = i;
				}
			}
			return elems;
		}
		
		public virtual long[] toPackedArray()
		{
			return dataBits;
		}
		
		override public string ToString()
		{
			return ToString(",");
		}
		
		/*Transform a bit set into a string by formatting each element as an integer
		* @separator The string to put in between elements
		* @return A commma-separated list of values
		*/
		public virtual string ToString(string separator)
		{
			string str = "";
			 for (int i = 0; i < (dataBits.Length << LOG_BITS); i++)
			{
				if (member(i))
				{
					if (str.Length > 0)
					{
						str += separator;
					}
					str = str + i;
				}
			}
			return str;
		}
		
		/*Create a string representation where instead of integer elements, the
		* ith element of vocabulary is displayed instead.  Vocabulary is a Vector
		* of Strings.
		* @separator The string to put in between elements
		* @return A commma-separated list of character constants.
		*/
		public virtual string ToString(string separator, ArrayList vocabulary)
		{
			if (vocabulary == null)
			{
				return ToString(separator);
			}
			string str = "";
			 for (int i = 0; i < (dataBits.Length << LOG_BITS); i++)
			{
				if (member(i))
				{
					if (str.Length > 0)
					{
						str += separator;
					}
					if (i >= vocabulary.Count)
					{
						str += "<bad element " + i + ">";
					}
					else if (vocabulary[i] == null)
					{
						str += "<" + i + ">";
					}
					else
					{
						str += (string) vocabulary[i];
					}
				}
			}
			return str;
		}
		
		/*
		* Dump a comma-separated list of the words making up the bit set.
		* Split each 64 bit number into two more manageable 32 bit numbers.
		* This generates a comma-separated list of C++-like unsigned long constants.
		*/
		public virtual string toStringOfHalfWords()
		{
			string s = new string("".ToCharArray());
			 for (int i = 0; i < dataBits.Length; i++)
			{
				if (i != 0)
					s += ", ";
				long tmp = dataBits[i];
				tmp &= 0xFFFFFFFFL;
				s += (tmp + "UL");
				s += ", ";
				tmp = SupportClass.URShift(dataBits[i], 32);
				tmp &= 0xFFFFFFFFL;
				s += (tmp + "UL");
			}
			return s;
		}
		
		/*
		* Dump a comma-separated list of the words making up the bit set.
		* This generates a comma-separated list of Java-like long int constants.
		*/
		public virtual string toStringOfWords()
		{
			string s = new string("".ToCharArray());
			 for (int i = 0; i < dataBits.Length; i++)
			{
				if (i != 0)
					s += ", ";
				s += (dataBits[i] + "L");
			}
			return s;
		}
		
		/*Print out the bit set but collapse char ranges. */
/*		public virtual string toStringWithRanges(string separator, CharFormatter formatter)
		{
			string str = "";
			int[] elems = this.toArray();
			if (elems.Length == 0)
			{
				return "";
			}
			// look for ranges
			int i = 0;
			while (i < elems.Length)
			{
				int lastInRange;
				lastInRange = 0;
				 for (int j = i + 1; j < elems.Length; j++)
				{
					if (elems[j] != elems[j - 1] + 1)
					{
						break;
					}
					lastInRange = j;
				}
				// found a range
				if (str.Length > 0)
				{
					str += separator;
				}
				if (lastInRange - i >= 2)
				{
					str += formatter.literalChar(elems[i]);
					str += "..";
					str += formatter.literalChar(elems[lastInRange]);
					i = lastInRange; // skip past end of range for next range
				}
				else
				{
					// no range, just print current char and move on
					str += formatter.literalChar(elems[i]);
				}
				i++;
			}
			return str;
		}
*/		
		private static int wordNumber(int bit)
		{
			return bit >> LOG_BITS; // bit / BITS
		}
	}
}