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
namespace Monodoc.Lucene.Net.Analysis.DE
{
	
	/// <summary> Loader for text files that represent a list of stopwords.
	/// 
	/// </summary>
	/// <author>  Gerhard Schwarz
	/// </author>
	/// <version>  $Id: WordlistLoader.java,v 1.10 2004/03/30 15:54:48 otis Exp $
	/// 
	/// </version>
	/// <todo>  this is not specific to German, it should be moved up </todo>
	public class WordlistLoader
	{
		
		/// <summary> Loads a text file and adds every line as an entry to a HashSet (omitting
		/// leading and trailing whitespace). Every line of the file should contain only 
		/// one word. The words need to be in lowercase if you make use of an
		/// Analyzer which uses LowerCaseFilter (like GermanAnalyzer).
		/// 
		/// </summary>
		/// <param name="wordfile">File containing the wordlist
		/// </param>
		/// <returns> A HashSet with the file's words
		/// </returns>
		public static System.Collections.Hashtable GetWordSet(System.IO.FileInfo wordfile)
		{
			System.Collections.Hashtable result = new System.Collections.Hashtable();
			System.IO.StreamReader freader = null;
			System.IO.StreamReader lnr = null;
			try
			{
				freader = new System.IO.StreamReader(wordfile.FullName, System.Text.Encoding.Default);
				lnr = new System.IO.StreamReader(freader.BaseStream, freader.CurrentEncoding);
				System.String word = null;
				while ((word = lnr.ReadLine()) != null)
				{
                    System.String trimedWord = word.Trim();
					result.Add(trimedWord, trimedWord);
				}
			}
			finally
			{
				if (lnr != null)
					lnr.Close();
				if (freader != null)
					freader.Close();
			}
			return result;
		}
		
		/// <param name="path">     Path to the wordlist
		/// </param>
		/// <param name="wordfile"> Name of the wordlist
		/// 
		/// </param>
		/// <deprecated> Use {@link #GetWordSet(File)} getWordSet(File)} instead
		/// </deprecated>
		public static System.Collections.Hashtable GetWordtable(System.String path, System.String wordfile)
		{
			return GetWordtable(new System.IO.FileInfo(System.IO.Path.Combine(path, wordfile)));
		}
		
		/// <param name="wordfile"> Complete path to the wordlist
		/// 
		/// </param>
		/// <deprecated> Use {@link #GetWordSet(File)} getWordSet(File)} instead
		/// </deprecated>
		public static System.Collections.Hashtable GetWordtable(System.String wordfile)
		{
			return GetWordtable(new System.IO.FileInfo(wordfile));
		}
		
		/// <param name="wordfile"> File object that points to the wordlist
		/// 
		/// </param>
		/// <deprecated> Use {@link #GetWordSet(File)} getWordSet(File)} instead
		/// </deprecated>
		public static System.Collections.Hashtable GetWordtable(System.IO.FileInfo wordfile)
		{
			System.Collections.Hashtable wordSet = (System.Collections.Hashtable) GetWordSet(wordfile);
			System.Collections.Hashtable result = MakeWordTable(wordSet);
			return result;
		}
		
		/// <summary> Builds a wordlist table, using words as both keys and values
		/// for backward compatibility.
		/// 
		/// </summary>
		/// <param name="wordSet">  stopword set
		/// </param>
		private static System.Collections.Hashtable MakeWordTable(System.Collections.Hashtable wordSet)
		{
			System.Collections.Hashtable table = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
			for (System.Collections.IEnumerator iter = wordSet.GetEnumerator(); iter.MoveNext(); )
			{
				System.String word = (System.String) iter.Current;
				table[word] = word;
			}
			return table;
		}
	}
}