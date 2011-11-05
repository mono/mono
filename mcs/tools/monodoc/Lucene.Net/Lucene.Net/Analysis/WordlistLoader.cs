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

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> Loader for text files that represent a list of stopwords.
	/// 
	/// 
	/// </summary>
	/// <version>  $Id: WordlistLoader.java 706342 2008-10-20 17:19:29Z gsingers $
	/// </version>
	public class WordlistLoader
	{
		
		/// <summary> Loads a text file and adds every line as an entry to a HashSet (omitting
		/// leading and trailing whitespace). Every line of the file should contain only
		/// one word. The words need to be in lowercase if you make use of an
		/// Analyzer which uses LowerCaseFilter (like StandardAnalyzer).
		/// 
		/// </summary>
		/// <param name="wordfile">File containing the wordlist
		/// </param>
		/// <returns> A HashSet with the file's words
		/// </returns>
		public static System.Collections.Hashtable GetWordSet(System.IO.FileInfo wordfile)
		{
			System.Collections.Hashtable result = new System.Collections.Hashtable();
			System.IO.StreamReader reader = null;
			try
			{
				reader = new System.IO.StreamReader(wordfile.FullName, System.Text.Encoding.Default);
				result = GetWordSet(reader);
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
			return result;
		}
		
		/// <summary> Loads a text file and adds every non-comment line as an entry to a HashSet (omitting
		/// leading and trailing whitespace). Every line of the file should contain only
		/// one word. The words need to be in lowercase if you make use of an
		/// Analyzer which uses LowerCaseFilter (like StandardAnalyzer).
		/// 
		/// </summary>
		/// <param name="wordfile">File containing the wordlist
		/// </param>
		/// <param name="comment">The comment string to ignore
		/// </param>
		/// <returns> A HashSet with the file's words
		/// </returns>
		public static System.Collections.Hashtable GetWordSet(System.IO.FileInfo wordfile, System.String comment)
		{
			System.Collections.Hashtable result = new System.Collections.Hashtable();
			System.IO.StreamReader reader = null;
			try
			{
				reader = new System.IO.StreamReader(wordfile.FullName, System.Text.Encoding.Default);
				result = GetWordSet(reader, comment);
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
			return result;
		}
		
		
		/// <summary> Reads lines from a Reader and adds every line as an entry to a HashSet (omitting
		/// leading and trailing whitespace). Every line of the Reader should contain only
		/// one word. The words need to be in lowercase if you make use of an
		/// Analyzer which uses LowerCaseFilter (like StandardAnalyzer).
		/// 
		/// </summary>
		/// <param name="reader">Reader containing the wordlist
		/// </param>
		/// <returns> A HashSet with the reader's words
		/// </returns>
		public static System.Collections.Hashtable GetWordSet(System.IO.TextReader reader)
		{
			System.Collections.Hashtable result = new System.Collections.Hashtable();
			System.IO.TextReader br = null;
			try
			{
				System.String word = null;
				while ((word = reader.ReadLine()) != null)
				{
					SupportClass.CollectionsHelper.Add(result, word.Trim());
				}
			}
			finally
			{
				if (br != null)
					br.Close();
			}
			return result;
		}
		
		/// <summary> Reads lines from a Reader and adds every non-comment line as an entry to a HashSet (omitting
		/// leading and trailing whitespace). Every line of the Reader should contain only
		/// one word. The words need to be in lowercase if you make use of an
		/// Analyzer which uses LowerCaseFilter (like StandardAnalyzer).
		/// 
		/// </summary>
		/// <param name="reader">Reader containing the wordlist
		/// </param>
		/// <param name="comment">The string representing a comment.
		/// </param>
		/// <returns> A HashSet with the reader's words
		/// </returns>
        public static System.Collections.Hashtable GetWordSet(System.IO.TextReader reader, System.String comment)
		{
			System.Collections.Hashtable result = new System.Collections.Hashtable();
			System.IO.StreamReader br = null;
			try
			{
				System.String word = null;
				while ((word = reader.ReadLine()) != null)
				{
					if (word.StartsWith(comment) == false)
					{
						SupportClass.CollectionsHelper.Add(result, word.Trim());
					}
				}
			}
			finally
			{
				if (br != null)
					br.Close();
			}
			return result;
		}
		
		
		
		/// <summary> Reads a stem dictionary. Each line contains:
		/// <pre>word<b>\t</b>stem</pre>
		/// (i.e. two tab seperated words)
		/// 
		/// </summary>
		/// <returns> stem dictionary that overrules the stemming algorithm
		/// </returns>
		/// <throws>  IOException  </throws>
		public static System.Collections.Hashtable GetStemDict(System.IO.FileInfo wordstemfile)
		{
			if (wordstemfile == null)
				throw new System.NullReferenceException("wordstemfile may not be null");
			System.Collections.Hashtable result = new System.Collections.Hashtable();
			System.IO.StreamReader br = null;
			System.IO.StreamReader fr = null;
			try
			{
				fr = new System.IO.StreamReader(wordstemfile.FullName, System.Text.Encoding.Default);
				br = new System.IO.StreamReader(fr.BaseStream, fr.CurrentEncoding);
				System.String line;
                char[] tab = {'\t'};
				while ((line = br.ReadLine()) != null)
				{
					System.String[] wordstem = line.Split(tab, 2);
					result[wordstem[0]] = wordstem[1];
				}
			}
			finally
			{
				if (fr != null)
					fr.Close();
				if (br != null)
					br.Close();
			}
			return result;
		}
	}
}
