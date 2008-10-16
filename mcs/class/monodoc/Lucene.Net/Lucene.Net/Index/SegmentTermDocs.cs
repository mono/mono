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
using InputStream = Monodoc.Lucene.Net.Store.InputStream;
using BitVector = Monodoc.Lucene.Net.Util.BitVector;
namespace Monodoc.Lucene.Net.Index
{
	
	public class SegmentTermDocs : TermDocs
	{
		protected internal SegmentReader parent;
		private InputStream freqStream;
		private int count;
		private int df;
		private BitVector deletedDocs;
		internal int doc = 0;
		internal int freq;
		
		private int skipInterval;
		private int numSkips;
		private int skipCount;
		private InputStream skipStream;
		private int skipDoc;
		private long freqPointer;
		private long proxPointer;
		private long skipPointer;
		private bool haveSkipped;
		
		public /*internal*/ SegmentTermDocs(SegmentReader parent)
		{
			this.parent = parent;
			this.freqStream = (InputStream) parent.freqStream.Clone();
			this.deletedDocs = parent.deletedDocs;
			this.skipInterval = parent.tis.GetSkipInterval();
		}
		
		public virtual void  Seek(Term term)
		{
			TermInfo ti = parent.tis.Get(term);
			Seek(ti);
		}
		
		public virtual void  Seek(TermEnum termEnum)
		{
			TermInfo ti;
			
			// use comparison of fieldinfos to verify that termEnum belongs to the same segment as this SegmentTermDocs
			if (termEnum is SegmentTermEnum && ((SegmentTermEnum) termEnum).fieldInfos == parent.fieldInfos)
			// optimized case
				ti = ((SegmentTermEnum) termEnum).TermInfo();
			// punt case
			else
				ti = parent.tis.Get(termEnum.Term());
			
			Seek(ti);
		}
		
		internal virtual void  Seek(TermInfo ti)
		{
			count = 0;
			if (ti == null)
			{
				df = 0;
			}
			else
			{
				df = ti.docFreq;
				doc = 0;
				skipDoc = 0;
				skipCount = 0;
				numSkips = df / skipInterval;
				freqPointer = ti.freqPointer;
				proxPointer = ti.proxPointer;
				skipPointer = freqPointer + ti.skipOffset;
				freqStream.Seek(freqPointer);
				haveSkipped = false;
			}
		}
		
		public virtual void  Close()
		{
			freqStream.Close();
			if (skipStream != null)
				skipStream.Close();
		}
		
		public int Doc()
		{
			return doc;
		}
		public int Freq()
		{
			return freq;
		}
		
		protected internal virtual void  SkippingDoc()
		{
		}
		
		public virtual bool Next()
		{
			while (true)
			{
				if (count == df)
					return false;
				
				int docCode = freqStream.ReadVInt();
				doc += (int) (((uint) docCode) >> 1); // shift off low bit
				if ((docCode & 1) != 0)
				// if low bit is set
					freq = 1;
				// freq is one
				else
					freq = freqStream.ReadVInt(); // else read freq
				
				count++;
				
				if (deletedDocs == null || !deletedDocs.Get(doc))
					break;
				SkippingDoc();
			}
			return true;
		}
		
		/// <summary>Optimized implementation. </summary>
		public virtual int Read(int[] docs, int[] freqs)
		{
			int length = docs.Length;
			int i = 0;
			while (i < length && count < df)
			{
				
				// manually inlined call to next() for speed
				int docCode = freqStream.ReadVInt();
				doc += (int) (((uint) docCode) >> 1); // shift off low bit
				if ((docCode & 1) != 0)
				// if low bit is set
					freq = 1;
				// freq is one
				else
					freq = freqStream.ReadVInt(); // else read freq
				count++;
				
				if (deletedDocs == null || !deletedDocs.Get(doc))
				{
					docs[i] = doc;
					freqs[i] = freq;
					++i;
				}
			}
			return i;
		}
		
		/// <summary>Overridden by SegmentTermPositions to skip in prox stream. </summary>
		protected internal virtual void  SkipProx(long proxPointer)
		{
		}
		
		/// <summary>Optimized implementation. </summary>
		public virtual bool SkipTo(int target)
		{
			if (df >= skipInterval)
			{
				// optimized case
				
				if (skipStream == null)
					skipStream = (InputStream) freqStream.Clone(); // lazily clone
				
				if (!haveSkipped)
				{
					// lazily seek skip stream
					skipStream.Seek(skipPointer);
					haveSkipped = true;
				}
				
				// scan skip data
				int lastSkipDoc = skipDoc;
				long lastFreqPointer = freqStream.GetFilePointer();
				long lastProxPointer = - 1;
				int numSkipped = - 1 - (count % skipInterval);
				
				while (target > skipDoc)
				{
					lastSkipDoc = skipDoc;
					lastFreqPointer = freqPointer;
					lastProxPointer = proxPointer;
					
					if (skipDoc != 0 && skipDoc >= doc)
						numSkipped += skipInterval;
					
					if (skipCount >= numSkips)
						break;
					
					skipDoc += skipStream.ReadVInt();
					freqPointer += skipStream.ReadVInt();
					proxPointer += skipStream.ReadVInt();
					
					skipCount++;
				}
				
				// if we found something to skip, then skip it
				if (lastFreqPointer > freqStream.GetFilePointer())
				{
					freqStream.Seek(lastFreqPointer);
					SkipProx(lastProxPointer);
					
					doc = lastSkipDoc;
					count += numSkipped;
				}
			}
			
			// done skipping, now just scan
			do 
			{
				if (!Next())
					return false;
			}
			while (target > doc);
			return true;
		}
	}
}