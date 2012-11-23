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
	
	sealed class FormatPostingsTermsWriter:FormatPostingsTermsConsumer
	{
		
		internal FormatPostingsFieldsWriter parent;
		internal FormatPostingsDocsWriter docsWriter;
		internal TermInfosWriter termsOut;
		internal FieldInfo fieldInfo;
		
		internal FormatPostingsTermsWriter(SegmentWriteState state, FormatPostingsFieldsWriter parent):base()
		{
			this.parent = parent;
			termsOut = parent.termsOut;
			docsWriter = new FormatPostingsDocsWriter(state, this);
		}
		
		internal void  SetField(FieldInfo fieldInfo)
		{
			this.fieldInfo = fieldInfo;
			docsWriter.SetField(fieldInfo);
		}
		
		internal char[] currentTerm;
		internal int currentTermStart;
		
		internal long freqStart;
		internal long proxStart;
		
		/// <summary>Adds a new term in this field </summary>
		internal override FormatPostingsDocsConsumer AddTerm(char[] text, int start)
		{
			currentTerm = text;
			currentTermStart = start;
			
			// TODO: this is abstraction violation -- ideally this
			// terms writer is not so "invasive", looking for file
			// pointers in its child consumers.
			freqStart = docsWriter.out_Renamed.GetFilePointer();
			if (docsWriter.posWriter.out_Renamed != null)
				proxStart = docsWriter.posWriter.out_Renamed.GetFilePointer();
			
			parent.skipListWriter.ResetSkip();
			
			return docsWriter;
		}
		
		/// <summary>Called when we are done adding terms to this field </summary>
		internal override void  Finish()
		{
		}
		
		internal void  Close()
		{
			docsWriter.Close();
		}
	}
}
