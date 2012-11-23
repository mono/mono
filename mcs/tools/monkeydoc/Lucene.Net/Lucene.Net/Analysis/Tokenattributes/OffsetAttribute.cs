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

using Attribute = Mono.Lucene.Net.Util.Attribute;

namespace Mono.Lucene.Net.Analysis.Tokenattributes
{
	
	/// <summary> The start and end character offset of a Token. </summary>
	public interface OffsetAttribute:Attribute
	{
		/// <summary>Returns this Token's starting offset, the position of the first character
		/// corresponding to this token in the source text.
		/// Note that the difference between endOffset() and startOffset() may not be
		/// equal to termText.length(), as the term text may have been altered by a
		/// stemmer or some other filter. 
		/// </summary>
		int StartOffset();
		
		
		/// <summary>Set the starting and ending offset.
        /// See StartOffset() and EndOffset()
        /// </summary>
		void  SetOffset(int startOffset, int endOffset);
		
		
		/// <summary>Returns this Token's ending offset, one greater than the position of the
		/// last character corresponding to this token in the source text. The length
		/// of the token in the source text is (endOffset - startOffset). 
		/// </summary>
		int EndOffset();
	}
}
