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
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary>TODO: relax synchro!</summary>
	public class TermVectorsReader
	{
		private FieldInfos fieldInfos;
		
		private InputStream tvx;
		private InputStream tvd;
		private InputStream tvf;
		private int size;
		
		public /*internal*/ TermVectorsReader(Directory d, System.String segment, FieldInfos fieldInfos)
		{
			if (d.FileExists(segment + TermVectorsWriter.TVX_EXTENSION))
			{
				tvx = d.OpenFile(segment + TermVectorsWriter.TVX_EXTENSION);
				CheckValidFormat(tvx);
				tvd = d.OpenFile(segment + TermVectorsWriter.TVD_EXTENSION);
				CheckValidFormat(tvd);
				tvf = d.OpenFile(segment + TermVectorsWriter.TVF_EXTENSION);
				CheckValidFormat(tvf);
				size = (int) tvx.Length() / 8;
			}
			
			this.fieldInfos = fieldInfos;
		}
		
		private void  CheckValidFormat(InputStream in_Renamed)
		{
			int format = in_Renamed.ReadInt();
			if (format > TermVectorsWriter.FORMAT_VERSION)
			{
				throw new System.IO.IOException("Incompatible format version: " + format + " expected " + TermVectorsWriter.FORMAT_VERSION + " or less");
			}
		}
		
		internal virtual void  Close()
		{
			lock (this)
			{
				// why don't we trap the exception and at least make sure that
				// all streams that we can close are closed?
				if (tvx != null)
					tvx.Close();
				if (tvd != null)
					tvd.Close();
				if (tvf != null)
					tvf.Close();
			}
		}
		
		/// <summary> </summary>
		/// <returns> The number of documents in the reader
		/// </returns>
		internal virtual int Size()
		{
			return size;
		}
		
		/// <summary> Retrieve the term vector for the given document and Field</summary>
		/// <param name="docNum">The document number to retrieve the vector for
		/// </param>
		/// <param name="Field">The Field within the document to retrieve
		/// </param>
		/// <returns> The TermFreqVector for the document and Field or null
		/// </returns>
		public /*internal*/ virtual TermFreqVector Get(int docNum, System.String field)
		{
			lock (this)
			{
				// Check if no term vectors are available for this segment at all
				int fieldNumber = fieldInfos.FieldNumber(field);
				TermFreqVector result = null;
				if (tvx != null)
				{
					try
					{
						//We need to account for the FORMAT_SIZE at when seeking in the tvx
						//We don't need to do this in other seeks because we already have the file pointer
						//that was written in another file
						tvx.Seek((docNum * 8L) + TermVectorsWriter.FORMAT_SIZE);
						//System.out.println("TVX Pointer: " + tvx.getFilePointer());
						long position = tvx.ReadLong();
						
						tvd.Seek(position);
						int fieldCount = tvd.ReadVInt();
						//System.out.println("Num Fields: " + fieldCount);
						// There are only a few fields per document. We opt for a full scan
						// rather then requiring that they be ordered. We need to read through
						// all of the fields anyway to get to the tvf pointers.
						int number = 0;
						int found = - 1;
						for (int i = 0; i < fieldCount; i++)
						{
							number += tvd.ReadVInt();
							if (number == fieldNumber)
								found = i;
						}
						
						// This Field, although valid in the segment, was not found in this document
						if (found != - 1)
						{
							// Compute position in the tvf file
							position = 0;
							for (int i = 0; i <= found; i++)
							{
								position += tvd.ReadVLong();
							}
							result = ReadTermVector(field, position);
						}
						else
						{
							//System.out.println("Field not found");
						}
					}
					catch (System.Exception e)
					{
						//System.Console.Out.WriteLine(e.StackTrace);
					}
				}
				else
				{
					System.Console.Out.WriteLine("No tvx file");
				}
				return result;
			}
		}
		
		
		/// <summary>Return all term vectors stored for this document or null if the could not be read in. </summary>
		internal virtual TermFreqVector[] Get(int docNum)
		{
			lock (this)
			{
				TermFreqVector[] result = null;
				// Check if no term vectors are available for this segment at all
				if (tvx != null)
				{
					try
					{
						//We need to offset by
						tvx.Seek((docNum * 8L) + TermVectorsWriter.FORMAT_SIZE);
						long position = tvx.ReadLong();
						
						tvd.Seek(position);
						int fieldCount = tvd.ReadVInt();
						
						// No fields are vectorized for this document
						if (fieldCount != 0)
						{
							int number = 0;
							System.String[] fields = new System.String[fieldCount];
							
							for (int i = 0; i < fieldCount; i++)
							{
								number += tvd.ReadVInt();
								fields[i] = fieldInfos.FieldName(number);
							}
							
							// Compute position in the tvf file
							position = 0;
							long[] tvfPointers = new long[fieldCount];
							for (int i = 0; i < fieldCount; i++)
							{
								position += tvd.ReadVLong();
								tvfPointers[i] = position;
							}
							
							result = ReadTermVectors(fields, tvfPointers);
						}
					}
					catch (System.IO.IOException e)
					{
                        Console.Error.Write(e.StackTrace);
                        Console.Error.Flush();
                    }
				}
				else
				{
					System.Console.Out.WriteLine("No tvx file");
				}
				return result;
			}
		}
		
		
		private SegmentTermVector[] ReadTermVectors(System.String[] fields, long[] tvfPointers)
		{
			SegmentTermVector[] res = new SegmentTermVector[fields.Length];
			for (int i = 0; i < fields.Length; i++)
			{
				res[i] = ReadTermVector(fields[i], tvfPointers[i]);
			}
			return res;
		}
		
		/// <summary> </summary>
		/// <param name="fieldNum">The Field to read in
		/// </param>
		/// <param name="tvfPointer">The pointer within the tvf file where we should start reading
		/// </param>
		/// <returns> The TermVector located at that position
		/// </returns>
		/// <throws>  IOException </throws>
		private SegmentTermVector ReadTermVector(System.String field, long tvfPointer)
		{
			
			// Now read the data from specified position
			//We don't need to offset by the FORMAT here since the pointer already includes the offset
			tvf.Seek(tvfPointer);
			
			int numTerms = tvf.ReadVInt();
			//System.out.println("Num Terms: " + numTerms);
			// If no terms - return a constant empty termvector
			if (numTerms == 0)
				return new SegmentTermVector(field, null, null);
			
			int length = numTerms + tvf.ReadVInt();
			
			System.String[] terms = new System.String[numTerms];
			
			int[] termFreqs = new int[numTerms];
			
			int start = 0;
			int deltaLength = 0;
			int totalLength = 0;
			char[] buffer = new char[]{};
			System.String previousString = "";
			for (int i = 0; i < numTerms; i++)
			{
				start = tvf.ReadVInt();
				deltaLength = tvf.ReadVInt();
				totalLength = start + deltaLength;
				if (buffer.Length < totalLength)
				{
					buffer = new char[totalLength];
					for (int j = 0; j < previousString.Length; j++)
					// copy contents
						buffer[j] = previousString[j];
				}
				tvf.ReadChars(buffer, start, deltaLength);
				terms[i] = new System.String(buffer, 0, totalLength);
				previousString = terms[i];
				termFreqs[i] = tvf.ReadVInt();
			}
			SegmentTermVector tv = new SegmentTermVector(field, terms, termFreqs);
			return tv;
		}
	}
}