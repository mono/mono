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
using Token = Monodoc.Lucene.Net.Analysis.Token;
using TokenStream = Monodoc.Lucene.Net.Analysis.TokenStream;
using TermFreqVector = Monodoc.Lucene.Net.Index.TermFreqVector;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> 
	/// 
	/// 
	/// </summary>
	public class QueryTermVector : TermFreqVector
	{
		private System.String[] terms = new System.String[0];
		private int[] termFreqs = new int[0];
		
		public virtual System.String GetField()
		{
			return null;
		}
		
		/// <summary> </summary>
		/// <param name="queryTerms">The original list of terms from the query, can contain duplicates
		/// </param>
		public QueryTermVector(System.String[] queryTerms)
		{
			
			ProcessTerms(queryTerms);
		}
		
		public QueryTermVector(System.String queryString, Analyzer analyzer)
		{
			if (analyzer != null)
			{
				TokenStream stream = analyzer.TokenStream("", new System.IO.StringReader(queryString));
				if (stream != null)
				{
					Token next = null;
					System.Collections.ArrayList terms = new System.Collections.ArrayList();
					try
					{
						while ((next = stream.Next()) != null)
						{
							terms.Add(next.TermText());
						}
						ProcessTerms((System.String[]) terms.ToArray(typeof(System.String)));
					}
					catch (System.IO.IOException)
					{
					}
				}
			}
		}
		
		private void  ProcessTerms(System.String[] queryTerms)
		{
			if (queryTerms != null)
			{
				System.Array.Sort(queryTerms);
				System.Collections.IDictionary tmpSet = new System.Collections.Hashtable(queryTerms.Length);
				//filter out duplicates
				System.Collections.ArrayList tmpList = new System.Collections.ArrayList(queryTerms.Length);
				System.Collections.ArrayList tmpFreqs = new System.Collections.ArrayList(queryTerms.Length);
				int j = 0;
				for (int i = 0; i < queryTerms.Length; i++)
				{
					System.String term = queryTerms[i];
                    System.Object tmpPosition = tmpSet[term];
					if (tmpPosition == null)
					{
						tmpSet[term] = (System.Int32) j++;
						tmpList.Add(term);
						tmpFreqs.Add(1);
					}
					else
					{
                        System.Int32 position = (System.Int32) tmpSet[term];
						System.Int32 integer = (System.Int32) tmpFreqs[position];
						tmpFreqs[position] = (System.Int32) (integer + 1);
					}
				}
                terms = (System.String[]) tmpList.ToArray(typeof(System.String));
				//termFreqs = (int[])tmpFreqs.toArray(termFreqs);
				termFreqs = new int[tmpFreqs.Count];
				int i2 = 0;
				for (System.Collections.IEnumerator iter = tmpFreqs.GetEnumerator(); iter.MoveNext(); )
				{
					System.Int32 integer = (System.Int32) iter.Current;
					termFreqs[i2++] = integer;
				}
			}
		}
		
		public override System.String ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append('{');
			for (int i = 0; i < terms.Length; i++)
			{
				if (i > 0)
					sb.Append(", ");
				sb.Append(terms[i]).Append('/').Append(termFreqs[i]);
			}
			sb.Append('}');
			return sb.ToString();
		}
		
		
		public virtual int Size()
		{
			return terms.Length;
		}
		
		public virtual System.String[] GetTerms()
		{
			return terms;
		}
		
		public virtual int[] GetTermFrequencies()
		{
			return termFreqs;
		}
		
		public virtual int IndexOf(System.String term)
		{
			int res = System.Array.BinarySearch(terms, term);
			return res >= 0?res:- 1;
		}
		
		public virtual int[] IndexesOf(System.String[] terms, int start, int len)
		{
			int[] res = new int[len];
			
			for (int i = 0; i < len; i++)
			{
				res[i] = IndexOf(terms[i]);
			}
			return res;
		}
	}
}