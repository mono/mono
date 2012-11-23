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

using PositionIncrementAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.PositionIncrementAttribute;
using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;
using QueryParser = Mono.Lucene.Net.QueryParsers.QueryParser;
using Version = Mono.Lucene.Net.Util.Version;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> Removes stop words from a token stream.</summary>
	
	public sealed class StopFilter:TokenFilter
	{
		
		// deprecated
        [Obsolete]
		private static bool ENABLE_POSITION_INCREMENTS_DEFAULT = false;
		
		private CharArraySet stopWords;
		private bool enablePositionIncrements = ENABLE_POSITION_INCREMENTS_DEFAULT;
		
		private TermAttribute termAtt;
		private PositionIncrementAttribute posIncrAtt;
		
		/// <summary> Construct a token stream filtering the given input.</summary>
		/// <deprecated> Use {@link #StopFilter(boolean, TokenStream, String[])} instead
		/// </deprecated>
        [Obsolete("Use StopFilter(bool, TokenStream, String[]) instead")]
		public StopFilter(TokenStream input, System.String[] stopWords):this(ENABLE_POSITION_INCREMENTS_DEFAULT, input, stopWords, false)
		{
		}
		
		/// <summary> Construct a token stream filtering the given input.</summary>
		/// <param name="enablePositionIncrements">true if token positions should record the removed stop words
		/// </param>
		/// <param name="input">input TokenStream
		/// </param>
		/// <param name="stopWords">array of stop words
		/// </param>
		/// <deprecated> Use {@link #StopFilter(boolean, TokenStream, Set)} instead.
		/// </deprecated>
        [Obsolete("Use StopFilter(bool, TokenStream, Hashtable) instead.")]
		public StopFilter(bool enablePositionIncrements, TokenStream input, System.String[] stopWords):this(enablePositionIncrements, input, stopWords, false)
		{
		}
		
		/// <summary> Constructs a filter which removes words from the input
		/// TokenStream that are named in the array of words.
		/// </summary>
		/// <deprecated> Use {@link #StopFilter(boolean, TokenStream, String[], boolean)} instead
		/// </deprecated>
        [Obsolete("Use {@link #StopFilter(bool, TokenStream, String[], bool)} instead")]
		public StopFilter(TokenStream in_Renamed, System.String[] stopWords, bool ignoreCase):this(ENABLE_POSITION_INCREMENTS_DEFAULT, in_Renamed, stopWords, ignoreCase)
		{
		}
		
		/// <summary> Constructs a filter which removes words from the input
		/// TokenStream that are named in the array of words.
		/// </summary>
		/// <param name="enablePositionIncrements">true if token positions should record the removed stop words
		/// </param>
		/// <param name="in">input TokenStream
		/// </param>
		/// <param name="stopWords">array of stop words
		/// </param>
		/// <param name="ignoreCase">true if case is ignored
		/// </param>
		/// <deprecated> Use {@link #StopFilter(boolean, TokenStream, Set, boolean)} instead.
		/// </deprecated>
        [Obsolete("Use StopFilter(bool, TokenStream, Hashtable, bool) instead.")]
		public StopFilter(bool enablePositionIncrements, TokenStream in_Renamed, System.String[] stopWords, bool ignoreCase):base(in_Renamed)
		{
			this.stopWords = (CharArraySet) MakeStopSet(stopWords, ignoreCase);
			this.enablePositionIncrements = enablePositionIncrements;
			Init();
		}
		
		
		/// <summary> Construct a token stream filtering the given input.
		/// If <code>stopWords</code> is an instance of {@link CharArraySet} (true if
		/// <code>makeStopSet()</code> was used to construct the set) it will be directly used
		/// and <code>ignoreCase</code> will be ignored since <code>CharArraySet</code>
		/// directly controls case sensitivity.
		/// <p/>
		/// If <code>stopWords</code> is not an instance of {@link CharArraySet},
		/// a new CharArraySet will be constructed and <code>ignoreCase</code> will be
		/// used to specify the case sensitivity of that set.
		/// 
		/// </summary>
		/// <param name="input">
		/// </param>
		/// <param name="stopWords">The set of Stop Words.
		/// </param>
		/// <param name="ignoreCase">-Ignore case when stopping.
		/// </param>
		/// <deprecated> Use {@link #StopFilter(boolean, TokenStream, Set, boolean)} instead
		/// </deprecated>
        [Obsolete("Use StopFilter(bool, TokenStream, Set, bool) instead")]
		public StopFilter(TokenStream input, System.Collections.Hashtable stopWords, bool ignoreCase):this(ENABLE_POSITION_INCREMENTS_DEFAULT, input, stopWords, ignoreCase)
		{
		}
		
		/// <summary> Construct a token stream filtering the given input.
		/// If <code>stopWords</code> is an instance of {@link CharArraySet} (true if
		/// <code>makeStopSet()</code> was used to construct the set) it will be directly used
		/// and <code>ignoreCase</code> will be ignored since <code>CharArraySet</code>
		/// directly controls case sensitivity.
		/// <p/>
		/// If <code>stopWords</code> is not an instance of {@link CharArraySet},
		/// a new CharArraySet will be constructed and <code>ignoreCase</code> will be
		/// used to specify the case sensitivity of that set.
		/// 
		/// </summary>
		/// <param name="enablePositionIncrements">true if token positions should record the removed stop words
		/// </param>
		/// <param name="input">Input TokenStream
		/// </param>
		/// <param name="stopWords">The set of Stop Words.
		/// </param>
		/// <param name="ignoreCase">-Ignore case when stopping.
		/// </param>
		public StopFilter(bool enablePositionIncrements, TokenStream input, System.Collections.Hashtable stopWords, bool ignoreCase):base(input)
		{
			if (stopWords is CharArraySet)
			{
				this.stopWords = (CharArraySet) stopWords;
			}
			else
			{
				this.stopWords = new CharArraySet(stopWords.Count, ignoreCase);
				this.stopWords.Add(stopWords);
			}
			this.enablePositionIncrements = enablePositionIncrements;
			Init();
		}
		
		/// <summary> Constructs a filter which removes words from the input
		/// TokenStream that are named in the Set.
		/// 
		/// </summary>
		/// <seealso cref="MakeStopSet(java.lang.String[])">
		/// </seealso>
		/// <deprecated> Use {@link #StopFilter(boolean, TokenStream, Set)} instead
		/// </deprecated>
        [Obsolete("Use StopFilter(bool, TokenStream, Hashtable) instead")]
		public StopFilter(TokenStream in_Renamed, System.Collections.Hashtable stopWords):this(ENABLE_POSITION_INCREMENTS_DEFAULT, in_Renamed, stopWords, false)
		{
		}
		
		/// <summary> Constructs a filter which removes words from the input
		/// TokenStream that are named in the Set.
		/// 
		/// </summary>
		/// <param name="enablePositionIncrements">true if token positions should record the removed stop words
		/// </param>
		/// <param name="in">Input stream
		/// </param>
		/// <param name="stopWords">The set of Stop Words.
		/// </param>
		/// <seealso cref="MakeStopSet(java.lang.String[])">
		/// </seealso>
		public StopFilter(bool enablePositionIncrements, TokenStream in_Renamed, System.Collections.Hashtable stopWords):this(enablePositionIncrements, in_Renamed, stopWords, false)
		{
		}
		
		public void  Init()
		{
			termAtt = (TermAttribute) AddAttribute(typeof(TermAttribute));
			posIncrAtt = (PositionIncrementAttribute) AddAttribute(typeof(PositionIncrementAttribute));
		}
		
		/// <summary> Builds a Set from an array of stop words,
		/// appropriate for passing into the StopFilter constructor.
		/// This permits this stopWords construction to be cached once when
		/// an Analyzer is constructed.
		/// 
		/// </summary>
		/// <seealso cref="MakeStopSet(java.lang.String[], boolean)"> passing false to ignoreCase
		/// </seealso>
		public static System.Collections.Hashtable MakeStopSet(System.String[] stopWords)
		{
			return MakeStopSet(stopWords, false);
		}
		
		/// <summary> Builds a Set from an array of stop words,
		/// appropriate for passing into the StopFilter constructor.
		/// This permits this stopWords construction to be cached once when
		/// an Analyzer is constructed.
		/// 
		/// </summary>
		/// <seealso cref="MakeStopSet(java.lang.String[], boolean)"> passing false to ignoreCase
		/// </seealso>
		public static System.Collections.Hashtable MakeStopSet(System.Collections.IList stopWords)
		{
			return MakeStopSet(stopWords, false);
		}
		
		/// <summary> </summary>
		/// <param name="stopWords">An array of stopwords
		/// </param>
		/// <param name="ignoreCase">If true, all words are lower cased first.  
		/// </param>
		/// <returns> a Set containing the words
		/// </returns>
		public static System.Collections.Hashtable MakeStopSet(System.String[] stopWords, bool ignoreCase)
		{
			CharArraySet stopSet = new CharArraySet(stopWords.Length, ignoreCase);
			stopSet.AddAll(new System.Collections.ArrayList(stopWords));
			return stopSet;
		}
		
		/// <summary> </summary>
		/// <param name="stopWords">A List of Strings representing the stopwords
		/// </param>
		/// <param name="ignoreCase">if true, all words are lower cased first
		/// </param>
		/// <returns> A Set containing the words
		/// </returns>
		public static System.Collections.Hashtable MakeStopSet(System.Collections.IList stopWords, bool ignoreCase)
		{
			CharArraySet stopSet = new CharArraySet(stopWords.Count, ignoreCase);
			stopSet.AddAll(stopWords);
			return stopSet;
		}
		
		/// <summary> Returns the next input Token whose term() is not a stop word.</summary>
		public override bool IncrementToken()
		{
			// return the first non-stop word found
			int skippedPositions = 0;
			while (input.IncrementToken())
			{
				if (!stopWords.Contains(termAtt.TermBuffer(), 0, termAtt.TermLength()))
				{
					if (enablePositionIncrements)
					{
						posIncrAtt.SetPositionIncrement(posIncrAtt.GetPositionIncrement() + skippedPositions);
					}
					return true;
				}
				skippedPositions += posIncrAtt.GetPositionIncrement();
			}
			// reached EOS -- return null
			return false;
		}
		
		/// <seealso cref="SetEnablePositionIncrementsDefault(bool)">
		/// </seealso>
		/// <deprecated> Please specify this when you create the StopFilter
		/// </deprecated>
        [Obsolete("Please specify this when you create the StopFilter")]
		public static bool GetEnablePositionIncrementsDefault()
		{
			return ENABLE_POSITION_INCREMENTS_DEFAULT;
		}
		
		/// <summary> Returns version-dependent default for enablePositionIncrements. Analyzers
		/// that embed StopFilter use this method when creating the StopFilter. Prior
		/// to 2.9, this returns {@link #getEnablePositionIncrementsDefault}. On 2.9
		/// or later, it returns true.
		/// </summary>
		public static bool GetEnablePositionIncrementsVersionDefault(Version matchVersion)
		{
			if (matchVersion.OnOrAfter(Version.LUCENE_29))
			{
				return true;
			}
			else
			{
				return ENABLE_POSITION_INCREMENTS_DEFAULT;
			}
		}
		
		/// <summary> Set the default position increments behavior of every StopFilter created
		/// from now on.
		/// <p/>
		/// Note: behavior of a single StopFilter instance can be modified with
		/// {@link #SetEnablePositionIncrements(boolean)}. This static method allows
		/// control over behavior of classes using StopFilters internally, for
		/// example {@link Mono.Lucene.Net.Analysis.Standard.StandardAnalyzer
		/// StandardAnalyzer} if used with the no-arg ctor.
		/// <p/>
		/// Default : false.
		/// 
		/// </summary>
		/// <seealso cref="setEnablePositionIncrements(bool)">
		/// </seealso>
		/// <deprecated> Please specify this when you create the StopFilter
		/// </deprecated>
        [Obsolete("Please specify this when you create the StopFilter")]
		public static void  SetEnablePositionIncrementsDefault(bool defaultValue)
		{
			ENABLE_POSITION_INCREMENTS_DEFAULT = defaultValue;
		}
		
		/// <seealso cref="SetEnablePositionIncrements(bool)">
		/// </seealso>
		public bool GetEnablePositionIncrements()
		{
			return enablePositionIncrements;
		}
		
		/// <summary> If <code>true</code>, this StopFilter will preserve
		/// positions of the incoming tokens (ie, accumulate and
		/// set position increments of the removed stop tokens).
		/// Generally, <code>true</code> is best as it does not
		/// lose information (positions of the original tokens)
		/// during indexing.
		/// 
		/// <p/> When set, when a token is stopped
		/// (omitted), the position increment of the following
		/// token is incremented.
		/// 
		/// <p/> <b>NOTE</b>: be sure to also
		/// set {@link QueryParser#setEnablePositionIncrements} if
		/// you use QueryParser to create queries.
		/// </summary>
		public void  SetEnablePositionIncrements(bool enable)
		{
			this.enablePositionIncrements = enable;
		}
	}
}
