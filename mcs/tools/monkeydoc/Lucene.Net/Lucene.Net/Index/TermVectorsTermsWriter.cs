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

using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using RAMOutputStream = Mono.Lucene.Net.Store.RAMOutputStream;
using ArrayUtil = Mono.Lucene.Net.Util.ArrayUtil;

namespace Mono.Lucene.Net.Index
{
	
	sealed class TermVectorsTermsWriter:TermsHashConsumer
	{
		private void  InitBlock()
		{
			docFreeList = new PerDoc[1];
		}
		
		internal DocumentsWriter docWriter;
		internal TermVectorsWriter termVectorsWriter;
		internal PerDoc[] docFreeList;
		internal int freeCount;
		internal IndexOutput tvx;
		internal IndexOutput tvd;
		internal IndexOutput tvf;
		internal int lastDocID;
		
		public TermVectorsTermsWriter(DocumentsWriter docWriter)
		{
			InitBlock();
			this.docWriter = docWriter;
		}
		
		public override TermsHashConsumerPerThread AddThread(TermsHashPerThread termsHashPerThread)
		{
			return new TermVectorsTermsWriterPerThread(termsHashPerThread, this);
		}
		
		internal override void  CreatePostings(RawPostingList[] postings, int start, int count)
		{
			int end = start + count;
			for (int i = start; i < end; i++)
				postings[i] = new PostingList();
		}
		
		public override void  Flush(System.Collections.IDictionary threadsAndFields, SegmentWriteState state)
		{
			lock (this)
			{
                // NOTE: it's possible that all documents seen in this segment
                // hit non-aborting exceptions, in which case we will
                // not have yet init'd the TermVectorsWriter.  This is
                // actually OK (unlike in the stored fields case)
                // because, although IieldInfos.hasVectors() will return
                // true, the TermVectorsReader gracefully handles
                // non-existence of the term vectors files.
				if (tvx != null)
				{
					
					if (state.numDocsInStore > 0)
					// In case there are some final documents that we
					// didn't see (because they hit a non-aborting exception):
						Fill(state.numDocsInStore - docWriter.GetDocStoreOffset());
					
					tvx.Flush();
					tvd.Flush();
					tvf.Flush();
				}

                System.Collections.IEnumerator it = new System.Collections.Hashtable(threadsAndFields).GetEnumerator();
				while (it.MoveNext())
				{
					System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) it.Current;
					System.Collections.IEnumerator it2 = ((System.Collections.ICollection) entry.Value).GetEnumerator();
					while (it2.MoveNext())
					{
						TermVectorsTermsWriterPerField perField = (TermVectorsTermsWriterPerField) ((System.Collections.DictionaryEntry) it2.Current).Key;
						perField.termsHashPerField.Reset();
						perField.ShrinkHash();
					}
					
					TermVectorsTermsWriterPerThread perThread = (TermVectorsTermsWriterPerThread) entry.Key;
					perThread.termsHashPerThread.Reset(true);
				}
			}
		}
		
		internal override void  CloseDocStore(SegmentWriteState state)
		{
			lock (this)
			{
				if (tvx != null)
				{
					// At least one doc in this run had term vectors
					// enabled
					Fill(state.numDocsInStore - docWriter.GetDocStoreOffset());
					tvx.Close();
					tvf.Close();
					tvd.Close();
					tvx = null;
					System.Diagnostics.Debug.Assert(state.docStoreSegmentName != null);
					System.String fileName = state.docStoreSegmentName + "." + IndexFileNames.VECTORS_INDEX_EXTENSION;
					if (4 + ((long) state.numDocsInStore) * 16 != state.directory.FileLength(fileName))
						throw new System.SystemException("after flush: tvx size mismatch: " + state.numDocsInStore + " docs vs " + state.directory.FileLength(fileName) + " length in bytes of " + fileName + " file exists?=" + state.directory.FileExists(fileName));
					
					SupportClass.CollectionsHelper.AddIfNotContains(state.flushedFiles, state.docStoreSegmentName + "." + IndexFileNames.VECTORS_INDEX_EXTENSION);
                    SupportClass.CollectionsHelper.AddIfNotContains(state.flushedFiles, state.docStoreSegmentName + "." + IndexFileNames.VECTORS_FIELDS_EXTENSION);
					SupportClass.CollectionsHelper.AddIfNotContains(state.flushedFiles, state.docStoreSegmentName + "." + IndexFileNames.VECTORS_DOCUMENTS_EXTENSION);
					
					docWriter.RemoveOpenFile(state.docStoreSegmentName + "." + IndexFileNames.VECTORS_INDEX_EXTENSION);
					docWriter.RemoveOpenFile(state.docStoreSegmentName + "." + IndexFileNames.VECTORS_FIELDS_EXTENSION);
					docWriter.RemoveOpenFile(state.docStoreSegmentName + "." + IndexFileNames.VECTORS_DOCUMENTS_EXTENSION);
					
					lastDocID = 0;
				}
			}
		}
		
		internal int allocCount;
		
		internal PerDoc GetPerDoc()
		{
			lock (this)
			{
				if (freeCount == 0)
				{
					allocCount++;
					if (allocCount > docFreeList.Length)
					{
						// Grow our free list up front to make sure we have
						// enough space to recycle all outstanding PerDoc
						// instances
						System.Diagnostics.Debug.Assert(allocCount == 1 + docFreeList.Length);
						docFreeList = new PerDoc[ArrayUtil.GetNextSize(allocCount)];
					}
					return new PerDoc(this);
				}
				else
					return docFreeList[--freeCount];
			}
		}
		
		/// <summary>Fills in no-term-vectors for all docs we haven't seen
		/// since the last doc that had term vectors. 
		/// </summary>
		internal void  Fill(int docID)
		{
			int docStoreOffset = docWriter.GetDocStoreOffset();
			int end = docID + docStoreOffset;
			if (lastDocID < end)
			{
				long tvfPosition = tvf.GetFilePointer();
				while (lastDocID < end)
				{
					tvx.WriteLong(tvd.GetFilePointer());
					tvd.WriteVInt(0);
					tvx.WriteLong(tvfPosition);
					lastDocID++;
				}
			}
		}
		
		internal void  InitTermVectorsWriter()
		{
			lock (this)
			{
				if (tvx == null)
				{
					
					System.String docStoreSegment = docWriter.GetDocStoreSegment();
					
					if (docStoreSegment == null)
						return ;
					
					System.Diagnostics.Debug.Assert(docStoreSegment != null);
					
					// If we hit an exception while init'ing the term
					// vector output files, we must abort this segment
					// because those files will be in an unknown
					// state:
					tvx = docWriter.directory.CreateOutput(docStoreSegment + "." + IndexFileNames.VECTORS_INDEX_EXTENSION);
					tvd = docWriter.directory.CreateOutput(docStoreSegment + "." + IndexFileNames.VECTORS_DOCUMENTS_EXTENSION);
					tvf = docWriter.directory.CreateOutput(docStoreSegment + "." + IndexFileNames.VECTORS_FIELDS_EXTENSION);
					
					tvx.WriteInt(TermVectorsReader.FORMAT_CURRENT);
					tvd.WriteInt(TermVectorsReader.FORMAT_CURRENT);
					tvf.WriteInt(TermVectorsReader.FORMAT_CURRENT);
					
					docWriter.AddOpenFile(docStoreSegment + "." + IndexFileNames.VECTORS_INDEX_EXTENSION);
					docWriter.AddOpenFile(docStoreSegment + "." + IndexFileNames.VECTORS_FIELDS_EXTENSION);
					docWriter.AddOpenFile(docStoreSegment + "." + IndexFileNames.VECTORS_DOCUMENTS_EXTENSION);
					
					lastDocID = 0;
				}
			}
		}
		
		internal void  FinishDocument(PerDoc perDoc)
		{
			lock (this)
			{
				
				System.Diagnostics.Debug.Assert(docWriter.writer.TestPoint("TermVectorsTermsWriter.finishDocument start"));
				
				InitTermVectorsWriter();
				
				Fill(perDoc.docID);
				
				// Append term vectors to the real outputs:
				tvx.WriteLong(tvd.GetFilePointer());
				tvx.WriteLong(tvf.GetFilePointer());
				tvd.WriteVInt(perDoc.numVectorFields);
				if (perDoc.numVectorFields > 0)
				{
					for (int i = 0; i < perDoc.numVectorFields; i++)
						tvd.WriteVInt(perDoc.fieldNumbers[i]);
					System.Diagnostics.Debug.Assert(0 == perDoc.fieldPointers [0]);
					long lastPos = perDoc.fieldPointers[0];
					for (int i = 1; i < perDoc.numVectorFields; i++)
					{
						long pos = perDoc.fieldPointers[i];
						tvd.WriteVLong(pos - lastPos);
						lastPos = pos;
					}
                    perDoc.perDocTvf.WriteTo(tvf);
					perDoc.numVectorFields = 0;
				}
				
				System.Diagnostics.Debug.Assert(lastDocID == perDoc.docID + docWriter.GetDocStoreOffset());
				
				lastDocID++;
                perDoc.Reset();
				Free(perDoc);
				System.Diagnostics.Debug.Assert(docWriter.writer.TestPoint("TermVectorsTermsWriter.finishDocument end"));
			}
		}
		
		public bool FreeRAM()
		{
			// We don't hold any state beyond one doc, so we don't
			// free persistent RAM here
			return false;
		}
		
		public override void  Abort()
		{
			if (tvx != null)
			{
				try
				{
					tvx.Close();
				}
				catch (System.Exception t)
				{
				}
				tvx = null;
			}
			if (tvd != null)
			{
				try
				{
					tvd.Close();
				}
				catch (System.Exception t)
				{
				}
				tvd = null;
			}
			if (tvf != null)
			{
				try
				{
					tvf.Close();
				}
				catch (System.Exception t)
				{
				}
				tvf = null;
			}
			lastDocID = 0;
		}
		
		internal void  Free(PerDoc doc)
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(freeCount < docFreeList.Length);
				docFreeList[freeCount++] = doc;
			}
		}
		
		internal class PerDoc:DocumentsWriter.DocWriter
		{
			public PerDoc(TermVectorsTermsWriter enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(TermVectorsTermsWriter enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
                buffer = enclosingInstance.docWriter.NewPerDocBuffer();
                perDocTvf = new RAMOutputStream(buffer);
			}
			private TermVectorsTermsWriter enclosingInstance;
			public TermVectorsTermsWriter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			internal DocumentsWriter.PerDocBuffer buffer;
            internal RAMOutputStream perDocTvf;
			internal int numVectorFields;
			
			internal int[] fieldNumbers = new int[1];
			internal long[] fieldPointers = new long[1];
			
			internal void  Reset()
			{
                perDocTvf.Reset();
                buffer.Recycle();
				numVectorFields = 0;
			}
			
			public override void  Abort()
			{
				Reset();
				Enclosing_Instance.Free(this);
			}
			
			internal void  AddField(int fieldNumber)
			{
				if (numVectorFields == fieldNumbers.Length)
				{
					fieldNumbers = ArrayUtil.Grow(fieldNumbers);
					fieldPointers = ArrayUtil.Grow(fieldPointers);
				}
				fieldNumbers[numVectorFields] = fieldNumber;
                fieldPointers[numVectorFields] = perDocTvf.GetFilePointer();
				numVectorFields++;
			}
			
			public override long SizeInBytes()
			{
                return buffer.GetSizeInBytes();
			}
			
			public override void  Finish()
			{
				Enclosing_Instance.FinishDocument(this);
			}
		}
		
		internal sealed class PostingList:RawPostingList
		{
			internal int freq; // How many times this term occurred in the current doc
			internal int lastOffset; // Last offset we saw
			internal int lastPosition; // Last position where this term occurred
		}
		
		internal override int BytesPerPosting()
		{
			return RawPostingList.BYTES_SIZE + 3 * DocumentsWriter.INT_NUM_BYTE;
		}
	}
}
