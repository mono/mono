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

using PayloadAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.PayloadAttribute;
using Fieldable = Mono.Lucene.Net.Documents.Fieldable;

namespace Mono.Lucene.Net.Index
{
	
	// TODO: break into separate freq and prox writers as
	// codecs; make separate container (tii/tis/skip/*) that can
	// be configured as any number of files 1..N
	sealed class FreqProxTermsWriterPerField:TermsHashConsumerPerField, System.IComparable
	{
		
		internal FreqProxTermsWriterPerThread perThread;
		internal TermsHashPerField termsHashPerField;
		internal FieldInfo fieldInfo;
		internal DocumentsWriter.DocState docState;
		internal FieldInvertState fieldState;
		internal bool omitTermFreqAndPositions;
		internal PayloadAttribute payloadAttribute;
		
		public FreqProxTermsWriterPerField(TermsHashPerField termsHashPerField, FreqProxTermsWriterPerThread perThread, FieldInfo fieldInfo)
		{
			this.termsHashPerField = termsHashPerField;
			this.perThread = perThread;
			this.fieldInfo = fieldInfo;
			docState = termsHashPerField.docState;
			fieldState = termsHashPerField.fieldState;
			omitTermFreqAndPositions = fieldInfo.omitTermFreqAndPositions;
		}
		
		internal override int GetStreamCount()
		{
			if (fieldInfo.omitTermFreqAndPositions)
				return 1;
			else
				return 2;
		}
		
		internal override void  Finish()
		{
		}
		
		internal bool hasPayloads;
		
		internal override void  SkippingLongTerm()
		{
		}
		
		public int CompareTo(System.Object other0)
		{
			FreqProxTermsWriterPerField other = (FreqProxTermsWriterPerField) other0;
			return String.CompareOrdinal(fieldInfo.name, other.fieldInfo.name);
		}
		
		internal void  Reset()
		{
			// Record, up front, whether our in-RAM format will be
			// with or without term freqs:
			omitTermFreqAndPositions = fieldInfo.omitTermFreqAndPositions;
			payloadAttribute = null;
		}
		
		internal override bool Start(Fieldable[] fields, int count)
		{
			for (int i = 0; i < count; i++)
				if (fields[i].IsIndexed())
					return true;
			return false;
		}
		
		internal override void  Start(Fieldable f)
		{
			if (fieldState.attributeSource.HasAttribute(typeof(PayloadAttribute)))
			{
				payloadAttribute = (PayloadAttribute) fieldState.attributeSource.GetAttribute(typeof(PayloadAttribute));
			}
			else
			{
				payloadAttribute = null;
			}
		}
		
		internal void  WriteProx(FreqProxTermsWriter.PostingList p, int proxCode)
		{
			Payload payload;
			if (payloadAttribute == null)
			{
				payload = null;
			}
			else
			{
				payload = payloadAttribute.GetPayload();
			}
			
			if (payload != null && payload.length > 0)
			{
				termsHashPerField.WriteVInt(1, (proxCode << 1) | 1);
				termsHashPerField.WriteVInt(1, payload.length);
				termsHashPerField.WriteBytes(1, payload.data, payload.offset, payload.length);
				hasPayloads = true;
			}
			else
				termsHashPerField.WriteVInt(1, proxCode << 1);
			p.lastPosition = fieldState.position;
		}
		
		internal override void  NewTerm(RawPostingList p0)
		{
			// First time we're seeing this term since the last
			// flush
			System.Diagnostics.Debug.Assert(docState.TestPoint("FreqProxTermsWriterPerField.newTerm start"));
			FreqProxTermsWriter.PostingList p = (FreqProxTermsWriter.PostingList) p0;
			p.lastDocID = docState.docID;
			if (omitTermFreqAndPositions)
			{
				p.lastDocCode = docState.docID;
			}
			else
			{
				p.lastDocCode = docState.docID << 1;
				p.docFreq = 1;
				WriteProx(p, fieldState.position);
			}
		}
		
		internal override void  AddTerm(RawPostingList p0)
		{
			
			System.Diagnostics.Debug.Assert(docState.TestPoint("FreqProxTermsWriterPerField.addTerm start"));
			
			FreqProxTermsWriter.PostingList p = (FreqProxTermsWriter.PostingList) p0;
			
			System.Diagnostics.Debug.Assert(omitTermFreqAndPositions || p.docFreq > 0);
			
			if (omitTermFreqAndPositions)
			{
				if (docState.docID != p.lastDocID)
				{
					System.Diagnostics.Debug.Assert(docState.docID > p.lastDocID);
					termsHashPerField.WriteVInt(0, p.lastDocCode);
					p.lastDocCode = docState.docID - p.lastDocID;
					p.lastDocID = docState.docID;
				}
			}
			else
			{
				if (docState.docID != p.lastDocID)
				{
					System.Diagnostics.Debug.Assert(docState.docID > p.lastDocID);
					// Term not yet seen in the current doc but previously
					// seen in other doc(s) since the last flush
					
					// Now that we know doc freq for previous doc,
					// write it & lastDocCode
					if (1 == p.docFreq)
						termsHashPerField.WriteVInt(0, p.lastDocCode | 1);
					else
					{
						termsHashPerField.WriteVInt(0, p.lastDocCode);
						termsHashPerField.WriteVInt(0, p.docFreq);
					}
					p.docFreq = 1;
					p.lastDocCode = (docState.docID - p.lastDocID) << 1;
					p.lastDocID = docState.docID;
					WriteProx(p, fieldState.position);
				}
				else
				{
					p.docFreq++;
					WriteProx(p, fieldState.position - p.lastPosition);
				}
			}
		}
		
		public void  Abort()
		{
		}
	}
}
