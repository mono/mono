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
using Token = Monodoc.Lucene.Net.Analysis.Token;
using TokenFilter = Monodoc.Lucene.Net.Analysis.TokenFilter;
using TokenStream = Monodoc.Lucene.Net.Analysis.TokenStream;
namespace Monodoc.Lucene.Net.Analysis.RU
{
	
	/// <summary> A filter that stems Russian words. The implementation was inspired by GermanStemFilter.
	/// The input should be filtered by RussianLowerCaseFilter before passing it to RussianStemFilter ,
	/// because RussianStemFilter only works  with lowercase part of any "russian" charset.
	/// 
	/// </summary>
	/// <author>     Boris Okner, b.okner@rogers.com
	/// </author>
	/// <version>    $Id: RussianStemFilter.java,v 1.5 2004/03/29 22:48:01 cutting Exp $
	/// </version>
	public sealed class RussianStemFilter : TokenFilter
	{
		/// <summary> The actual token in the input stream.</summary>
		private Token token = null;
		private RussianStemmer stemmer = null;
		
		public RussianStemFilter(TokenStream in_Renamed, char[] charset):base(in_Renamed)
		{
			stemmer = new RussianStemmer(charset);
		}
		
		/// <returns>  Returns the next token in the stream, or null at EOS
		/// </returns>
		public override Token Next()
		{
			if ((token = input.Next()) == null)
			{
				return null;
			}
			else
			{
				System.String s = stemmer.Stem(token.TermText());
				if (!s.Equals(token.TermText()))
				{
					return new Token(s, token.StartOffset(), token.EndOffset(), token.Type());
				}
				return token;
			}
		}
		
		/// <summary> Set a alternative/custom RussianStemmer for this filter.</summary>
		public void  SetStemmer(RussianStemmer stemmer)
		{
			if (stemmer != null)
			{
				this.stemmer = stemmer;
			}
		}
	}
}