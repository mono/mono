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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> Common util methods for dealing with {@link IndexReader}s.
	/// 
	/// </summary>
	public class ReaderUtil
	{
		
		/// <summary> Gathers sub-readers from reader into a List.
		/// 
		/// </summary>
		/// <param name="allSubReaders">
		/// </param>
		/// <param name="reader">
		/// </param>
		public static void  GatherSubReaders(System.Collections.IList allSubReaders, IndexReader reader)
		{
			IndexReader[] subReaders = reader.GetSequentialSubReaders();
			if (subReaders == null)
			{
				// Add the reader itself, and do not recurse
				allSubReaders.Add(reader);
			}
			else
			{
				for (int i = 0; i < subReaders.Length; i++)
				{
					GatherSubReaders(allSubReaders, subReaders[i]);
				}
			}
		}
		
		/// <summary> Returns sub IndexReader that contains the given document id.
		/// 
		/// </summary>
		/// <param name="doc">id of document
		/// </param>
		/// <param name="reader">parent reader
		/// </param>
		/// <returns> sub reader of parent which contains the specified doc id
		/// </returns>
		public static IndexReader SubReader(int doc, IndexReader reader)
		{
			System.Collections.ArrayList subReadersList = new System.Collections.ArrayList();
			ReaderUtil.GatherSubReaders(subReadersList, reader);
			IndexReader[] subReaders = (IndexReader[]) subReadersList.ToArray(typeof(IndexReader));
			int[] docStarts = new int[subReaders.Length];
			int maxDoc = 0;
			for (int i = 0; i < subReaders.Length; i++)
			{
				docStarts[i] = maxDoc;
				maxDoc += subReaders[i].MaxDoc();
			}
			return subReaders[ReaderUtil.SubIndex(doc, docStarts)];
		}
		
		/// <summary> Returns sub-reader subIndex from reader.
		/// 
		/// </summary>
		/// <param name="reader">parent reader
		/// </param>
		/// <param name="subIndex">index of desired sub reader
		/// </param>
		/// <returns> the subreader at subINdex
		/// </returns>
		public static IndexReader SubReader(IndexReader reader, int subIndex)
		{
			System.Collections.ArrayList subReadersList = new System.Collections.ArrayList();
			ReaderUtil.GatherSubReaders(subReadersList, reader);
			IndexReader[] subReaders = (IndexReader[]) subReadersList.ToArray(typeof(IndexReader));
			return subReaders[subIndex];
		}
		
		
		/// <summary> Returns index of the searcher/reader for document <code>n</code> in the
		/// array used to construct this searcher/reader.
		/// </summary>
		public static int SubIndex(int n, int[] docStarts)
		{
			// find
			// searcher/reader for doc n:
			int size = docStarts.Length;
			int lo = 0; // search starts array
			int hi = size - 1; // for first element less than n, return its index
			while (hi >= lo)
			{
				int mid = SupportClass.Number.URShift((lo + hi), 1);
				int midValue = docStarts[mid];
				if (n < midValue)
					hi = mid - 1;
				else if (n > midValue)
					lo = mid + 1;
				else
				{
					// found a match
					while (mid + 1 < size && docStarts[mid + 1] == midValue)
					{
						mid++; // scan to last match
					}
					return mid;
				}
			}
			return hi;
		}
	}
}
