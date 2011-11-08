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

using Directory = Mono.Lucene.Net.Store.Directory;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using UnicodeUtil = Mono.Lucene.Net.Util.UnicodeUtil;

namespace Mono.Lucene.Net.Index
{

    /// <summary>This stores a monotonically increasing set of &lt;Term, TermInfo&gt; pairs in a
	/// Directory.  A TermInfos can be written once, in order.  
	/// </summary>
	
	sealed class TermInfosWriter
	{
		/// <summary>The file format version, a negative number. </summary>
		public const int FORMAT = - 3;
		
		// Changed strings to true utf8 with length-in-bytes not
		// length-in-chars
		public const int FORMAT_VERSION_UTF8_LENGTH_IN_BYTES = - 4;
		
		// NOTE: always change this if you switch to a new format!
		public static readonly int FORMAT_CURRENT = FORMAT_VERSION_UTF8_LENGTH_IN_BYTES;
		
		private FieldInfos fieldInfos;
		private IndexOutput output;
		private TermInfo lastTi = new TermInfo();
		private long size;
		
		// TODO: the default values for these two parameters should be settable from
		// IndexWriter.  However, once that's done, folks will start setting them to
		// ridiculous values and complaining that things don't work well, as with
		// mergeFactor.  So, let's wait until a number of folks find that alternate
		// values work better.  Note that both of these values are stored in the
		// segment, so that it's safe to change these w/o rebuilding all indexes.
		
		/// <summary>Expert: The fraction of terms in the "dictionary" which should be stored
		/// in RAM.  Smaller values use more memory, but make searching slightly
		/// faster, while larger values use less memory and make searching slightly
		/// slower.  Searching is typically not dominated by dictionary lookup, so
		/// tweaking this is rarely useful.
		/// </summary>
		internal int indexInterval = 128;
		
		/// <summary>Expert: The fraction of {@link TermDocs} entries stored in skip tables,
		/// used to accellerate {@link TermDocs#SkipTo(int)}.  Larger values result in
		/// smaller indexes, greater acceleration, but fewer accelerable cases, while
		/// smaller values result in bigger indexes, less acceleration and more
		/// accelerable cases. More detailed experiments would be useful here. 
		/// </summary>
		internal int skipInterval = 16;
		
		/// <summary>Expert: The maximum number of skip levels. Smaller values result in 
		/// slightly smaller indexes, but slower skipping in big posting lists.
		/// </summary>
		internal int maxSkipLevels = 10;
		
		private long lastIndexPointer;
		private bool isIndex;
		private byte[] lastTermBytes = new byte[10];
		private int lastTermBytesLength = 0;
		private int lastFieldNumber = - 1;
		
		private TermInfosWriter other;
		private UnicodeUtil.UTF8Result utf8Result = new UnicodeUtil.UTF8Result();
		
		internal TermInfosWriter(Directory directory, System.String segment, FieldInfos fis, int interval)
		{
			Initialize(directory, segment, fis, interval, false);
			other = new TermInfosWriter(directory, segment, fis, interval, true);
			other.other = this;
		}
		
		private TermInfosWriter(Directory directory, System.String segment, FieldInfos fis, int interval, bool isIndex)
		{
			Initialize(directory, segment, fis, interval, isIndex);
		}
		
		private void  Initialize(Directory directory, System.String segment, FieldInfos fis, int interval, bool isi)
		{
			indexInterval = interval;
			fieldInfos = fis;
			isIndex = isi;
			output = directory.CreateOutput(segment + (isIndex?".tii":".tis"));
			output.WriteInt(FORMAT_CURRENT); // write format
			output.WriteLong(0); // leave space for size
			output.WriteInt(indexInterval); // write indexInterval
			output.WriteInt(skipInterval); // write skipInterval
			output.WriteInt(maxSkipLevels); // write maxSkipLevels
			System.Diagnostics.Debug.Assert(InitUTF16Results());
		}
		
		internal void  Add(Term term, TermInfo ti)
		{
			UnicodeUtil.UTF16toUTF8(term.text, 0, term.text.Length, utf8Result);
			Add(fieldInfos.FieldNumber(term.field), utf8Result.result, utf8Result.length, ti);
		}
		
		// Currently used only by assert statements
		internal UnicodeUtil.UTF16Result utf16Result1;
		internal UnicodeUtil.UTF16Result utf16Result2;
		
		// Currently used only by assert statements
		private bool InitUTF16Results()
		{
			utf16Result1 = new UnicodeUtil.UTF16Result();
			utf16Result2 = new UnicodeUtil.UTF16Result();
			return true;
		}
		
		// Currently used only by assert statement
		private int CompareToLastTerm(int fieldNumber, byte[] termBytes, int termBytesLength)
		{
			
			if (lastFieldNumber != fieldNumber)
			{
				int cmp = String.CompareOrdinal(fieldInfos.FieldName(lastFieldNumber), fieldInfos.FieldName(fieldNumber));
				// If there is a field named "" (empty string) then we
				// will get 0 on this comparison, yet, it's "OK".  But
				// it's not OK if two different field numbers map to
				// the same name.
				if (cmp != 0 || lastFieldNumber != - 1)
					return cmp;
			}
			
			UnicodeUtil.UTF8toUTF16(lastTermBytes, 0, lastTermBytesLength, utf16Result1);
			UnicodeUtil.UTF8toUTF16(termBytes, 0, termBytesLength, utf16Result2);
			int len;
			if (utf16Result1.length < utf16Result2.length)
				len = utf16Result1.length;
			else
				len = utf16Result2.length;
			
			for (int i = 0; i < len; i++)
			{
				char ch1 = utf16Result1.result[i];
				char ch2 = utf16Result2.result[i];
				if (ch1 != ch2)
					return ch1 - ch2;
			}
			return utf16Result1.length - utf16Result2.length;
		}

        /// <summary>Adds a new &lt;fieldNumber, termBytes&gt;, TermInfo> pair to the set.
		/// Term must be lexicographically greater than all previous Terms added.
		/// TermInfo pointers must be positive and greater than all previous.
		/// </summary>
		internal void  Add(int fieldNumber, byte[] termBytes, int termBytesLength, TermInfo ti)
		{
			
			System.Diagnostics.Debug.Assert(CompareToLastTerm(fieldNumber, termBytes, termBytesLength) < 0 ||
					(isIndex && termBytesLength == 0 && lastTermBytesLength == 0), 
				"Terms are out of order: field=" + fieldInfos.FieldName(fieldNumber) + " (number " + fieldNumber + ")" + 
			 	" lastField=" + fieldInfos.FieldName(lastFieldNumber) + " (number " + lastFieldNumber + ")" + 
			 	" text=" + System.Text.Encoding.UTF8.GetString(termBytes, 0, termBytesLength) + " lastText=" + System.Text.Encoding.UTF8.GetString(lastTermBytes, 0, lastTermBytesLength));
			
			System.Diagnostics.Debug.Assert(ti.freqPointer >= lastTi.freqPointer, "freqPointer out of order (" + ti.freqPointer + " < " + lastTi.freqPointer + ")");
			System.Diagnostics.Debug.Assert(ti.proxPointer >= lastTi.proxPointer, "proxPointer out of order (" + ti.proxPointer + " < " + lastTi.proxPointer + ")");
			
			if (!isIndex && size % indexInterval == 0)
				other.Add(lastFieldNumber, lastTermBytes, lastTermBytesLength, lastTi); // add an index term
			
			WriteTerm(fieldNumber, termBytes, termBytesLength); // write term
			
			output.WriteVInt(ti.docFreq); // write doc freq
			output.WriteVLong(ti.freqPointer - lastTi.freqPointer); // write pointers
			output.WriteVLong(ti.proxPointer - lastTi.proxPointer);
			
			if (ti.docFreq >= skipInterval)
			{
				output.WriteVInt(ti.skipOffset);
			}
			
			if (isIndex)
			{
				output.WriteVLong(other.output.GetFilePointer() - lastIndexPointer);
				lastIndexPointer = other.output.GetFilePointer(); // write pointer
			}
			
			lastFieldNumber = fieldNumber;
			lastTi.Set(ti);
			size++;
		}
		
		private void  WriteTerm(int fieldNumber, byte[] termBytes, int termBytesLength)
		{
			
			// TODO: UTF16toUTF8 could tell us this prefix
			// Compute prefix in common with last term:
			int start = 0;
			int limit = termBytesLength < lastTermBytesLength?termBytesLength:lastTermBytesLength;
			while (start < limit)
			{
				if (termBytes[start] != lastTermBytes[start])
					break;
				start++;
			}
			
			int length = termBytesLength - start;
			output.WriteVInt(start); // write shared prefix length
			output.WriteVInt(length); // write delta length
			output.WriteBytes(termBytes, start, length); // write delta bytes
			output.WriteVInt(fieldNumber); // write field num
			if (lastTermBytes.Length < termBytesLength)
			{
				byte[] newArray = new byte[(int) (termBytesLength * 1.5)];
				Array.Copy(lastTermBytes, 0, newArray, 0, start);
				lastTermBytes = newArray;
			}
			Array.Copy(termBytes, start, lastTermBytes, start, length);
			lastTermBytesLength = termBytesLength;
		}
		
		/// <summary>Called to complete TermInfos creation. </summary>
		internal void  Close()
		{
			output.Seek(4); // write size after format
			output.WriteLong(size);
			output.Close();
			
			if (!isIndex)
				other.Close();
		}
	}
}
