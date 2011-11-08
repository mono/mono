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

using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;
using Fieldable = Mono.Lucene.Net.Documents.Fieldable;
using UnicodeUtil = Mono.Lucene.Net.Util.UnicodeUtil;

namespace Mono.Lucene.Net.Index
{
	
	sealed class TermsHashPerField:InvertedDocConsumerPerField
	{
		private void  InitBlock()
		{
			postingsHashHalfSize = postingsHashSize / 2;
			postingsHashMask = postingsHashSize - 1;
			postingsHash = new RawPostingList[postingsHashSize];
		}
		
		internal TermsHashConsumerPerField consumer;
		internal TermsHashPerField nextPerField;
		internal TermsHashPerThread perThread;
		internal DocumentsWriter.DocState docState;
		internal FieldInvertState fieldState;
		internal TermAttribute termAtt;
		
		// Copied from our perThread
		internal CharBlockPool charPool;
		internal IntBlockPool intPool;
		internal ByteBlockPool bytePool;
		
		internal int streamCount;
		internal int numPostingInt;
		
		internal FieldInfo fieldInfo;
		
		internal bool postingsCompacted;
		internal int numPostings;
		private int postingsHashSize = 4;
		private int postingsHashHalfSize;
		private int postingsHashMask;
		private RawPostingList[] postingsHash;
		private RawPostingList p;
		
		public TermsHashPerField(DocInverterPerField docInverterPerField, TermsHashPerThread perThread, TermsHashPerThread nextPerThread, FieldInfo fieldInfo)
		{
			InitBlock();
			this.perThread = perThread;
			intPool = perThread.intPool;
			charPool = perThread.charPool;
			bytePool = perThread.bytePool;
			docState = perThread.docState;
			fieldState = docInverterPerField.fieldState;
			this.consumer = perThread.consumer.AddField(this, fieldInfo);
			streamCount = consumer.GetStreamCount();
			numPostingInt = 2 * streamCount;
			this.fieldInfo = fieldInfo;
			if (nextPerThread != null)
				nextPerField = (TermsHashPerField) nextPerThread.AddField(docInverterPerField, fieldInfo);
			else
				nextPerField = null;
		}
		
		internal void  ShrinkHash(int targetSize)
		{
			System.Diagnostics.Debug.Assert(postingsCompacted || numPostings == 0);

            int newSize = 4;
			
			if (newSize != postingsHash.Length)
			{
				postingsHash = new RawPostingList[newSize];
				postingsHashSize = newSize;
				postingsHashHalfSize = newSize / 2;
				postingsHashMask = newSize - 1;
			}
            System.Array.Clear(postingsHash,0,postingsHash.Length);
		}
		
		public void  Reset()
		{
			if (!postingsCompacted)
				CompactPostings();
			System.Diagnostics.Debug.Assert(numPostings <= postingsHash.Length);
			if (numPostings > 0)
			{
				perThread.termsHash.RecyclePostings(postingsHash, numPostings);
                Array.Clear(postingsHash, 0, numPostings);
				numPostings = 0;
			}
			postingsCompacted = false;
			if (nextPerField != null)
				nextPerField.Reset();
		}
		
		public override void  Abort()
		{
			lock (this)
			{
				Reset();
				if (nextPerField != null)
					nextPerField.Abort();
			}
		}
		
		public void  InitReader(ByteSliceReader reader, RawPostingList p, int stream)
		{
			System.Diagnostics.Debug.Assert(stream < streamCount);
			int[] ints = intPool.buffers[p.intStart >> DocumentsWriter.INT_BLOCK_SHIFT];
			int upto = p.intStart & DocumentsWriter.INT_BLOCK_MASK;
			reader.Init(bytePool, p.byteStart + stream * ByteBlockPool.FIRST_LEVEL_SIZE, ints[upto + stream]);
		}
		
		private void  CompactPostings()
		{
			lock (this)
			{
				int upto = 0;
				for (int i = 0; i < postingsHashSize; i++)
				{
					if (postingsHash[i] != null)
					{
						if (upto < i)
						{
							postingsHash[upto] = postingsHash[i];
							postingsHash[i] = null;
						}
						upto++;
					}
				}
				
				System.Diagnostics.Debug.Assert(upto == numPostings);
				postingsCompacted = true;
			}
		}
		
		/// <summary>Collapse the hash table &amp; sort in-place. </summary>
		public RawPostingList[] SortPostings()
		{
			CompactPostings();
			QuickSort(postingsHash, 0, numPostings - 1);
			return postingsHash;
		}
		
		internal void  QuickSort(RawPostingList[] postings, int lo, int hi)
		{
			if (lo >= hi)
				return ;
			else if (hi == 1 + lo)
			{
				if (ComparePostings(postings[lo], postings[hi]) > 0)
				{
					RawPostingList tmp = postings[lo];
					postings[lo] = postings[hi];
					postings[hi] = tmp;
				}
				return ;
			}
			
			int mid = SupportClass.Number.URShift((lo + hi), 1);
			
			if (ComparePostings(postings[lo], postings[mid]) > 0)
			{
				RawPostingList tmp = postings[lo];
				postings[lo] = postings[mid];
				postings[mid] = tmp;
			}
			
			if (ComparePostings(postings[mid], postings[hi]) > 0)
			{
				RawPostingList tmp = postings[mid];
				postings[mid] = postings[hi];
				postings[hi] = tmp;
				
				if (ComparePostings(postings[lo], postings[mid]) > 0)
				{
					RawPostingList tmp2 = postings[lo];
					postings[lo] = postings[mid];
					postings[mid] = tmp2;
				}
			}
			
			int left = lo + 1;
			int right = hi - 1;
			
			if (left >= right)
				return ;
			
			RawPostingList partition = postings[mid];
			
			for (; ; )
			{
				while (ComparePostings(postings[right], partition) > 0)
					--right;
				
				while (left < right && ComparePostings(postings[left], partition) <= 0)
					++left;
				
				if (left < right)
				{
					RawPostingList tmp = postings[left];
					postings[left] = postings[right];
					postings[right] = tmp;
					--right;
				}
				else
				{
					break;
				}
			}
			
			QuickSort(postings, lo, left);
			QuickSort(postings, left + 1, hi);
		}
		
		/// <summary>Compares term text for two Posting instance and
        /// returns -1 if p1 &lt; p2; 1 if p1 &gt; p2; else 0. 
		/// </summary>
		internal int ComparePostings(RawPostingList p1, RawPostingList p2)
		{
			
			if (p1 == p2)
				return 0;
			
			char[] text1 = charPool.buffers[p1.textStart >> DocumentsWriter.CHAR_BLOCK_SHIFT];
			int pos1 = p1.textStart & DocumentsWriter.CHAR_BLOCK_MASK;
			char[] text2 = charPool.buffers[p2.textStart >> DocumentsWriter.CHAR_BLOCK_SHIFT];
			int pos2 = p2.textStart & DocumentsWriter.CHAR_BLOCK_MASK;
			
			System.Diagnostics.Debug.Assert(text1 != text2 || pos1 != pos2);
			
			while (true)
			{
				char c1 = text1[pos1++];
				char c2 = text2[pos2++];
				if (c1 != c2)
				{
					if (0xffff == c2)
						return 1;
					else if (0xffff == c1)
						return - 1;
					else
						return c1 - c2;
				}
				else
					// This method should never compare equal postings
					// unless p1==p2
					System.Diagnostics.Debug.Assert(c1 != 0xffff);
			}
		}
		
		/// <summary>Test whether the text for current RawPostingList p equals
		/// current tokenText. 
		/// </summary>
		private bool PostingEquals(char[] tokenText, int tokenTextLen)
		{
			
			char[] text = perThread.charPool.buffers[p.textStart >> DocumentsWriter.CHAR_BLOCK_SHIFT];
			System.Diagnostics.Debug.Assert(text != null);
			int pos = p.textStart & DocumentsWriter.CHAR_BLOCK_MASK;
			
			int tokenPos = 0;
			for (; tokenPos < tokenTextLen; pos++, tokenPos++)
				if (tokenText[tokenPos] != text[pos])
					return false;
			return 0xffff == text[pos];
		}
		
		private bool doCall;
		private bool doNextCall;
		
		internal override void  Start(Fieldable f)
		{
			termAtt = (TermAttribute) fieldState.attributeSource.AddAttribute(typeof(TermAttribute));
			consumer.Start(f);
			if (nextPerField != null)
			{
				nextPerField.Start(f);
			}
		}
		
		internal override bool Start(Fieldable[] fields, int count)
		{
			doCall = consumer.Start(fields, count);
			if (nextPerField != null)
				doNextCall = nextPerField.Start(fields, count);
			return doCall || doNextCall;
		}
		
		// Secondary entry point (for 2nd & subsequent TermsHash),
		// because token text has already been "interned" into
		// textStart, so we hash by textStart
		public void  Add(int textStart)
		{
			
			int code = textStart;
			
			int hashPos = code & postingsHashMask;
			
			System.Diagnostics.Debug.Assert(!postingsCompacted);
			
			// Locate RawPostingList in hash
			p = postingsHash[hashPos];
			
			if (p != null && p.textStart != textStart)
			{
				// Conflict: keep searching different locations in
				// the hash table.
				int inc = ((code >> 8) + code) | 1;
				do 
				{
					code += inc;
					hashPos = code & postingsHashMask;
					p = postingsHash[hashPos];
				}
				while (p != null && p.textStart != textStart);
			}
			
			if (p == null)
			{
				
				// First time we are seeing this token since we last
				// flushed the hash.
				
				// Refill?
				if (0 == perThread.freePostingsCount)
					perThread.MorePostings();
				
				// Pull next free RawPostingList from free list
				p = perThread.freePostings[--perThread.freePostingsCount];
				System.Diagnostics.Debug.Assert(p != null);
				
				p.textStart = textStart;
				
				System.Diagnostics.Debug.Assert(postingsHash [hashPos] == null);
				postingsHash[hashPos] = p;
				numPostings++;
				
				if (numPostings == postingsHashHalfSize)
					RehashPostings(2 * postingsHashSize);
				
				// Init stream slices
				if (numPostingInt + intPool.intUpto > DocumentsWriter.INT_BLOCK_SIZE)
					intPool.NextBuffer();
				
				if (DocumentsWriter.BYTE_BLOCK_SIZE - bytePool.byteUpto < numPostingInt * ByteBlockPool.FIRST_LEVEL_SIZE)
					bytePool.NextBuffer();
				
				intUptos = intPool.buffer;
				intUptoStart = intPool.intUpto;
				intPool.intUpto += streamCount;
				
				p.intStart = intUptoStart + intPool.intOffset;
				
				for (int i = 0; i < streamCount; i++)
				{
					int upto = bytePool.NewSlice(ByteBlockPool.FIRST_LEVEL_SIZE);
					intUptos[intUptoStart + i] = upto + bytePool.byteOffset;
				}
				p.byteStart = intUptos[intUptoStart];
				
				consumer.NewTerm(p);
			}
			else
			{
				intUptos = intPool.buffers[p.intStart >> DocumentsWriter.INT_BLOCK_SHIFT];
				intUptoStart = p.intStart & DocumentsWriter.INT_BLOCK_MASK;
				consumer.AddTerm(p);
			}
		}
		
		// Primary entry point (for first TermsHash)
		internal override void  Add()
		{
			
			System.Diagnostics.Debug.Assert(!postingsCompacted);
			
			// We are first in the chain so we must "intern" the
			// term text into textStart address
			
			// Get the text of this term.
			char[] tokenText = termAtt.TermBuffer();
			;
			int tokenTextLen = termAtt.TermLength();
			
			// Compute hashcode & replace any invalid UTF16 sequences
			int downto = tokenTextLen;
			int code = 0;
			while (downto > 0)
			{
				char ch = tokenText[--downto];
				
				if (ch >= UnicodeUtil.UNI_SUR_LOW_START && ch <= UnicodeUtil.UNI_SUR_LOW_END)
				{
					if (0 == downto)
					{
						// Unpaired
						ch = tokenText[downto] = (char) (UnicodeUtil.UNI_REPLACEMENT_CHAR);
					}
					else
					{
						char ch2 = tokenText[downto - 1];
						if (ch2 >= UnicodeUtil.UNI_SUR_HIGH_START && ch2 <= UnicodeUtil.UNI_SUR_HIGH_END)
						{
							// OK: high followed by low.  This is a valid
							// surrogate pair.
							code = ((code * 31) + ch) * 31 + ch2;
							downto--;
							continue;
						}
						else
						{
							// Unpaired
							ch = tokenText[downto] = (char) (UnicodeUtil.UNI_REPLACEMENT_CHAR);
						}
					}
				}
				else if (ch >= UnicodeUtil.UNI_SUR_HIGH_START && (ch <= UnicodeUtil.UNI_SUR_HIGH_END || ch == 0xffff))
				{
					// Unpaired or 0xffff
					ch = tokenText[downto] = (char) (UnicodeUtil.UNI_REPLACEMENT_CHAR);
				}
				
				code = (code * 31) + ch;
			}
			
			int hashPos = code & postingsHashMask;
			
			// Locate RawPostingList in hash
			p = postingsHash[hashPos];
			
			if (p != null && !PostingEquals(tokenText, tokenTextLen))
			{
				// Conflict: keep searching different locations in
				// the hash table.
				int inc = ((code >> 8) + code) | 1;
				do 
				{
					code += inc;
					hashPos = code & postingsHashMask;
					p = postingsHash[hashPos];
				}
				while (p != null && !PostingEquals(tokenText, tokenTextLen));
			}
			
			if (p == null)
			{
				
				// First time we are seeing this token since we last
				// flushed the hash.
				int textLen1 = 1 + tokenTextLen;
				if (textLen1 + charPool.charUpto > DocumentsWriter.CHAR_BLOCK_SIZE)
				{
					if (textLen1 > DocumentsWriter.CHAR_BLOCK_SIZE)
					{
						// Just skip this term, to remain as robust as
						// possible during indexing.  A TokenFilter
						// can be inserted into the analyzer chain if
						// other behavior is wanted (pruning the term
						// to a prefix, throwing an exception, etc).
						
						if (docState.maxTermPrefix == null)
							docState.maxTermPrefix = new System.String(tokenText, 0, 30);
						
						consumer.SkippingLongTerm();
						return ;
					}
					charPool.NextBuffer();
				}
				
				// Refill?
				if (0 == perThread.freePostingsCount)
					perThread.MorePostings();
				
				// Pull next free RawPostingList from free list
				p = perThread.freePostings[--perThread.freePostingsCount];
				System.Diagnostics.Debug.Assert(p != null);
				
				char[] text = charPool.buffer;
				int textUpto = charPool.charUpto;
				p.textStart = textUpto + charPool.charOffset;
				charPool.charUpto += textLen1;
				Array.Copy(tokenText, 0, text, textUpto, tokenTextLen);
				text[textUpto + tokenTextLen] = (char) (0xffff);
				
				System.Diagnostics.Debug.Assert(postingsHash [hashPos] == null);
				postingsHash[hashPos] = p;
				numPostings++;
				
				if (numPostings == postingsHashHalfSize)
					RehashPostings(2 * postingsHashSize);
				
				// Init stream slices
				if (numPostingInt + intPool.intUpto > DocumentsWriter.INT_BLOCK_SIZE)
					intPool.NextBuffer();
				
				if (DocumentsWriter.BYTE_BLOCK_SIZE - bytePool.byteUpto < numPostingInt * ByteBlockPool.FIRST_LEVEL_SIZE)
					bytePool.NextBuffer();
				
				intUptos = intPool.buffer;
				intUptoStart = intPool.intUpto;
				intPool.intUpto += streamCount;
				
				p.intStart = intUptoStart + intPool.intOffset;
				
				for (int i = 0; i < streamCount; i++)
				{
					int upto = bytePool.NewSlice(ByteBlockPool.FIRST_LEVEL_SIZE);
					intUptos[intUptoStart + i] = upto + bytePool.byteOffset;
				}
				p.byteStart = intUptos[intUptoStart];
				
				consumer.NewTerm(p);
			}
			else
			{
				intUptos = intPool.buffers[p.intStart >> DocumentsWriter.INT_BLOCK_SHIFT];
				intUptoStart = p.intStart & DocumentsWriter.INT_BLOCK_MASK;
				consumer.AddTerm(p);
			}
			
			if (doNextCall)
				nextPerField.Add(p.textStart);
		}
		
		internal int[] intUptos;
		internal int intUptoStart;
		
		internal void  WriteByte(int stream, byte b)
		{
			int upto = intUptos[intUptoStart + stream];
			byte[] bytes = bytePool.buffers[upto >> DocumentsWriter.BYTE_BLOCK_SHIFT];
			System.Diagnostics.Debug.Assert(bytes != null);
			int offset = upto & DocumentsWriter.BYTE_BLOCK_MASK;
			if (bytes[offset] != 0)
			{
				// End of slice; allocate a new one
				offset = bytePool.AllocSlice(bytes, offset);
				bytes = bytePool.buffer;
				intUptos[intUptoStart + stream] = offset + bytePool.byteOffset;
			}
			bytes[offset] = b;
			(intUptos[intUptoStart + stream])++;
		}
		
		public void  WriteBytes(int stream, byte[] b, int offset, int len)
		{
			// TODO: optimize
			int end = offset + len;
			for (int i = offset; i < end; i++)
				WriteByte(stream, b[i]);
		}
		
		internal void  WriteVInt(int stream, int i)
		{
			System.Diagnostics.Debug.Assert(stream < streamCount);
			while ((i & ~ 0x7F) != 0)
			{
				WriteByte(stream, (byte) ((i & 0x7f) | 0x80));
				i = SupportClass.Number.URShift(i, 7);
			}
			WriteByte(stream, (byte) i);
		}
		
		internal override void  Finish()
		{
			consumer.Finish();
			if (nextPerField != null)
				nextPerField.Finish();
		}
		
		/// <summary>Called when postings hash is too small (> 50%
        /// occupied) or too large (&lt; 20% occupied). 
		/// </summary>
		internal void  RehashPostings(int newSize)
		{
			
			int newMask = newSize - 1;
			
			RawPostingList[] newHash = new RawPostingList[newSize];
			for (int i = 0; i < postingsHashSize; i++)
			{
				RawPostingList p0 = postingsHash[i];
				if (p0 != null)
				{
					int code;
					if (perThread.primary)
					{
						int start = p0.textStart & DocumentsWriter.CHAR_BLOCK_MASK;
						char[] text = charPool.buffers[p0.textStart >> DocumentsWriter.CHAR_BLOCK_SHIFT];
						int pos = start;
						while (text[pos] != 0xffff)
							pos++;
						code = 0;
						while (pos > start)
							code = (code * 31) + text[--pos];
					}
					else
						code = p0.textStart;
					
					int hashPos = code & newMask;
					System.Diagnostics.Debug.Assert(hashPos >= 0);
					if (newHash[hashPos] != null)
					{
						int inc = ((code >> 8) + code) | 1;
						do 
						{
							code += inc;
							hashPos = code & newMask;
						}
						while (newHash[hashPos] != null);
					}
					newHash[hashPos] = p0;
				}
			}
			
			postingsHashMask = newMask;
			postingsHash = newHash;
			postingsHashSize = newSize;
			postingsHashHalfSize = newSize >> 1;
		}
	}
}
