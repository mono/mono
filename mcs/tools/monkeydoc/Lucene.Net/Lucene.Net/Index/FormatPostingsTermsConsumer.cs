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
	
	/// <summary> NOTE: this API is experimental and will likely change</summary>
	
	abstract class FormatPostingsTermsConsumer
	{
		
		/// <summary>Adds a new term in this field; term ends with U+FFFF
		/// char 
		/// </summary>
		internal abstract FormatPostingsDocsConsumer AddTerm(char[] text, int start);
		
		internal char[] termBuffer;
		internal virtual FormatPostingsDocsConsumer AddTerm(System.String text)
		{
			int len = text.Length;
			if (termBuffer == null || termBuffer.Length < 1 + len)
				termBuffer = new char[ArrayUtil.GetNextSize(1 + len)];
	        for (int i = 0; i < len; i++)
	        {
		        termBuffer[i] = (char) text[i];
	        }
			termBuffer[len] = (char) (0xffff);
			return AddTerm(termBuffer, 0);
		}
		
		/// <summary>Called when we are done adding terms to this field </summary>
		internal abstract void  Finish();
	}
}
