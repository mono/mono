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
using StopFilter = Monodoc.Lucene.Net.Analysis.StopFilter;
using TokenStream = Monodoc.Lucene.Net.Analysis.TokenStream;
namespace Monodoc.Lucene.Net.Analysis.RU
{
	
	/// <summary> Analyzer for Russian language. Supports an external list of stopwords (words that
	/// will not be indexed at all).
	/// A default set of stopwords is used unless an alternative list is specified.
	/// 
	/// </summary>
	/// <author>   Boris Okner, b.okner@rogers.com
	/// </author>
	/// <version>  $Id: RussianAnalyzer.java,v 1.7 2004/03/29 22:48:01 cutting Exp $
	/// </version>
	public sealed class RussianAnalyzer : Analyzer
	{
		// letters
		private static char A = (char) (0);
		private static char B = (char) (1);
		private static char V = (char) (2);
		private static char G = (char) (3);
		private static char D = (char) (4);
		private static char E = (char) (5);
		private static char ZH = (char) (6);
		private static char Z = (char) (7);
		private static char I = (char) (8);
		private static char I_ = (char) (9);
		private static char K = (char) (10);
		private static char L = (char) (11);
		private static char M = (char) (12);
		private static char N = (char) (13);
		private static char O = (char) (14);
		private static char P = (char) (15);
		private static char R = (char) (16);
		private static char S = (char) (17);
		private static char T = (char) (18);
		private static char U = (char) (19);
		private static char F = (char) (20);
		private static char X = (char) (21);
		private static char TS = (char) (22);
		private static char CH = (char) (23);
		private static char SH = (char) (24);
		private static char SHCH = (char) (25);
		private static char HARD = (char) (26);
		private static char Y = (char) (27);
		private static char SOFT = (char) (28);
		private static char AE = (char) (29);
		private static char IU = (char) (30);
		private static char IA = (char) (31);
		
		/// <summary> List of typical Russian stopwords.</summary>
		private static char[][] RUSSIAN_STOP_WORDS = new char[][]{new char[]{A}, new char[]{B, E, Z}, new char[]{B, O, L, E, E}, new char[]{B, Y}, new char[]{B, Y, L}, new char[]{B, Y, L, A}, new char[]{B, Y, L, I}, new char[]{B, Y, L, O}, new char[]{B, Y, T, SOFT}, new char[]{V}, new char[]{V, A, M}, new char[]{V, A, S}, new char[]{V, E, S, SOFT}, new char[]{V, O}, new char[]{V, O, T}, new char[]{V, S, E}, new char[]{V, S, E, G, O}, new char[]{V, S, E, X}, new char[]{V, Y}, new char[]{G, D, E}, new char[]{D, A}, new char[]{D, A, ZH, E}, new char[]{D, L, IA}, new char[]{D, O}, new char[]{E, G, O}, new char[]{E, E}, new char[]{E, I_}, new char[]{E, IU}, new char[]{E, S, L, I}, new char[]{E, S, T, SOFT}, new char[]{E, SHCH, E}, new char[]{ZH, E}, new char[]{Z, A}, new char[]{Z, D, E, S, SOFT}, new char[]{I}, new char[]{I, Z}, new char[]{I, L, I}, new char[]{I, M}, new char[]{I, X}, new char[]{K}, new char[]{K, A, K}, new char[]{K, O}, new char[]{K, O, G, D, A}, new char[]{K, T, O}, new char[]{L, I}, new char[]{L, I, B, O}, new char[]{M, N, E}, new char[]{M, O, ZH, E, T}, new char[]{M, Y}, new char[]{N, A}, new char[]{N, A, D, O}, new char[]{N, A, SH}, new char[]{N, E}, new char[]{N, E, G, O}, new char[]{N, E, E}, new char[]{N, E, T}, new char[]{N, I}, new char[]{N, I, X}, new char[]{N, O}, new char[]{N, U}, new char[]{O}, new char[]{O, B}, new char[]{O, D, N, A, K, O}, new char[]{O, N}, new char[]{O, N, A}, new char[]{O, N, I}, new char[]{O, N, O}, new char[]{O, T}, new char[]{O, CH, E, N, SOFT}, new char[]{P, O}, new char[]{P, O, D}, new char[]{P, R, I}, new char[]{S}, new char[]{S, O}, new char[]{T, A, K}, new char[]{T, A, K, ZH, E}, new char[]{T, A, K, O, I_}, new char[]{T, A, M}, new char[]{T, E}, new char[]{T, E, M}, new char[]{T, O}, new char[]{T, O, G, O}, new char[]{T, O, ZH, E}, new char[]{T, O, I_}, new char[]{T, O, L, SOFT, K, O}, new char[]{T, O, M}, new char[]{T, Y}, new char[]{U}, new char[]{U, ZH, E}, new char[]{X, O, T, IA}, new char[]{CH, E, G, O}, new char[]{CH, E, I_}, new char[]{CH, E, M}, 
			new char[]{CH, T, O}, new char[]{CH, T, O, B, Y}, new char[]{CH, SOFT, E}, new char[]{CH, SOFT, IA}, new char[]{AE, T, A}, new char[]{AE, T, I}, new char[]{AE, T, O}, new char[]{IA}};
		
		/// <summary> Contains the stopwords used with the StopFilter.</summary>
		private System.Collections.Hashtable stopSet = new System.Collections.Hashtable();
		
		/// <summary> Charset for Russian letters.
		/// Represents encoding for 32 lowercase Russian letters.
		/// Predefined charsets can be taken from RussianCharSets class
		/// </summary>
		private char[] charset;
		
		
		public RussianAnalyzer()
		{
			charset = RussianCharsets.UnicodeRussian;
			stopSet = StopFilter.MakeStopSet(makeStopWords(RussianCharsets.UnicodeRussian));
		}
		
		/// <summary> Builds an analyzer.</summary>
		public RussianAnalyzer(char[] charset)
		{
			this.charset = charset;
			stopSet = StopFilter.MakeStopSet(makeStopWords(charset));
		}
		
		/// <summary> Builds an analyzer with the given stop words.</summary>
		public RussianAnalyzer(char[] charset, System.String[] stopwords)
		{
			this.charset = charset;
			stopSet = StopFilter.MakeStopSet(stopwords);
		}
		
		// Takes russian stop words and translates them to a String array, using
		// the given charset
		private static System.String[] makeStopWords(char[] charset)
		{
			System.String[] res = new System.String[RUSSIAN_STOP_WORDS.Length];
			for (int i = 0; i < res.Length; i++)
			{
				char[] theStopWord = RUSSIAN_STOP_WORDS[i];
				// translate the word,using the charset
				System.Text.StringBuilder theWord = new System.Text.StringBuilder();
				for (int j = 0; j < theStopWord.Length; j++)
				{
					theWord.Append(charset[theStopWord[j]]);
				}
				res[i] = theWord.ToString();
			}
			return res;
		}
		
		/// <summary> Builds an analyzer with the given stop words.</summary>
		/// <todo>  create a Set version of this ctor </todo>
		public RussianAnalyzer(char[] charset, System.Collections.Hashtable stopwords)
		{
			this.charset = charset;
			stopSet = new System.Collections.Hashtable(new System.Collections.Hashtable(stopwords));
		}
		
		/// <summary> Creates a TokenStream which tokenizes all the text in the provided Reader.
		/// 
		/// </summary>
		/// <returns>  A TokenStream build from a RussianLetterTokenizer filtered with
		/// RussianLowerCaseFilter, StopFilter, and RussianStemFilter
		/// </returns>
		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			TokenStream result = new RussianLetterTokenizer(reader, charset);
			result = new RussianLowerCaseFilter(result, charset);
			result = new StopFilter(result, stopSet);
			result = new RussianStemFilter(result, charset);
			return result;
		}
	}
}