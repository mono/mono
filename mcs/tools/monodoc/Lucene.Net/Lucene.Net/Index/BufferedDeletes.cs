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

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>Holds buffered deletes, by docID, term or query.  We
	/// hold two instances of this class: one for the deletes
	/// prior to the last flush, the other for deletes after
	/// the last flush.  This is so if we need to abort
	/// (discard all buffered docs) we can also discard the
	/// buffered deletes yet keep the deletes done during
	/// previously flushed segments. 
	/// </summary>
	class BufferedDeletes
	{
		internal int numTerms;
        internal System.Collections.IDictionary terms = null;
		internal System.Collections.Hashtable queries = new System.Collections.Hashtable();
		internal System.Collections.ArrayList docIDs = new System.Collections.ArrayList();
		internal long bytesUsed;
        internal  bool doTermSort;

        public BufferedDeletes(bool doTermSort)
        {
            this.doTermSort = doTermSort;
            if (doTermSort)
            {
                terms = new System.Collections.Generic.SortedDictionary<object, object>();
            }
            else
            {
                terms = new System.Collections.Hashtable();
            }
        }
                
		
		// Number of documents a delete term applies to.
		internal sealed class Num
		{
			internal int num;
			
			internal Num(int num)
			{
				this.num = num;
			}
			
			internal int GetNum()
			{
				return num;
			}
			
			internal void  SetNum(int num)
			{
				// Only record the new number if it's greater than the
				// current one.  This is important because if multiple
				// threads are replacing the same doc at nearly the
				// same time, it's possible that one thread that got a
				// higher docID is scheduled before the other
				// threads.
				if (num > this.num)
					this.num = num;
			}
		}
		
		internal virtual int Size()
		{
			// We use numTerms not terms.size() intentionally, so
			// that deletes by the same term multiple times "count",
			// ie if you ask to flush every 1000 deletes then even
			// dup'd terms are counted towards that 1000
			return numTerms + queries.Count + docIDs.Count;
		}
		
		internal virtual void  Update(BufferedDeletes in_Renamed)
		{
			numTerms += in_Renamed.numTerms;
			bytesUsed += in_Renamed.bytesUsed;

			System.Collections.ArrayList keys = new System.Collections.ArrayList(in_Renamed.terms.Keys);
			System.Collections.ArrayList values = new System.Collections.ArrayList(in_Renamed.terms.Values);
			for (int i=0; i < keys.Count; i++)
				terms[keys[i]] = values[i];

			keys = new System.Collections.ArrayList(in_Renamed.queries.Keys);
			values = new System.Collections.ArrayList(in_Renamed.queries.Values);
			for (int i=0; i < keys.Count; i++)
				queries[keys[i]] = values[i];

			docIDs.AddRange(in_Renamed.docIDs);
			in_Renamed.Clear();
		}
		
		internal virtual void  Clear()
		{
			terms.Clear();
			queries.Clear();
			docIDs.Clear();
			numTerms = 0;
			bytesUsed = 0;
		}
		
		internal virtual void  AddBytesUsed(long b)
		{
			bytesUsed += b;
		}
		
		internal virtual bool Any()
		{
			return terms.Count > 0 || docIDs.Count > 0 || queries.Count > 0;
		}
		
		// Remaps all buffered deletes based on a completed
		// merge
		internal virtual void  Remap(MergeDocIDRemapper mapper, SegmentInfos infos, int[][] docMaps, int[] delCounts, MergePolicy.OneMerge merge, int mergeDocCount)
		{
			lock (this)
			{
				
				System.Collections.IDictionary newDeleteTerms;
				
				// Remap delete-by-term
				if (terms.Count > 0)
				{
                    if (doTermSort)
                    {
                        newDeleteTerms = new System.Collections.Generic.SortedDictionary<object, object>();
                    }
                    else
                    {
                        newDeleteTerms = new System.Collections.Hashtable();
                    }
					System.Collections.IEnumerator iter = new System.Collections.Hashtable(terms).GetEnumerator();
					while (iter.MoveNext())
					{
						System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) iter.Current;
						Num num = (Num) entry.Value;
						newDeleteTerms[entry.Key] = new Num(mapper.Remap(num.GetNum()));
					}
				}
				else
					newDeleteTerms = null;
				
				// Remap delete-by-docID
				System.Collections.ArrayList newDeleteDocIDs;
				
				if (docIDs.Count > 0)
				{
					newDeleteDocIDs = new System.Collections.ArrayList(docIDs.Count);
					System.Collections.IEnumerator iter = docIDs.GetEnumerator();
					while (iter.MoveNext())
					{
						System.Int32 num = (System.Int32) iter.Current;
						newDeleteDocIDs.Add((System.Int32) mapper.Remap(num));
					}
				}
				else
					newDeleteDocIDs = null;
				
				// Remap delete-by-query
				System.Collections.Hashtable newDeleteQueries;
				
				if (queries.Count > 0)
				{
					newDeleteQueries = new System.Collections.Hashtable(queries.Count);
					System.Collections.IEnumerator iter = new System.Collections.Hashtable(queries).GetEnumerator();
					while (iter.MoveNext())
					{
						System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) iter.Current;
						System.Int32 num = (System.Int32) entry.Value;
						newDeleteQueries[entry.Key] = (System.Int32) mapper.Remap(num);
					}
				}
				else
					newDeleteQueries = null;
				
				if (newDeleteTerms != null)
					terms = newDeleteTerms;
				if (newDeleteDocIDs != null)
					docIDs = newDeleteDocIDs;
				if (newDeleteQueries != null)
					queries = newDeleteQueries;
			}
		}
	}
}
