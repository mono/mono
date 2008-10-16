/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
namespace Monodoc.Lucene.Net.Analysis
{
	
	/// <summary>A WhitespaceTokenizer is a tokenizer that divides text at whitespace.
	/// Adjacent sequences of non-Whitespace characters form tokens. 
	/// </summary>
	
	public class WhitespaceTokenizer:CharTokenizer
	{
		/// <summary>Construct a new WhitespaceTokenizer. </summary>
		public WhitespaceTokenizer(System.IO.TextReader in_Renamed):base(in_Renamed)
		{
		}
		
		/// <summary>Collects only characters which do not satisfy
		/// {@link Character#isWhitespace(char)}.
		/// </summary>
		protected internal override bool IsTokenChar(char c)
		{
			return !System.Char.IsWhiteSpace(c);
		}
	}
}