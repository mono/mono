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
	
	/// <summary>This is a {@link LogMergePolicy} that measures size of a
	/// segment as the number of documents (not taking deletions
	/// into account). 
	/// </summary>
	
	public class LogDocMergePolicy:LogMergePolicy
	{
		
		/// <seealso cref="setMinMergeDocs">
		/// </seealso>
		public const int DEFAULT_MIN_MERGE_DOCS = 1000;
		
		public LogDocMergePolicy(IndexWriter writer):base(writer)
		{
			minMergeSize = DEFAULT_MIN_MERGE_DOCS;
			
			// maxMergeSize is never used by LogDocMergePolicy; set
			// it to Long.MAX_VALUE to disable it
			maxMergeSize = System.Int64.MaxValue;
		}
		protected internal override long Size(SegmentInfo info)
		{
			return SizeDocs(info);
		}
		
		/// <summary>Sets the minimum size for the lowest level segments.
		/// Any segments below this size are considered to be on
		/// the same level (even if they vary drastically in size)
		/// and will be merged whenever there are mergeFactor of
		/// them.  This effectively truncates the "long tail" of
		/// small segments that would otherwise be created into a
		/// single level.  If you set this too large, it could
		/// greatly increase the merging cost during indexing (if
		/// you flush many small segments). 
		/// </summary>
		public virtual void  SetMinMergeDocs(int minMergeDocs)
		{
			minMergeSize = minMergeDocs;
		}
		
		/// <summary>Get the minimum size for a segment to remain
		/// un-merged.
		/// </summary>
		/// <seealso cref="setMinMergeDocs">
		/// </seealso>
		public virtual int GetMinMergeDocs()
		{
			return (int) minMergeSize;
		}
	}
}
