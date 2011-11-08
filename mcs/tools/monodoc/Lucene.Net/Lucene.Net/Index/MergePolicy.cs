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

using Directory = Mono.Lucene.Net.Store.Directory;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> <p/>Expert: a MergePolicy determines the sequence of
	/// primitive merge operations to be used for overall merge
	/// and optimize operations.<p/>
	/// 
	/// <p/>Whenever the segments in an index have been altered by
	/// {@link IndexWriter}, either the addition of a newly
	/// flushed segment, addition of many segments from
	/// addIndexes* calls, or a previous merge that may now need
	/// to cascade, {@link IndexWriter} invokes {@link
	/// #findMerges} to give the MergePolicy a chance to pick
	/// merges that are now required.  This method returns a
	/// {@link MergeSpecification} instance describing the set of
	/// merges that should be done, or null if no merges are
	/// necessary.  When IndexWriter.optimize is called, it calls
	/// {@link #findMergesForOptimize} and the MergePolicy should
	/// then return the necessary merges.<p/>
	/// 
	/// <p/>Note that the policy can return more than one merge at
	/// a time.  In this case, if the writer is using {@link
	/// SerialMergeScheduler}, the merges will be run
	/// sequentially but if it is using {@link
	/// ConcurrentMergeScheduler} they will be run concurrently.<p/>
	/// 
	/// <p/>The default MergePolicy is {@link
	/// LogByteSizeMergePolicy}.<p/>
	/// 
	/// <p/><b>NOTE:</b> This API is new and still experimental
	/// (subject to change suddenly in the next release)<p/>
	/// 
	/// <p/><b>NOTE</b>: This class typically requires access to
	/// package-private APIs (e.g. <code>SegmentInfos</code>) to do its job;
	/// if you implement your own MergePolicy, you'll need to put
	/// it in package Mono.Lucene.Net.Index in order to use
	/// these APIs.
	/// </summary>
	
	public abstract class MergePolicy
	{
		
		/// <summary>OneMerge provides the information necessary to perform
		/// an individual primitive merge operation, resulting in
		/// a single new segment.  The merge spec includes the
		/// subset of segments to be merged as well as whether the
		/// new segment should use the compound file format. 
		/// </summary>
		
		public class OneMerge
		{
			
			internal SegmentInfo info; // used by IndexWriter
			internal bool mergeDocStores; // used by IndexWriter
			internal bool optimize; // used by IndexWriter
			internal bool registerDone; // used by IndexWriter
			internal long mergeGen; // used by IndexWriter
			internal bool isExternal; // used by IndexWriter
			internal int maxNumSegmentsOptimize; // used by IndexWriter
			internal SegmentReader[] readers; // used by IndexWriter
			internal SegmentReader[] readersClone; // used by IndexWriter
			internal SegmentInfos segments;
			internal bool useCompoundFile;
			internal bool aborted;
			internal System.Exception error;

            internal volatile bool mergeDone;     // used by IndexWriter
			
			public OneMerge(SegmentInfos segments, bool useCompoundFile)
			{
				if (0 == segments.Count)
					throw new System.SystemException("segments must include at least one segment");
				this.segments = segments;
				this.useCompoundFile = useCompoundFile;
			}
			
			/// <summary>Record that an exception occurred while executing
			/// this merge 
			/// </summary>
			internal virtual void  SetException(System.Exception error)
			{
				lock (this)
				{
					this.error = error;
				}
			}
			
			/// <summary>Retrieve previous exception set by {@link
			/// #setException}. 
			/// </summary>
			internal virtual System.Exception GetException()
			{
				lock (this)
				{
					return error;
				}
			}
			
			/// <summary>Mark this merge as aborted.  If this is called
			/// before the merge is committed then the merge will
			/// not be committed. 
			/// </summary>
			internal virtual void  Abort()
			{
				lock (this)
				{
					aborted = true;
				}
			}
			
			/// <summary>Returns true if this merge was aborted. </summary>
			internal virtual bool IsAborted()
			{
				lock (this)
				{
					return aborted;
				}
			}
			
			internal virtual void  CheckAborted(Directory dir)
			{
				lock (this)
				{
					if (aborted)
						throw new MergeAbortedException("merge is aborted: " + SegString(dir));
				}
			}
			
			internal virtual System.String SegString(Directory dir)
			{
				System.Text.StringBuilder b = new System.Text.StringBuilder();
				int numSegments = segments.Count;
				for (int i = 0; i < numSegments; i++)
				{
					if (i > 0)
						b.Append(' ');
					b.Append(segments.Info(i).SegString(dir));
				}
				if (info != null)
					b.Append(" into ").Append(info.name);
				if (optimize)
					b.Append(" [optimize]");
				if (mergeDocStores)
				{
					b.Append(" [mergeDocStores]");
				}
				return b.ToString();
			}

            public SegmentInfos segments_ForNUnit
            {
                get { return segments; }
            }
		}
		
		/// <summary> A MergeSpecification instance provides the information
		/// necessary to perform multiple merges.  It simply
		/// contains a list of {@link OneMerge} instances.
		/// </summary>
		
		public class MergeSpecification
		{
			
			/// <summary> The subset of segments to be included in the primitive merge.</summary>
			
			public System.Collections.IList merges = new System.Collections.ArrayList();
			
			public virtual void  Add(OneMerge merge)
			{
				merges.Add(merge);
			}
			
			public virtual System.String SegString(Directory dir)
			{
				System.Text.StringBuilder b = new System.Text.StringBuilder();
				b.Append("MergeSpec:\n");
				int count = merges.Count;
				for (int i = 0; i < count; i++)
					b.Append("  ").Append(1 + i).Append(": ").Append(((OneMerge) merges[i]).SegString(dir));
				return b.ToString();
			}
		}
		
		/// <summary>Exception thrown if there are any problems while
		/// executing a merge. 
		/// </summary>
		[Serializable]
		public class MergeException:System.SystemException
		{
			private Directory dir;
			/// <deprecated>
			/// Use {@link #MergePolicy.MergeException(String,Directory)} instead 
			/// </deprecated>
            [Obsolete("Use MergePolicy.MergeException(String,Directory) instead ")]
			public MergeException(System.String message):base(message)
			{
			}
			public MergeException(System.String message, Directory dir):base(message)
			{
				this.dir = dir;
			}
			/// <deprecated>
			/// Use {@link #MergePolicy.MergeException(Throwable,Directory)} instead 
			/// </deprecated>
            [Obsolete("Use MergePolicy.MergeException(Throwable,Directory) instead ")]
			public MergeException(System.Exception exc):base(null, exc)
			{
			}
			public MergeException(System.Exception exc, Directory dir):base(null, exc)
			{
				this.dir = dir;
			}
			/// <summary>Returns the {@link Directory} of the index that hit
			/// the exception. 
			/// </summary>
			public virtual Directory GetDirectory()
			{
				return dir;
			}
		}
		
		[Serializable]
		public class MergeAbortedException:System.IO.IOException
		{
			public MergeAbortedException():base("merge is aborted")
			{
			}
			public MergeAbortedException(System.String message):base(message)
			{
			}
		}
		
		protected internal IndexWriter writer;
		
		public MergePolicy(IndexWriter writer)
		{
			this.writer = writer;
		}
		
		/// <summary> Determine what set of merge operations are now necessary on the index.
		/// {@link IndexWriter} calls this whenever there is a change to the segments.
		/// This call is always synchronized on the {@link IndexWriter} instance so
		/// only one thread at a time will call this method.
		/// 
		/// </summary>
		/// <param name="segmentInfos">the total set of segments in the index
		/// </param>
		public abstract MergeSpecification FindMerges(SegmentInfos segmentInfos);
		
		/// <summary> Determine what set of merge operations is necessary in order to optimize
		/// the index. {@link IndexWriter} calls this when its
		/// {@link IndexWriter#Optimize()} method is called. This call is always
		/// synchronized on the {@link IndexWriter} instance so only one thread at a
		/// time will call this method.
		/// 
		/// </summary>
		/// <param name="segmentInfos">the total set of segments in the index
		/// </param>
		/// <param name="maxSegmentCount">requested maximum number of segments in the index (currently this
		/// is always 1)
		/// </param>
		/// <param name="segmentsToOptimize">contains the specific SegmentInfo instances that must be merged
		/// away. This may be a subset of all SegmentInfos.
		/// </param>
		public abstract MergeSpecification FindMergesForOptimize(SegmentInfos segmentInfos, int maxSegmentCount, System.Collections.Hashtable segmentsToOptimize);
		
		/// <summary> Determine what set of merge operations is necessary in order to expunge all
		/// deletes from the index.
		/// 
		/// </summary>
		/// <param name="segmentInfos">the total set of segments in the index
		/// </param>
		public abstract MergeSpecification FindMergesToExpungeDeletes(SegmentInfos segmentInfos);
		
		/// <summary> Release all resources for the policy.</summary>
		public abstract void  Close();
		
		/// <summary> Returns true if a newly flushed (not from merge)
		/// segment should use the compound file format.
		/// </summary>
		public abstract bool UseCompoundFile(SegmentInfos segments, SegmentInfo newSegment);
		
		/// <summary> Returns true if the doc store files should use the
		/// compound file format.
		/// </summary>
		public abstract bool UseCompoundDocStore(SegmentInfos segments);
	}
}
