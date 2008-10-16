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
namespace Monodoc.Lucene.Net.Index
{
	
	sealed class SegmentMergeInfo
	{
		internal Term term;
		internal int base_Renamed;
		internal TermEnum termEnum;
		internal Monodoc.Lucene.Net.Index.IndexReader reader;
		internal TermPositions postings;
		internal int[] docMap = null; // maps around deleted docs
		
		internal SegmentMergeInfo(int b, TermEnum te, Monodoc.Lucene.Net.Index.IndexReader r)
		{
			base_Renamed = b;
			reader = r;
			termEnum = te;
			term = te.Term();
			postings = reader.TermPositions();
			
			// build array which maps document numbers around deletions 
			if (reader.HasDeletions())
			{
				int maxDoc = reader.MaxDoc();
				docMap = new int[maxDoc];
				int j = 0;
				for (int i = 0; i < maxDoc; i++)
				{
					if (reader.IsDeleted(i))
						docMap[i] = - 1;
					else
						docMap[i] = j++;
				}
			}
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
			postings.Close();
		}
	}
}