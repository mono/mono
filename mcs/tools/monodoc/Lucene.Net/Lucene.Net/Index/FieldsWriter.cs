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

using CompressionTools = Mono.Lucene.Net.Documents.CompressionTools;
using Document = Mono.Lucene.Net.Documents.Document;
using Fieldable = Mono.Lucene.Net.Documents.Fieldable;
using Directory = Mono.Lucene.Net.Store.Directory;
using IndexInput = Mono.Lucene.Net.Store.IndexInput;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using RAMOutputStream = Mono.Lucene.Net.Store.RAMOutputStream;

namespace Mono.Lucene.Net.Index
{
	
	sealed class FieldsWriter
	{
		internal const byte FIELD_IS_TOKENIZED = (byte) (0x1);
		internal const byte FIELD_IS_BINARY = (byte) (0x2);
		internal const byte FIELD_IS_COMPRESSED = (byte) (0x4);
		
		// Original format
		internal const int FORMAT = 0;
		
		// Changed strings to UTF8
		internal const int FORMAT_VERSION_UTF8_LENGTH_IN_BYTES = 1;
                 
        // Lucene 3.0: Removal of compressed fields: This is only to provide compatibility with 3.0-created indexes
        // new segments always use the FORMAT_CURRENT. As the index format did not change in 3.0, only
        // new stored field files that no longer support compression are marked as such to optimize merging.
        // But 2.9 can still read them.
        internal static int FORMAT_LUCENE_3_0_NO_COMPRESSED_FIELDS = 2;
		
		// NOTE: if you introduce a new format, make it 1 higher
		// than the current one, and always change this if you
		// switch to a new format!
		internal static readonly int FORMAT_CURRENT = FORMAT_VERSION_UTF8_LENGTH_IN_BYTES;
		
		private FieldInfos fieldInfos;
		
		private IndexOutput fieldsStream;
		
		private IndexOutput indexStream;
		
		private bool doClose;
		
		internal FieldsWriter(Directory d, System.String segment, FieldInfos fn)
		{
			fieldInfos = fn;
			
			bool success = false;
			System.String fieldsName = segment + "." + IndexFileNames.FIELDS_EXTENSION;
			try
			{
				fieldsStream = d.CreateOutput(fieldsName);
				fieldsStream.WriteInt(FORMAT_CURRENT);
				success = true;
			}
			finally
			{
				if (!success)
				{
					try
					{
						Close();
					}
					catch (System.Exception t)
					{
						// Suppress so we keep throwing the original exception
					}
					try
					{
						d.DeleteFile(fieldsName);
					}
					catch (System.Exception t)
					{
						// Suppress so we keep throwing the original exception
					}
				}
			}
			
			success = false;
			System.String indexName = segment + "." + IndexFileNames.FIELDS_INDEX_EXTENSION;
			try
			{
				indexStream = d.CreateOutput(indexName);
				indexStream.WriteInt(FORMAT_CURRENT);
				success = true;
			}
			finally
			{
				if (!success)
				{
					try
					{
						Close();
					}
					catch (System.IO.IOException ioe)
					{
					}
					try
					{
						d.DeleteFile(fieldsName);
					}
					catch (System.Exception t)
					{
						// Suppress so we keep throwing the original exception
					}
					try
					{
						d.DeleteFile(indexName);
					}
					catch (System.Exception t)
					{
						// Suppress so we keep throwing the original exception
					}
				}
			}
			
			doClose = true;
		}
		
		internal FieldsWriter(IndexOutput fdx, IndexOutput fdt, FieldInfos fn)
		{
			fieldInfos = fn;
			fieldsStream = fdt;
			indexStream = fdx;
			doClose = false;
		}
		
		internal void  SetFieldsStream(IndexOutput stream)
		{
			this.fieldsStream = stream;
		}
		
		// Writes the contents of buffer into the fields stream
		// and adds a new entry for this document into the index
		// stream.  This assumes the buffer was already written
		// in the correct fields format.
		internal void  FlushDocument(int numStoredFields, RAMOutputStream buffer)
		{
			indexStream.WriteLong(fieldsStream.GetFilePointer());
			fieldsStream.WriteVInt(numStoredFields);
			buffer.WriteTo(fieldsStream);
		}
		
		internal void  SkipDocument()
		{
			indexStream.WriteLong(fieldsStream.GetFilePointer());
			fieldsStream.WriteVInt(0);
		}
		
		internal void  Flush()
		{
			indexStream.Flush();
			fieldsStream.Flush();
		}
		
		internal void  Close()
		{
			if (doClose)
			{
				
				try
				{
					if (fieldsStream != null)
					{
						try
						{
							fieldsStream.Close();
						}
						finally
						{
							fieldsStream = null;
						}
					}
				}
				catch (System.IO.IOException ioe)
				{
					try
					{
						if (indexStream != null)
						{
							try
							{
								indexStream.Close();
							}
							finally
							{
								indexStream = null;
							}
						}
					}
					catch (System.IO.IOException ioe2)
					{
						// Ignore so we throw only first IOException hit
					}
					throw ioe;
				}
				finally
				{
					if (indexStream != null)
					{
						try
						{
							indexStream.Close();
						}
						finally
						{
							indexStream = null;
						}
					}
				}
			}
		}
		
		internal void  WriteField(FieldInfo fi, Fieldable field)
		{
			// if the field as an instanceof FieldsReader.FieldForMerge, we're in merge mode
			// and field.binaryValue() already returns the compressed value for a field
			// with isCompressed()==true, so we disable compression in that case
			bool disableCompression = (field is FieldsReader.FieldForMerge);
			fieldsStream.WriteVInt(fi.number);
			byte bits = 0;
			if (field.IsTokenized())
				bits |= FieldsWriter.FIELD_IS_TOKENIZED;
			if (field.IsBinary())
				bits |= FieldsWriter.FIELD_IS_BINARY;
			if (field.IsCompressed())
				bits |= FieldsWriter.FIELD_IS_COMPRESSED;
			
			fieldsStream.WriteByte(bits);
			
			if (field.IsCompressed())
			{
				// compression is enabled for the current field
				byte[] data;
				int len;
				int offset;
				if (disableCompression)
				{
					// optimized case for merging, the data
					// is already compressed
					data = field.GetBinaryValue();
					System.Diagnostics.Debug.Assert(data != null);
					len = field.GetBinaryLength();
					offset = field.GetBinaryOffset();
				}
				else
				{
					// check if it is a binary field
					if (field.IsBinary())
					{
						data = CompressionTools.Compress(field.GetBinaryValue(), field.GetBinaryOffset(), field.GetBinaryLength());
					}
					else
					{
						byte[] x = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(field.StringValue());
						data = CompressionTools.Compress(x, 0, x.Length);
					}
					len = data.Length;
					offset = 0;
				}
				
				fieldsStream.WriteVInt(len);
				fieldsStream.WriteBytes(data, offset, len);
			}
			else
			{
				// compression is disabled for the current field
				if (field.IsBinary())
				{
					byte[] data;
					int len;
					int offset;
					data = field.GetBinaryValue();
					len = field.GetBinaryLength();
					offset = field.GetBinaryOffset();
					
					fieldsStream.WriteVInt(len);
					fieldsStream.WriteBytes(data, offset, len);
				}
				else
				{
					fieldsStream.WriteString(field.StringValue());
				}
			}
		}
		
		/// <summary>Bulk write a contiguous series of documents.  The
		/// lengths array is the length (in bytes) of each raw
		/// document.  The stream IndexInput is the
		/// fieldsStream from which we should bulk-copy all
		/// bytes. 
		/// </summary>
		internal void  AddRawDocuments(IndexInput stream, int[] lengths, int numDocs)
		{
			long position = fieldsStream.GetFilePointer();
			long start = position;
			for (int i = 0; i < numDocs; i++)
			{
				indexStream.WriteLong(position);
				position += lengths[i];
			}
			fieldsStream.CopyBytes(stream, position - start);
			System.Diagnostics.Debug.Assert(fieldsStream.GetFilePointer() == position);
		}
		
		internal void  AddDocument(Document doc)
		{
			indexStream.WriteLong(fieldsStream.GetFilePointer());
			
			int storedCount = 0;
			System.Collections.IEnumerator fieldIterator = doc.GetFields().GetEnumerator();
			while (fieldIterator.MoveNext())
			{
				Fieldable field = (Fieldable) fieldIterator.Current;
				if (field.IsStored())
					storedCount++;
			}
			fieldsStream.WriteVInt(storedCount);
			
			fieldIterator = doc.GetFields().GetEnumerator();
			while (fieldIterator.MoveNext())
			{
				Fieldable field = (Fieldable) fieldIterator.Current;
				if (field.IsStored())
					WriteField(fieldInfos.FieldInfo(field.Name()), field);
			}
		}
	}
}
