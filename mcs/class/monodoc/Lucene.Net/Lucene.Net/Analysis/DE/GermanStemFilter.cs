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
namespace Monodoc.Lucene.Net.Analysis.DE
{
	
	/// <summary> A filter that stems German words. It supports a table of words that should
	/// not be stemmed at all. The stemmer used can be changed at runtime after the
	/// filter object is created (as long as it is a GermanStemmer).
	/// 
	/// </summary>
	/// <author>     Gerhard Schwarz
	/// </author>
	/// <version>    $Id: GermanStemFilter.java,v 1.8 2004/03/30 15:54:48 otis Exp $
	/// </version>
	public sealed class GermanStemFilter : TokenFilter
	{
		/// <summary> The actual token in the input stream.</summary>
		private Token token = null;
		private GermanStemmer stemmer = null;
		private System.Collections.Hashtable exclusionSet = null;
		
		public GermanStemFilter(TokenStream in_Renamed) : base(in_Renamed)
		{
			stemmer = new GermanStemmer();
		}
		
		/// <summary> Builds a GermanStemFilter that uses an exclusiontable.</summary>
		/// <deprecated> Use {@link #GermanStemFilter(Monodoc.Lucene.Net.Analysis.TokenStream, java.util.Set)} instead.
		/// </deprecated>
		public GermanStemFilter(TokenStream in_Renamed, System.Collections.Hashtable exclusiontable):this(in_Renamed)
		{
			exclusionSet = new System.Collections.Hashtable(new System.Collections.Hashtable(exclusiontable));
		}
		
		/// <returns>  Returns the next token in the stream, or null at EOS
		/// </returns>
		public override Token Next()
		{
			if ((token = input.Next()) == null)
			{
				return null;
			}
			// Check the exclusiontable
			else if (exclusionSet != null && exclusionSet.Contains(token.TermText()))
			{
				return token;
			}
			else
			{
				System.String s = stemmer.Stem(token.TermText());
				// If not stemmed, dont waste the time creating a new token
				if (!s.Equals(token.TermText()))
				{
					return new Token(s, token.StartOffset(), token.EndOffset(), token.Type());
				}
				return token;
			}
		}
		
		/// <summary> Set a alternative/custom GermanStemmer for this filter.</summary>
		public void  SetStemmer(GermanStemmer stemmer)
		{
			if (stemmer != null)
			{
				this.stemmer = stemmer;
			}
		}
		
		/// <summary> Set an alternative exclusion list for this filter.</summary>
		/// <deprecated> Use {@link #SetExclusionSet(java.util.Set)} instead.
		/// </deprecated>
		public void  SetExclusionTable(System.Collections.Hashtable exclusiontable)
		{
			exclusionSet = new System.Collections.Hashtable(new System.Collections.Hashtable(exclusiontable));
		}
		
		/// <summary> Set an alternative exclusion list for this filter.</summary>
		public void  SetExclusionSet(System.Collections.Hashtable exclusionSet)
		{
			this.exclusionSet = exclusionSet;
		}
	}
}