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

using BufferedIndexInput = Mono.Lucene.Net.Store.BufferedIndexInput;
using Directory = Mono.Lucene.Net.Store.Directory;
using IndexInput = Mono.Lucene.Net.Store.IndexInput;

namespace Mono.Lucene.Net.Index
{
	
	/// <version>  $Id: TermVectorsReader.java 687046 2008-08-19 13:01:11Z mikemccand $
	/// </version>
	public class TermVectorsReader : System.ICloneable
	{
		
		// NOTE: if you make a new format, it must be larger than
		// the current format
		internal const int FORMAT_VERSION = 2;
		
		// Changes to speed up bulk merging of term vectors:
		internal const int FORMAT_VERSION2 = 3;
		
		// Changed strings to UTF8 with length-in-bytes not length-in-chars
		internal const int FORMAT_UTF8_LENGTH_IN_BYTES = 4;
		
		// NOTE: always change this if you switch to a new format!
		internal static readonly int FORMAT_CURRENT = FORMAT_UTF8_LENGTH_IN_BYTES;
		
		//The size in bytes that the FORMAT_VERSION will take up at the beginning of each file 
		internal const int FORMAT_SIZE = 4;
		
		internal const byte STORE_POSITIONS_WITH_TERMVECTOR = (byte) (0x1);
		internal const byte STORE_OFFSET_WITH_TERMVECTOR = (byte) (0x2);
		
		private FieldInfos fieldInfos;
		
		private IndexInput tvx;
		private IndexInput tvd;
		private IndexInput tvf;
		private int size;
		private int numTotalDocs;
		
		// The docID offset where our docs begin in the index
		// file.  This will be 0 if we have our own private file.
		private int docStoreOffset;
		
		private int format;
		
		public /*internal*/ TermVectorsReader(Directory d, System.String segment, FieldInfos fieldInfos):this(d, segment, fieldInfos, BufferedIndexInput.BUFFER_SIZE)
		{
		}
		
		internal TermVectorsReader(Directory d, System.String segment, FieldInfos fieldInfos, int readBufferSize):this(d, segment, fieldInfos, readBufferSize, - 1, 0)
		{
		}
		
		internal TermVectorsReader(Directory d, System.String segment, FieldInfos fieldInfos, int readBufferSize, int docStoreOffset, int size)
		{
			bool success = false;
			
			try
			{
                if (d.FileExists(segment + "." + IndexFileNames.VECTORS_INDEX_EXTENSION))
                {
                    tvx = d.OpenInput(segment + "." + IndexFileNames.VECTORS_INDEX_EXTENSION, readBufferSize);
                    format = CheckValidFormat(tvx);
                    tvd = d.OpenInput(segment + "." + IndexFileNames.VECTORS_DOCUMENTS_EXTENSION, readBufferSize);
                    int tvdFormat = CheckValidFormat(tvd);
                    tvf = d.OpenInput(segment + "." + IndexFileNames.VECTORS_FIELDS_EXTENSION, readBufferSize);
                    int tvfFormat = CheckValidFormat(tvf);

                    System.Diagnostics.Debug.Assert(format == tvdFormat);
                    System.Diagnostics.Debug.Assert(format == tvfFormat);

                    if (format >= FORMAT_VERSION2)
                    {
                        System.Diagnostics.Debug.Assert((tvx.Length() - FORMAT_SIZE) % 16 == 0);
                        numTotalDocs = (int)(tvx.Length() >> 4);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert((tvx.Length() - FORMAT_SIZE) % 8 == 0);
                        numTotalDocs = (int)(tvx.Length() >> 3);
                    }

                    if (-1 == docStoreOffset)
                    {
                        this.docStoreOffset = 0;
                        this.size = numTotalDocs;
                        System.Diagnostics.Debug.Assert(size == 0 || numTotalDocs == size);
                    }
                    else
                    {
                        this.docStoreOffset = docStoreOffset;
                        this.size = size;
                        // Verify the file is long enough to hold all of our
                        // docs
                        System.Diagnostics.Debug.Assert(numTotalDocs >= size + docStoreOffset, "numTotalDocs=" + numTotalDocs + " size=" + size + " docStoreOffset=" + docStoreOffset);
                    }
                }
                else
                {
                    // If all documents flushed in a segment had hit
                    // non-aborting exceptions, it's possible that
                    // FieldInfos.hasVectors returns true yet the term
                    // vector files don't exist.
                    format = 0;
                }

				
				this.fieldInfos = fieldInfos;
				success = true;
			}
			finally
			{
				// With lock-less commits, it's entirely possible (and
				// fine) to hit a FileNotFound exception above. In
				// this case, we want to explicitly close any subset
				// of things that were opened so that we don't have to
				// wait for a GC to do so.
				if (!success)
				{
					Close();
				}
			}
		}
		
		// Used for bulk copy when merging
		internal virtual IndexInput GetTvdStream()
		{
			return tvd;
		}
		
		// Used for bulk copy when merging
		internal virtual IndexInput GetTvfStream()
		{
			return tvf;
		}
		
		private void  SeekTvx(int docNum)
		{
			if (format < FORMAT_VERSION2)
				tvx.Seek((docNum + docStoreOffset) * 8L + FORMAT_SIZE);
			else
				tvx.Seek((docNum + docStoreOffset) * 16L + FORMAT_SIZE);
		}
		
		internal virtual bool CanReadRawDocs()
		{
			return format >= FORMAT_UTF8_LENGTH_IN_BYTES;
		}
		
		/// <summary>Retrieve the length (in bytes) of the tvd and tvf
		/// entries for the next numDocs starting with
		/// startDocID.  This is used for bulk copying when
		/// merging segments, if the field numbers are
		/// congruent.  Once this returns, the tvf &amp; tvd streams
		/// are seeked to the startDocID. 
		/// </summary>
		internal void  RawDocs(int[] tvdLengths, int[] tvfLengths, int startDocID, int numDocs)
		{
			
			if (tvx == null)
			{
                for (int i = 0; i < tvdLengths.Length; i++)
                {
                    tvdLengths[i] = 0;
                }
                for (int i = 0; i < tvfLengths.Length; i++)
                {
                    tvfLengths[i] = 0;
                }
				return ;
			}
			
			// SegmentMerger calls canReadRawDocs() first and should
			// not call us if that returns false.
			if (format < FORMAT_VERSION2)
				throw new System.SystemException("cannot read raw docs with older term vector formats");
			
			SeekTvx(startDocID);
			
			long tvdPosition = tvx.ReadLong();
			tvd.Seek(tvdPosition);
			
			long tvfPosition = tvx.ReadLong();
			tvf.Seek(tvfPosition);
			
			long lastTvdPosition = tvdPosition;
			long lastTvfPosition = tvfPosition;
			
			int count = 0;
			while (count < numDocs)
			{
				int docID = docStoreOffset + startDocID + count + 1;
				System.Diagnostics.Debug.Assert(docID <= numTotalDocs);
				if (docID < numTotalDocs)
				{
					tvdPosition = tvx.ReadLong();
					tvfPosition = tvx.ReadLong();
				}
				else
				{
					tvdPosition = tvd.Length();
					tvfPosition = tvf.Length();
					System.Diagnostics.Debug.Assert(count == numDocs - 1);
				}
				tvdLengths[count] = (int) (tvdPosition - lastTvdPosition);
				tvfLengths[count] = (int) (tvfPosition - lastTvfPosition);
				count++;
				lastTvdPosition = tvdPosition;
				lastTvfPosition = tvfPosition;
			}
		}
		
		private int CheckValidFormat(IndexInput in_Renamed)
		{
			int format = in_Renamed.ReadInt();
			if (format > FORMAT_CURRENT)
			{
				throw new CorruptIndexException("Incompatible format version: " + format + " expected " + FORMAT_CURRENT + " or less");
			}
			return format;
		}
		
		internal virtual void  Close()
		{
			// make all effort to close up. Keep the first exception
			// and throw it as a new one.
			System.IO.IOException keep = null;
			if (tvx != null)
				try
				{
					tvx.Close();
				}
				catch (System.IO.IOException e)
				{
					if (keep == null)
						keep = e;
				}
			if (tvd != null)
				try
				{
					tvd.Close();
				}
				catch (System.IO.IOException e)
				{
					if (keep == null)
						keep = e;
				}
			if (tvf != null)
				try
				{
					tvf.Close();
				}
				catch (System.IO.IOException e)
				{
					if (keep == null)
						keep = e;
				}
			if (keep != null)
			{
                throw new System.IO.IOException(keep.StackTrace);
			}
		}
		
		/// <summary> </summary>
		/// <returns> The number of documents in the reader
		/// </returns>
		internal virtual int Size()
		{
			return size;
		}
		
		public virtual void  Get(int docNum, System.String field, TermVectorMapper mapper)
		{
			if (tvx != null)
			{
				int fieldNumber = fieldInfos.FieldNumber(field);
				//We need to account for the FORMAT_SIZE at when seeking in the tvx
				//We don't need to do this in other seeks because we already have the
				// file pointer
				//that was written in another file
				SeekTvx(docNum);
				//System.out.println("TVX Pointer: " + tvx.getFilePointer());
				long tvdPosition = tvx.ReadLong();
				
				tvd.Seek(tvdPosition);
				int fieldCount = tvd.ReadVInt();
				//System.out.println("Num Fields: " + fieldCount);
				// There are only a few fields per document. We opt for a full scan
				// rather then requiring that they be ordered. We need to read through
				// all of the fields anyway to get to the tvf pointers.
				int number = 0;
				int found = - 1;
				for (int i = 0; i < fieldCount; i++)
				{
					if (format >= FORMAT_VERSION)
						number = tvd.ReadVInt();
					else
						number += tvd.ReadVInt();
					
					if (number == fieldNumber)
						found = i;
				}
				
				// This field, although valid in the segment, was not found in this
				// document
				if (found != - 1)
				{
					// Compute position in the tvf file
					long position;
					if (format >= FORMAT_VERSION2)
						position = tvx.ReadLong();
					else
						position = tvd.ReadVLong();
					for (int i = 1; i <= found; i++)
						position += tvd.ReadVLong();
					
					mapper.SetDocumentNumber(docNum);
					ReadTermVector(field, position, mapper);
				}
				else
				{
					//System.out.println("Fieldable not found");
				}
			}
			else
			{
				//System.out.println("No tvx file");
			}
		}
		
		
		
		/// <summary> Retrieve the term vector for the given document and field</summary>
		/// <param name="docNum">The document number to retrieve the vector for
		/// </param>
		/// <param name="field">The field within the document to retrieve
		/// </param>
		/// <returns> The TermFreqVector for the document and field or null if there is no termVector for this field.
		/// </returns>
		/// <throws>  IOException if there is an error reading the term vector files </throws>
		public /*internal*/ virtual TermFreqVector Get(int docNum, System.String field)
		{
			// Check if no term vectors are available for this segment at all
			ParallelArrayTermVectorMapper mapper = new ParallelArrayTermVectorMapper();
			Get(docNum, field, mapper);
			
			return mapper.MaterializeVector();
		}
		
		// Reads the String[] fields; you have to pre-seek tvd to
		// the right point
		private System.String[] ReadFields(int fieldCount)
		{
			int number = 0;
			System.String[] fields = new System.String[fieldCount];
			
			for (int i = 0; i < fieldCount; i++)
			{
				if (format >= FORMAT_VERSION)
					number = tvd.ReadVInt();
				else
					number += tvd.ReadVInt();
				
				fields[i] = fieldInfos.FieldName(number);
			}
			
			return fields;
		}
		
		// Reads the long[] offsets into TVF; you have to pre-seek
		// tvx/tvd to the right point
		private long[] ReadTvfPointers(int fieldCount)
		{
			// Compute position in the tvf file
			long position;
			if (format >= FORMAT_VERSION2)
				position = tvx.ReadLong();
			else
				position = tvd.ReadVLong();
			
			long[] tvfPointers = new long[fieldCount];
			tvfPointers[0] = position;
			
			for (int i = 1; i < fieldCount; i++)
			{
				position += tvd.ReadVLong();
				tvfPointers[i] = position;
			}
			
			return tvfPointers;
		}
		
		/// <summary> Return all term vectors stored for this document or null if the could not be read in.
		/// 
		/// </summary>
		/// <param name="docNum">The document number to retrieve the vector for
		/// </param>
		/// <returns> All term frequency vectors
		/// </returns>
		/// <throws>  IOException if there is an error reading the term vector files  </throws>
		public /*internal*/ virtual TermFreqVector[] Get(int docNum)
		{
			TermFreqVector[] result = null;
			if (tvx != null)
			{
				//We need to offset by
				SeekTvx(docNum);
				long tvdPosition = tvx.ReadLong();
				
				tvd.Seek(tvdPosition);
				int fieldCount = tvd.ReadVInt();
				
				// No fields are vectorized for this document
				if (fieldCount != 0)
				{
					System.String[] fields = ReadFields(fieldCount);
					long[] tvfPointers = ReadTvfPointers(fieldCount);
					result = ReadTermVectors(docNum, fields, tvfPointers);
				}
			}
			else
			{
				//System.out.println("No tvx file");
			}
			return result;
		}
		
		public virtual void  Get(int docNumber, TermVectorMapper mapper)
		{
			// Check if no term vectors are available for this segment at all
			if (tvx != null)
			{
				//We need to offset by
				
				SeekTvx(docNumber);
				long tvdPosition = tvx.ReadLong();
				
				tvd.Seek(tvdPosition);
				int fieldCount = tvd.ReadVInt();
				
				// No fields are vectorized for this document
				if (fieldCount != 0)
				{
					System.String[] fields = ReadFields(fieldCount);
					long[] tvfPointers = ReadTvfPointers(fieldCount);
					mapper.SetDocumentNumber(docNumber);
					ReadTermVectors(fields, tvfPointers, mapper);
				}
			}
			else
			{
				//System.out.println("No tvx file");
			}
		}
		
		
		private SegmentTermVector[] ReadTermVectors(int docNum, System.String[] fields, long[] tvfPointers)
		{
			SegmentTermVector[] res = new SegmentTermVector[fields.Length];
			for (int i = 0; i < fields.Length; i++)
			{
				ParallelArrayTermVectorMapper mapper = new ParallelArrayTermVectorMapper();
				mapper.SetDocumentNumber(docNum);
				ReadTermVector(fields[i], tvfPointers[i], mapper);
				res[i] = (SegmentTermVector) mapper.MaterializeVector();
			}
			return res;
		}
		
		private void  ReadTermVectors(System.String[] fields, long[] tvfPointers, TermVectorMapper mapper)
		{
			for (int i = 0; i < fields.Length; i++)
			{
				ReadTermVector(fields[i], tvfPointers[i], mapper);
			}
		}
		
		
		/// <summary> </summary>
		/// <param name="field">The field to read in
		/// </param>
		/// <param name="tvfPointer">The pointer within the tvf file where we should start reading
		/// </param>
		/// <param name="mapper">The mapper used to map the TermVector
		/// </param>
		/// <throws>  IOException </throws>
		private void  ReadTermVector(System.String field, long tvfPointer, TermVectorMapper mapper)
		{
			
			// Now read the data from specified position
			//We don't need to offset by the FORMAT here since the pointer already includes the offset
			tvf.Seek(tvfPointer);
			
			int numTerms = tvf.ReadVInt();
			//System.out.println("Num Terms: " + numTerms);
			// If no terms - return a constant empty termvector. However, this should never occur!
			if (numTerms == 0)
				return ;
			
			bool storePositions;
			bool storeOffsets;
			
			if (format >= FORMAT_VERSION)
			{
				byte bits = tvf.ReadByte();
				storePositions = (bits & STORE_POSITIONS_WITH_TERMVECTOR) != 0;
				storeOffsets = (bits & STORE_OFFSET_WITH_TERMVECTOR) != 0;
			}
			else
			{
				tvf.ReadVInt();
				storePositions = false;
				storeOffsets = false;
			}
			mapper.SetExpectations(field, numTerms, storeOffsets, storePositions);
			int start = 0;
			int deltaLength = 0;
			int totalLength = 0;
			byte[] byteBuffer;
			char[] charBuffer;
			bool preUTF8 = format < FORMAT_UTF8_LENGTH_IN_BYTES;
			
			// init the buffers
			if (preUTF8)
			{
				charBuffer = new char[10];
				byteBuffer = null;
			}
			else
			{
				charBuffer = null;
				byteBuffer = new byte[20];
			}
			
			for (int i = 0; i < numTerms; i++)
			{
				start = tvf.ReadVInt();
				deltaLength = tvf.ReadVInt();
				totalLength = start + deltaLength;
				
				System.String term;
				
				if (preUTF8)
				{
					// Term stored as java chars
					if (charBuffer.Length < totalLength)
					{
						char[] newCharBuffer = new char[(int) (1.5 * totalLength)];
						Array.Copy(charBuffer, 0, newCharBuffer, 0, start);
						charBuffer = newCharBuffer;
					}
					tvf.ReadChars(charBuffer, start, deltaLength);
					term = new System.String(charBuffer, 0, totalLength);
				}
				else
				{
					// Term stored as utf8 bytes
					if (byteBuffer.Length < totalLength)
					{
						byte[] newByteBuffer = new byte[(int) (1.5 * totalLength)];
						Array.Copy(byteBuffer, 0, newByteBuffer, 0, start);
						byteBuffer = newByteBuffer;
					}
					tvf.ReadBytes(byteBuffer, start, deltaLength);
                    term = System.Text.Encoding.UTF8.GetString(byteBuffer, 0, totalLength);
				}
				int freq = tvf.ReadVInt();
				int[] positions = null;
				if (storePositions)
				{
					//read in the positions
					//does the mapper even care about positions?
					if (mapper.IsIgnoringPositions() == false)
					{
						positions = new int[freq];
						int prevPosition = 0;
						for (int j = 0; j < freq; j++)
						{
							positions[j] = prevPosition + tvf.ReadVInt();
							prevPosition = positions[j];
						}
					}
					else
					{
						//we need to skip over the positions.  Since these are VInts, I don't believe there is anyway to know for sure how far to skip
						//
						for (int j = 0; j < freq; j++)
						{
							tvf.ReadVInt();
						}
					}
				}
				TermVectorOffsetInfo[] offsets = null;
				if (storeOffsets)
				{
					//does the mapper even care about offsets?
					if (mapper.IsIgnoringOffsets() == false)
					{
						offsets = new TermVectorOffsetInfo[freq];
						int prevOffset = 0;
						for (int j = 0; j < freq; j++)
						{
							int startOffset = prevOffset + tvf.ReadVInt();
							int endOffset = startOffset + tvf.ReadVInt();
							offsets[j] = new TermVectorOffsetInfo(startOffset, endOffset);
							prevOffset = endOffset;
						}
					}
					else
					{
						for (int j = 0; j < freq; j++)
						{
							tvf.ReadVInt();
							tvf.ReadVInt();
						}
					}
				}
				mapper.Map(term, freq, offsets, positions);
			}
		}
		
		public virtual System.Object Clone()
		{
			
			TermVectorsReader clone = (TermVectorsReader) base.MemberwiseClone();
			
			// These are null when a TermVectorsReader was created
			// on a segment that did not have term vectors saved
			if (tvx != null && tvd != null && tvf != null)
			{
				clone.tvx = (IndexInput) tvx.Clone();
				clone.tvd = (IndexInput) tvd.Clone();
				clone.tvf = (IndexInput) tvf.Clone();
			}
			
			return clone;
		}
	}
	
	
	/// <summary> Models the existing parallel array structure</summary>
	class ParallelArrayTermVectorMapper:TermVectorMapper
	{
		
		private System.String[] terms;
		private int[] termFreqs;
		private int[][] positions;
		private TermVectorOffsetInfo[][] offsets;
		private int currentPosition;
		private bool storingOffsets;
		private bool storingPositions;
		private System.String field;
		
		public override void  SetExpectations(System.String field, int numTerms, bool storeOffsets, bool storePositions)
		{
			this.field = field;
			terms = new System.String[numTerms];
			termFreqs = new int[numTerms];
			this.storingOffsets = storeOffsets;
			this.storingPositions = storePositions;
			if (storePositions)
				this.positions = new int[numTerms][];
			if (storeOffsets)
				this.offsets = new TermVectorOffsetInfo[numTerms][];
		}
		
		public override void  Map(System.String term, int frequency, TermVectorOffsetInfo[] offsets, int[] positions)
		{
			terms[currentPosition] = term;
			termFreqs[currentPosition] = frequency;
			if (storingOffsets)
			{
				this.offsets[currentPosition] = offsets;
			}
			if (storingPositions)
			{
				this.positions[currentPosition] = positions;
			}
			currentPosition++;
		}
		
		/// <summary> Construct the vector</summary>
		/// <returns> The {@link TermFreqVector} based on the mappings.
		/// </returns>
		public virtual TermFreqVector MaterializeVector()
		{
			SegmentTermVector tv = null;
			if (field != null && terms != null)
			{
				if (storingPositions || storingOffsets)
				{
					tv = new SegmentTermPositionVector(field, terms, termFreqs, positions, offsets);
				}
				else
				{
					tv = new SegmentTermVector(field, terms, termFreqs);
				}
			}
			return tv;
		}
	}
}
