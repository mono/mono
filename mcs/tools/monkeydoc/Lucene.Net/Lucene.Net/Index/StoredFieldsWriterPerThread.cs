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

using Fieldable = Mono.Lucene.Net.Documents.Fieldable;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;

namespace Mono.Lucene.Net.Index
{
	
	sealed class StoredFieldsWriterPerThread
	{
		
		internal FieldsWriter localFieldsWriter;
		internal StoredFieldsWriter storedFieldsWriter;
		internal DocumentsWriter.DocState docState;
		
		internal StoredFieldsWriter.PerDoc doc;
		
		public StoredFieldsWriterPerThread(DocumentsWriter.DocState docState, StoredFieldsWriter storedFieldsWriter)
		{
			this.storedFieldsWriter = storedFieldsWriter;
			this.docState = docState;
			localFieldsWriter = new FieldsWriter((IndexOutput) null, (IndexOutput) null, storedFieldsWriter.fieldInfos);
		}
		
		public void  StartDocument()
		{
			if (doc != null)
			{
				// Only happens if previous document hit non-aborting
				// exception while writing stored fields into
				// localFieldsWriter:
				doc.Reset();
				doc.docID = docState.docID;
			}
		}
		
		public void  AddField(Fieldable field, FieldInfo fieldInfo)
		{
			if (doc == null)
			{
				doc = storedFieldsWriter.GetPerDoc();
				doc.docID = docState.docID;
				localFieldsWriter.SetFieldsStream(doc.fdt);
				System.Diagnostics.Debug.Assert(doc.numStoredFields == 0, "doc.numStoredFields=" + doc.numStoredFields);
				System.Diagnostics.Debug.Assert(0 == doc.fdt.Length());
				System.Diagnostics.Debug.Assert(0 == doc.fdt.GetFilePointer());
			}
			
			localFieldsWriter.WriteField(fieldInfo, field);
			System.Diagnostics.Debug.Assert(docState.TestPoint("StoredFieldsWriterPerThread.processFields.writeField"));
			doc.numStoredFields++;
		}
		
		public DocumentsWriter.DocWriter FinishDocument()
		{
			// If there were any stored fields in this doc, doc will
			// be non-null; else it's null.
			try
			{
				return doc;
			}
			finally
			{
				doc = null;
			}
		}
		
		public void  Abort()
		{
			if (doc != null)
			{
				doc.Abort();
				doc = null;
			}
		}
	}
}
