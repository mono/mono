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

namespace Mono.Lucene.Net.Index
{
	
	sealed class TermsHashPerThread:InvertedDocConsumerPerThread
	{
		
		internal TermsHash termsHash;
		internal TermsHashConsumerPerThread consumer;
		internal TermsHashPerThread nextPerThread;
		
		internal CharBlockPool charPool;
		internal IntBlockPool intPool;
		internal ByteBlockPool bytePool;
		internal bool primary;
		internal DocumentsWriter.DocState docState;
		
		internal RawPostingList[] freePostings = new RawPostingList[256];
		internal int freePostingsCount;
		
		public TermsHashPerThread(DocInverterPerThread docInverterPerThread, TermsHash termsHash, TermsHash nextTermsHash, TermsHashPerThread primaryPerThread)
		{
			docState = docInverterPerThread.docState;
			
			this.termsHash = termsHash;
			this.consumer = termsHash.consumer.AddThread(this);
			
			if (nextTermsHash != null)
			{
				// We are primary
				charPool = new CharBlockPool(termsHash.docWriter);
				primary = true;
			}
			else
			{
				charPool = primaryPerThread.charPool;
				primary = false;
			}
			
			intPool = new IntBlockPool(termsHash.docWriter, termsHash.trackAllocations);
			bytePool = new ByteBlockPool(termsHash.docWriter.byteBlockAllocator, termsHash.trackAllocations);
			
			if (nextTermsHash != null)
				nextPerThread = nextTermsHash.AddThread(docInverterPerThread, this);
			else
				nextPerThread = null;
		}
		
		internal override InvertedDocConsumerPerField AddField(DocInverterPerField docInverterPerField, FieldInfo fieldInfo)
		{
			return new TermsHashPerField(docInverterPerField, this, nextPerThread, fieldInfo);
		}
		
		public override void  Abort()
		{
			lock (this)
			{
				Reset(true);
				consumer.Abort();
				if (nextPerThread != null)
					nextPerThread.Abort();
			}
		}
		
		// perField calls this when it needs more postings:
		internal void  MorePostings()
		{
			System.Diagnostics.Debug.Assert(freePostingsCount == 0);
			termsHash.GetPostings(freePostings);
			freePostingsCount = freePostings.Length;
			System.Diagnostics.Debug.Assert(noNullPostings(freePostings, freePostingsCount, "consumer=" + consumer));
		}
		
		private static bool noNullPostings(RawPostingList[] postings, int count, System.String details)
		{
			for (int i = 0; i < count; i++)
				System.Diagnostics.Debug.Assert(postings[i] != null, "postings[" + i + "] of " + count + " is null: " + details);
			return true;
		}
		
		public override void  StartDocument()
		{
			consumer.StartDocument();
			if (nextPerThread != null)
				nextPerThread.consumer.StartDocument();
		}
		
		public override DocumentsWriter.DocWriter FinishDocument()
		{
			DocumentsWriter.DocWriter doc = consumer.FinishDocument();
			
			DocumentsWriter.DocWriter doc2;
			if (nextPerThread != null)
				doc2 = nextPerThread.consumer.FinishDocument();
			else
				doc2 = null;
			if (doc == null)
				return doc2;
			else
			{
				doc.SetNext(doc2);
				return doc;
			}
		}
		
		// Clear all state
		internal void  Reset(bool recyclePostings)
		{
			intPool.Reset();
			bytePool.Reset();
			
			if (primary)
				charPool.Reset();
			
			if (recyclePostings)
			{
				termsHash.RecyclePostings(freePostings, freePostingsCount);
				freePostingsCount = 0;
			}
		}
	}
}
