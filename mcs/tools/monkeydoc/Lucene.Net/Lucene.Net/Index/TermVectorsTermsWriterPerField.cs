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

using OffsetAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.OffsetAttribute;
using Fieldable = Mono.Lucene.Net.Documents.Fieldable;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using UnicodeUtil = Mono.Lucene.Net.Util.UnicodeUtil;

namespace Mono.Lucene.Net.Index
{
	
	sealed class TermVectorsTermsWriterPerField:TermsHashConsumerPerField
	{
		
		internal TermVectorsTermsWriterPerThread perThread;
		internal TermsHashPerField termsHashPerField;
		internal TermVectorsTermsWriter termsWriter;
		internal FieldInfo fieldInfo;
		internal DocumentsWriter.DocState docState;
		internal FieldInvertState fieldState;
		
		internal bool doVectors;
		internal bool doVectorPositions;
		internal bool doVectorOffsets;
		
		internal int maxNumPostings;
		internal OffsetAttribute offsetAttribute = null;
		
		public TermVectorsTermsWriterPerField(TermsHashPerField termsHashPerField, TermVectorsTermsWriterPerThread perThread, FieldInfo fieldInfo)
		{
			this.termsHashPerField = termsHashPerField;
			this.perThread = perThread;
			this.termsWriter = perThread.termsWriter;
			this.fieldInfo = fieldInfo;
			docState = termsHashPerField.docState;
			fieldState = termsHashPerField.fieldState;
		}
		
		internal override int GetStreamCount()
		{
			return 2;
		}
		
		internal override bool Start(Fieldable[] fields, int count)
		{
			doVectors = false;
			doVectorPositions = false;
			doVectorOffsets = false;
			
			for (int i = 0; i < count; i++)
			{
				Fieldable field = fields[i];
				if (field.IsIndexed() && field.IsTermVectorStored())
				{
					doVectors = true;
					doVectorPositions |= field.IsStorePositionWithTermVector();
					doVectorOffsets |= field.IsStoreOffsetWithTermVector();
				}
			}
			
			if (doVectors)
			{
				if (perThread.doc == null)
				{
					perThread.doc = termsWriter.GetPerDoc();
					perThread.doc.docID = docState.docID;
					System.Diagnostics.Debug.Assert(perThread.doc.numVectorFields == 0);
					System.Diagnostics.Debug.Assert(0 == perThread.doc.perDocTvf.Length());
					System.Diagnostics.Debug.Assert(0 == perThread.doc.perDocTvf.GetFilePointer());
				}

                System.Diagnostics.Debug.Assert(perThread.doc.docID == docState.docID);
                if (termsHashPerField.numPostings != 0)
                {
                    // Only necessary if previous doc hit a
                    // non-aborting exception while writing vectors in
                    // this field:
                    termsHashPerField.Reset();
                    perThread.termsHashPerThread.Reset(false);
                }
			}
			
			// TODO: only if needed for performance
			//perThread.postingsCount = 0;
			
			return doVectors;
		}
		
		public void  Abort()
		{
		}
		
		/// <summary>Called once per field per document if term vectors
		/// are enabled, to write the vectors to
		/// RAMOutputStream, which is then quickly flushed to
		/// the real term vectors files in the Directory. 
		/// </summary>
		internal override void  Finish()
		{
			
			System.Diagnostics.Debug.Assert(docState.TestPoint("TermVectorsTermsWriterPerField.finish start"));
			
			int numPostings = termsHashPerField.numPostings;
			
			System.Diagnostics.Debug.Assert(numPostings >= 0);
			
			if (!doVectors || numPostings == 0)
				return ;
			
			if (numPostings > maxNumPostings)
				maxNumPostings = numPostings;
			
			IndexOutput tvf = perThread.doc.perDocTvf;
			
			// This is called once, after inverting all occurences
			// of a given field in the doc.  At this point we flush
			// our hash into the DocWriter.
			
			System.Diagnostics.Debug.Assert(fieldInfo.storeTermVector);
			System.Diagnostics.Debug.Assert(perThread.VectorFieldsInOrder(fieldInfo));
			
			perThread.doc.AddField(termsHashPerField.fieldInfo.number);
			
			RawPostingList[] postings = termsHashPerField.SortPostings();
			
			tvf.WriteVInt(numPostings);
			byte bits = (byte) (0x0);
			if (doVectorPositions)
				bits |= TermVectorsReader.STORE_POSITIONS_WITH_TERMVECTOR;
			if (doVectorOffsets)
				bits |= TermVectorsReader.STORE_OFFSET_WITH_TERMVECTOR;
			tvf.WriteByte(bits);
			
			int encoderUpto = 0;
			int lastTermBytesCount = 0;
			
			ByteSliceReader reader = perThread.vectorSliceReader;
			char[][] charBuffers = perThread.termsHashPerThread.charPool.buffers;
			for (int j = 0; j < numPostings; j++)
			{
				TermVectorsTermsWriter.PostingList posting = (TermVectorsTermsWriter.PostingList) postings[j];
				int freq = posting.freq;
				
				char[] text2 = charBuffers[posting.textStart >> DocumentsWriter.CHAR_BLOCK_SHIFT];
				int start2 = posting.textStart & DocumentsWriter.CHAR_BLOCK_MASK;
				
				// We swap between two encoders to save copying
				// last Term's byte array
				UnicodeUtil.UTF8Result utf8Result = perThread.utf8Results[encoderUpto];
				
				// TODO: we could do this incrementally
				UnicodeUtil.UTF16toUTF8(text2, start2, utf8Result);
				int termBytesCount = utf8Result.length;
				
				// TODO: UTF16toUTF8 could tell us this prefix
				// Compute common prefix between last term and
				// this term
				int prefix = 0;
				if (j > 0)
				{
					byte[] lastTermBytes = perThread.utf8Results[1 - encoderUpto].result;
					byte[] termBytes = perThread.utf8Results[encoderUpto].result;
					while (prefix < lastTermBytesCount && prefix < termBytesCount)
					{
						if (lastTermBytes[prefix] != termBytes[prefix])
							break;
						prefix++;
					}
				}
				encoderUpto = 1 - encoderUpto;
				lastTermBytesCount = termBytesCount;
				
				int suffix = termBytesCount - prefix;
				tvf.WriteVInt(prefix);
				tvf.WriteVInt(suffix);
				tvf.WriteBytes(utf8Result.result, prefix, suffix);
				tvf.WriteVInt(freq);
				
				if (doVectorPositions)
				{
					termsHashPerField.InitReader(reader, posting, 0);
					reader.WriteTo(tvf);
				}
				
				if (doVectorOffsets)
				{
					termsHashPerField.InitReader(reader, posting, 1);
					reader.WriteTo(tvf);
				}
			}
			
			termsHashPerField.Reset();

            // NOTE: we clear, per-field, at the thread level,
            // because term vectors fully write themselves on each
            // field; this saves RAM (eg if large doc has two large
            // fields w/ term vectors on) because we recycle/reuse
            // all RAM after each field:
			perThread.termsHashPerThread.Reset(false);
		}
		
		internal void  ShrinkHash()
		{
			termsHashPerField.ShrinkHash(maxNumPostings);
			maxNumPostings = 0;
		}
		
		internal override void  Start(Fieldable f)
		{
			if (doVectorOffsets)
			{
				offsetAttribute = (OffsetAttribute) fieldState.attributeSource.AddAttribute(typeof(OffsetAttribute));
			}
			else
			{
				offsetAttribute = null;
			}
		}
		
		internal override void  NewTerm(RawPostingList p0)
		{
			
			System.Diagnostics.Debug.Assert(docState.TestPoint("TermVectorsTermsWriterPerField.newTerm start"));
			
			TermVectorsTermsWriter.PostingList p = (TermVectorsTermsWriter.PostingList) p0;
			
			p.freq = 1;
			
			if (doVectorOffsets)
			{
				int startOffset = fieldState.offset + offsetAttribute.StartOffset(); ;
				int endOffset = fieldState.offset + offsetAttribute.EndOffset();
				
				termsHashPerField.WriteVInt(1, startOffset);
				termsHashPerField.WriteVInt(1, endOffset - startOffset);
				p.lastOffset = endOffset;
			}
			
			if (doVectorPositions)
			{
				termsHashPerField.WriteVInt(0, fieldState.position);
				p.lastPosition = fieldState.position;
			}
		}
		
		internal override void  AddTerm(RawPostingList p0)
		{
			
			System.Diagnostics.Debug.Assert(docState.TestPoint("TermVectorsTermsWriterPerField.addTerm start"));
			
			TermVectorsTermsWriter.PostingList p = (TermVectorsTermsWriter.PostingList) p0;
			p.freq++;
			
			if (doVectorOffsets)
			{
				int startOffset = fieldState.offset + offsetAttribute.StartOffset(); ;
				int endOffset = fieldState.offset + offsetAttribute.EndOffset();
				
				termsHashPerField.WriteVInt(1, startOffset - p.lastOffset);
				termsHashPerField.WriteVInt(1, endOffset - startOffset);
				p.lastOffset = endOffset;
			}
			
			if (doVectorPositions)
			{
				termsHashPerField.WriteVInt(0, fieldState.position - p.lastPosition);
				p.lastPosition = fieldState.position;
			}
		}
		
		internal override void  SkippingLongTerm()
		{
		}
	}
}
