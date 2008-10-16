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
using Analyzer = Monodoc.Lucene.Net.Analysis.Analyzer;
using LowerCaseFilter = Monodoc.Lucene.Net.Analysis.LowerCaseFilter;
using StopFilter = Monodoc.Lucene.Net.Analysis.StopFilter;
using TokenStream = Monodoc.Lucene.Net.Analysis.TokenStream;
using StandardFilter = Monodoc.Lucene.Net.Analysis.Standard.StandardFilter;
using StandardTokenizer = Monodoc.Lucene.Net.Analysis.Standard.StandardTokenizer;
namespace Monodoc.Lucene.Net.Analysis.DE
{
	
	/// <summary> Analyzer for German language. Supports an external list of stopwords (words that
	/// will not be indexed at all) and an external list of exclusions (word that will
	/// not be stemmed, but indexed).
	/// A default set of stopwords is used unless an alternative list is specified, the
	/// exclusion list is empty by default.
	/// 
	/// </summary>
	/// <author>  Gerhard Schwarz
	/// </author>
	/// <version>  $Id: GermanAnalyzer.java,v 1.16 2004/05/30 20:24:20 otis Exp $
	/// </version>
	public class GermanAnalyzer : Analyzer
	{
		/// <summary> List of typical german stopwords.</summary>
		private System.String[] GERMAN_STOP_WORDS = new System.String[]
            {
                "einer", "eine", "eines", "einem", "einen", "der", "die", 
                "das", "dass", "daß", "du", "er", "sie", "es", "was", "wer", 
                "wie", "wir", "und", "oder", "ohne", "mit", "am", "im", "in", 
                "aus", "auf", "ist", "sein", "war", "wird", "ihr", "ihre", 
                "ihres", "als", "für", "von", "mit", "dich", "dir", "mich", 
                "mir", "mein", "sein", "kein", "durch", "wegen", "wird"
            };
		
		/// <summary> Contains the stopwords used with the StopFilter.</summary>
		private System.Collections.Hashtable stopSet = new System.Collections.Hashtable();
		
		/// <summary> Contains words that should be indexed but not stemmed.</summary>
		private System.Collections.Hashtable exclusionSet = new System.Collections.Hashtable();
		
		/// <summary> Builds an analyzer.</summary>
		public GermanAnalyzer()
		{
			stopSet = StopFilter.MakeStopSet(GERMAN_STOP_WORDS);
		}
		
		/// <summary> Builds an analyzer with the given stop words.</summary>
		public GermanAnalyzer(System.String[] stopwords)
		{
			stopSet = StopFilter.MakeStopSet(stopwords);
		}
		
		/// <summary> Builds an analyzer with the given stop words.</summary>
		public GermanAnalyzer(System.Collections.Hashtable stopwords)
		{
			stopSet = new System.Collections.Hashtable(new System.Collections.Hashtable(stopwords));
		}
		
		/// <summary> Builds an analyzer with the given stop words.</summary>
		public GermanAnalyzer(System.IO.FileInfo stopwords)
		{
			stopSet = WordlistLoader.GetWordSet(stopwords);
		}
		
		/// <summary> Builds an exclusionlist from an array of Strings.</summary>
		public virtual void  SetStemExclusionTable(System.String[] exclusionlist)
		{
			exclusionSet = StopFilter.MakeStopSet(exclusionlist);
		}
		
		/// <summary> Builds an exclusionlist from a Hashtable.</summary>
		public virtual void  SetStemExclusionTable(System.Collections.Hashtable exclusionlist)
		{
			exclusionSet = new System.Collections.Hashtable(new System.Collections.Hashtable(exclusionlist));
		}
		
		/// <summary> Builds an exclusionlist from the words contained in the given file.</summary>
		public virtual void  SetStemExclusionTable(System.IO.FileInfo exclusionlist)
		{
			exclusionSet = WordlistLoader.GetWordSet(exclusionlist);
		}
		
		/// <summary> Creates a TokenStream which tokenizes all the text in the provided Reader.
		/// 
		/// </summary>
		/// <returns> A TokenStream build from a StandardTokenizer filtered with
		/// StandardFilter, LowerCaseFilter, StopFilter, GermanStemFilter
		/// </returns>
		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			TokenStream result = new StandardTokenizer(reader);
			result = new StandardFilter(result);
			result = new LowerCaseFilter(result);
			result = new StopFilter(result, stopSet);
			result = new GermanStemFilter(result, exclusionSet);
			return result;
		}
	}
}