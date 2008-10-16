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
	
	/// <summary> Removes stop words from a token stream.</summary>
	
	public sealed class StopFilter : TokenFilter
	{
		
		private System.Collections.Hashtable stopWords;
		
		/// <summary> Constructs a filter which removes words from the input
		/// TokenStream that are named in the array of words.
		/// </summary>
		public StopFilter(TokenStream in_Renamed, System.String[] stopWords) : base(in_Renamed)
		{
			this.stopWords = MakeStopSet(stopWords);
		}
		
		/// <summary> Constructs a filter which removes words from the input
		/// TokenStream that are named in the Hashtable.
		/// 
		/// </summary>
		/// <deprecated> Use {@link #StopFilter(TokenStream, Set)} instead
		/// </deprecated>
		public StopFilter(TokenStream in_Renamed, System.Collections.Hashtable stopTable) : base(in_Renamed)
		{
			stopWords = new System.Collections.Hashtable(new System.Collections.Hashtable(stopTable));
		}
		
		/// <summary> Builds a Hashtable from an array of stop words,
		/// appropriate for passing into the StopFilter constructor.
		/// This permits this table construction to be cached once when
		/// an Analyzer is constructed.
		/// 
		/// </summary>
		/// <deprecated> Use {@link #MakeStopSet(String[])} instead.
		/// </deprecated>
		public static System.Collections.Hashtable MakeStopTable(System.String[] stopWords)
		{
			System.Collections.Hashtable stopTable = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable(stopWords.Length));
			for (int i = 0; i < stopWords.Length; i++)
				stopTable[stopWords[i]] = stopWords[i];
			return stopTable;
		}
		
		/// <summary> Builds a Set from an array of stop words,
		/// appropriate for passing into the StopFilter constructor.
		/// This permits this stopWords construction to be cached once when
		/// an Analyzer is constructed.
		/// </summary>
		public static System.Collections.Hashtable MakeStopSet(System.String[] stopWords)
		{
			System.Collections.Hashtable stopTable = new System.Collections.Hashtable(stopWords.Length);
			for (int i = 0; i < stopWords.Length; i++)
				stopTable.Add(stopWords[i], stopWords[i]);
			return stopTable;
		}
		
		/// <summary> Returns the next input Token whose termText() is not a stop word.</summary>
		public override Token Next()
		{
			// return the first non-stop word found
			for (Token token = input.Next(); token != null; token = input.Next())
				if (!stopWords.Contains(token.termText))
					return token;
			// reached EOS -- return null
			return null;
		}
	}
}