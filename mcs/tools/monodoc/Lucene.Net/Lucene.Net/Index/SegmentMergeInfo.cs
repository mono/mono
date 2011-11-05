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
	
	sealed class SegmentMergeInfo
	{
		internal Term term;
		internal int base_Renamed;
		internal int ord; // the position of the segment in a MultiReader
		internal TermEnum termEnum;
		internal IndexReader reader;
		internal int delCount;
		private TermPositions postings; // use getPositions()
		private int[] docMap; // use getDocMap()
		
		internal SegmentMergeInfo(int b, TermEnum te, IndexReader r)
		{
			base_Renamed = b;
			reader = r;
			termEnum = te;
			term = te.Term();
		}
		
		// maps around deleted docs
		internal int[] GetDocMap()
		{
			if (docMap == null)
			{
				delCount = 0;
				// build array which maps document numbers around deletions 
				if (reader.HasDeletions())
				{
					int maxDoc = reader.MaxDoc();
					docMap = new int[maxDoc];
					int j = 0;
					for (int i = 0; i < maxDoc; i++)
					{
						if (reader.IsDeleted(i))
						{
							delCount++;
							docMap[i] = - 1;
						}
						else
							docMap[i] = j++;
					}
				}
			}
			return docMap;
		}
		
		internal TermPositions GetPositions()
		{
			if (postings == null)
			{
				postings = reader.TermPositions();
			}
			return postings;
		}
		
		internal bool Next()
		{
			if (termEnum.Next())
			{
				term = termEnum.Term();
				return true;
			}
			else
			{
				term = null;
				return false;
			}
		}
		
		internal void  Close()
		{
			termEnum.Close();
			if (postings != null)
			{
				postings.Close();
			}
		}
	}
}
