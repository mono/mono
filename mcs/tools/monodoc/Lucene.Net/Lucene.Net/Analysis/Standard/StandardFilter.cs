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

using Token = Mono.Lucene.Net.Analysis.Token;
using TokenFilter = Mono.Lucene.Net.Analysis.TokenFilter;
using TokenStream = Mono.Lucene.Net.Analysis.TokenStream;
using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;
using TypeAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TypeAttribute;

namespace Mono.Lucene.Net.Analysis.Standard
{
	
	/// <summary>Normalizes tokens extracted with {@link StandardTokenizer}. </summary>
	
	public sealed class StandardFilter:TokenFilter
	{
		
		
		/// <summary>Construct filtering <i>in</i>. </summary>
		public StandardFilter(TokenStream in_Renamed):base(in_Renamed)
		{
			termAtt = (TermAttribute) AddAttribute(typeof(TermAttribute));
			typeAtt = (TypeAttribute) AddAttribute(typeof(TypeAttribute));
		}
		
		private static readonly System.String APOSTROPHE_TYPE;
		private static readonly System.String ACRONYM_TYPE;
		
		// this filters uses attribute type
		private TypeAttribute typeAtt;
		private TermAttribute termAtt;
		
		/// <summary>Returns the next token in the stream, or null at EOS.
		/// <p/>Removes <tt>'s</tt> from the end of words.
		/// <p/>Removes dots from acronyms.
		/// </summary>
		public override bool IncrementToken()
		{
			if (!input.IncrementToken())
			{
				return false;
			}
			
			char[] buffer = termAtt.TermBuffer();
			int bufferLength = termAtt.TermLength();
			System.String type = typeAtt.Type();
			
			if ((System.Object) type == (System.Object) APOSTROPHE_TYPE && bufferLength >= 2 && buffer[bufferLength - 2] == '\'' && (buffer[bufferLength - 1] == 's' || buffer[bufferLength - 1] == 'S'))
			{
				// Strip last 2 characters off
				termAtt.SetTermLength(bufferLength - 2);
			}
			else if ((System.Object) type == (System.Object) ACRONYM_TYPE)
			{
				// remove dots
				int upto = 0;
				for (int i = 0; i < bufferLength; i++)
				{
					char c = buffer[i];
					if (c != '.')
						buffer[upto++] = c;
				}
				termAtt.SetTermLength(upto);
			}
			
			return true;
		}
		static StandardFilter()
		{
			APOSTROPHE_TYPE = StandardTokenizerImpl.TOKEN_TYPES[StandardTokenizerImpl.APOSTROPHE];
			ACRONYM_TYPE = StandardTokenizerImpl.TOKEN_TYPES[StandardTokenizerImpl.ACRONYM];
		}
	}
}
