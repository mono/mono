/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
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

using DocIdSet = Mono.Lucene.Net.Search.DocIdSet;
using DocIdSetIterator = Mono.Lucene.Net.Search.DocIdSetIterator;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> Stores and iterate on sorted integers in compressed form in RAM. <br/>
	/// The code for compressing the differences between ascending integers was
	/// borrowed from {@link Mono.Lucene.Net.Store.IndexInput} and
	/// {@link Mono.Lucene.Net.Store.IndexOutput}.
	/// <p/>
	/// <b>NOTE:</b> this class assumes the stored integers are doc Ids (hence why it
	/// extends {@link DocIdSet}). Therefore its {@link #Iterator()} assumes {@link
	/// DocIdSetIterator#NO_MORE_DOCS} can be used as sentinel. If you intent to use
	/// this value, then make sure it's not used during search flow.
	/// </summary>
	public class SortedVIntList:DocIdSet
	{
		private class AnonymousClassDocIdSetIterator:DocIdSetIterator
		{
			public AnonymousClassDocIdSetIterator(SortedVIntList enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(SortedVIntList enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SortedVIntList enclosingInstance;
			public SortedVIntList Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal int bytePos = 0;
			internal int lastInt = 0;
			internal int doc = - 1;
			
			private void  Advance()
			{
				// See Mono.Lucene.Net.Store.IndexInput.readVInt()
				sbyte b = Enclosing_Instance.bytes[bytePos++];
				lastInt += (b & Mono.Lucene.Net.Util.SortedVIntList.VB1);
				for (int s = Mono.Lucene.Net.Util.SortedVIntList.BIT_SHIFT; (b & ~ Mono.Lucene.Net.Util.SortedVIntList.VB1) != 0; s += Mono.Lucene.Net.Util.SortedVIntList.BIT_SHIFT)
				{
					b = Enclosing_Instance.bytes[bytePos++];
					lastInt += ((b & Mono.Lucene.Net.Util.SortedVIntList.VB1) << s);
				}
			}
			
			/// <deprecated> use {@link #DocID()} instead. 
			/// </deprecated>
            [Obsolete("use DocID() instead.")]
			public override int Doc()
			{
				return lastInt;
			}
			
			public override int DocID()
			{
				return doc;
			}
			
			/// <deprecated> use {@link #NextDoc()} instead. 
			/// </deprecated>
            [Obsolete("use NextDoc() instead.")]
			public override bool Next()
			{
				return NextDoc() != NO_MORE_DOCS;
			}
			
			public override int NextDoc()
			{
				if (bytePos >= Enclosing_Instance.lastBytePos)
				{
					doc = NO_MORE_DOCS;
				}
				else
				{
					Advance();
					doc = lastInt;
				}
				return doc;
			}
			
			/// <deprecated> use {@link #Advance(int)} instead. 
			/// </deprecated>
            [Obsolete("use Advance(int) instead.")]
			public override bool SkipTo(int docNr)
			{
				return Advance(docNr) != NO_MORE_DOCS;
			}
			
			public override int Advance(int target)
			{
				while (bytePos < Enclosing_Instance.lastBytePos)
				{
					Advance();
					if (lastInt >= target)
					{
						return doc = lastInt;
					}
				}
				return doc = NO_MORE_DOCS;
			}
		}
		/// <summary>When a BitSet has fewer than 1 in BITS2VINTLIST_SIZE bits set,
		/// a SortedVIntList representing the index numbers of the set bits
		/// will be smaller than that BitSet.
		/// </summary>
		internal const int BITS2VINTLIST_SIZE = 8;
		
		private int size;
		private sbyte[] bytes;
		private int lastBytePos;
		
		/// <summary>  Create a SortedVIntList from all elements of an array of integers.
		/// 
		/// </summary>
		/// <param name="sortedInts"> A sorted array of non negative integers.
		/// </param>
		public SortedVIntList(int[] sortedInts):this(sortedInts, sortedInts.Length)
		{
		}
		
		/// <summary> Create a SortedVIntList from an array of integers.</summary>
		/// <param name="sortedInts"> An array of sorted non negative integers.
		/// </param>
		/// <param name="inputSize">  The number of integers to be used from the array.
		/// </param>
		public SortedVIntList(int[] sortedInts, int inputSize)
		{
			SortedVIntListBuilder builder = new SortedVIntListBuilder(this);
			for (int i = 0; i < inputSize; i++)
			{
				builder.AddInt(sortedInts[i]);
			}
			builder.Done();
		}
		
		/// <summary> Create a SortedVIntList from a BitSet.</summary>
		/// <param name="bits"> A bit set representing a set of integers.
		/// </param>
		public SortedVIntList(System.Collections.BitArray bits)
		{
			SortedVIntListBuilder builder = new SortedVIntListBuilder(this);
			int nextInt = SupportClass.BitSetSupport.NextSetBit(bits, 0);
			while (nextInt != - 1)
			{
				builder.AddInt(nextInt);
				nextInt = SupportClass.BitSetSupport.NextSetBit(bits, nextInt + 1);
			}
			builder.Done();
		}
		
		/// <summary> Create a SortedVIntList from an OpenBitSet.</summary>
		/// <param name="bits"> A bit set representing a set of integers.
		/// </param>
		public SortedVIntList(OpenBitSet bits)
		{
			SortedVIntListBuilder builder = new SortedVIntListBuilder(this);
			int nextInt = bits.NextSetBit(0);
			while (nextInt != - 1)
			{
				builder.AddInt(nextInt);
				nextInt = bits.NextSetBit(nextInt + 1);
			}
			builder.Done();
		}
		
		/// <summary> Create a SortedVIntList.</summary>
		/// <param name="docIdSetIterator"> An iterator providing document numbers as a set of integers.
		/// This DocIdSetIterator is iterated completely when this constructor
		/// is called and it must provide the integers in non
		/// decreasing order.
		/// </param>
		public SortedVIntList(DocIdSetIterator docIdSetIterator)
		{
			SortedVIntListBuilder builder = new SortedVIntListBuilder(this);
			int doc;
			while ((doc = docIdSetIterator.NextDoc()) != DocIdSetIterator.NO_MORE_DOCS)
			{
				builder.AddInt(doc);
			}
			builder.Done();
		}
		
		
		private class SortedVIntListBuilder
		{
			private void  InitBlock(SortedVIntList enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SortedVIntList enclosingInstance;
			public SortedVIntList Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private int lastInt = 0;
			
			internal SortedVIntListBuilder(SortedVIntList enclosingInstance)
			{
				InitBlock(enclosingInstance);
				Enclosing_Instance.InitBytes();
				lastInt = 0;
			}
			
			internal virtual void  AddInt(int nextInt)
			{
				int diff = nextInt - lastInt;
				if (diff < 0)
				{
					throw new System.ArgumentException("Input not sorted or first element negative.");
				}
				
				if ((Enclosing_Instance.lastBytePos + Enclosing_Instance.MAX_BYTES_PER_INT) > Enclosing_Instance.bytes.Length)
				{
					// biggest possible int does not fit
					Enclosing_Instance.ResizeBytes((Enclosing_Instance.bytes.Length * 2) + Enclosing_Instance.MAX_BYTES_PER_INT);
				}
				
				// See Mono.Lucene.Net.Store.IndexOutput.writeVInt()
				while ((diff & ~ Mono.Lucene.Net.Util.SortedVIntList.VB1) != 0)
				{
					// The high bit of the next byte needs to be set.
					Enclosing_Instance.bytes[Enclosing_Instance.lastBytePos++] = (sbyte) ((diff & Mono.Lucene.Net.Util.SortedVIntList.VB1) | ~ Mono.Lucene.Net.Util.SortedVIntList.VB1);
					diff = SupportClass.Number.URShift(diff, Mono.Lucene.Net.Util.SortedVIntList.BIT_SHIFT);
				}
				Enclosing_Instance.bytes[Enclosing_Instance.lastBytePos++] = (sbyte) diff; // Last byte, high bit not set.
				Enclosing_Instance.size++;
				lastInt = nextInt;
			}
			
			internal virtual void  Done()
			{
				Enclosing_Instance.ResizeBytes(Enclosing_Instance.lastBytePos);
			}
		}
		
		
		private void  InitBytes()
		{
			size = 0;
			bytes = new sbyte[128]; // initial byte size
			lastBytePos = 0;
		}
		
		private void  ResizeBytes(int newSize)
		{
			if (newSize != bytes.Length)
			{
				sbyte[] newBytes = new sbyte[newSize];
				Array.Copy(bytes, 0, newBytes, 0, lastBytePos);
				bytes = newBytes;
			}
		}
		
		private const int VB1 = 0x7F;
		private const int BIT_SHIFT = 7;
		private int MAX_BYTES_PER_INT = (31 / BIT_SHIFT) + 1;
		
		/// <returns>    The total number of sorted integers.
		/// </returns>
		public virtual int Size()
		{
			return size;
		}
		
		/// <returns> The size of the byte array storing the compressed sorted integers.
		/// </returns>
		public virtual int GetByteSize()
		{
			return bytes.Length;
		}
		
		/// <summary>This DocIdSet implementation is cacheable. </summary>
		public override bool IsCacheable()
		{
			return true;
		}
		
		/// <returns>    An iterator over the sorted integers.
		/// </returns>
		public override DocIdSetIterator Iterator()
		{
			return new AnonymousClassDocIdSetIterator(this);
		}
	}
}
