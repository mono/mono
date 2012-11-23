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
	
	/// <summary> Compares {@link Mono.Lucene.Net.Index.TermVectorEntry}s first by frequency and then by
	/// the term (case-sensitive)
	/// 
	/// 
	/// </summary>
	//public class TermVectorEntryFreqSortedComparator : System.Collections.IComparer
    public class TermVectorEntryFreqSortedComparator : System.Collections.Generic.IComparer<System.Object>
	{
		public virtual int Compare(System.Object object_Renamed, System.Object object1)
		{
			int result = 0;
			TermVectorEntry entry = (TermVectorEntry) object_Renamed;
			TermVectorEntry entry1 = (TermVectorEntry) object1;
			result = entry1.GetFrequency() - entry.GetFrequency();
			if (result == 0)
			{
				result = String.CompareOrdinal(entry.GetTerm(), entry1.GetTerm());
				if (result == 0)
				{
					result = String.CompareOrdinal(entry.GetField(), entry1.GetField());
				}
			}
			return result;
		}
	}
}
