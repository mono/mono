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

using Document = Mono.Lucene.Net.Documents.Document;
using Fieldable = Mono.Lucene.Net.Documents.Fieldable;
using Directory = Mono.Lucene.Net.Store.Directory;
using IndexInput = Mono.Lucene.Net.Store.IndexInput;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using StringHelper = Mono.Lucene.Net.Util.StringHelper;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>Access to the Fieldable Info file that describes document fields and whether or
	/// not they are indexed. Each segment has a separate Fieldable Info file. Objects
	/// of this class are thread-safe for multiple readers, but only one thread can
	/// be adding documents at a time, with no other reader or writer threads
	/// accessing this object.
	/// </summary>
	public sealed class FieldInfos : System.ICloneable
	{
		
		// Used internally (ie not written to *.fnm files) for pre-2.9 files
		public const int FORMAT_PRE = - 1;
		
		// First used in 2.9; prior to 2.9 there was no format header
		public const int FORMAT_START = - 2;
		
		internal static readonly int CURRENT_FORMAT = FORMAT_START;
		
		internal const byte IS_INDEXED = (byte) (0x1);
		internal const byte STORE_TERMVECTOR = (byte) (0x2);
		internal const byte STORE_POSITIONS_WITH_TERMVECTOR = (byte) (0x4);
		internal const byte STORE_OFFSET_WITH_TERMVECTOR = (byte) (0x8);
		internal const byte OMIT_NORMS = (byte) (0x10);
		internal const byte STORE_PAYLOADS = (byte) (0x20);
		internal const byte OMIT_TERM_FREQ_AND_POSITIONS = (byte) (0x40);
		
		private System.Collections.ArrayList byNumber = new System.Collections.ArrayList();
		private System.Collections.Hashtable byName = new System.Collections.Hashtable();
		private int format;
		
		public /*internal*/ FieldInfos()
		{
		}
		
		/// <summary> Construct a FieldInfos object using the directory and the name of the file
		/// IndexInput
		/// </summary>
		/// <param name="d">The directory to open the IndexInput from
		/// </param>
		/// <param name="name">The name of the file to open the IndexInput from in the Directory
		/// </param>
		/// <throws>  IOException </throws>
		public /*internal*/ FieldInfos(Directory d, System.String name)
		{
			IndexInput input = d.OpenInput(name);
			try
			{
				try
				{
					Read(input, name);
				}
				catch (System.IO.IOException ioe)
				{
					if (format == FORMAT_PRE)
					{
						// LUCENE-1623: FORMAT_PRE (before there was a
						// format) may be 2.3.2 (pre-utf8) or 2.4.x (utf8)
						// encoding; retry with input set to pre-utf8
						input.Seek(0);
						input.SetModifiedUTF8StringsMode();
						byNumber.Clear();
						byName.Clear();
						try
						{
							Read(input, name);
						}
						catch (System.Exception t)
						{
							// Ignore any new exception & throw original IOE
							throw ioe;
						}
					}
					else
					{
						// The IOException cannot be caused by
						// LUCENE-1623, so re-throw it
						throw ioe;
					}
				}
			}
			finally
			{
				input.Close();
			}
		}
		
		/// <summary> Returns a deep clone of this FieldInfos instance.</summary>
		public System.Object Clone()
		{
            lock (this)
            {
                FieldInfos fis = new FieldInfos();
                int numField = byNumber.Count;
                for (int i = 0; i < numField; i++)
                {
                    FieldInfo fi = (FieldInfo)((FieldInfo)byNumber[i]).Clone();
                    fis.byNumber.Add(fi);
                    fis.byName[fi.name] = fi;
                }
                return fis;
            }
		}
		
		/// <summary>Adds field info for a Document. </summary>
		public void  Add(Document doc)
		{
			lock (this)
			{
				System.Collections.IList fields = doc.GetFields();
				System.Collections.IEnumerator fieldIterator = fields.GetEnumerator();
				while (fieldIterator.MoveNext())
				{
					Fieldable field = (Fieldable) fieldIterator.Current;
					Add(field.Name(), field.IsIndexed(), field.IsTermVectorStored(), field.IsStorePositionWithTermVector(), field.IsStoreOffsetWithTermVector(), field.GetOmitNorms(), false, field.GetOmitTf());
				}
			}
		}
		
		/// <summary>Returns true if any fields do not omitTermFreqAndPositions </summary>
		internal bool HasProx()
		{
			int numFields = byNumber.Count;
			for (int i = 0; i < numFields; i++)
			{
				FieldInfo fi = FieldInfo(i);
				if (fi.isIndexed && !fi.omitTermFreqAndPositions)
				{
					return true;
				}
			}
			return false;
		}
		
		/// <summary> Add fields that are indexed. Whether they have termvectors has to be specified.
		/// 
		/// </summary>
		/// <param name="names">The names of the fields
		/// </param>
		/// <param name="storeTermVectors">Whether the fields store term vectors or not
		/// </param>
		/// <param name="storePositionWithTermVector">true if positions should be stored.
		/// </param>
		/// <param name="storeOffsetWithTermVector">true if offsets should be stored
		/// </param>
		public void  AddIndexed(System.Collections.ICollection names, bool storeTermVectors, bool storePositionWithTermVector, bool storeOffsetWithTermVector)
		{
			lock (this)
			{
				System.Collections.IEnumerator i = names.GetEnumerator();
				while (i.MoveNext())
				{
					Add((System.String) i.Current, true, storeTermVectors, storePositionWithTermVector, storeOffsetWithTermVector);
				}
			}
		}
		
		/// <summary> Assumes the fields are not storing term vectors.
		/// 
		/// </summary>
		/// <param name="names">The names of the fields
		/// </param>
		/// <param name="isIndexed">Whether the fields are indexed or not
		/// 
		/// </param>
		/// <seealso cref="Add(String, boolean)">
		/// </seealso>
        public void Add(System.Collections.Generic.ICollection<string> names, bool isIndexed)
		{
			lock (this)
			{
				System.Collections.IEnumerator i = names.GetEnumerator();
				while (i.MoveNext())
				{
					Add((System.String) i.Current, isIndexed);
				}
			}
		}
		
		/// <summary> Calls 5 parameter add with false for all TermVector parameters.
		/// 
		/// </summary>
		/// <param name="name">The name of the Fieldable
		/// </param>
		/// <param name="isIndexed">true if the field is indexed
		/// </param>
		/// <seealso cref="Add(String, boolean, boolean, boolean, boolean)">
		/// </seealso>
		public void  Add(System.String name, bool isIndexed)
		{
			lock (this)
			{
				Add(name, isIndexed, false, false, false, false);
			}
		}
		
		/// <summary> Calls 5 parameter add with false for term vector positions and offsets.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="isIndexed"> true if the field is indexed
		/// </param>
		/// <param name="storeTermVector">true if the term vector should be stored
		/// </param>
		public void  Add(System.String name, bool isIndexed, bool storeTermVector)
		{
			lock (this)
			{
				Add(name, isIndexed, storeTermVector, false, false, false);
			}
		}
		
		/// <summary>If the field is not yet known, adds it. If it is known, checks to make
		/// sure that the isIndexed flag is the same as was given previously for this
		/// field. If not - marks it as being indexed.  Same goes for the TermVector
		/// parameters.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="isIndexed">true if the field is indexed
		/// </param>
		/// <param name="storeTermVector">true if the term vector should be stored
		/// </param>
		/// <param name="storePositionWithTermVector">true if the term vector with positions should be stored
		/// </param>
		/// <param name="storeOffsetWithTermVector">true if the term vector with offsets should be stored
		/// </param>
		public void  Add(System.String name, bool isIndexed, bool storeTermVector, bool storePositionWithTermVector, bool storeOffsetWithTermVector)
		{
			lock (this)
			{
				
				Add(name, isIndexed, storeTermVector, storePositionWithTermVector, storeOffsetWithTermVector, false);
			}
		}
		
		/// <summary>If the field is not yet known, adds it. If it is known, checks to make
		/// sure that the isIndexed flag is the same as was given previously for this
		/// field. If not - marks it as being indexed.  Same goes for the TermVector
		/// parameters.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="isIndexed">true if the field is indexed
		/// </param>
		/// <param name="storeTermVector">true if the term vector should be stored
		/// </param>
		/// <param name="storePositionWithTermVector">true if the term vector with positions should be stored
		/// </param>
		/// <param name="storeOffsetWithTermVector">true if the term vector with offsets should be stored
		/// </param>
		/// <param name="omitNorms">true if the norms for the indexed field should be omitted
		/// </param>
		public void  Add(System.String name, bool isIndexed, bool storeTermVector, bool storePositionWithTermVector, bool storeOffsetWithTermVector, bool omitNorms)
		{
			lock (this)
			{
				Add(name, isIndexed, storeTermVector, storePositionWithTermVector, storeOffsetWithTermVector, omitNorms, false, false);
			}
		}
		
		/// <summary>If the field is not yet known, adds it. If it is known, checks to make
		/// sure that the isIndexed flag is the same as was given previously for this
		/// field. If not - marks it as being indexed.  Same goes for the TermVector
		/// parameters.
		/// 
		/// </summary>
		/// <param name="name">The name of the field
		/// </param>
		/// <param name="isIndexed">true if the field is indexed
		/// </param>
		/// <param name="storeTermVector">true if the term vector should be stored
		/// </param>
		/// <param name="storePositionWithTermVector">true if the term vector with positions should be stored
		/// </param>
		/// <param name="storeOffsetWithTermVector">true if the term vector with offsets should be stored
		/// </param>
		/// <param name="omitNorms">true if the norms for the indexed field should be omitted
		/// </param>
		/// <param name="storePayloads">true if payloads should be stored for this field
		/// </param>
		/// <param name="omitTermFreqAndPositions">true if term freqs should be omitted for this field
		/// </param>
		public FieldInfo Add(System.String name, bool isIndexed, bool storeTermVector, bool storePositionWithTermVector, bool storeOffsetWithTermVector, bool omitNorms, bool storePayloads, bool omitTermFreqAndPositions)
		{
			lock (this)
			{
				FieldInfo fi = FieldInfo(name);
				if (fi == null)
				{
					return AddInternal(name, isIndexed, storeTermVector, storePositionWithTermVector, storeOffsetWithTermVector, omitNorms, storePayloads, omitTermFreqAndPositions);
				}
				else
				{
					fi.Update(isIndexed, storeTermVector, storePositionWithTermVector, storeOffsetWithTermVector, omitNorms, storePayloads, omitTermFreqAndPositions);
				}
				return fi;
			}
		}
		
		private FieldInfo AddInternal(System.String name, bool isIndexed, bool storeTermVector, bool storePositionWithTermVector, bool storeOffsetWithTermVector, bool omitNorms, bool storePayloads, bool omitTermFreqAndPositions)
		{
			name = StringHelper.Intern(name);
			FieldInfo fi = new FieldInfo(name, isIndexed, byNumber.Count, storeTermVector, storePositionWithTermVector, storeOffsetWithTermVector, omitNorms, storePayloads, omitTermFreqAndPositions);
			byNumber.Add(fi);
			byName[name] = fi;
			return fi;
		}
		
		public int FieldNumber(System.String fieldName)
		{
			FieldInfo fi = FieldInfo(fieldName);
			return (fi != null)?fi.number:- 1;
		}
		
		public FieldInfo FieldInfo(System.String fieldName)
		{
			return (FieldInfo) byName[fieldName];
		}
		
		/// <summary> Return the fieldName identified by its number.
		/// 
		/// </summary>
		/// <param name="fieldNumber">
		/// </param>
		/// <returns> the fieldName or an empty string when the field
		/// with the given number doesn't exist.
		/// </returns>
		public System.String FieldName(int fieldNumber)
		{
			FieldInfo fi = FieldInfo(fieldNumber);
			return (fi != null)?fi.name:"";
		}
		
		/// <summary> Return the fieldinfo object referenced by the fieldNumber.</summary>
		/// <param name="fieldNumber">
		/// </param>
		/// <returns> the FieldInfo object or null when the given fieldNumber
		/// doesn't exist.
		/// </returns>
		public FieldInfo FieldInfo(int fieldNumber)
		{
			return (fieldNumber >= 0)?(FieldInfo) byNumber[fieldNumber]:null;
		}
		
		public int Size()
		{
			return byNumber.Count;
		}
		
		public bool HasVectors()
		{
			bool hasVectors = false;
			for (int i = 0; i < Size(); i++)
			{
				if (FieldInfo(i).storeTermVector)
				{
					hasVectors = true;
					break;
				}
			}
			return hasVectors;
		}
		
		public void  Write(Directory d, System.String name)
		{
			IndexOutput output = d.CreateOutput(name);
			try
			{
				Write(output);
			}
			finally
			{
				output.Close();
			}
		}
		
		public void  Write(IndexOutput output)
		{
			output.WriteVInt(CURRENT_FORMAT);
			output.WriteVInt(Size());
			for (int i = 0; i < Size(); i++)
			{
				FieldInfo fi = FieldInfo(i);
				byte bits = (byte) (0x0);
				if (fi.isIndexed)
					bits |= IS_INDEXED;
				if (fi.storeTermVector)
					bits |= STORE_TERMVECTOR;
				if (fi.storePositionWithTermVector)
					bits |= STORE_POSITIONS_WITH_TERMVECTOR;
				if (fi.storeOffsetWithTermVector)
					bits |= STORE_OFFSET_WITH_TERMVECTOR;
				if (fi.omitNorms)
					bits |= OMIT_NORMS;
				if (fi.storePayloads)
					bits |= STORE_PAYLOADS;
				if (fi.omitTermFreqAndPositions)
					bits |= OMIT_TERM_FREQ_AND_POSITIONS;
				
				output.WriteString(fi.name);
				output.WriteByte(bits);
			}
		}
		
		private void  Read(IndexInput input, System.String fileName)
		{
			int firstInt = input.ReadVInt();
			
			if (firstInt < 0)
			{
				// This is a real format
				format = firstInt;
			}
			else
			{
				format = FORMAT_PRE;
			}
			
			if (format != FORMAT_PRE & format != FORMAT_START)
			{
				throw new CorruptIndexException("unrecognized format " + format + " in file \"" + fileName + "\"");
			}
			
			int size;
			if (format == FORMAT_PRE)
			{
				size = firstInt;
			}
			else
			{
				size = input.ReadVInt(); //read in the size
			}
			
			for (int i = 0; i < size; i++)
			{
				System.String name = StringHelper.Intern(input.ReadString());
				byte bits = input.ReadByte();
				bool isIndexed = (bits & IS_INDEXED) != 0;
				bool storeTermVector = (bits & STORE_TERMVECTOR) != 0;
				bool storePositionsWithTermVector = (bits & STORE_POSITIONS_WITH_TERMVECTOR) != 0;
				bool storeOffsetWithTermVector = (bits & STORE_OFFSET_WITH_TERMVECTOR) != 0;
				bool omitNorms = (bits & OMIT_NORMS) != 0;
				bool storePayloads = (bits & STORE_PAYLOADS) != 0;
				bool omitTermFreqAndPositions = (bits & OMIT_TERM_FREQ_AND_POSITIONS) != 0;
				
				AddInternal(name, isIndexed, storeTermVector, storePositionsWithTermVector, storeOffsetWithTermVector, omitNorms, storePayloads, omitTermFreqAndPositions);
			}
			
			if (input.GetFilePointer() != input.Length())
			{
				throw new CorruptIndexException("did not read all bytes from file \"" + fileName + "\": read " + input.GetFilePointer() + " vs size " + input.Length());
			}
		}
	}
}
