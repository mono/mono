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
	
	/// <summary>Used by DocumentsWriter to maintain per-thread state.
	/// We keep a separate Posting hash and other state for each
	/// thread and then merge postings hashes from all threads
	/// when writing the segment. 
	/// </summary>
	sealed class DocumentsWriterThreadState
	{
		
		internal bool isIdle = true; // false if this is currently in use by a thread
		internal int numThreads = 1; // Number of threads that share this instance
		internal bool doFlushAfter; // true if we should flush after processing current doc
		internal DocConsumerPerThread consumer;
		internal DocumentsWriter.DocState docState;
		
		internal DocumentsWriter docWriter;
		
		public DocumentsWriterThreadState(DocumentsWriter docWriter)
		{
			this.docWriter = docWriter;
			docState = new DocumentsWriter.DocState();
			docState.maxFieldLength = docWriter.maxFieldLength;
			docState.infoStream = docWriter.infoStream;
			docState.similarity = docWriter.similarity;
			docState.docWriter = docWriter;
			docState.allowMinus1Position = docWriter.writer.GetAllowMinus1Position();
			consumer = docWriter.consumer.AddThread(this);
		}
		
		internal void  DoAfterFlush()
		{
			numThreads = 0;
			doFlushAfter = false;
		}
	}
}
