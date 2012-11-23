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
	
	/// <summary>Remaps docIDs after a merge has completed, where the
	/// merged segments had at least one deletion.  This is used
	/// to renumber the buffered deletes in IndexWriter when a
	/// merge of segments with deletions commits. 
	/// </summary>
	
	sealed class MergeDocIDRemapper
	{
		internal int[] starts; // used for binary search of mapped docID
		internal int[] newStarts; // starts, minus the deletes
		internal int[][] docMaps; // maps docIDs in the merged set
		internal int minDocID; // minimum docID that needs renumbering
		internal int maxDocID; // 1+ the max docID that needs renumbering
		internal int docShift; // total # deleted docs that were compacted by this merge
		
		public MergeDocIDRemapper(SegmentInfos infos, int[][] docMaps, int[] delCounts, MergePolicy.OneMerge merge, int mergedDocCount)
		{
			this.docMaps = docMaps;
			SegmentInfo firstSegment = merge.segments.Info(0);
			int i = 0;
			while (true)
			{
				SegmentInfo info = infos.Info(i);
				if (info.Equals(firstSegment))
					break;
				minDocID += info.docCount;
				i++;
			}
			
			int numDocs = 0;
			for (int j = 0; j < docMaps.Length; i++, j++)
			{
				numDocs += infos.Info(i).docCount;
				System.Diagnostics.Debug.Assert(infos.Info(i).Equals(merge.segments.Info(j)));
			}
			maxDocID = minDocID + numDocs;
			
			starts = new int[docMaps.Length];
			newStarts = new int[docMaps.Length];
			
			starts[0] = minDocID;
			newStarts[0] = minDocID;
			for (i = 1; i < docMaps.Length; i++)
			{
				int lastDocCount = merge.segments.Info(i - 1).docCount;
				starts[i] = starts[i - 1] + lastDocCount;
				newStarts[i] = newStarts[i - 1] + lastDocCount - delCounts[i - 1];
			}
			docShift = numDocs - mergedDocCount;
			
			// There are rare cases when docShift is 0.  It happens
			// if you try to delete a docID that's out of bounds,
			// because the SegmentReader still allocates deletedDocs
			// and pretends it has deletions ... so we can't make
			// this assert here
			// assert docShift > 0;
			
			// Make sure it all adds up:
			System.Diagnostics.Debug.Assert(docShift == maxDocID -(newStarts [docMaps.Length - 1] + merge.segments.Info(docMaps.Length - 1).docCount - delCounts [docMaps.Length - 1]));
		}
		
		public int Remap(int oldDocID)
		{
			if (oldDocID < minDocID)
			// Unaffected by merge
				return oldDocID;
			else if (oldDocID >= maxDocID)
			// This doc was "after" the merge, so simple shift
				return oldDocID - docShift;
			else
			{
				// Binary search to locate this document & find its new docID
				int lo = 0; // search starts array
				int hi = docMaps.Length - 1; // for first element less
				
				while (hi >= lo)
				{
					int mid = SupportClass.Number.URShift((lo + hi), 1);
					int midValue = starts[mid];
					if (oldDocID < midValue)
						hi = mid - 1;
					else if (oldDocID > midValue)
						lo = mid + 1;
					else
					{
						// found a match
						while (mid + 1 < docMaps.Length && starts[mid + 1] == midValue)
						{
							mid++; // scan to last match
						}
						if (docMaps[mid] != null)
							return newStarts[mid] + docMaps[mid][oldDocID - starts[mid]];
						else
							return newStarts[mid] + oldDocID - starts[mid];
					}
				}
				if (docMaps[hi] != null)
					return newStarts[hi] + docMaps[hi][oldDocID - starts[hi]];
				else
					return newStarts[hi] + oldDocID - starts[hi];
			}
		}
	}
}
