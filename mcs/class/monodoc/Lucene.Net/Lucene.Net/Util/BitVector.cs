/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using Directory = Monodoc.Lucene.Net.Store.Directory;
using InputStream = Monodoc.Lucene.Net.Store.InputStream;
using OutputStream = Monodoc.Lucene.Net.Store.OutputStream;
namespace Monodoc.Lucene.Net.Util
{
	
	/// <summary>Optimized implementation of a vector of bits.  This is more-or-less like
	/// java.util.BitSet, but also includes the following:
	/// <ul>
	/// <li>a count() method, which efficiently computes the number of one bits;</li>
	/// <li>optimized read from and write to disk;</li>
	/// <li>inlinable get() method;</li>
	/// </ul>
	/// </summary>
	/// <author>  Doug Cutting
	/// </author>
	/// <version>  $Id: BitVector.java,v 1.4 2004/03/29 22:48:05 cutting Exp $
	/// </version>
	public sealed class BitVector
	{
		
		private byte[] bits;
		private int size;
		private int count = - 1;
		
		/// <summary>Constructs a vector capable of holding <code>n</code> bits. </summary>
		public BitVector(int n)
		{
			size = n;
			bits = new byte[(size >> 3) + 1];
		}
		
		/// <summary>Sets the value of <code>bit</code> to one. </summary>
		public void  Set(int bit)
		{
			bits[bit >> 3] |= (byte) (1 << (bit & 7));
			count = - 1;
		}
		
		/// <summary>Sets the value of <code>bit</code> to zero. </summary>
		public void  Clear(int bit)
		{
			bits[bit >> 3] &= (byte) ~ (1 << (bit & 7));
			count = - 1;
		}
		
		/// <summary>Returns <code>true</code> if <code>bit</code> is one and
		/// <code>false</code> if it is zero. 
		/// </summary>
		public bool Get(int bit)
		{
			return (bits[bit >> 3] & (1 << (bit & 7))) != 0;
		}
		
		/// <summary>Returns the number of bits in this vector.  This is also one greater than
		/// the number of the largest valid bit number. 
		/// </summary>
		public int Size()
		{
			return size;
		}
		
		/// <summary>Returns the total number of one bits in this vector.  This is efficiently
		/// computed and cached, so that, if the vector is not changed, no
		/// recomputation is done for repeated calls. 
		/// </summary>
		public int Count()
		{
			// if the vector has been modified
			if (count == - 1)
			{
				int c = 0;
				int end = bits.Length;
				for (int i = 0; i < end; i++)
					c += BYTE_COUNTS[bits[i] & 0xFF]; // sum bits per byte
				count = c;
			}
			return count;
		}
		
		private static readonly byte[] BYTE_COUNTS = new byte[]{0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8};
		
		
		/// <summary>Writes this vector to the file <code>name</code> in Directory
		/// <code>d</code>, in a format that can be read by the constructor {@link
		/// #BitVector(Directory, String)}.  
		/// </summary>
		public void  Write(Directory d, System.String name)
		{
			OutputStream output = d.CreateFile(name);
			try
			{
				output.WriteInt(Size()); // write size
				output.WriteInt(Count()); // write count
				output.WriteBytes(bits, bits.Length); // write bits
			}
			finally
			{
				output.Close();
			}
		}
		
		/// <summary>Constructs a bit vector from the file <code>name</code> in Directory
		/// <code>d</code>, as written by the {@link #write} method.
		/// </summary>
		public BitVector(Directory d, System.String name)
		{
			InputStream input = d.OpenFile(name);
			try
			{
				size = input.ReadInt(); // read size
				count = input.ReadInt(); // read count
				bits = new byte[(size >> 3) + 1]; // allocate bits
				input.ReadBytes(bits, 0, bits.Length); // read bits
			}
			finally
			{
				input.Close();
			}
		}
	}
}