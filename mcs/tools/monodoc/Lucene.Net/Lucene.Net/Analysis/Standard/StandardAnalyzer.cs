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
using Monodoc.Lucene.Net.Analysis;
namespace Monodoc.Lucene.Net.Analysis.Standard
{
	
	/// <summary> Filters {@link StandardTokenizer} with {@link StandardFilter}, {@link
	/// LowerCaseFilter} and {@link StopFilter}.
	/// 
	/// </summary>
	/// <version>  $Id: StandardAnalyzer.java,v 1.8 2004/03/29 22:48:01 cutting Exp $
	/// </version>
	public class StandardAnalyzer : Analyzer
	{
		private System.Collections.Hashtable stopSet;
		
		/// <summary>An array containing some common English words that are usually not
		/// useful for searching. 
		/// </summary>
		public static readonly System.String[] STOP_WORDS;
		
		/// <summary>Builds an analyzer. </summary>
		public StandardAnalyzer():this(STOP_WORDS)
		{
		}
		
		/// <summary>Builds an analyzer with the given stop words. </summary>
		public StandardAnalyzer(System.String[] stopWords)
		{
			stopSet = StopFilter.MakeStopSet(stopWords);
		}
		
		/// <summary>Constructs a {@link StandardTokenizer} filtered by a {@link
		/// StandardFilter}, a {@link LowerCaseFilter} and a {@link StopFilter}. 
		/// </summary>
		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			TokenStream result = new StandardTokenizer(reader);
			result = new StandardFilter(result);
			result = new LowerCaseFilter(result);
			result = new StopFilter(result, stopSet);
			return result;
		}
		static StandardAnalyzer()
		{
			STOP_WORDS = StopAnalyzer.ENGLISH_STOP_WORDS;
		}
	}
}