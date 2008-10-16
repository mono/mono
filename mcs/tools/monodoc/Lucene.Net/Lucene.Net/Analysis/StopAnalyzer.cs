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
	
	/// <summary>Filters LetterTokenizer with LowerCaseFilter and StopFilter. </summary>
	
	public sealed class StopAnalyzer:Analyzer
	{
		private System.Collections.Hashtable stopWords;
		
		/// <summary>An array containing some common English words that are not usually useful
		/// for searching. 
		/// </summary>
		public static readonly System.String[] ENGLISH_STOP_WORDS = new System.String[]{"a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "s", "such", "t", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with"};
		
		/// <summary>Builds an analyzer which removes words in ENGLISH_STOP_WORDS. </summary>
		public StopAnalyzer()
		{
			stopWords = StopFilter.MakeStopSet(ENGLISH_STOP_WORDS);
		}
		
		/// <summary>Builds an analyzer which removes words in the provided array. </summary>
		public StopAnalyzer(System.String[] stopWords)
		{
			this.stopWords = StopFilter.MakeStopSet(stopWords);
		}
		
		/// <summary>Filters LowerCaseTokenizer with StopFilter. </summary>
		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			return new StopFilter(new LowerCaseTokenizer(reader), stopWords);
		}
	}
}