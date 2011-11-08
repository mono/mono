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

using Mono.Lucene.Net.Analysis;
using Version = Mono.Lucene.Net.Util.Version;

namespace Mono.Lucene.Net.Analysis.Standard
{
	
	/// <summary> Filters {@link StandardTokenizer} with {@link StandardFilter},
	/// {@link LowerCaseFilter} and {@link StopFilter}, using a list of English stop
	/// words.
	/// 
	/// <a name="version"/>
	/// <p/>
	/// You must specify the required {@link Version} compatibility when creating
	/// StandardAnalyzer:
	/// <ul>
	/// <li>As of 2.9, StopFilter preserves position increments</li>
	/// <li>As of 2.4, Tokens incorrectly identified as acronyms are corrected (see
	/// <a href="https://issues.apache.org/jira/browse/LUCENE-1068">LUCENE-1608</a></li>
	/// </ul>
	/// 
	/// </summary>
	/// <version>  $Id: StandardAnalyzer.java 829134 2009-10-23 17:18:53Z mikemccand $
	/// </version>
	public class StandardAnalyzer : Analyzer
	{
		private System.Collections.Hashtable stopSet;
		
		/// <summary> Specifies whether deprecated acronyms should be replaced with HOST type.
		/// This is false by default to support backward compatibility.
		/// 
		/// </summary>
		/// <deprecated> this should be removed in the next release (3.0).
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// </deprecated>
        [Obsolete("this should be removed in the next release (3.0).")]
		private bool replaceInvalidAcronym = defaultReplaceInvalidAcronym;
		
		private static bool defaultReplaceInvalidAcronym;
		private bool enableStopPositionIncrements;
		
		// @deprecated
        [Obsolete]
		private bool useDefaultStopPositionIncrements;
		
		/// <summary> </summary>
		/// <returns> true if new instances of StandardTokenizer will
		/// replace mischaracterized acronyms
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// </returns>
		/// <deprecated> This will be removed (hardwired to true) in 3.0
		/// </deprecated>
        [Obsolete("This will be removed (hardwired to true) in 3.0")]
		public static bool GetDefaultReplaceInvalidAcronym()
		{
			return defaultReplaceInvalidAcronym;
		}
		
		/// <summary> </summary>
		/// <param name="replaceInvalidAcronym">Set to true to have new
		/// instances of StandardTokenizer replace mischaracterized
		/// acronyms by default.  Set to false to preserve the
		/// previous (before 2.4) buggy behavior.  Alternatively,
		/// set the system property
		/// Mono.Lucene.Net.Analysis.Standard.StandardAnalyzer.replaceInvalidAcronym
		/// to false.
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// </param>
		/// <deprecated> This will be removed (hardwired to true) in 3.0
		/// </deprecated>
        [Obsolete("This will be removed (hardwired to true) in 3.0")]
		public static void  SetDefaultReplaceInvalidAcronym(bool replaceInvalidAcronym)
		{
			defaultReplaceInvalidAcronym = replaceInvalidAcronym;
		}
		
		
		/// <summary>An array containing some common English words that are usually not
		/// useful for searching. 
		/// </summary>
		/// <deprecated> Use {@link #STOP_WORDS_SET} instead 
		/// </deprecated>
        [Obsolete("Use STOP_WORDS_SET instead ")]
		public static readonly System.String[] STOP_WORDS;
		
		/// <summary>An unmodifiable set containing some common English words that are usually not
		/// useful for searching. 
		/// </summary>
		public static readonly System.Collections.Hashtable STOP_WORDS_SET;
		
		/// <summary>Builds an analyzer with the default stop words ({@link
		/// #STOP_WORDS_SET}).
		/// </summary>
		/// <deprecated> Use {@link #StandardAnalyzer(Version)} instead. 
		/// </deprecated>
        [Obsolete("Use StandardAnalyzer(Version) instead")]
		public StandardAnalyzer():this(Version.LUCENE_24, STOP_WORDS_SET)
		{
		}
		
		/// <summary>Builds an analyzer with the default stop words ({@link
		/// #STOP_WORDS}).
		/// </summary>
		/// <param name="matchVersion">Lucene version to match See {@link
		/// <a href="#version">above</a>}
		/// </param>
		public StandardAnalyzer(Version matchVersion):this(matchVersion, STOP_WORDS_SET)
		{
		}
		
		/// <summary>Builds an analyzer with the given stop words.</summary>
		/// <deprecated> Use {@link #StandardAnalyzer(Version, Set)}
		/// instead 
		/// </deprecated>
        [Obsolete("Use StandardAnalyzer(Version, Set) instead")]
		public StandardAnalyzer(System.Collections.Hashtable stopWords):this(Version.LUCENE_24, stopWords)
		{
		}
		
		/// <summary>Builds an analyzer with the given stop words.</summary>
		/// <param name="matchVersion">Lucene version to match See {@link
		/// <a href="#version">above</a>}
		/// </param>
		/// <param name="stopWords">stop words 
		/// </param>
		public StandardAnalyzer(Version matchVersion, System.Collections.Hashtable stopWords)
		{
			stopSet = stopWords;
			Init(matchVersion);
		}
		
		/// <summary>Builds an analyzer with the given stop words.</summary>
		/// <deprecated> Use {@link #StandardAnalyzer(Version, Set)} instead 
		/// </deprecated>
        [Obsolete("Use StandardAnalyzer(Version, Set) instead")]
		public StandardAnalyzer(System.String[] stopWords):this(Version.LUCENE_24, StopFilter.MakeStopSet(stopWords))
		{
		}
		
		/// <summary>Builds an analyzer with the stop words from the given file.</summary>
		/// <seealso cref="WordlistLoader.GetWordSet(File)">
		/// </seealso>
		/// <deprecated> Use {@link #StandardAnalyzer(Version, File)}
		/// instead
		/// </deprecated>
        [Obsolete("Use StandardAnalyzer(Version, File) instead")]
		public StandardAnalyzer(System.IO.FileInfo stopwords):this(Version.LUCENE_24, stopwords)
		{
		}
		
		/// <summary>Builds an analyzer with the stop words from the given file.</summary>
		/// <seealso cref="WordlistLoader.GetWordSet(File)">
		/// </seealso>
		/// <param name="matchVersion">Lucene version to match See {@link
		/// <a href="#version">above</a>}
		/// </param>
		/// <param name="stopwords">File to read stop words from 
		/// </param>
		public StandardAnalyzer(Version matchVersion, System.IO.FileInfo stopwords)
		{
			stopSet = WordlistLoader.GetWordSet(stopwords);
			Init(matchVersion);
		}
		
		/// <summary>Builds an analyzer with the stop words from the given reader.</summary>
		/// <seealso cref="WordlistLoader.GetWordSet(Reader)">
		/// </seealso>
		/// <deprecated> Use {@link #StandardAnalyzer(Version, Reader)}
		/// instead
		/// </deprecated>
        [Obsolete("Use StandardAnalyzer(Version, Reader) instead")]
		public StandardAnalyzer(System.IO.TextReader stopwords):this(Version.LUCENE_24, stopwords)
		{
		}
		
		/// <summary>Builds an analyzer with the stop words from the given reader.</summary>
		/// <seealso cref="WordlistLoader.GetWordSet(Reader)">
		/// </seealso>
		/// <param name="matchVersion">Lucene version to match See {@link
		/// <a href="#version">above</a>}
		/// </param>
		/// <param name="stopwords">Reader to read stop words from 
		/// </param>
		public StandardAnalyzer(Version matchVersion, System.IO.TextReader stopwords)
		{
			stopSet = WordlistLoader.GetWordSet(stopwords);
			Init(matchVersion);
		}
		
		/// <summary> </summary>
		/// <param name="replaceInvalidAcronym">Set to true if this analyzer should replace mischaracterized acronyms in the StandardTokenizer
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// 
		/// </param>
		/// <deprecated> Remove in 3.X and make true the only valid value
		/// </deprecated>
        [Obsolete("Remove in 3.X and make true the only valid value")]
		public StandardAnalyzer(bool replaceInvalidAcronym):this(Version.LUCENE_24, STOP_WORDS_SET)
		{
			this.replaceInvalidAcronym = replaceInvalidAcronym;
			useDefaultStopPositionIncrements = true;
		}
		
		/// <param name="stopwords">The stopwords to use
		/// </param>
		/// <param name="replaceInvalidAcronym">Set to true if this analyzer should replace mischaracterized acronyms in the StandardTokenizer
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// 
		/// </param>
		/// <deprecated> Remove in 3.X and make true the only valid value
		/// </deprecated>
        [Obsolete("Remove in 3.X and make true the only valid value")]
		public StandardAnalyzer(System.IO.TextReader stopwords, bool replaceInvalidAcronym):this(Version.LUCENE_24, stopwords)
		{
			this.replaceInvalidAcronym = replaceInvalidAcronym;
		}
		
		/// <param name="stopwords">The stopwords to use
		/// </param>
		/// <param name="replaceInvalidAcronym">Set to true if this analyzer should replace mischaracterized acronyms in the StandardTokenizer
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// 
		/// </param>
		/// <deprecated> Remove in 3.X and make true the only valid value
		/// </deprecated>
        [Obsolete("Remove in 3.X and make true the only valid value")]
		public StandardAnalyzer(System.IO.FileInfo stopwords, bool replaceInvalidAcronym):this(Version.LUCENE_24, stopwords)
		{
			this.replaceInvalidAcronym = replaceInvalidAcronym;
		}
		
		/// <summary> </summary>
		/// <param name="stopwords">The stopwords to use
		/// </param>
		/// <param name="replaceInvalidAcronym">Set to true if this analyzer should replace mischaracterized acronyms in the StandardTokenizer
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// 
		/// </param>
		/// <deprecated> Remove in 3.X and make true the only valid value
		/// </deprecated>
        [Obsolete("Remove in 3.X and make true the only valid value")]
		public StandardAnalyzer(System.String[] stopwords, bool replaceInvalidAcronym):this(Version.LUCENE_24, StopFilter.MakeStopSet(stopwords))
		{
			this.replaceInvalidAcronym = replaceInvalidAcronym;
		}
		
		/// <param name="stopwords">The stopwords to use
		/// </param>
		/// <param name="replaceInvalidAcronym">Set to true if this analyzer should replace mischaracterized acronyms in the StandardTokenizer
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// 
		/// </param>
		/// <deprecated> Remove in 3.X and make true the only valid value
		/// </deprecated>
        [Obsolete("Remove in 3.X and make true the only valid value")]
		public StandardAnalyzer(System.Collections.Hashtable stopwords, bool replaceInvalidAcronym):this(Version.LUCENE_24, stopwords)
		{
			this.replaceInvalidAcronym = replaceInvalidAcronym;
		}
		
		private void  Init(Version matchVersion)
		{
			SetOverridesTokenStreamMethod(typeof(StandardAnalyzer));
			if (matchVersion.OnOrAfter(Version.LUCENE_29))
			{
				enableStopPositionIncrements = true;
			}
			else
			{
				useDefaultStopPositionIncrements = true;
			}
			if (matchVersion.OnOrAfter(Version.LUCENE_24))
			{
				replaceInvalidAcronym = defaultReplaceInvalidAcronym;
			}
			else
			{
				replaceInvalidAcronym = false;
			}
		}
		
		/// <summary>Constructs a {@link StandardTokenizer} filtered by a {@link
		/// StandardFilter}, a {@link LowerCaseFilter} and a {@link StopFilter}. 
		/// </summary>
		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			StandardTokenizer tokenStream = new StandardTokenizer(reader, replaceInvalidAcronym);
			tokenStream.SetMaxTokenLength(maxTokenLength);
			TokenStream result = new StandardFilter(tokenStream);
			result = new LowerCaseFilter(result);
			if (useDefaultStopPositionIncrements)
			{
				result = new StopFilter(result, stopSet);
			}
			else
			{
				result = new StopFilter(enableStopPositionIncrements, result, stopSet);
			}
			return result;
		}
		
		private sealed class SavedStreams
		{
			internal StandardTokenizer tokenStream;
			internal TokenStream filteredTokenStream;
		}
		
		/// <summary>Default maximum allowed token length </summary>
		public const int DEFAULT_MAX_TOKEN_LENGTH = 255;
		
		private int maxTokenLength = DEFAULT_MAX_TOKEN_LENGTH;
		
		/// <summary> Set maximum allowed token length.  If a token is seen
		/// that exceeds this length then it is discarded.  This
		/// setting only takes effect the next time tokenStream or
		/// reusableTokenStream is called.
		/// </summary>
		public virtual void  SetMaxTokenLength(int length)
		{
			maxTokenLength = length;
		}
		
		/// <seealso cref="setMaxTokenLength">
		/// </seealso>
		public virtual int GetMaxTokenLength()
		{
			return maxTokenLength;
		}
		
		/// <deprecated> Use {@link #tokenStream} instead 
		/// </deprecated>
        [Obsolete("Use TokenStream instead")]
		public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			if (overridesTokenStreamMethod)
			{
				// LUCENE-1678: force fallback to tokenStream() if we
				// have been subclassed and that subclass overrides
				// tokenStream but not reusableTokenStream
				return TokenStream(fieldName, reader);
			}
			SavedStreams streams = (SavedStreams) GetPreviousTokenStream();
			if (streams == null)
			{
				streams = new SavedStreams();
				SetPreviousTokenStream(streams);
				streams.tokenStream = new StandardTokenizer(reader);
				streams.filteredTokenStream = new StandardFilter(streams.tokenStream);
				streams.filteredTokenStream = new LowerCaseFilter(streams.filteredTokenStream);
				if (useDefaultStopPositionIncrements)
				{
					streams.filteredTokenStream = new StopFilter(streams.filteredTokenStream, stopSet);
				}
				else
				{
					streams.filteredTokenStream = new StopFilter(enableStopPositionIncrements, streams.filteredTokenStream, stopSet);
				}
			}
			else
			{
				streams.tokenStream.Reset(reader);
			}
			streams.tokenStream.SetMaxTokenLength(maxTokenLength);
			
			streams.tokenStream.SetReplaceInvalidAcronym(replaceInvalidAcronym);
			
			return streams.filteredTokenStream;
		}
		
		/// <summary> </summary>
		/// <returns> true if this Analyzer is replacing mischaracterized acronyms in the StandardTokenizer
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// </returns>
		/// <deprecated> This will be removed (hardwired to true) in 3.0
		/// </deprecated>
        [Obsolete("This will be removed (hardwired to true) in 3.0")]
		public virtual bool IsReplaceInvalidAcronym()
		{
			return replaceInvalidAcronym;
		}
		
		/// <summary> </summary>
		/// <param name="replaceInvalidAcronym">Set to true if this Analyzer is replacing mischaracterized acronyms in the StandardTokenizer
		/// 
		/// See https://issues.apache.org/jira/browse/LUCENE-1068
		/// </param>
		/// <deprecated> This will be removed (hardwired to true) in 3.0
		/// </deprecated>
        [Obsolete("This will be removed (hardwired to true) in 3.0")]
		public virtual void  SetReplaceInvalidAcronym(bool replaceInvalidAcronym)
		{
			this.replaceInvalidAcronym = replaceInvalidAcronym;
		}
		static StandardAnalyzer()
		{
			// Default to true (fixed the bug), unless the system prop is set
			{
				System.String v = SupportClass.AppSettings.Get("Mono.Lucene.Net.Analysis.Standard.StandardAnalyzer.replaceInvalidAcronym", "true");
				if (v == null || v.Equals("true"))
					defaultReplaceInvalidAcronym = true;
				else
					defaultReplaceInvalidAcronym = false;
			}
			STOP_WORDS = StopAnalyzer.ENGLISH_STOP_WORDS;
			STOP_WORDS_SET = StopAnalyzer.ENGLISH_STOP_WORDS_SET;
		}
	}
}
