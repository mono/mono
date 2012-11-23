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

using IndexInput = Mono.Lucene.Net.Store.IndexInput;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using UnicodeUtil = Mono.Lucene.Net.Util.UnicodeUtil;

namespace Mono.Lucene.Net.Index
{
	
	sealed class FreqProxTermsWriter:TermsHashConsumer
	{
		
		public override TermsHashConsumerPerThread AddThread(TermsHashPerThread perThread)
		{
			return new FreqProxTermsWriterPerThread(perThread);
		}
		
		internal override void  CreatePostings(RawPostingList[] postings, int start, int count)
		{
			int end = start + count;
			for (int i = start; i < end; i++)
				postings[i] = new PostingList();
		}
		
		private static int compareText(char[] text1, int pos1, char[] text2, int pos2)
		{
			while (true)
			{
				char c1 = text1[pos1++];
				char c2 = text2[pos2++];
				if (c1 != c2)
				{
					if (0xffff == c2)
						return 1;
					else if (0xffff == c1)
						return - 1;
					else
						return c1 - c2;
				}
				else if (0xffff == c1)
					return 0;
			}
		}
		
		internal override void  CloseDocStore(SegmentWriteState state)
		{
		}
		public override void  Abort()
		{
		}
		
		
		// TODO: would be nice to factor out more of this, eg the
		// FreqProxFieldMergeState, and code to visit all Fields
		// under the same FieldInfo together, up into TermsHash*.
		// Other writers would presumably share alot of this...
		
		public override void  Flush(System.Collections.IDictionary threadsAndFields, SegmentWriteState state)
		{
			
			// Gather all FieldData's that have postings, across all
			// ThreadStates
			System.Collections.ArrayList allFields = new System.Collections.ArrayList();

            System.Collections.IEnumerator it = new System.Collections.Hashtable(threadsAndFields).GetEnumerator();
			while (it.MoveNext())
			{
				
				System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) it.Current;
				
				System.Collections.ICollection fields = (System.Collections.ICollection) entry.Value;
				
				System.Collections.IEnumerator fieldsIt = fields.GetEnumerator();
				
				while (fieldsIt.MoveNext())
				{
					FreqProxTermsWriterPerField perField = (FreqProxTermsWriterPerField) ((System.Collections.DictionaryEntry) fieldsIt.Current).Key;
					if (perField.termsHashPerField.numPostings > 0)
						allFields.Add(perField);
				}
			}
			
			// Sort by field name
            allFields.Sort();
			int numAllFields = allFields.Count;
			
			// TODO: allow Lucene user to customize this consumer:
			FormatPostingsFieldsConsumer consumer = new FormatPostingsFieldsWriter(state, fieldInfos);
			/*
			Current writer chain:
			FormatPostingsFieldsConsumer
			-> IMPL: FormatPostingsFieldsWriter
			-> FormatPostingsTermsConsumer
			-> IMPL: FormatPostingsTermsWriter
			-> FormatPostingsDocConsumer
			-> IMPL: FormatPostingsDocWriter
			-> FormatPostingsPositionsConsumer
			-> IMPL: FormatPostingsPositionsWriter
			*/
			
			int start = 0;
			while (start < numAllFields)
			{
				FieldInfo fieldInfo = ((FreqProxTermsWriterPerField) allFields[start]).fieldInfo;
				System.String fieldName = fieldInfo.name;
				
				int end = start + 1;
				while (end < numAllFields && ((FreqProxTermsWriterPerField) allFields[end]).fieldInfo.name.Equals(fieldName))
					end++;
				
				FreqProxTermsWriterPerField[] fields = new FreqProxTermsWriterPerField[end - start];
				for (int i = start; i < end; i++)
				{
					fields[i - start] = (FreqProxTermsWriterPerField) allFields[i];
					
					// Aggregate the storePayload as seen by the same
					// field across multiple threads
					fieldInfo.storePayloads |= fields[i - start].hasPayloads;
				}
				
				// If this field has postings then add them to the
				// segment
				AppendPostings(fields, consumer);
				
				for (int i = 0; i < fields.Length; i++)
				{
					TermsHashPerField perField = fields[i].termsHashPerField;
					int numPostings = perField.numPostings;
					perField.Reset();
					perField.ShrinkHash(numPostings);
					fields[i].Reset();
				}
				
				start = end;
			}

            it = new System.Collections.Hashtable(threadsAndFields).GetEnumerator();
			while (it.MoveNext())
			{
				System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) it.Current;
				FreqProxTermsWriterPerThread perThread = (FreqProxTermsWriterPerThread) entry.Key;
				perThread.termsHashPerThread.Reset(true);
			}
			
			consumer.Finish();
		}
		
		private byte[] payloadBuffer;
		
		/* Walk through all unique text tokens (Posting
		* instances) found in this field and serialize them
		* into a single RAM segment. */
		internal void  AppendPostings(FreqProxTermsWriterPerField[] fields, FormatPostingsFieldsConsumer consumer)
		{
			
			int numFields = fields.Length;
			
			FreqProxFieldMergeState[] mergeStates = new FreqProxFieldMergeState[numFields];
			
			for (int i = 0; i < numFields; i++)
			{
				FreqProxFieldMergeState fms = mergeStates[i] = new FreqProxFieldMergeState(fields[i]);
				
				System.Diagnostics.Debug.Assert(fms.field.fieldInfo == fields [0].fieldInfo);
				
				// Should always be true
				bool result = fms.NextTerm();
				System.Diagnostics.Debug.Assert(result);
			}
			
			FormatPostingsTermsConsumer termsConsumer = consumer.AddField(fields[0].fieldInfo);
			
			FreqProxFieldMergeState[] termStates = new FreqProxFieldMergeState[numFields];
			
			bool currentFieldOmitTermFreqAndPositions = fields[0].fieldInfo.omitTermFreqAndPositions;
			
			while (numFields > 0)
			{
				
				// Get the next term to merge
				termStates[0] = mergeStates[0];
				int numToMerge = 1;
				
				for (int i = 1; i < numFields; i++)
				{
					char[] text = mergeStates[i].text;
					int textOffset = mergeStates[i].textOffset;
					int cmp = compareText(text, textOffset, termStates[0].text, termStates[0].textOffset);
					
					if (cmp < 0)
					{
						termStates[0] = mergeStates[i];
						numToMerge = 1;
					}
					else if (cmp == 0)
						termStates[numToMerge++] = mergeStates[i];
				}
				
				FormatPostingsDocsConsumer docConsumer = termsConsumer.AddTerm(termStates[0].text, termStates[0].textOffset);
				
				// Now termStates has numToMerge FieldMergeStates
				// which all share the same term.  Now we must
				// interleave the docID streams.
				while (numToMerge > 0)
				{
					
					FreqProxFieldMergeState minState = termStates[0];
					for (int i = 1; i < numToMerge; i++)
						if (termStates[i].docID < minState.docID)
							minState = termStates[i];
					
					int termDocFreq = minState.termFreq;
					
					FormatPostingsPositionsConsumer posConsumer = docConsumer.AddDoc(minState.docID, termDocFreq);
					
					ByteSliceReader prox = minState.prox;
					
					// Carefully copy over the prox + payload info,
					// changing the format to match Lucene's segment
					// format.
					if (!currentFieldOmitTermFreqAndPositions)
					{
						// omitTermFreqAndPositions == false so we do write positions &
						// payload          
						int position = 0;
						for (int j = 0; j < termDocFreq; j++)
						{
							int code = prox.ReadVInt();
							position += (code >> 1);
							
							int payloadLength;
							if ((code & 1) != 0)
							{
								// This position has a payload
								payloadLength = prox.ReadVInt();
								
								if (payloadBuffer == null || payloadBuffer.Length < payloadLength)
									payloadBuffer = new byte[payloadLength];
								
								prox.ReadBytes(payloadBuffer, 0, payloadLength);
							}
							else
								payloadLength = 0;
							
							posConsumer.AddPosition(position, payloadBuffer, 0, payloadLength);
						} //End for
						
						posConsumer.Finish();
					}
					
					if (!minState.NextDoc())
					{
						
						// Remove from termStates
						int upto = 0;
						for (int i = 0; i < numToMerge; i++)
							if (termStates[i] != minState)
								termStates[upto++] = termStates[i];
						numToMerge--;
						System.Diagnostics.Debug.Assert(upto == numToMerge);
						
						// Advance this state to the next term
						
						if (!minState.NextTerm())
						{
							// OK, no more terms, so remove from mergeStates
							// as well
							upto = 0;
							for (int i = 0; i < numFields; i++)
								if (mergeStates[i] != minState)
									mergeStates[upto++] = mergeStates[i];
							numFields--;
							System.Diagnostics.Debug.Assert(upto == numFields);
						}
					}
				}
				
				docConsumer.Finish();
			}
			
			termsConsumer.Finish();
		}
		
		private TermInfo termInfo = new TermInfo(); // minimize consing
		
		internal UnicodeUtil.UTF8Result termsUTF8 = new UnicodeUtil.UTF8Result();
		
		internal void  Files(System.Collections.ICollection files)
		{
		}
		
		internal sealed class PostingList:RawPostingList
		{
			internal int docFreq; // # times this term occurs in the current doc
			internal int lastDocID; // Last docID where this term occurred
			internal int lastDocCode; // Code for prior doc
			internal int lastPosition; // Last position where this term occurred
		}
		
		internal override int BytesPerPosting()
		{
			return RawPostingList.BYTES_SIZE + 4 * DocumentsWriter.INT_NUM_BYTE;
		}
	}
}
