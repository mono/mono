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

using ArrayUtil = Mono.Lucene.Net.Util.ArrayUtil;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>This is just a "splitter" class: it lets you wrap two
	/// DocFieldConsumer instances as a single consumer. 
	/// </summary>
	
	sealed class DocFieldConsumers:DocFieldConsumer
	{
		private void  InitBlock()
		{
			docFreeList = new PerDoc[1];
		}
		internal DocFieldConsumer one;
		internal DocFieldConsumer two;
		
		public DocFieldConsumers(DocFieldConsumer one, DocFieldConsumer two)
		{
			InitBlock();
			this.one = one;
			this.two = two;
		}
		
		internal override void  SetFieldInfos(FieldInfos fieldInfos)
		{
			base.SetFieldInfos(fieldInfos);
			one.SetFieldInfos(fieldInfos);
			two.SetFieldInfos(fieldInfos);
		}
		
		public override void  Flush(System.Collections.IDictionary threadsAndFields, SegmentWriteState state)
		{
			
			System.Collections.IDictionary oneThreadsAndFields = new System.Collections.Hashtable();
			System.Collections.IDictionary twoThreadsAndFields = new System.Collections.Hashtable();
			
			System.Collections.IEnumerator it = new System.Collections.Hashtable(threadsAndFields).GetEnumerator();
			while (it.MoveNext())
			{
				
				System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) it.Current;
				
				DocFieldConsumersPerThread perThread = (DocFieldConsumersPerThread) entry.Key;
				
				System.Collections.ICollection fields = (System.Collections.ICollection) entry.Value;
				
				System.Collections.IEnumerator fieldsIt = fields.GetEnumerator();
				System.Collections.Hashtable oneFields = new System.Collections.Hashtable();
				System.Collections.Hashtable twoFields = new System.Collections.Hashtable();
				while (fieldsIt.MoveNext())
				{
					DocFieldConsumersPerField perField = (DocFieldConsumersPerField) fieldsIt.Current;
					SupportClass.CollectionsHelper.AddIfNotContains(oneFields, perField.one);
					SupportClass.CollectionsHelper.AddIfNotContains(twoFields, perField.two);
				}
				
				oneThreadsAndFields[perThread.one] = oneFields;
				twoThreadsAndFields[perThread.two] = twoFields;
			}
			
			
			one.Flush(oneThreadsAndFields, state);
			two.Flush(twoThreadsAndFields, state);
		}
		
		public override void  CloseDocStore(SegmentWriteState state)
		{
			try
			{
				one.CloseDocStore(state);
			}
			finally
			{
				two.CloseDocStore(state);
			}
		}
		
		public override void  Abort()
		{
			try
			{
				one.Abort();
			}
			finally
			{
				two.Abort();
			}
		}
		
		public override bool FreeRAM()
		{
			bool any = one.FreeRAM();
			any |= two.FreeRAM();
			return any;
		}
		
		public override DocFieldConsumerPerThread AddThread(DocFieldProcessorPerThread docFieldProcessorPerThread)
		{
			return new DocFieldConsumersPerThread(docFieldProcessorPerThread, this, one.AddThread(docFieldProcessorPerThread), two.AddThread(docFieldProcessorPerThread));
		}
		
		internal PerDoc[] docFreeList;
		internal int freeCount;
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
		
		internal void  FreePerDoc(PerDoc perDoc)
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(freeCount < docFreeList.Length);
				docFreeList[freeCount++] = perDoc;
			}
		}
		
		internal class PerDoc:DocumentsWriter.DocWriter
		{
			public PerDoc(DocFieldConsumers enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(DocFieldConsumers enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private DocFieldConsumers enclosingInstance;
			public DocFieldConsumers Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			internal DocumentsWriter.DocWriter one;
			internal DocumentsWriter.DocWriter two;
			
			public override long SizeInBytes()
			{
				return one.SizeInBytes() + two.SizeInBytes();
			}
			
			public override void  Finish()
			{
				try
				{
					try
					{
						one.Finish();
					}
					finally
					{
						two.Finish();
					}
				}
				finally
				{
					Enclosing_Instance.FreePerDoc(this);
				}
			}
			
			public override void  Abort()
			{
				try
				{
					try
					{
						one.Abort();
					}
					finally
					{
						two.Abort();
					}
				}
				finally
				{
					Enclosing_Instance.FreePerDoc(this);
				}
			}
		}
	}
}
