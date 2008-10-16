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
namespace Monodoc.Lucene.Net.Index
{
	
	sealed public class SegmentTermEnum:TermEnum, System.ICloneable
	{
		private InputStream input;
		internal FieldInfos fieldInfos;
		internal long size;
		internal long position = - 1;
		
		private Term term = new Term("", "");
		private TermInfo termInfo = new TermInfo();
		
		private int format;
		private bool isIndex = false;
		internal long indexPointer = 0;
		internal int indexInterval;
		internal int skipInterval;
		private int formatM1SkipInterval;
		internal Term prev;
		
		private char[] buffer = new char[]{};
		
		internal SegmentTermEnum(InputStream i, FieldInfos fis, bool isi)
		{
			input = i;
			fieldInfos = fis;
			isIndex = isi;
			
			int firstInt = input.ReadInt();
			if (firstInt >= 0)
			{
				// original-format file, without explicit format version number
				format = 0;
				size = firstInt;
				
				// back-compatible settings
				indexInterval = 128;
				skipInterval = System.Int32.MaxValue; // switch off skipTo optimization
			}
			else
			{
				// we have a format version number
				format = firstInt;
				
				// check that it is a format we can understand
				if (format < TermInfosWriter.FORMAT)
					throw new System.IO.IOException("Unknown format version:" + format);
				
				size = input.ReadLong(); // read the size
				
				if (format == - 1)
				{
					if (!isIndex)
					{
						indexInterval = input.ReadInt();
						formatM1SkipInterval = input.ReadInt();
					}
					// switch off skipTo optimization for file format prior to 1.4rc2 in order to avoid a bug in 
					// skipTo implementation of these versions
					skipInterval = System.Int32.MaxValue;
				}
				else
				{
					indexInterval = input.ReadInt();
					skipInterval = input.ReadInt();
				}
			}
		}
		
		public System.Object Clone()
		{
			SegmentTermEnum clone = null;
			try
			{
				clone = (SegmentTermEnum) base.MemberwiseClone();
			}
			catch (System.Exception)
			{
			}
			
			clone.input = (InputStream) input.Clone();
			clone.termInfo = new TermInfo(termInfo);
			if (term != null)
				clone.GrowBuffer(term.text.Length);
			
			return clone;
		}
		
		internal void  Seek(long pointer, int p, Term t, TermInfo ti)
		{
			input.Seek(pointer);
			position = p;
			term = t;
			prev = null;
			termInfo.Set(ti);
			GrowBuffer(term.text.Length); // copy term text into buffer
		}
		
		/// <summary>Increments the enumeration to the next element.  True if one exists.</summary>
		public override bool Next()
		{
			if (position++ >= size - 1)
			{
				term = null;
				return false;
			}
			
			prev = term;
			term = ReadTerm();
			
			termInfo.docFreq = input.ReadVInt(); // read doc freq
			termInfo.freqPointer += input.ReadVLong(); // read freq pointer
			termInfo.proxPointer += input.ReadVLong(); // read prox pointer
			
			if (format == - 1)
			{
				//  just read skipOffset in order to increment  file pointer; 
				// value is never used since skipTo is switched off
				if (!isIndex)
				{
					if (termInfo.docFreq > formatM1SkipInterval)
					{
						termInfo.skipOffset = input.ReadVInt();
					}
				}
			}
			else
			{
				if (termInfo.docFreq >= skipInterval)
					termInfo.skipOffset = input.ReadVInt();
			}
			
			if (isIndex)
				indexPointer += input.ReadVLong(); // read index pointer
			
			return true;
		}
		
		private Term ReadTerm()
		{
			int start = input.ReadVInt();
			int length = input.ReadVInt();
			int totalLength = start + length;
			if (buffer.Length < totalLength)
				GrowBuffer(totalLength);
			
			input.ReadChars(buffer, start, length);
			return new Term(fieldInfos.FieldName(input.ReadVInt()), new System.String(buffer, 0, totalLength), false);
		}
		
		private void  GrowBuffer(int length)
		{
			buffer = new char[length];
			for (int i = 0; i < term.text.Length; i++)
			// copy contents
				buffer[i] = term.text[i];
		}
		
		/// <summary>Returns the current Term in the enumeration.
		/// Initially invalid, valid after next() called for the first time.
		/// </summary>
		public override Term Term()
		{
			return term;
		}
		
		/// <summary>Returns the current TermInfo in the enumeration.
		/// Initially invalid, valid after next() called for the first time.
		/// </summary>
		public /*internal*/ TermInfo TermInfo()
		{
			return new TermInfo(termInfo);
		}
		
		/// <summary>Sets the argument to the current TermInfo in the enumeration.
		/// Initially invalid, valid after next() called for the first time.
		/// </summary>
		internal void  TermInfo(TermInfo ti)
		{
			ti.Set(termInfo);
		}
		
		/// <summary>Returns the docFreq from the current TermInfo in the enumeration.
		/// Initially invalid, valid after next() called for the first time.
		/// </summary>
		public override int DocFreq()
		{
			return termInfo.docFreq;
		}
		
		/* Returns the freqPointer from the current TermInfo in the enumeration.
		Initially invalid, valid after next() called for the first time.*/
		internal long FreqPointer()
		{
			return termInfo.freqPointer;
		}
		
		/* Returns the proxPointer from the current TermInfo in the enumeration.
		Initially invalid, valid after next() called for the first time.*/
		internal long ProxPointer()
		{
			return termInfo.proxPointer;
		}
		
		/// <summary>Closes the enumeration to further activity, freeing resources. </summary>
		public override void  Close()
		{
			input.Close();
		}
	}
}