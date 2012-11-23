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
	
	/// <summary> For each Field, store a sorted collection of {@link TermVectorEntry}s
	/// <p/>
	/// This is not thread-safe.
	/// </summary>
	public class FieldSortedTermVectorMapper:TermVectorMapper
	{
		private System.Collections.IDictionary fieldToTerms = new System.Collections.Hashtable();
		private System.Collections.Generic.SortedDictionary<object, object> currentSet;
		private System.String currentField;
		private System.Collections.Generic.IComparer<object> comparator;
		
		/// <summary> </summary>
		/// <param name="comparator">A Comparator for sorting {@link TermVectorEntry}s
		/// </param>
		public FieldSortedTermVectorMapper(System.Collections.Generic.IComparer<object> comparator):this(false, false, comparator)
		{
		}
		
		
		public FieldSortedTermVectorMapper(bool ignoringPositions, bool ignoringOffsets, System.Collections.Generic.IComparer<object> comparator):base(ignoringPositions, ignoringOffsets)
		{
			this.comparator = comparator;
		}
		
		public override void  Map(System.String term, int frequency, TermVectorOffsetInfo[] offsets, int[] positions)
		{
			TermVectorEntry entry = new TermVectorEntry(currentField, term, frequency, offsets, positions);
			currentSet.Add(entry, entry);
		}
		
		public override void  SetExpectations(System.String field, int numTerms, bool storeOffsets, bool storePositions)
		{
			currentSet = new System.Collections.Generic.SortedDictionary<object, object>(comparator);
			currentField = field;
			fieldToTerms[field] = currentSet;
		}
		
		/// <summary> Get the mapping between fields and terms, sorted by the comparator
		/// 
		/// </summary>
		/// <returns> A map between field names and {@link java.util.SortedSet}s per field.  SortedSet entries are {@link TermVectorEntry}
		/// </returns>
		public virtual System.Collections.IDictionary GetFieldToTerms()
		{
			return fieldToTerms;
		}
		
		
		public virtual System.Collections.Generic.IComparer<object> GetComparator()
		{
			return comparator;
		}
	}
}
