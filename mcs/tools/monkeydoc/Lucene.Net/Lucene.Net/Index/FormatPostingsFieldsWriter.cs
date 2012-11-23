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

using Directory = Mono.Lucene.Net.Store.Directory;

namespace Mono.Lucene.Net.Index
{
	
	sealed class FormatPostingsFieldsWriter:FormatPostingsFieldsConsumer
	{
		
		internal Directory dir;
		internal System.String segment;
		internal TermInfosWriter termsOut;
		internal FieldInfos fieldInfos;
		internal FormatPostingsTermsWriter termsWriter;
		internal DefaultSkipListWriter skipListWriter;
		internal int totalNumDocs;
		
		public FormatPostingsFieldsWriter(SegmentWriteState state, FieldInfos fieldInfos):base()
		{
			
			dir = state.directory;
			segment = state.segmentName;
			totalNumDocs = state.numDocs;
			this.fieldInfos = fieldInfos;
			termsOut = new TermInfosWriter(dir, segment, fieldInfos, state.termIndexInterval);
			
			// TODO: this is a nasty abstraction violation (that we
			// peek down to find freqOut/proxOut) -- we need a
			// better abstraction here whereby these child consumers
			// can provide skip data or not
			skipListWriter = new DefaultSkipListWriter(termsOut.skipInterval, termsOut.maxSkipLevels, totalNumDocs, null, null);
			
			SupportClass.CollectionsHelper.AddIfNotContains(state.flushedFiles, state.SegmentFileName(IndexFileNames.TERMS_EXTENSION));
			SupportClass.CollectionsHelper.AddIfNotContains(state.flushedFiles, state.SegmentFileName(IndexFileNames.TERMS_INDEX_EXTENSION));
			
			termsWriter = new FormatPostingsTermsWriter(state, this);
		}
		
		/// <summary>Add a new field </summary>
		internal override FormatPostingsTermsConsumer AddField(FieldInfo field)
		{
			termsWriter.SetField(field);
			return termsWriter;
		}
		
		/// <summary>Called when we are done adding everything. </summary>
		internal override void  Finish()
		{
			termsOut.Close();
			termsWriter.Close();
		}
	}
}
