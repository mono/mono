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
	
	/// <summary> This is a DocConsumer that gathers all fields under the
	/// same name, and calls per-field consumers to process field
	/// by field.  This class doesn't doesn't do any "real" work
	/// of its own: it just forwards the fields to a
	/// DocFieldConsumer.
	/// </summary>
	
	sealed class DocFieldProcessor:DocConsumer
	{
		
		internal DocumentsWriter docWriter;
		internal FieldInfos fieldInfos = new FieldInfos();
		internal DocFieldConsumer consumer;
		internal StoredFieldsWriter fieldsWriter;
		
		public DocFieldProcessor(DocumentsWriter docWriter, DocFieldConsumer consumer)
		{
			this.docWriter = docWriter;
			this.consumer = consumer;
			consumer.SetFieldInfos(fieldInfos);
			fieldsWriter = new StoredFieldsWriter(docWriter, fieldInfos);
		}
		
		public override void  CloseDocStore(SegmentWriteState state)
		{
			consumer.CloseDocStore(state);
			fieldsWriter.CloseDocStore(state);
		}
		
		public override void  Flush(System.Collections.ICollection threads, SegmentWriteState state)
		{
			
			System.Collections.IDictionary childThreadsAndFields = new System.Collections.Hashtable();
			System.Collections.IEnumerator it = threads.GetEnumerator();
			while (it.MoveNext())
			{
				DocFieldProcessorPerThread perThread = (DocFieldProcessorPerThread) ((System.Collections.DictionaryEntry) it.Current).Key;
				childThreadsAndFields[perThread.consumer] = perThread.Fields();
				perThread.TrimFields(state);
			}
			fieldsWriter.Flush(state);
			consumer.Flush(childThreadsAndFields, state);
			
			// Important to save after asking consumer to flush so
			// consumer can alter the FieldInfo* if necessary.  EG,
			// FreqProxTermsWriter does this with
			// FieldInfo.storePayload.
			System.String fileName = state.SegmentFileName(IndexFileNames.FIELD_INFOS_EXTENSION);
			fieldInfos.Write(state.directory, fileName);
			SupportClass.CollectionsHelper.AddIfNotContains(state.flushedFiles, fileName);
		}
		
		public override void  Abort()
		{
			fieldsWriter.Abort();
			consumer.Abort();
		}
		
		public override bool FreeRAM()
		{
			return consumer.FreeRAM();
		}
		
		public override DocConsumerPerThread AddThread(DocumentsWriterThreadState threadState)
		{
			return new DocFieldProcessorPerThread(threadState, this);
		}
	}
}
