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
	
	/// <summary> Store a sorted collection of {@link Mono.Lucene.Net.Index.TermVectorEntry}s.  Collects all term information
	/// into a single, SortedSet.
	/// <br/>
	/// NOTE: This Mapper ignores all Field information for the Document.  This means that if you are using offset/positions you will not
	/// know what Fields they correlate with.
	/// <br/>
	/// This is not thread-safe  
	/// </summary>
	public class SortedTermVectorMapper:TermVectorMapper
	{


        private System.Collections.Generic.SortedDictionary<System.Object, System.Object> currentSet;
		private System.Collections.IDictionary termToTVE = new System.Collections.Hashtable();
		private bool storeOffsets;
		private bool storePositions;
		/// <summary> Stand-in name for the field in {@link TermVectorEntry}.</summary>
		public const System.String ALL = "_ALL_";
		
		/// <summary> </summary>
		/// <param name="comparator">A Comparator for sorting {@link TermVectorEntry}s
		/// </param>
		public SortedTermVectorMapper(System.Collections.Generic.IComparer<System.Object> comparator):this(false, false, comparator)
		{
		}
		
		
		public SortedTermVectorMapper(bool ignoringPositions, bool ignoringOffsets, System.Collections.Generic.IComparer<System.Object> comparator):base(ignoringPositions, ignoringOffsets)
		{
            currentSet = new System.Collections.Generic.SortedDictionary<System.Object, System.Object>(comparator);
		}
		
		/// <summary> </summary>
		/// <param name="term">The term to map
		/// </param>
		/// <param name="frequency">The frequency of the term
		/// </param>
		/// <param name="offsets">Offset information, may be null
		/// </param>
		/// <param name="positions">Position information, may be null
		/// </param>
		//We need to combine any previous mentions of the term
		public override void  Map(System.String term, int frequency, TermVectorOffsetInfo[] offsets, int[] positions)
		{
			TermVectorEntry entry = (TermVectorEntry) termToTVE[term];
			if (entry == null)
			{
				entry = new TermVectorEntry(ALL, term, frequency, storeOffsets == true?offsets:null, storePositions == true?positions:null);
				termToTVE[term] = entry;
				currentSet.Add(entry, entry);
			}
			else
			{
				entry.SetFrequency(entry.GetFrequency() + frequency);
				if (storeOffsets)
				{
					TermVectorOffsetInfo[] existingOffsets = entry.GetOffsets();
					//A few diff. cases here:  offsets is null, existing offsets is null, both are null, same for positions
					if (existingOffsets != null && offsets != null && offsets.Length > 0)
					{
						//copy over the existing offsets
						TermVectorOffsetInfo[] newOffsets = new TermVectorOffsetInfo[existingOffsets.Length + offsets.Length];
						Array.Copy(existingOffsets, 0, newOffsets, 0, existingOffsets.Length);
						Array.Copy(offsets, 0, newOffsets, existingOffsets.Length, offsets.Length);
						entry.SetOffsets(newOffsets);
					}
					else if (existingOffsets == null && offsets != null && offsets.Length > 0)
					{
						entry.SetOffsets(offsets);
					}
					//else leave it alone
				}
				if (storePositions)
				{
					int[] existingPositions = entry.GetPositions();
					if (existingPositions != null && positions != null && positions.Length > 0)
					{
						int[] newPositions = new int[existingPositions.Length + positions.Length];
						Array.Copy(existingPositions, 0, newPositions, 0, existingPositions.Length);
						Array.Copy(positions, 0, newPositions, existingPositions.Length, positions.Length);
						entry.SetPositions(newPositions);
					}
					else if (existingPositions == null && positions != null && positions.Length > 0)
					{
						entry.SetPositions(positions);
					}
				}
			}
		}
		
		public override void  SetExpectations(System.String field, int numTerms, bool storeOffsets, bool storePositions)
		{
			
			this.storeOffsets = storeOffsets;
			this.storePositions = storePositions;
		}
		
		/// <summary> The TermVectorEntrySet.  A SortedSet of {@link TermVectorEntry} objects.  Sort is by the comparator passed into the constructor.
		/// <br/>
		/// This set will be empty until after the mapping process takes place.
		/// 
		/// </summary>
		/// <returns> The SortedSet of {@link TermVectorEntry}.
		/// </returns>
        public virtual System.Collections.Generic.SortedDictionary<Object, Object> GetTermVectorEntrySet()
		{
			return currentSet;
		}
	}
}
