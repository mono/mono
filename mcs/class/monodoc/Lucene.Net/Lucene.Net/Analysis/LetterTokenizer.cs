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
	
	/// <summary>A LetterTokenizer is a tokenizer that divides text at non-letters.  That's
	/// to say, it defines tokens as maximal strings of adjacent letters, as defined
	/// by java.lang.Character.isLetter() predicate.
	/// Note: this does a decent job for most European languages, but does a terrible
	/// job for some Asian languages, where words are not separated by spaces. 
	/// </summary>
	
	public class LetterTokenizer : CharTokenizer
	{
		/// <summary>Construct a new LetterTokenizer. </summary>
		public LetterTokenizer(System.IO.TextReader in_Renamed) : base(in_Renamed)
		{
		}
		
		/// <summary>Collects only characters which satisfy
		/// {@link Character#isLetter(char)}.
		/// </summary>
		protected internal override bool IsTokenChar(char c)
		{
			return System.Char.IsLetter(c);
		}
	}
}