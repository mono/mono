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

using AttributeSource = Mono.Lucene.Net.Util.AttributeSource;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> LowerCaseTokenizer performs the function of LetterTokenizer
	/// and LowerCaseFilter together.  It divides text at non-letters and converts
	/// them to lower case.  While it is functionally equivalent to the combination
	/// of LetterTokenizer and LowerCaseFilter, there is a performance advantage
	/// to doing the two tasks at once, hence this (redundant) implementation.
	/// <p/>
	/// Note: this does a decent job for most European languages, but does a terrible
	/// job for some Asian languages, where words are not separated by spaces.
	/// </summary>
	public sealed class LowerCaseTokenizer:LetterTokenizer
	{
		/// <summary>Construct a new LowerCaseTokenizer. </summary>
		public LowerCaseTokenizer(System.IO.TextReader in_Renamed):base(in_Renamed)
		{
		}
		
		/// <summary>Construct a new LowerCaseTokenizer using a given {@link AttributeSource}. </summary>
		public LowerCaseTokenizer(AttributeSource source, System.IO.TextReader in_Renamed):base(source, in_Renamed)
		{
		}
		
		/// <summary>Construct a new LowerCaseTokenizer using a given {@link Mono.Lucene.Net.Util.AttributeSource.AttributeFactory}. </summary>
		public LowerCaseTokenizer(AttributeFactory factory, System.IO.TextReader in_Renamed):base(factory, in_Renamed)
		{
		}
		
		/// <summary>Converts char to lower case
		/// {@link Character#toLowerCase(char)}.
		/// </summary>
		protected internal override char Normalize(char c)
		{
			return System.Char.ToLower(c);
		}
	}
}
