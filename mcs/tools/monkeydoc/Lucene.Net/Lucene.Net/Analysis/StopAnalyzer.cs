/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
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

using Version = Mono.Lucene.Net.Util.Version;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> Filters {@link LetterTokenizer} with {@link LowerCaseFilter} and
	/// {@link StopFilter}.
	/// 
	/// <a name="version"/>
	/// <p/>
	/// You must specify the required {@link Version} compatibility when creating
	/// StopAnalyzer:
	/// <ul>
	/// <li>As of 2.9, position increments are preserved</li>
	/// </ul>
	/// </summary>
	
	public sealed class StopAnalyzer:Analyzer
	{
		private System.Collections.Hashtable stopWords;
		// @deprecated
        [Obsolete]
		private bool useDefaultStopPositionIncrement;
		private bool enablePositionIncrements;
		
		/// <summary>An array containing some common English words that are not usually useful
		/// for searching. 
		/// </summary>
		/// <deprecated> Use {@link #ENGLISH_STOP_WORDS_SET} instead 
		/// </deprecated>
        [Obsolete("Use ENGLISH_STOP_WORDS_SET instead ")]
		public static readonly System.String[] ENGLISH_STOP_WORDS = new System.String[]{"a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with"};
		
		/// <summary>An unmodifiable set containing some common English words that are not usually useful
		/// for searching.
		/// </summary>
		public static System.Collections.Hashtable ENGLISH_STOP_WORDS_SET;
		
		/// <summary>Builds an analyzer which removes words in
		/// ENGLISH_STOP_WORDS.
		/// </summary>
		/// <deprecated> Use {@link #StopAnalyzer(Version)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version) instead")]
		public StopAnalyzer()
		{
			stopWords = ENGLISH_STOP_WORDS_SET;
			useDefaultStopPositionIncrement = true;
			enablePositionIncrements = false;
		}
		
		/// <summary> Builds an analyzer which removes words in ENGLISH_STOP_WORDS.</summary>
		public StopAnalyzer(Version matchVersion)
		{
			stopWords = ENGLISH_STOP_WORDS_SET;
			useDefaultStopPositionIncrement = false;
			enablePositionIncrements = StopFilter.GetEnablePositionIncrementsVersionDefault(matchVersion);
		}
		
		/// <summary>Builds an analyzer which removes words in
		/// ENGLISH_STOP_WORDS.
		/// </summary>
		/// <param name="enablePositionIncrements">
		/// See {@link StopFilter#SetEnablePositionIncrements}
		/// </param>
		/// <deprecated> Use {@link #StopAnalyzer(Version)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version) instead")]
		public StopAnalyzer(bool enablePositionIncrements)
		{
			stopWords = ENGLISH_STOP_WORDS_SET;
			this.enablePositionIncrements = enablePositionIncrements;
			useDefaultStopPositionIncrement = false;
		}
		
		/// <summary>Builds an analyzer with the stop words from the given set.</summary>
		/// <deprecated> Use {@link #StopAnalyzer(Version, Set)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version, Set) instead")]
		public StopAnalyzer(System.Collections.Hashtable stopWords)
		{
			this.stopWords = stopWords;
			useDefaultStopPositionIncrement = true;
			enablePositionIncrements = false;
		}
		
		/// <summary>Builds an analyzer with the stop words from the given set.</summary>
		public StopAnalyzer(Version matchVersion, System.Collections.Hashtable stopWords)
		{
			this.stopWords = stopWords;
			useDefaultStopPositionIncrement = false;
			enablePositionIncrements = StopFilter.GetEnablePositionIncrementsVersionDefault(matchVersion);
		}
		
		/// <summary>Builds an analyzer with the stop words from the given set.</summary>
		/// <param name="stopWords">Set of stop words
		/// </param>
		/// <param name="enablePositionIncrements">
		/// See {@link StopFilter#SetEnablePositionIncrements}
		/// </param>
		/// <deprecated> Use {@link #StopAnalyzer(Version, Set)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version, Set) instead")]
		public StopAnalyzer(System.Collections.Hashtable stopWords, bool enablePositionIncrements)
		{
			this.stopWords = stopWords;
			this.enablePositionIncrements = enablePositionIncrements;
			useDefaultStopPositionIncrement = false;
		}
		
		/// <summary>Builds an analyzer which removes words in the provided array.</summary>
		/// <deprecated> Use {@link #StopAnalyzer(Set, boolean)} instead 
		/// </deprecated>
		/// <deprecated> Use {@link #StopAnalyzer(Version, Set)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Set, boolean) or StopAnalyzer(Version, Set) instead ")]
		public StopAnalyzer(System.String[] stopWords)
		{
			this.stopWords = StopFilter.MakeStopSet(stopWords);
			useDefaultStopPositionIncrement = true;
			enablePositionIncrements = false;
		}
		
		/// <summary>Builds an analyzer which removes words in the provided array.</summary>
		/// <param name="stopWords">Array of stop words
		/// </param>
		/// <param name="enablePositionIncrements">
		/// See {@link StopFilter#SetEnablePositionIncrements}
		/// </param>
		/// <deprecated> Use {@link #StopAnalyzer(Version, Set)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version, Set) instead")]
		public StopAnalyzer(System.String[] stopWords, bool enablePositionIncrements)
		{
			this.stopWords = StopFilter.MakeStopSet(stopWords);
			this.enablePositionIncrements = enablePositionIncrements;
			useDefaultStopPositionIncrement = false;
		}
		
		/// <summary>Builds an analyzer with the stop words from the given file.</summary>
		/// <seealso cref="WordlistLoader.GetWordSet(File)">
		/// </seealso>
		/// <deprecated> Use {@link #StopAnalyzer(Version, File)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version, File) instead")]
		public StopAnalyzer(System.IO.FileInfo stopwordsFile)
		{
			stopWords = WordlistLoader.GetWordSet(stopwordsFile);
			useDefaultStopPositionIncrement = true;
			enablePositionIncrements = false;
		}
		
		/// <summary>Builds an analyzer with the stop words from the given file.</summary>
		/// <seealso cref="WordlistLoader.GetWordSet(File)">
		/// </seealso>
		/// <param name="stopwordsFile">File to load stop words from
		/// </param>
		/// <param name="enablePositionIncrements">
		/// See {@link StopFilter#SetEnablePositionIncrements}
		/// </param>
		/// <deprecated> Use {@link #StopAnalyzer(Version, File)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version, File) instead")]
		public StopAnalyzer(System.IO.FileInfo stopwordsFile, bool enablePositionIncrements)
		{
			stopWords = WordlistLoader.GetWordSet(stopwordsFile);
			this.enablePositionIncrements = enablePositionIncrements;
			useDefaultStopPositionIncrement = false;
		}
		
		/// <summary> Builds an analyzer with the stop words from the given file.
		/// 
		/// </summary>
		/// <seealso cref="WordlistLoader.getWordSet(File)">
		/// </seealso>
		/// <param name="matchVersion">See <a href="#version">above</a>
		/// </param>
		/// <param name="stopwordsFile">File to load stop words from
		/// </param>
		public StopAnalyzer(Version matchVersion, System.IO.FileInfo stopwordsFile)
		{
			stopWords = WordlistLoader.GetWordSet(stopwordsFile);
			this.enablePositionIncrements = StopFilter.GetEnablePositionIncrementsVersionDefault(matchVersion);
			useDefaultStopPositionIncrement = false;
		}
		
		/// <summary>Builds an analyzer with the stop words from the given reader.</summary>
		/// <seealso cref="WordlistLoader.GetWordSet(Reader)">
		/// </seealso>
		/// <deprecated> Use {@link #StopAnalyzer(Version, Reader)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version, Reader) instead")]
		public StopAnalyzer(System.IO.TextReader stopwords)
		{
			stopWords = WordlistLoader.GetWordSet(stopwords);
			useDefaultStopPositionIncrement = true;
			enablePositionIncrements = false;
		}
		
		/// <summary>Builds an analyzer with the stop words from the given reader.</summary>
		/// <seealso cref="WordlistLoader.GetWordSet(Reader)">
		/// </seealso>
		/// <param name="stopwords">Reader to load stop words from
		/// </param>
		/// <param name="enablePositionIncrements">
		/// See {@link StopFilter#SetEnablePositionIncrements}
		/// </param>
		/// <deprecated> Use {@link #StopAnalyzer(Version, Reader)} instead
		/// </deprecated>
        [Obsolete("Use StopAnalyzer(Version, Reader) instead")]
		public StopAnalyzer(System.IO.TextReader stopwords, bool enablePositionIncrements)
		{
			stopWords = WordlistLoader.GetWordSet(stopwords);
			this.enablePositionIncrements = enablePositionIncrements;
			useDefaultStopPositionIncrement = false;
		}

        /// <summary>Builds an analyzer with the stop words from the given reader. </summary>
        /// <seealso cref="WordlistLoader.GetWordSet(Reader)">
        /// </seealso>
        /// <param name="matchVersion">See <a href="#Version">above</a>
        /// </param>
        /// <param name="stopwords">Reader to load stop words from
        /// </param>
        public StopAnalyzer(Version matchVersion, System.IO.TextReader stopwords)
        {
            stopWords = WordlistLoader.GetWordSet(stopwords);
            this.enablePositionIncrements = StopFilter.GetEnablePositionIncrementsVersionDefault(matchVersion);
            useDefaultStopPositionIncrement = false;
        }

        /// <summary>Filters LowerCaseTokenizer with StopFilter. </summary>
		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			if (useDefaultStopPositionIncrement)
			{
				return new StopFilter(new LowerCaseTokenizer(reader), stopWords);
			}
			else
			{
				return new StopFilter(enablePositionIncrements, new LowerCaseTokenizer(reader), stopWords);
			}
		}
		
		/// <summary>Filters LowerCaseTokenizer with StopFilter. </summary>
		private class SavedStreams
		{
			public SavedStreams(StopAnalyzer enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(StopAnalyzer enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private StopAnalyzer enclosingInstance;
			public StopAnalyzer Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal Tokenizer source;
			internal TokenStream result;
		}
		
		public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			SavedStreams streams = (SavedStreams) GetPreviousTokenStream();
			if (streams == null)
			{
				streams = new SavedStreams(this);
				streams.source = new LowerCaseTokenizer(reader);
				if (useDefaultStopPositionIncrement)
				{
					streams.result = new StopFilter(streams.source, stopWords);
				}
				else
				{
					streams.result = new StopFilter(enablePositionIncrements, streams.source, stopWords);
				}
				SetPreviousTokenStream(streams);
			}
			else
				streams.source.Reset(reader);
			return streams.result;
		}
		static StopAnalyzer()
		{
			{
				System.String[] stopWords = new System.String[]{"a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with"};
				CharArraySet stopSet = new CharArraySet(stopWords.Length, false);
				stopSet.AddAll(new System.Collections.ArrayList(stopWords));
				ENGLISH_STOP_WORDS_SET = CharArraySet.UnmodifiableSet(stopSet);
			}
		}
	}
}
