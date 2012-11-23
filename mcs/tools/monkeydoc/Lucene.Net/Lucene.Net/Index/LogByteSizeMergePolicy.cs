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
	/// segment as the total byte size of the segment's files. 
	/// </summary>
	public class LogByteSizeMergePolicy:LogMergePolicy
	{
		
		/// <seealso cref="setMinMergeMB">
		/// </seealso>
		public const double DEFAULT_MIN_MERGE_MB = 1.6;
		
		/// <summary>Default maximum segment size.  A segment of this size</summary>
		/// <seealso cref="setMaxMergeMB">
		/// </seealso>
		public static readonly long DEFAULT_MAX_MERGE_MB = System.Int64.MaxValue;
		
		public LogByteSizeMergePolicy(IndexWriter writer):base(writer)
		{
			minMergeSize = (long) (DEFAULT_MIN_MERGE_MB * 1024 * 1024);
            //mgarski - the line below causes an overflow in .NET, resulting in a negative number...
			//maxMergeSize = (long) (DEFAULT_MAX_MERGE_MB * 1024 * 1024);
            maxMergeSize = DEFAULT_MAX_MERGE_MB;
		}
		protected internal override long Size(SegmentInfo info)
		{
			return SizeBytes(info);
		}
		
		/// <summary><p/>Determines the largest segment (measured by total
		/// byte size of the segment's files, in MB) that may be
		/// merged with other segments.  Small values (e.g., less
		/// than 50 MB) are best for interactive indexing, as this
		/// limits the length of pauses while indexing to a few
		/// seconds.  Larger values are best for batched indexing
		/// and speedier searches.<p/>
		/// 
		/// <p/>Note that {@link #setMaxMergeDocs} is also
		/// used to check whether a segment is too large for
		/// merging (it's either or).<p/>
		/// </summary>
		public virtual void  SetMaxMergeMB(double mb)
		{
            //mgarski: java gracefully overflows to Int64.MaxValue, .NET to MinValue...
			maxMergeSize = (long) (mb * 1024 * 1024);
            if (maxMergeSize < 0)
            {
                maxMergeSize = DEFAULT_MAX_MERGE_MB;
            }
		}
		
		/// <summary>Returns the largest segment (meaured by total byte
		/// size of the segment's files, in MB) that may be merged
		/// with other segments.
		/// </summary>
		/// <seealso cref="setMaxMergeMB">
		/// </seealso>
		public virtual double GetMaxMergeMB()
		{
			return ((double) maxMergeSize) / 1024 / 1024;
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
		public virtual void  SetMinMergeMB(double mb)
		{
			minMergeSize = (long) (mb * 1024 * 1024);
		}
		
		/// <summary>Get the minimum size for a segment to remain
		/// un-merged.
		/// </summary>
		/// <seealso cref="setMinMergeMB">
		/// </seealso>
		public virtual double GetMinMergeMB()
		{
			return ((double) minMergeSize) / 1024 / 1024;
		}
	}
}
