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
	
	/// <summary>Transforms the token stream as per the Porter stemming algorithm.
	/// Note: the input to the stemming filter must already be in lower case,
	/// so you will need to use LowerCaseFilter or LowerCaseTokenizer farther
	/// down the Tokenizer chain in order for this to work properly!
	/// <P>
	/// To use this filter with other analyzers, you'll want to write an
	/// Analyzer class that sets up the TokenStream chain as you want it.
	/// To use this with LowerCaseTokenizer, for example, you'd write an
	/// analyzer like this:
	/// <P>
	/// <PRE>
	/// class MyAnalyzer extends Analyzer {
	/// public final TokenStream tokenStream(String fieldName, Reader reader) {
	/// return new PorterStemFilter(new LowerCaseTokenizer(reader));
	/// }
	/// }
	/// </PRE>
	/// </summary>
	public sealed class PorterStemFilter : TokenFilter
	{
		private PorterStemmer stemmer;
		
		public PorterStemFilter(TokenStream in_Renamed) : base(in_Renamed)
		{
			stemmer = new PorterStemmer();
		}
		
		/// <summary>Returns the next input Token, after being stemmed </summary>
		public override Token Next()
		{
			Token token = input.Next();
			if (token == null)
				return null;
			else
			{
				System.String s = stemmer.Stem(token.termText);
				if ((System.Object) s != (System.Object) token.termText)
				// Yes, I mean object reference comparison here
					token.termText = s;
				return token;
			}
		}
	}
}