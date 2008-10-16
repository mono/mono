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
using CharTokenizer = Monodoc.Lucene.Net.Analysis.CharTokenizer;
namespace Monodoc.Lucene.Net.Analysis.RU
{
	
	/// <summary> A RussianLetterTokenizer is a tokenizer that extends LetterTokenizer by additionally looking up letters
	/// in a given "russian charset". The problem with LeterTokenizer is that it uses Character.isLetter() method,
	/// which doesn't know how to detect letters in encodings like CP1252 and KOI8
	/// (well-known problems with 0xD7 and 0xF7 chars)
	/// 
	/// </summary>
	/// <author>   Boris Okner, b.okner@rogers.com
	/// </author>
	/// <version>  $Id: RussianLetterTokenizer.java,v 1.3 2004/03/29 22:48:01 cutting Exp $
	/// </version>
	
	public class RussianLetterTokenizer : CharTokenizer
	{
		/// <summary>Construct a new LetterTokenizer. </summary>
		private char[] charset;
		
		public RussianLetterTokenizer(System.IO.TextReader in_Renamed, char[] charset) : base(in_Renamed)
		{
			this.charset = charset;
		}
		
		/// <summary> Collects only characters which satisfy
		/// {@link Character#isLetter(char)}.
		/// </summary>
		protected internal override bool IsTokenChar(char c)
		{
			if (System.Char.IsLetter(c))
				return true;
			for (int i = 0; i < charset.Length; i++)
			{
				if (c == charset[i])
					return true;
			}
			return false;
		}
	}
}