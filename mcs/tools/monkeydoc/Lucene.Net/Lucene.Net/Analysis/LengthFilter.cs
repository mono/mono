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

using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> Removes words that are too long or too short from the stream.
	/// 
	/// 
	/// </summary>
	/// <version>  $Id: LengthFilter.java 807201 2009-08-24 13:22:34Z markrmiller $
	/// </version>
	public sealed class LengthFilter:TokenFilter
	{
		
		internal int min;
		internal int max;
		
		private TermAttribute termAtt;
		
		/// <summary> Build a filter that removes words that are too long or too
		/// short from the text.
		/// </summary>
		public LengthFilter(TokenStream in_Renamed, int min, int max):base(in_Renamed)
		{
			this.min = min;
			this.max = max;
			termAtt = (TermAttribute) AddAttribute(typeof(TermAttribute));
		}
		
		/// <summary> Returns the next input Token whose term() is the right len</summary>
		public override bool IncrementToken()
		{
			// return the first non-stop word found
			while (input.IncrementToken())
			{
				int len = termAtt.TermLength();
				if (len >= min && len <= max)
				{
					return true;
				}
				// note: else we ignore it but should we index each part of it?
			}
			// reached EOS -- return null
			return false;
		}
	}
}
