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
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary>This stores a monotonically increasing set of <Term, TermInfo> pairs in a
	/// Directory.  Pairs are accessed either by Term or by ordinal position the
	/// set.  
	/// </summary>
	
	sealed public class TermInfosReader
	{
		private Directory directory;
		private System.String segment;
		private FieldInfos fieldInfos;
		
		private System.LocalDataStoreSlot enumerators = System.Threading.Thread.AllocateDataSlot();
		private SegmentTermEnum origEnum;
		private long size;
		
		public /*internal*/ TermInfosReader(Directory dir, System.String seg, FieldInfos fis)
		{
			directory = dir;
			segment = seg;
			fieldInfos = fis;
			
			origEnum = new SegmentTermEnum(directory.OpenFile(segment + ".tis"), fieldInfos, false);
			size = origEnum.size;
			ReadIndex();
		}
		
		public int GetSkipInterval()
		{
			return origEnum.skipInterval;
		}
		
		public /*internal*/ void  Close()
		{
			if (origEnum != null)
				origEnum.Close();
		}
		
		/// <summary>Returns the number of term/value pairs in the set. </summary>
		internal long Size()
		{
			return size;
		}
		
		private SegmentTermEnum GetEnum()
		{
			SegmentTermEnum termEnum = (SegmentTermEnum) System.Threading.Thread.GetData(enumerators);
			if (termEnum == null)
			{
				termEnum = Terms();
				System.Threading.Thread.SetData(enumerators, termEnum);
			}
			return termEnum;
		}
		
		internal Term[] indexTerms = null;
		internal TermInfo[] indexInfos;
		internal long[] indexPointers;
		
		private void  ReadIndex()
		{
			SegmentTermEnum indexEnum = new SegmentTermEnum(directory.OpenFile(segment + ".tii"), fieldInfos, true);
			try
			{
				int indexSize = (int) indexEnum.size;
				
				indexTerms = new Term[indexSize];
				indexInfos = new TermInfo[indexSize];
				indexPointers = new long[indexSize];
				
				for (int i = 0; indexEnum.Next(); i++)
				{
					indexTerms[i] = indexEnum.Term();
					indexInfos[i] = indexEnum.TermInfo();
					indexPointers[i] = indexEnum.indexPointer;
				}
			}
			finally
			{
				indexEnum.Close();
			}
		}
		
		/// <summary>Returns the offset of the greatest index entry which is less than or equal to term.</summary>
		private int GetIndexOffset(Term term)
		{
			int lo = 0; // binary search indexTerms[]
			int hi = indexTerms.Length - 1;
			
			while (hi >= lo)
			{
				int mid = (lo + hi) >> 1;
				int delta = term.CompareTo(indexTerms[mid]);
				if (delta < 0)
					hi = mid - 1;
				else if (delta > 0)
					lo = mid + 1;
				else
					return mid;
			}
			return hi;
		}
		
		private void  SeekEnum(int indexOffset)
		{
			GetEnum().Seek(indexPointers[indexOffset], (indexOffset * GetEnum().indexInterval) - 1, indexTerms[indexOffset], indexInfos[indexOffset]);
		}
		
		/// <summary>Returns the TermInfo for a Term in the set, or null. </summary>
		public /*internal*/ TermInfo Get(Term term)
		{
			if (size == 0)
				return null;
			
			// optimize sequential access: first try scanning cached enum w/o seeking
			SegmentTermEnum enumerator = GetEnum();
			if (enumerator.Term() != null && ((enumerator.prev != null && term.CompareTo(enumerator.prev) > 0) || term.CompareTo(enumerator.Term()) >= 0))
			{
				int enumOffset = (int) (enumerator.position / enumerator.indexInterval) + 1;
				if (indexTerms.Length == enumOffset || term.CompareTo(indexTerms[enumOffset]) < 0)
					return ScanEnum(term); // no need to seek
			}
			
			// random-access: must seek
			SeekEnum(GetIndexOffset(term));
			return ScanEnum(term);
		}
		
		/// <summary>Scans within block for matching term. </summary>
		private TermInfo ScanEnum(Term term)
		{
			SegmentTermEnum enumerator = GetEnum();
			while (term.CompareTo(enumerator.Term()) > 0 && enumerator.Next())
			{
			}
			if (enumerator.Term() != null && term.CompareTo(enumerator.Term()) == 0)
				return enumerator.TermInfo();
			else
				return null;
		}
		
		/// <summary>Returns the nth term in the set. </summary>
		internal Term Get(int position)
		{
			if (size == 0)
				return null;
			
			SegmentTermEnum enumerator = GetEnum();
			if (enumerator != null && enumerator.Term() != null && position >= enumerator.position && position < (enumerator.position + enumerator.indexInterval))
				return ScanEnum(position); // can avoid seek
			
			SeekEnum(position / enumerator.indexInterval); // must seek
			return ScanEnum(position);
		}
		
		private Term ScanEnum(int position)
		{
			SegmentTermEnum enumerator = GetEnum();
			while (enumerator.position < position)
				if (!enumerator.Next())
					return null;
			
			return enumerator.Term();
		}
		
		/// <summary>Returns the position of a Term in the set or -1. </summary>
		internal long GetPosition(Term term)
		{
			if (size == 0)
				return - 1;
			
			int indexOffset = GetIndexOffset(term);
			SeekEnum(indexOffset);
			
			SegmentTermEnum enumerator = GetEnum();
			while (term.CompareTo(enumerator.Term()) > 0 && enumerator.Next())
			{
			}
			
			if (term.CompareTo(enumerator.Term()) == 0)
				return enumerator.position;
			else
				return - 1;
		}
		
		/// <summary>Returns an enumeration of all the Terms and TermInfos in the set. </summary>
		public SegmentTermEnum Terms()
		{
			return (SegmentTermEnum) origEnum.Clone();
		}
		
		/// <summary>Returns an enumeration of terms starting at or after the named term. </summary>
		public SegmentTermEnum Terms(Term term)
		{
			Get(term);
			return (SegmentTermEnum) GetEnum().Clone();
		}
	}
}