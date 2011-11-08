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

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> CharStream adds {@link #CorrectOffset}
	/// functionality over {@link Reader}.  All Tokenizers accept a
	/// CharStream instead of {@link Reader} as input, which enables
	/// arbitrary character based filtering before tokenization. 
	/// The {@link #CorrectOffset} method fixed offsets to account for
	/// removal or insertion of characters, so that the offsets
	/// reported in the tokens match the character offsets of the
	/// original Reader.
    /// </summary>
	public abstract class CharStream:System.IO.StreamReader
	{
        public CharStream(System.IO.StreamReader reader) : base(reader.BaseStream)
        {
        }
		
		/// <summary> Called by CharFilter(s) and Tokenizer to correct token offset.
		/// 
		/// </summary>
		/// <param name="currentOff">offset as seen in the output
		/// </param>
		/// <returns> corrected offset based on the input
		/// </returns>
		public abstract int CorrectOffset(int currentOff);
	}
}
