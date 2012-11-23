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

using Analyzer = Mono.Lucene.Net.Analysis.Analyzer;
using Document = Mono.Lucene.Net.Documents.Document;
using IndexingChain = Mono.Lucene.Net.Index.DocumentsWriter.IndexingChain;
using AlreadyClosedException = Mono.Lucene.Net.Store.AlreadyClosedException;
using BufferedIndexInput = Mono.Lucene.Net.Store.BufferedIndexInput;
using Directory = Mono.Lucene.Net.Store.Directory;
using FSDirectory = Mono.Lucene.Net.Store.FSDirectory;
using Lock = Mono.Lucene.Net.Store.Lock;
using LockObtainFailedException = Mono.Lucene.Net.Store.LockObtainFailedException;
using Constants = Mono.Lucene.Net.Util.Constants;
using Query = Mono.Lucene.Net.Search.Query;
using Similarity = Mono.Lucene.Net.Search.Similarity;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>An <code>IndexWriter</code> creates and maintains an index.
	/// <p/>The <code>create</code> argument to the {@link
	/// #IndexWriter(Directory, Analyzer, boolean) constructor} determines 
	/// whether a new index is created, or whether an existing index is
	/// opened.  Note that you can open an index with <code>create=true</code>
	/// even while readers are using the index.  The old readers will 
	/// continue to search the "point in time" snapshot they had opened, 
	/// and won't see the newly created index until they re-open.  There are
	/// also {@link #IndexWriter(Directory, Analyzer) constructors}
	/// with no <code>create</code> argument which will create a new index
	/// if there is not already an index at the provided path and otherwise 
	/// open the existing index.<p/>
	/// <p/>In either case, documents are added with {@link #AddDocument(Document)
	/// addDocument} and removed with {@link #DeleteDocuments(Term)} or {@link
	/// #DeleteDocuments(Query)}. A document can be updated with {@link
	/// #UpdateDocument(Term, Document) updateDocument} (which just deletes
	/// and then adds the entire document). When finished adding, deleting 
	/// and updating documents, {@link #Close() close} should be called.<p/>
	/// <a name="flush"></a>
	/// <p/>These changes are buffered in memory and periodically
	/// flushed to the {@link Directory} (during the above method
	/// calls).  A flush is triggered when there are enough
	/// buffered deletes (see {@link #setMaxBufferedDeleteTerms})
	/// or enough added documents since the last flush, whichever
	/// is sooner.  For the added documents, flushing is triggered
	/// either by RAM usage of the documents (see {@link
	/// #setRAMBufferSizeMB}) or the number of added documents.
	/// The default is to flush when RAM usage hits 16 MB.  For
	/// best indexing speed you should flush by RAM usage with a
	/// large RAM buffer.  Note that flushing just moves the
	/// internal buffered state in IndexWriter into the index, but
	/// these changes are not visible to IndexReader until either
	/// {@link #Commit()} or {@link #close} is called.  A flush may
	/// also trigger one or more segment merges which by default
	/// run with a background thread so as not to block the
	/// addDocument calls (see <a href="#mergePolicy">below</a>
	/// for changing the {@link MergeScheduler}).<p/>
	/// <a name="autoCommit"></a>
	/// <p/>The optional <code>autoCommit</code> argument to the {@link
	/// #IndexWriter(Directory, boolean, Analyzer) constructors}
	/// controls visibility of the changes to {@link IndexReader}
	/// instances reading the same index.  When this is
	/// <code>false</code>, changes are not visible until {@link
	/// #Close()} or {@link #Commit()} is called.  Note that changes will still be
	/// flushed to the {@link Directory} as new files, but are 
	/// not committed (no new <code>segments_N</code> file is written 
	/// referencing the new files, nor are the files sync'd to stable storage)
	/// until {@link #Close()} or {@link #Commit()} is called.  If something
	/// goes terribly wrong (for example the JVM crashes), then
	/// the index will reflect none of the changes made since the
	/// last commit, or the starting state if commit was not called.
	/// You can also call {@link #Rollback()}, which closes the writer
	/// without committing any changes, and removes any index
	/// files that had been flushed but are now unreferenced.
	/// This mode is useful for preventing readers from refreshing
	/// at a bad time (for example after you've done all your
	/// deletes but before you've done your adds).  It can also be
	/// used to implement simple single-writer transactional
	/// semantics ("all or none").  You can do a two-phase commit
	/// by calling {@link #PrepareCommit()}
	/// followed by {@link #Commit()}. This is necessary when
	/// Lucene is working with an external resource (for example,
	/// a database) and both must either commit or rollback the
	/// transaction.<p/>
	/// <p/>When <code>autoCommit</code> is <code>true</code> then
	/// the writer will periodically commit on its own.  [<b>Deprecated</b>: Note that in 3.0, IndexWriter will
	/// no longer accept autoCommit=true (it will be hardwired to
	/// false).  You can always call {@link #Commit()} yourself
	/// when needed]. There is
	/// no guarantee when exactly an auto commit will occur (it
	/// used to be after every flush, but it is now after every
	/// completed merge, as of 2.4).  If you want to force a
	/// commit, call {@link #Commit()}, or, close the writer.  Once
	/// a commit has finished, newly opened {@link IndexReader} instances will
	/// see the changes to the index as of that commit.  When
	/// running in this mode, be careful not to refresh your
	/// readers while optimize or segment merges are taking place
	/// as this can tie up substantial disk space.<p/>
	/// </summary>
	/// <summary><p/>Regardless of <code>autoCommit</code>, an {@link
	/// IndexReader} or {@link Mono.Lucene.Net.Search.IndexSearcher} will only see the
	/// index as of the "point in time" that it was opened.  Any
	/// changes committed to the index after the reader was opened
	/// are not visible until the reader is re-opened.<p/>
	/// <p/>If an index will not have more documents added for a while and optimal search
	/// performance is desired, then either the full {@link #Optimize() optimize}
	/// method or partial {@link #Optimize(int)} method should be
	/// called before the index is closed.<p/>
	/// <p/>Opening an <code>IndexWriter</code> creates a lock file for the directory in use. Trying to open
	/// another <code>IndexWriter</code> on the same directory will lead to a
	/// {@link LockObtainFailedException}. The {@link LockObtainFailedException}
	/// is also thrown if an IndexReader on the same directory is used to delete documents
	/// from the index.<p/>
	/// </summary>
	/// <summary><a name="deletionPolicy"></a>
	/// <p/>Expert: <code>IndexWriter</code> allows an optional
	/// {@link IndexDeletionPolicy} implementation to be
	/// specified.  You can use this to control when prior commits
	/// are deleted from the index.  The default policy is {@link
	/// KeepOnlyLastCommitDeletionPolicy} which removes all prior
	/// commits as soon as a new commit is done (this matches
	/// behavior before 2.2).  Creating your own policy can allow
	/// you to explicitly keep previous "point in time" commits
	/// alive in the index for some time, to allow readers to
	/// refresh to the new commit without having the old commit
	/// deleted out from under them.  This is necessary on
	/// filesystems like NFS that do not support "delete on last
	/// close" semantics, which Lucene's "point in time" search
	/// normally relies on. <p/>
	/// <a name="mergePolicy"></a> <p/>Expert:
	/// <code>IndexWriter</code> allows you to separately change
	/// the {@link MergePolicy} and the {@link MergeScheduler}.
	/// The {@link MergePolicy} is invoked whenever there are
	/// changes to the segments in the index.  Its role is to
	/// select which merges to do, if any, and return a {@link
	/// MergePolicy.MergeSpecification} describing the merges.  It
	/// also selects merges to do for optimize().  (The default is
	/// {@link LogByteSizeMergePolicy}.  Then, the {@link
	/// MergeScheduler} is invoked with the requested merges and
	/// it decides when and how to run the merges.  The default is
	/// {@link ConcurrentMergeScheduler}. <p/>
	/// <a name="OOME"></a><p/><b>NOTE</b>: if you hit an
	/// OutOfMemoryError then IndexWriter will quietly record this
	/// fact and block all future segment commits.  This is a
	/// defensive measure in case any internal state (buffered
	/// documents and deletions) were corrupted.  Any subsequent
	/// calls to {@link #Commit()} will throw an
	/// IllegalStateException.  The only course of action is to
	/// call {@link #Close()}, which internally will call {@link
	/// #Rollback()}, to undo any changes to the index since the
	/// last commit.  If you opened the writer with autoCommit
	/// false you can also just call {@link #Rollback()}
	/// directly.<p/>
	/// <a name="thread-safety"></a><p/><b>NOTE</b>: {@link
	/// <code>IndexWriter</code>} instances are completely thread
	/// safe, meaning multiple threads can call any of its
	/// methods, concurrently.  If your application requires
	/// external synchronization, you should <b>not</b>
	/// synchronize on the <code>IndexWriter</code> instance as
	/// this may cause deadlock; use your own (non-Lucene) objects
	/// instead. <p/>
	/// </summary>
	
	/*
	* Clarification: Check Points (and commits)
	* Being able to set autoCommit=false allows IndexWriter to flush and 
	* write new index files to the directory without writing a new segments_N
	* file which references these new files. It also means that the state of 
	* the in memory SegmentInfos object is different than the most recent
	* segments_N file written to the directory.
	* 
	* Each time the SegmentInfos is changed, and matches the (possibly 
	* modified) directory files, we have a new "check point". 
	* If the modified/new SegmentInfos is written to disk - as a new 
	* (generation of) segments_N file - this check point is also an 
	* IndexCommit.
	* 
	* With autoCommit=true, every checkPoint is also a CommitPoint.
	* With autoCommit=false, some checkPoints may not be commits.
	* 
	* A new checkpoint always replaces the previous checkpoint and 
	* becomes the new "front" of the index. This allows the IndexFileDeleter 
	* to delete files that are referenced only by stale checkpoints.
	* (files that were created since the last commit, but are no longer
	* referenced by the "front" of the index). For this, IndexFileDeleter 
	* keeps track of the last non commit checkpoint.
	*/
	public class IndexWriter : System.IDisposable
	{
		private void  InitBlock()
		{
			similarity = Similarity.GetDefault();
			mergePolicy = new LogByteSizeMergePolicy(this);
			readerPool = new ReaderPool(this);
		}
		
		/// <summary> Default value for the write lock timeout (1,000).</summary>
		/// <seealso cref="setDefaultWriteLockTimeout">
		/// </seealso>
		public static long WRITE_LOCK_TIMEOUT = 1000;
		
		private long writeLockTimeout = WRITE_LOCK_TIMEOUT;
		
		/// <summary> Name of the write lock in the index.</summary>
		public const System.String WRITE_LOCK_NAME = "write.lock";
		
		/// <deprecated>
		/// </deprecated>
		/// <seealso cref="LogMergePolicy.DEFAULT_MERGE_FACTOR">
		/// </seealso>
        [Obsolete("See LogMergePolicy.DEFAULT_MERGE_FACTOR")]
		public static readonly int DEFAULT_MERGE_FACTOR;
		
		/// <summary> Value to denote a flush trigger is disabled</summary>
		public const int DISABLE_AUTO_FLUSH = - 1;
		
		/// <summary> Disabled by default (because IndexWriter flushes by RAM usage
		/// by default). Change using {@link #SetMaxBufferedDocs(int)}.
		/// </summary>
		public static readonly int DEFAULT_MAX_BUFFERED_DOCS = DISABLE_AUTO_FLUSH;
		
		/// <summary> Default value is 16 MB (which means flush when buffered
		/// docs consume 16 MB RAM).  Change using {@link #setRAMBufferSizeMB}.
		/// </summary>
		public const double DEFAULT_RAM_BUFFER_SIZE_MB = 16.0;
		
		/// <summary> Disabled by default (because IndexWriter flushes by RAM usage
		/// by default). Change using {@link #SetMaxBufferedDeleteTerms(int)}.
		/// </summary>
		public static readonly int DEFAULT_MAX_BUFFERED_DELETE_TERMS = DISABLE_AUTO_FLUSH;
		
		/// <deprecated>
		/// </deprecated>
		/// <seealso cref="LogDocMergePolicy.DEFAULT_MAX_MERGE_DOCS">
		/// </seealso>
        [Obsolete("See LogDocMergePolicy.DEFAULT_MAX_MERGE_DOCS")]
		public static readonly int DEFAULT_MAX_MERGE_DOCS;
		
		/// <summary> Default value is 10,000. Change using {@link #SetMaxFieldLength(int)}.</summary>
		public const int DEFAULT_MAX_FIELD_LENGTH = 10000;
		
		/// <summary> Default value is 128. Change using {@link #SetTermIndexInterval(int)}.</summary>
		public const int DEFAULT_TERM_INDEX_INTERVAL = 128;
		
		/// <summary> Absolute hard maximum length for a term.  If a term
		/// arrives from the analyzer longer than this length, it
		/// is skipped and a message is printed to infoStream, if
		/// set (see {@link #setInfoStream}).
		/// </summary>
		public static readonly int MAX_TERM_LENGTH;
		
		/// <summary> Default for {@link #getMaxSyncPauseSeconds}.  On
		/// Windows this defaults to 10.0 seconds; elsewhere it's
		/// 0.
		/// </summary>
		public static double DEFAULT_MAX_SYNC_PAUSE_SECONDS;
		
		// The normal read buffer size defaults to 1024, but
		// increasing this during merging seems to yield
		// performance gains.  However we don't want to increase
		// it too much because there are quite a few
		// BufferedIndexInputs created during merging.  See
		// LUCENE-888 for details.
		private const int MERGE_READ_BUFFER_SIZE = 4096;
		
		// Used for printing messages
		private static System.Object MESSAGE_ID_LOCK = new System.Object();
		private static int MESSAGE_ID = 0;
		private int messageID = - 1;
		private volatile bool hitOOM;
		
		private Directory directory; // where this index resides
		private Analyzer analyzer; // how to analyze text
		
		private Similarity similarity; // how to normalize
		
		private volatile uint changeCount; // increments every time a change is completed
		private long lastCommitChangeCount; // last changeCount that was committed
		
		private SegmentInfos rollbackSegmentInfos; // segmentInfos we will fallback to if the commit fails
		private System.Collections.Hashtable rollbackSegments;
		
		internal volatile SegmentInfos pendingCommit; // set when a commit is pending (after prepareCommit() & before commit())
		internal volatile uint pendingCommitChangeCount;
		
		private SegmentInfos localRollbackSegmentInfos; // segmentInfos we will fallback to if the commit fails
		private bool localAutoCommit; // saved autoCommit during local transaction
		private int localFlushedDocCount; // saved docWriter.getFlushedDocCount during local transaction
		private bool autoCommit = true; // false if we should commit only on close
		
		private SegmentInfos segmentInfos = new SegmentInfos(); // the segments
        private int optimizeMaxNumSegments;

		private DocumentsWriter docWriter;
		private IndexFileDeleter deleter;

        private System.Collections.Hashtable segmentsToOptimize = new System.Collections.Hashtable(); // used by optimize to note those needing optimization
		
		private Lock writeLock;
		
		private int termIndexInterval = DEFAULT_TERM_INDEX_INTERVAL;
		
		private bool closeDir;
		private bool closed;
		private bool closing;
		
		// Holds all SegmentInfo instances currently involved in
		// merges
        private System.Collections.Hashtable mergingSegments = new System.Collections.Hashtable();
		
		private MergePolicy mergePolicy;
		private MergeScheduler mergeScheduler = new ConcurrentMergeScheduler();
        private System.Collections.Generic.LinkedList<MergePolicy.OneMerge> pendingMerges = new System.Collections.Generic.LinkedList<MergePolicy.OneMerge>();
		private System.Collections.Generic.List<MergePolicy.OneMerge> runningMerges = new System.Collections.Generic.List<MergePolicy.OneMerge>();
		private System.Collections.IList mergeExceptions = new System.Collections.ArrayList();
		private long mergeGen;
		private bool stopMerges;
		
		private int flushCount;
		private int flushDeletesCount;
		private double maxSyncPauseSeconds = DEFAULT_MAX_SYNC_PAUSE_SECONDS;
		
		// Used to only allow one addIndexes to proceed at once
		// TODO: use ReadWriteLock once we are on 5.0
		private int readCount; // count of how many threads are holding read lock
		private SupportClass.ThreadClass writeThread; // non-null if any thread holds write lock
		internal ReaderPool readerPool;
		private int upgradeCount;

        private int readerTermsIndexDivisor = IndexReader.DEFAULT_TERMS_INDEX_DIVISOR;
		
		// This is a "write once" variable (like the organic dye
		// on a DVD-R that may or may not be heated by a laser and
		// then cooled to permanently record the event): it's
		// false, until getReader() is called for the first time,
		// at which point it's switched to true and never changes
		// back to false.  Once this is true, we hold open and
		// reuse SegmentReader instances internally for applying
		// deletes, doing merges, and reopening near real-time
		// readers.
		private volatile bool poolReaders;
		
		/// <summary> Expert: returns a readonly reader, covering all committed as well as
		/// un-committed changes to the index. This provides "near real-time"
		/// searching, in that changes made during an IndexWriter session can be
		/// quickly made available for searching without closing the writer nor
		/// calling {@link #commit}.
		/// 
		/// <p/>
		/// Note that this is functionally equivalent to calling {#commit} and then
		/// using {@link IndexReader#open} to open a new reader. But the turarnound
		/// time of this method should be faster since it avoids the potentially
		/// costly {@link #commit}.
		/// <p/>
		/// 
        /// You must close the {@link IndexReader} returned by  this method once you are done using it.
        /// 
		/// <p/>
		/// It's <i>near</i> real-time because there is no hard
		/// guarantee on how quickly you can get a new reader after
		/// making changes with IndexWriter.  You'll have to
		/// experiment in your situation to determine if it's
		/// faster enough.  As this is a new and experimental
		/// feature, please report back on your findings so we can
		/// learn, improve and iterate.<p/>
		/// 
		/// <p/>The resulting reader suppports {@link
		/// IndexReader#reopen}, but that call will simply forward
		/// back to this method (though this may change in the
		/// future).<p/>
		/// 
		/// <p/>The very first time this method is called, this
		/// writer instance will make every effort to pool the
		/// readers that it opens for doing merges, applying
		/// deletes, etc.  This means additional resources (RAM,
		/// file descriptors, CPU time) will be consumed.<p/>
		/// 
		/// <p/>For lower latency on reopening a reader, you should call {@link #setMergedSegmentWarmer} 
        /// to call {@link #setMergedSegmentWarmer} to
		/// pre-warm a newly merged segment before it's committed
		/// to the index. This is important for minimizing index-to-search 
        /// delay after a large merge.
		/// 
		/// <p/>If an addIndexes* call is running in another thread,
		/// then this reader will only search those segments from
		/// the foreign index that have been successfully copied
		/// over, so far<p/>.
		/// 
		/// <p/><b>NOTE</b>: Once the writer is closed, any
		/// outstanding readers may continue to be used.  However,
		/// if you attempt to reopen any of those readers, you'll
		/// hit an {@link AlreadyClosedException}.<p/>
		/// 
		/// <p/><b>NOTE:</b> This API is experimental and might
		/// change in incompatible ways in the next release.<p/>
		/// 
		/// </summary>
		/// <returns> IndexReader that covers entire index plus all
		/// changes made so far by this IndexWriter instance
		/// 
		/// </returns>
		/// <throws>  IOException </throws>
		public virtual IndexReader GetReader()
		{
            return GetReader(readerTermsIndexDivisor);
		}
		
		/// <summary>Expert: like {@link #getReader}, except you can
		/// specify which termInfosIndexDivisor should be used for
		/// any newly opened readers.
		/// </summary>
		/// <param name="termInfosIndexDivisor">Subsambles which indexed
		/// terms are loaded into RAM. This has the same effect as {@link
		/// IndexWriter#setTermIndexInterval} except that setting
		/// must be done at indexing time while this setting can be
		/// set per reader.  When set to N, then one in every
		/// N*termIndexInterval terms in the index is loaded into
		/// memory.  By setting this to a value > 1 you can reduce
		/// memory usage, at the expense of higher latency when
		/// loading a TermInfo.  The default value is 1.  Set this
		/// to -1 to skip loading the terms index entirely. 
		/// </param>
		public virtual IndexReader GetReader(int termInfosIndexDivisor)
		{
            EnsureOpen();

			if (infoStream != null)
			{
				Message("flush at getReader");
			}
			
			// Do this up front before flushing so that the readers
			// obtained during this flush are pooled, the first time
			// this method is called:
			poolReaders = true;
			
			// Prevent segmentInfos from changing while opening the
			// reader; in theory we could do similar retry logic,
			// just like we do when loading segments_N
            IndexReader r;
			lock (this)
			{
                Flush(false, true, true);
                r = new ReadOnlyDirectoryReader(this, segmentInfos, termInfosIndexDivisor);
			}
            MaybeMerge();
            return r;
		}
		
		/// <summary>Holds shared SegmentReader instances. IndexWriter uses
		/// SegmentReaders for 1) applying deletes, 2) doing
		/// merges, 3) handing out a real-time reader.  This pool
		/// reuses instances of the SegmentReaders in all these
		/// places if it is in "near real-time mode" (getReader()
		/// has been called on this instance). 
		/// </summary>
		
		internal class ReaderPool
		{
			public ReaderPool(IndexWriter enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(IndexWriter enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private IndexWriter enclosingInstance;
			public IndexWriter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			private System.Collections.IDictionary readerMap = new System.Collections.Hashtable();
			
			/// <summary>Forcefully clear changes for the specifed segments,
			/// and remove from the pool.   This is called on succesful merge. 
			/// </summary>
			internal virtual void  Clear(SegmentInfos infos)
			{
				lock (this)
				{
					if (infos == null)
					{
                        System.Collections.IEnumerator iter = new System.Collections.Hashtable(readerMap).GetEnumerator();
						while (iter.MoveNext())
						{
							System.Collections.DictionaryEntry ent = (System.Collections.DictionaryEntry) iter.Current;
							((SegmentReader) ent.Value).hasChanges = false;
						}
					}
					else
					{
						int numSegments = infos.Count;
						for (int i = 0; i < numSegments; i++)
						{
							SegmentInfo info = infos.Info(i);
							if (readerMap.Contains(info))
							{
								((SegmentReader) readerMap[info]).hasChanges = false;
							}
						}
					}
				}
			}
			
			// used only by asserts
			public virtual bool InfoIsLive(SegmentInfo info)
			{
				lock (this)
				{
					int idx = Enclosing_Instance.segmentInfos.IndexOf(info);
					System.Diagnostics.Debug.Assert(idx != -1);
                    System.Diagnostics.Debug.Assert(Enclosing_Instance.segmentInfos[idx] == info);
					return true;
				}
			}
			
			public virtual SegmentInfo MapToLive(SegmentInfo info)
			{
				lock (this)
				{
					int idx = Enclosing_Instance.segmentInfos.IndexOf(info);
					if (idx != - 1)
					{
						info = (SegmentInfo) Enclosing_Instance.segmentInfos[idx];
					}
					return info;
				}
			}
			
			/// <summary> Release the segment reader (i.e. decRef it and close if there
			/// are no more references.
			/// </summary>
			/// <param name="sr">
			/// </param>
			/// <throws>  IOException </throws>
			public virtual void  Release(SegmentReader sr)
			{
				lock (this)
				{
					Release(sr, false);
				}
			}
			
			/// <summary> Release the segment reader (i.e. decRef it and close if there
			/// are no more references.
			/// </summary>
			/// <param name="sr">
			/// </param>
			/// <throws>  IOException </throws>
			public virtual void  Release(SegmentReader sr, bool drop)
			{
				lock (this)
				{
					
					bool pooled = readerMap.Contains(sr.GetSegmentInfo());

                    System.Diagnostics.Debug.Assert(!pooled || readerMap[sr.GetSegmentInfo()] == sr);

                    // Drop caller's ref; for an external reader (not
                    // pooled), this decRef will close it
					sr.DecRef();
					
					if (pooled && (drop || (!Enclosing_Instance.poolReaders && sr.GetRefCount() == 1)))
					{

                        // We invoke deleter.checkpoint below, so we must be
                        // sync'd on IW if there are changes:
						
						// TODO: java 5
						// assert !sr.hasChanges || Thread.holdsLock(IndexWriter.this);

                        // Discard (don't save) changes when we are dropping
                        // the reader; this is used only on the sub-readers
                        // after a successful merge.
                        sr.hasChanges &= !drop;

                        bool hasChanges = sr.hasChanges;
						
						// Drop our ref -- this will commit any pending
						// changes to the dir
                        sr.Close();

                        // We are the last ref to this reader; since we're
                        // not pooling readers, we release it:
                        readerMap.Remove(sr.GetSegmentInfo());

                        if (hasChanges)
                        {
                            // Must checkpoint w/ deleter, because this
                            // segment reader will have created new _X_N.del
                            // file.
                            enclosingInstance.deleter.Checkpoint(enclosingInstance.segmentInfos, false);
                        }
					}
				}
			}
			
			/// <summary>Remove all our references to readers, and commits
			/// any pending changes. 
			/// </summary>
			internal virtual void  Close()
			{
				lock (this)
				{
                    System.Collections.IEnumerator iter = new System.Collections.Hashtable(readerMap).GetEnumerator();
					while (iter.MoveNext())
					{
						System.Collections.DictionaryEntry ent = (System.Collections.DictionaryEntry) iter.Current;
						
						SegmentReader sr = (SegmentReader) ent.Value;
						if (sr.hasChanges)
						{
							System.Diagnostics.Debug.Assert(InfoIsLive(sr.GetSegmentInfo()));
							sr.DoCommit(null);
                            // Must checkpoint w/ deleter, because this
                            // segment reader will have created new _X_N.del
                            // file.
                            enclosingInstance.deleter.Checkpoint(enclosingInstance.segmentInfos, false);
						}

                        readerMap.Remove(ent.Key); 
						
						// NOTE: it is allowed that this decRef does not
						// actually close the SR; this can happen when a
						// near real-time reader is kept open after the
						// IndexWriter instance is closed
						sr.DecRef();
					}
				}
			}
			
			/// <summary> Commit all segment reader in the pool.</summary>
			/// <throws>  IOException </throws>
			internal virtual void  Commit()
			{
				lock (this)
				{
                    System.Collections.IEnumerator iter = new System.Collections.Hashtable(readerMap).GetEnumerator();
					while (iter.MoveNext())
					{
						System.Collections.DictionaryEntry ent = (System.Collections.DictionaryEntry) iter.Current;
						
						SegmentReader sr = (SegmentReader) ent.Value;
						if (sr.hasChanges)
						{
							System.Diagnostics.Debug.Assert(InfoIsLive(sr.GetSegmentInfo()));
							sr.DoCommit(null);
                            // Must checkpoint w/ deleter, because this
                            // segment reader will have created new _X_N.del
                            // file.
                            enclosingInstance.deleter.Checkpoint(enclosingInstance.segmentInfos, false);
						}
					}
				}
			}
			
			/// <summary> Returns a ref to a clone.  NOTE: this clone is not
			/// enrolled in the pool, so you should simply close()
			/// it when you're done (ie, do not call release()).
			/// </summary>
			public virtual SegmentReader GetReadOnlyClone(SegmentInfo info, bool doOpenStores, int termInfosIndexDivisor)
			{
				lock (this)
				{
					SegmentReader sr = Get(info, doOpenStores, BufferedIndexInput.BUFFER_SIZE, termInfosIndexDivisor);
					try
					{
						return (SegmentReader) sr.Clone(true);
					}
					finally
					{
						sr.DecRef();
					}
				}
			}
			
			/// <summary> Obtain a SegmentReader from the readerPool.  The reader
			/// must be returned by calling {@link #Release(SegmentReader)}
			/// </summary>
			/// <seealso cref="Release(SegmentReader)">
			/// </seealso>
			/// <param name="info">
			/// </param>
			/// <param name="doOpenStores">
			/// </param>
			/// <throws>  IOException </throws>
			public virtual SegmentReader Get(SegmentInfo info, bool doOpenStores)
			{
				lock (this)
				{
                    return Get(info, doOpenStores, BufferedIndexInput.BUFFER_SIZE, enclosingInstance.readerTermsIndexDivisor);
				}
			}
			/// <summary> Obtain a SegmentReader from the readerPool.  The reader
			/// must be returned by calling {@link #Release(SegmentReader)}
			/// 
			/// </summary>
			/// <seealso cref="Release(SegmentReader)">
			/// </seealso>
			/// <param name="info">
			/// </param>
			/// <param name="doOpenStores">
			/// </param>
			/// <param name="readBufferSize">
			/// </param>
			/// <param name="termsIndexDivisor">
			/// </param>
			/// <throws>  IOException </throws>
			public virtual SegmentReader Get(SegmentInfo info, bool doOpenStores, int readBufferSize, int termsIndexDivisor)
			{
				lock (this)
				{
					
					if (Enclosing_Instance.poolReaders)
					{
						readBufferSize = BufferedIndexInput.BUFFER_SIZE;
					}
					
					SegmentReader sr = (SegmentReader) readerMap[info];
					if (sr == null)
					{
						// TODO: we may want to avoid doing this while
						// synchronized
						// Returns a ref, which we xfer to readerMap:
						sr = SegmentReader.Get(info, readBufferSize, doOpenStores, termsIndexDivisor);
                        if (info.dir == enclosingInstance.directory)
                        {
                            // Only pool if reader is not external
                            readerMap[info]=sr;
                        }
					}
					else
					{
						if (doOpenStores)
						{
							sr.OpenDocStores();
						}
						if (termsIndexDivisor != - 1 && !sr.TermsIndexLoaded())
						{
							// If this reader was originally opened because we
							// needed to merge it, we didn't load the terms
							// index.  But now, if the caller wants the terms
							// index (eg because it's doing deletes, or an NRT
							// reader is being opened) we ask the reader to
							// load its terms index.
							sr.LoadTermsIndex(termsIndexDivisor);
						}
					}
					
					// Return a ref to our caller
                    if (info.dir == enclosingInstance.directory)
                    {
                        // Only incRef if we pooled (reader is not external)
                        sr.IncRef();
                    }
					return sr;
				}
			}
			
			// Returns a ref
			public virtual SegmentReader GetIfExists(SegmentInfo info)
			{
				lock (this)
				{
					SegmentReader sr = (SegmentReader) readerMap[info];
					if (sr != null)
					{
						sr.IncRef();
					}
					return sr;
				}
			}
		}
		
		/// <summary> Obtain the number of deleted docs for a pooled reader.
		/// If the reader isn't being pooled, the segmentInfo's 
		/// delCount is returned.
		/// </summary>
		public virtual int NumDeletedDocs(SegmentInfo info)
		{
			SegmentReader reader = readerPool.GetIfExists(info);
			try
			{
				if (reader != null)
				{
					return reader.NumDeletedDocs();
				}
				else
				{
					return info.GetDelCount();
				}
			}
			finally
			{
				if (reader != null)
				{
					readerPool.Release(reader);
				}
			}
		}
		
		internal virtual void  AcquireWrite()
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(writeThread != SupportClass.ThreadClass.Current());
				while (writeThread != null || readCount > 0)
					DoWait();
				
				// We could have been closed while we were waiting:
				EnsureOpen();
				
				writeThread = SupportClass.ThreadClass.Current();
			}
		}
		
		internal virtual void  ReleaseWrite()
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(SupportClass.ThreadClass.Current() == writeThread);
				writeThread = null;
				System.Threading.Monitor.PulseAll(this);
			}
		}
		
		internal virtual void  AcquireRead()
		{
			lock (this)
			{
				SupportClass.ThreadClass current = SupportClass.ThreadClass.Current();
				while (writeThread != null && writeThread != current)
					DoWait();
				
				readCount++;
			}
		}
		
		// Allows one readLock to upgrade to a writeLock even if
		// there are other readLocks as long as all other
		// readLocks are also blocked in this method:
		internal virtual void  UpgradeReadToWrite()
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(readCount > 0);
				upgradeCount++;
				while (readCount > upgradeCount || writeThread != null)
				{
					DoWait();
				}
				
				writeThread = SupportClass.ThreadClass.Current();
				readCount--;
				upgradeCount--;
			}
		}
		
		internal virtual void  ReleaseRead()
		{
			lock (this)
			{
				readCount--;
				System.Diagnostics.Debug.Assert(readCount >= 0);
				System.Threading.Monitor.PulseAll(this);
			}
		}
		
		internal bool IsOpen(bool includePendingClose)
		{
			lock (this)
			{
				return !(closed || (includePendingClose && closing));
			}
		}
		
		/// <summary> Used internally to throw an {@link
		/// AlreadyClosedException} if this IndexWriter has been
		/// closed.
		/// </summary>
		/// <throws>  AlreadyClosedException if this IndexWriter is </throws>
		protected internal void  EnsureOpen(bool includePendingClose)
		{
			lock (this)
			{
				if (!IsOpen(includePendingClose))
				{
					throw new AlreadyClosedException("this IndexWriter is closed");
				}
			}
		}
		
		protected internal void  EnsureOpen()
		{
			lock (this)
			{
				EnsureOpen(true);
			}
		}
		
		/// <summary> Prints a message to the infoStream (if non-null),
		/// prefixed with the identifying information for this
		/// writer and the thread that's calling it.
		/// </summary>
		public virtual void  Message(System.String message)
		{
			if (infoStream != null)
                infoStream.WriteLine("IW " + messageID + " [" + DateTime.Now.ToString() + "; " + SupportClass.ThreadClass.Current().Name + "]: " + message);
		}
		
		private void  SetMessageID(System.IO.StreamWriter infoStream)
		{
			lock (this)
			{
				if (infoStream != null && messageID == - 1)
				{
					lock (MESSAGE_ID_LOCK)
					{
						messageID = MESSAGE_ID++;
					}
				}
				this.infoStream = infoStream;
			}
		}
		
		/// <summary> Casts current mergePolicy to LogMergePolicy, and throws
		/// an exception if the mergePolicy is not a LogMergePolicy.
		/// </summary>
		private LogMergePolicy GetLogMergePolicy()
		{
			if (mergePolicy is LogMergePolicy)
				return (LogMergePolicy) mergePolicy;
			else
				throw new System.ArgumentException("this method can only be called when the merge policy is the default LogMergePolicy");
		}
		
		/// <summary><p/>Get the current setting of whether newly flushed
		/// segments will use the compound file format.  Note that
		/// this just returns the value previously set with
		/// setUseCompoundFile(boolean), or the default value
		/// (true).  You cannot use this to query the status of
		/// previously flushed segments.<p/>
		/// 
		/// <p/>Note that this method is a convenience method: it
		/// just calls mergePolicy.getUseCompoundFile as long as
		/// mergePolicy is an instance of {@link LogMergePolicy}.
		/// Otherwise an IllegalArgumentException is thrown.<p/>
		/// 
		/// </summary>
		/// <seealso cref="SetUseCompoundFile(boolean)">
		/// </seealso>
		public virtual bool GetUseCompoundFile()
		{
			return GetLogMergePolicy().GetUseCompoundFile();
		}
		
		/// <summary><p/>Setting to turn on usage of a compound file. When on,
		/// multiple files for each segment are merged into a
		/// single file when a new segment is flushed.<p/>
		/// 
		/// <p/>Note that this method is a convenience method: it
		/// just calls mergePolicy.setUseCompoundFile as long as
		/// mergePolicy is an instance of {@link LogMergePolicy}.
		/// Otherwise an IllegalArgumentException is thrown.<p/>
		/// </summary>
		public virtual void  SetUseCompoundFile(bool value_Renamed)
		{
			GetLogMergePolicy().SetUseCompoundFile(value_Renamed);
			GetLogMergePolicy().SetUseCompoundDocStore(value_Renamed);
		}
		
		/// <summary>Expert: Set the Similarity implementation used by this IndexWriter.
		/// 
		/// </summary>
		/// <seealso cref="Similarity.SetDefault(Similarity)">
		/// </seealso>
		public virtual void  SetSimilarity(Similarity similarity)
		{
			EnsureOpen();
			this.similarity = similarity;
			docWriter.SetSimilarity(similarity);
		}
		
		/// <summary>Expert: Return the Similarity implementation used by this IndexWriter.
		/// 
		/// <p/>This defaults to the current value of {@link Similarity#GetDefault()}.
		/// </summary>
		public virtual Similarity GetSimilarity()
		{
			EnsureOpen();
			return this.similarity;
		}
		
		/// <summary>Expert: Set the interval between indexed terms.  Large values cause less
		/// memory to be used by IndexReader, but slow random-access to terms.  Small
		/// values cause more memory to be used by an IndexReader, and speed
		/// random-access to terms.
		/// 
		/// This parameter determines the amount of computation required per query
		/// term, regardless of the number of documents that contain that term.  In
		/// particular, it is the maximum number of other terms that must be
		/// scanned before a term is located and its frequency and position information
		/// may be processed.  In a large index with user-entered query terms, query
		/// processing time is likely to be dominated not by term lookup but rather
		/// by the processing of frequency and positional data.  In a small index
		/// or when many uncommon query terms are generated (e.g., by wildcard
		/// queries) term lookup may become a dominant cost.
		/// 
		/// In particular, <code>numUniqueTerms/interval</code> terms are read into
		/// memory by an IndexReader, and, on average, <code>interval/2</code> terms
		/// must be scanned for each random term access.
		/// 
		/// </summary>
		/// <seealso cref="DEFAULT_TERM_INDEX_INTERVAL">
		/// </seealso>
		public virtual void  SetTermIndexInterval(int interval)
		{
			EnsureOpen();
			this.termIndexInterval = interval;
		}
		
		/// <summary>Expert: Return the interval between indexed terms.
		/// 
		/// </summary>
		/// <seealso cref="SetTermIndexInterval(int)">
		/// </seealso>
		public virtual int GetTermIndexInterval()
		{
			// We pass false because this method is called by SegmentMerger while we are in the process of closing
			EnsureOpen(false);
			return termIndexInterval;
		}
		
		/// <summary> Constructs an IndexWriter for the index in <code>path</code>.
		/// Text will be analyzed with <code>a</code>.  If <code>create</code>
		/// is true, then a new, empty index will be created in
		/// <code>path</code>, replacing the index already there,
		/// if any.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <param name="mfl">Maximum field length in number of tokens/terms: LIMITED, UNLIMITED, or user-specified
		/// via the MaxFieldLength constructor.
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> Use {@link #IndexWriter(Directory, Analyzer,
		/// boolean, MaxFieldLength)}
        /// </deprecated>
        [Obsolete("Use IndexWriter(Directory, Analyzer,boolean, MaxFieldLength)")]
        public IndexWriter(System.String path, Analyzer a, bool create, MaxFieldLength mfl)
		{
			InitBlock();
			Init(FSDirectory.GetDirectory(path), a, create, true, null, false, mfl.GetLimit(), null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in <code>path</code>.
		/// Text will be analyzed with <code>a</code>.  If <code>create</code>
		/// is true, then a new, empty index will be created in
		/// <code>path</code>, replacing the index already there, if any.
		/// 
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0 release.
		/// Use {@link
		/// #IndexWriter(Directory,Analyzer,boolean,MaxFieldLength)}
		/// instead, and call {@link #Commit()} when needed.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release. Use IndexWriter(Directory,Analyzer,bool,MaxFieldLength) instead, and call Commit() when needed")]
		public IndexWriter(System.String path, Analyzer a, bool create)
		{
			InitBlock();
			Init(FSDirectory.GetDirectory(path), a, create, true, null, true, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in <code>path</code>.
		/// Text will be analyzed with <code>a</code>.  If <code>create</code>
		/// is true, then a new, empty index will be created in
		/// <code>path</code>, replacing the index already there, if any.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <param name="mfl">Maximum field length in number of terms/tokens: LIMITED, UNLIMITED, or user-specified
		/// via the MaxFieldLength constructor.
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> Use {@link #IndexWriter(Directory,
		/// Analyzer, boolean, MaxFieldLength)}
		/// </deprecated>
        [Obsolete("Use IndexWriter(Directory, Analyzer, boolean, MaxFieldLength)")]
		public IndexWriter(System.IO.FileInfo path, Analyzer a, bool create, MaxFieldLength mfl)
		{
			InitBlock();
			Init(FSDirectory.GetDirectory(path), a, create, true, null, false, mfl.GetLimit(), null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in <code>path</code>.
		/// Text will be analyzed with <code>a</code>.  If <code>create</code>
		/// is true, then a new, empty index will be created in
		/// <code>path</code>, replacing the index already there, if any.
		/// 
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0 release.
		/// Use {@link
		/// #IndexWriter(Directory,Analyzer,boolean,MaxFieldLength)}
		/// instead, and call {@link #Commit()} when needed.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release. Use IndexWriter(Directory,Analyzer,bool,MaxFieldLength) instead, and call Commit() when needed.")]
		public IndexWriter(System.IO.FileInfo path, Analyzer a, bool create)
		{
			InitBlock();
			Init(FSDirectory.GetDirectory(path), a, create, true, null, true, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in <code>d</code>.
		/// Text will be analyzed with <code>a</code>.  If <code>create</code>
		/// is true, then a new, empty index will be created in
		/// <code>d</code>, replacing the index already there, if any.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <param name="mfl">Maximum field length in number of terms/tokens: LIMITED, UNLIMITED, or user-specified
		/// via the MaxFieldLength constructor.
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		public IndexWriter(Directory d, Analyzer a, bool create, MaxFieldLength mfl)
		{
			InitBlock();
			Init(d, a, create, false, null, false, mfl.GetLimit(), null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in <code>d</code>.
		/// Text will be analyzed with <code>a</code>.  If <code>create</code>
		/// is true, then a new, empty index will be created in
		/// <code>d</code>, replacing the index already there, if any.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0
		/// release, and call {@link #Commit()} when needed.
		/// Use {@link #IndexWriter(Directory,Analyzer,boolean,MaxFieldLength)} instead.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release, and call Commit() when needed. Use IndexWriter(Directory,Analyzer,bool,MaxFieldLength) instead.")]
		public IndexWriter(Directory d, Analyzer a, bool create)
		{
			InitBlock();
			Init(d, a, create, false, null, true, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in
		/// <code>path</code>, first creating it if it does not
		/// already exist.  Text will be analyzed with
		/// <code>a</code>.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="mfl">Maximum field length in number of terms/tokens: LIMITED, UNLIMITED, or user-specified
		/// via the MaxFieldLength constructor.
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> Use {@link #IndexWriter(Directory, Analyzer, MaxFieldLength)}
		/// </deprecated>
        [Obsolete("Use IndexWriter(Directory, Analyzer, MaxFieldLength)")]
		public IndexWriter(System.String path, Analyzer a, MaxFieldLength mfl)
		{
			InitBlock();
			Init(FSDirectory.GetDirectory(path), a, true, null, false, mfl.GetLimit(), null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in
		/// <code>path</code>, first creating it if it does not
		/// already exist.  Text will be analyzed with
		/// <code>a</code>.
		/// 
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0
		/// release, and call {@link #Commit()} when needed.
		/// Use {@link #IndexWriter(Directory,Analyzer,MaxFieldLength)} instead.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release, and call Commit() when needed. Use IndexWriter(Directory,Analyzer,MaxFieldLength) instead.")]
		public IndexWriter(System.String path, Analyzer a)
		{
			InitBlock();
			Init(FSDirectory.GetDirectory(path), a, true, null, true, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in
		/// <code>path</code>, first creating it if it does not
		/// already exist.  Text will be analyzed with
		/// <code>a</code>.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="mfl">Maximum field length in number of terms/tokens: LIMITED, UNLIMITED, or user-specified
		/// via the MaxFieldLength constructor.
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> Use {@link #IndexWriter(Directory,
		/// Analyzer, MaxFieldLength)}
		/// </deprecated>
        [Obsolete("Use {@link #IndexWriter(Directory,Analyzer, MaxFieldLength)")]
		public IndexWriter(System.IO.FileInfo path, Analyzer a, MaxFieldLength mfl)
		{
			InitBlock();
			Init(FSDirectory.GetDirectory(path), a, true, null, false, mfl.GetLimit(), null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in
		/// <code>path</code>, first creating it if it does not
		/// already exist.  Text will be analyzed with
		/// <code>a</code>.
		/// 
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0 release.
		/// Use {@link #IndexWriter(Directory,Analyzer,MaxFieldLength)}
		/// instead, and call {@link #Commit()} when needed.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release. Use IndexWriter(Directory,Analyzer,MaxFieldLength) instead, and call Commit() when needed.")]
		public IndexWriter(System.IO.FileInfo path, Analyzer a)
		{
			InitBlock();
			Init(FSDirectory.GetDirectory(path), a, true, null, true, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in
		/// <code>d</code>, first creating it if it does not
		/// already exist.  Text will be analyzed with
		/// <code>a</code>.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="mfl">Maximum field length in number of terms/tokens: LIMITED, UNLIMITED, or user-specified
		/// via the MaxFieldLength constructor.
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		public IndexWriter(Directory d, Analyzer a, MaxFieldLength mfl)
		{
			InitBlock();
			Init(d, a, false, null, false, mfl.GetLimit(), null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in
		/// <code>d</code>, first creating it if it does not
		/// already exist.  Text will be analyzed with
		/// <code>a</code>.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0 release.
		/// Use {@link
		/// #IndexWriter(Directory,Analyzer,MaxFieldLength)}
		/// instead, and call {@link #Commit()} when needed.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release. Use IndexWriter(Directory,Analyzer,MaxFieldLength) instead, and call Commit() when needed.")]
		public IndexWriter(Directory d, Analyzer a)
		{
			InitBlock();
			Init(d, a, false, null, true, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in
		/// <code>d</code>, first creating it if it does not
		/// already exist.  Text will be analyzed with
		/// <code>a</code>.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="autoCommit">see <a href="#autoCommit">above</a>
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0 release.
		/// Use {@link
		/// #IndexWriter(Directory,Analyzer,MaxFieldLength)}
		/// instead, and call {@link #Commit()} when needed.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release. Use IndexWriter(Directory,Analyzer,MaxFieldLength) instead, and call Commit() when needed.")]
		public IndexWriter(Directory d, bool autoCommit, Analyzer a)
		{
			InitBlock();
			Init(d, a, false, null, autoCommit, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Constructs an IndexWriter for the index in <code>d</code>.
		/// Text will be analyzed with <code>a</code>.  If <code>create</code>
		/// is true, then a new, empty index will be created in
		/// <code>d</code>, replacing the index already there, if any.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="autoCommit">see <a href="#autoCommit">above</a>
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0 release.
		/// Use {@link
		/// #IndexWriter(Directory,Analyzer,boolean,MaxFieldLength)}
		/// instead, and call {@link #Commit()} when needed.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release. Use IndexWriter(Directory,Analyzer,boolean,MaxFieldLength) instead, and call Commit() when needed.")]
		public IndexWriter(Directory d, bool autoCommit, Analyzer a, bool create)
		{
			InitBlock();
			Init(d, a, create, false, null, autoCommit, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Expert: constructs an IndexWriter with a custom {@link
		/// IndexDeletionPolicy}, for the index in <code>d</code>,
		/// first creating it if it does not already exist.  Text
		/// will be analyzed with <code>a</code>.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="deletionPolicy">see <a href="#deletionPolicy">above</a>
		/// </param>
		/// <param name="mfl">whether or not to limit field lengths
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		public IndexWriter(Directory d, Analyzer a, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl)
		{
			InitBlock();
			Init(d, a, false, deletionPolicy, false, mfl.GetLimit(), null, null);
		}
		
		/// <summary> Expert: constructs an IndexWriter with a custom {@link
		/// IndexDeletionPolicy}, for the index in <code>d</code>,
		/// first creating it if it does not already exist.  Text
		/// will be analyzed with <code>a</code>.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="autoCommit">see <a href="#autoCommit">above</a>
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="deletionPolicy">see <a href="#deletionPolicy">above</a>
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be </throws>
		/// <summary>  read/written to or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0 release.
		/// Use {@link
		/// #IndexWriter(Directory,Analyzer,IndexDeletionPolicy,MaxFieldLength)}
		/// instead, and call {@link #Commit()} when needed.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release. Use IndexWriter(Directory,Analyzer,IndexDeletionPolicy,MaxFieldLength) instead, and call Commit() when needed.")]
		public IndexWriter(Directory d, bool autoCommit, Analyzer a, IndexDeletionPolicy deletionPolicy)
		{
			InitBlock();
			Init(d, a, false, deletionPolicy, autoCommit, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Expert: constructs an IndexWriter with a custom {@link
		/// IndexDeletionPolicy}, for the index in <code>d</code>.
		/// Text will be analyzed with <code>a</code>.  If
		/// <code>create</code> is true, then a new, empty index
		/// will be created in <code>d</code>, replacing the index
		/// already there, if any.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <param name="deletionPolicy">see <a href="#deletionPolicy">above</a>
		/// </param>
		/// <param name="mfl">{@link Mono.Lucene.Net.Index.IndexWriter.MaxFieldLength}, whether or not to limit field lengths.  Value is in number of terms/tokens
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		public IndexWriter(Directory d, Analyzer a, bool create, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl)
		{
			InitBlock();
			Init(d, a, create, false, deletionPolicy, false, mfl.GetLimit(), null, null);
		}
		
		/// <summary> Expert: constructs an IndexWriter with a custom {@link
		/// IndexDeletionPolicy} and {@link IndexingChain}, 
		/// for the index in <code>d</code>.
		/// Text will be analyzed with <code>a</code>.  If
		/// <code>create</code> is true, then a new, empty index
		/// will be created in <code>d</code>, replacing the index
		/// already there, if any.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <param name="deletionPolicy">see <a href="#deletionPolicy">above</a>
		/// </param>
		/// <param name="mfl">whether or not to limit field lengths, value is in number of terms/tokens.  See {@link Mono.Lucene.Net.Index.IndexWriter.MaxFieldLength}.
		/// </param>
		/// <param name="indexingChain">the {@link DocConsumer} chain to be used to 
		/// process documents
		/// </param>
		/// <param name="commit">which commit to open
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		internal IndexWriter(Directory d, Analyzer a, bool create, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl, IndexingChain indexingChain, IndexCommit commit)
		{
			InitBlock();
			Init(d, a, create, false, deletionPolicy, false, mfl.GetLimit(), indexingChain, commit);
		}
		
		/// <summary> Expert: constructs an IndexWriter with a custom {@link
		/// IndexDeletionPolicy}, for the index in <code>d</code>.
		/// Text will be analyzed with <code>a</code>.  If
		/// <code>create</code> is true, then a new, empty index
		/// will be created in <code>d</code>, replacing the index
		/// already there, if any.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="autoCommit">see <a href="#autoCommit">above</a>
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite
		/// the existing one; <code>false</code> to append to the existing
		/// index
		/// </param>
		/// <param name="deletionPolicy">see <a href="#deletionPolicy">above</a>
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		/// <deprecated> This constructor will be removed in the 3.0 release.
		/// Use {@link
		/// #IndexWriter(Directory,Analyzer,boolean,IndexDeletionPolicy,MaxFieldLength)}
		/// instead, and call {@link #Commit()} when needed.
		/// </deprecated>
        [Obsolete("This constructor will be removed in the 3.0 release. Use IndexWriter(Directory,Analyzer,boolean,IndexDeletionPolicy,MaxFieldLength) instead, and call Commit() when needed.")]
		public IndexWriter(Directory d, bool autoCommit, Analyzer a, bool create, IndexDeletionPolicy deletionPolicy)
		{
			InitBlock();
			Init(d, a, create, false, deletionPolicy, autoCommit, DEFAULT_MAX_FIELD_LENGTH, null, null);
		}
		
		/// <summary> Expert: constructs an IndexWriter on specific commit
		/// point, with a custom {@link IndexDeletionPolicy}, for
		/// the index in <code>d</code>.  Text will be analyzed
		/// with <code>a</code>.
		/// 
		/// <p/> This is only meaningful if you've used a {@link
		/// IndexDeletionPolicy} in that past that keeps more than
		/// just the last commit.
		/// 
		/// <p/>This operation is similar to {@link #Rollback()},
		/// except that method can only rollback what's been done
		/// with the current instance of IndexWriter since its last
		/// commit, whereas this method can rollback to an
		/// arbitrary commit point from the past, assuming the
		/// {@link IndexDeletionPolicy} has preserved past
		/// commits.
		/// 
		/// <p/><b>NOTE</b>: autoCommit (see <a
		/// href="#autoCommit">above</a>) is set to false with this
		/// constructor.
		/// 
		/// </summary>
		/// <param name="d">the index directory
		/// </param>
		/// <param name="a">the analyzer to use
		/// </param>
		/// <param name="deletionPolicy">see <a href="#deletionPolicy">above</a>
		/// </param>
		/// <param name="mfl">whether or not to limit field lengths, value is in number of terms/tokens.  See {@link Mono.Lucene.Net.Index.IndexWriter.MaxFieldLength}.
		/// </param>
		/// <param name="commit">which commit to open
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist and <code>create</code> is
		/// <code>false</code> or if there is any other low-level
		/// IO error
		/// </summary>
		public IndexWriter(Directory d, Analyzer a, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl, IndexCommit commit)
		{
			InitBlock();
			Init(d, a, false, false, deletionPolicy, false, mfl.GetLimit(), null, commit);
		}
		
		private void  Init(Directory d, Analyzer a, bool closeDir, IndexDeletionPolicy deletionPolicy, bool autoCommit, int maxFieldLength, IndexingChain indexingChain, IndexCommit commit)
		{
			if (IndexReader.IndexExists(d))
			{
				Init(d, a, false, closeDir, deletionPolicy, autoCommit, maxFieldLength, indexingChain, commit);
			}
			else
			{
				Init(d, a, true, closeDir, deletionPolicy, autoCommit, maxFieldLength, indexingChain, commit);
			}
		}
		
		private void  Init(Directory d, Analyzer a, bool create, bool closeDir, IndexDeletionPolicy deletionPolicy, bool autoCommit, int maxFieldLength, IndexingChain indexingChain, IndexCommit commit)
		{
			this.closeDir = closeDir;
			directory = d;
			analyzer = a;
			SetMessageID(defaultInfoStream);
			this.maxFieldLength = maxFieldLength;
			
			if (indexingChain == null)
				indexingChain = DocumentsWriter.DefaultIndexingChain;
			
			if (create)
			{
				// Clear the write lock in case it's leftover:
				directory.ClearLock(WRITE_LOCK_NAME);
			}
			
			Lock writeLock = directory.MakeLock(WRITE_LOCK_NAME);
			if (!writeLock.Obtain(writeLockTimeout))
			// obtain write lock
			{
				throw new LockObtainFailedException("Index locked for write: " + writeLock);
			}
			this.writeLock = writeLock; // save it

            bool success = false;
			try
			{
				if (create)
				{
					// Try to read first.  This is to allow create
					// against an index that's currently open for
					// searching.  In this case we write the next
					// segments_N file with no segments:
					bool doCommit;
					try
					{
						segmentInfos.Read(directory);
						segmentInfos.Clear();
						doCommit = false;
					}
					catch (System.IO.IOException e)
					{
						// Likely this means it's a fresh directory
						doCommit = true;
					}
					
					if (autoCommit || doCommit)
					{
						// Always commit if autoCommit=true, else only
						// commit if there is no segments file in this dir
						// already.
						segmentInfos.Commit(directory);
						SupportClass.CollectionsHelper.AddAllIfNotContains(synced, segmentInfos.Files(directory, true));
					}
					else
					{
						// Record that we have a change (zero out all
						// segments) pending:
						changeCount++;
					}
				}
				else
				{
					segmentInfos.Read(directory);
					
					if (commit != null)
					{
						// Swap out all segments, but, keep metadata in
						// SegmentInfos, like version & generation, to
						// preserve write-once.  This is important if
						// readers are open against the future commit
						// points.
						if (commit.GetDirectory() != directory)
							throw new System.ArgumentException("IndexCommit's directory doesn't match my directory");
						SegmentInfos oldInfos = new SegmentInfos();
						oldInfos.Read(directory, commit.GetSegmentsFileName());
						segmentInfos.Replace(oldInfos);
						changeCount++;
						if (infoStream != null)
							Message("init: loaded commit \"" + commit.GetSegmentsFileName() + "\"");
					}
					
					// We assume that this segments_N was previously
					// properly sync'd:
					SupportClass.CollectionsHelper.AddAllIfNotContains(synced, segmentInfos.Files(directory, true));
				}
				
				this.autoCommit = autoCommit;
				SetRollbackSegmentInfos(segmentInfos);
				
				docWriter = new DocumentsWriter(directory, this, indexingChain);
				docWriter.SetInfoStream(infoStream);
				docWriter.SetMaxFieldLength(maxFieldLength);
				
				// Default deleter (for backwards compatibility) is
				// KeepOnlyLastCommitDeleter:
				deleter = new IndexFileDeleter(directory, deletionPolicy == null?new KeepOnlyLastCommitDeletionPolicy():deletionPolicy, segmentInfos, infoStream, docWriter,synced);
				
				if (deleter.startingCommitDeleted)
				// Deletion policy deleted the "head" commit point.
				// We have to mark ourself as changed so that if we
				// are closed w/o any further changes we write a new
				// segments_N file.
					changeCount++;
				
				PushMaxBufferedDocs();
				
				if (infoStream != null)
				{
					Message("init: create=" + create);
					MessageState();
				}

                success = true;
			}
			finally
			{
                if (!success)
                {
                    if (infoStream != null)
                    {
                        Message("init: hit exception on init; releasing write lock");
                    }
                    try
                    {
                        writeLock.Release();
                    }
                    catch (Exception t)
                    {
                        // don't mask the original exception
                    }
                    writeLock = null;
                }
			}
		}
		
		private void  SetRollbackSegmentInfos(SegmentInfos infos)
		{
			lock (this)
			{
				rollbackSegmentInfos = (SegmentInfos) infos.Clone();
				System.Diagnostics.Debug.Assert(!rollbackSegmentInfos.HasExternalSegments(directory));
				rollbackSegments = new System.Collections.Hashtable();
				int size = rollbackSegmentInfos.Count;
				for (int i = 0; i < size; i++)
					rollbackSegments[rollbackSegmentInfos.Info(i)] = (System.Int32) i;
			}
		}
		
		/// <summary> Expert: set the merge policy used by this writer.</summary>
		public virtual void  SetMergePolicy(MergePolicy mp)
		{
			EnsureOpen();
			if (mp == null)
				throw new System.NullReferenceException("MergePolicy must be non-null");
			
			if (mergePolicy != mp)
				mergePolicy.Close();
			mergePolicy = mp;
			PushMaxBufferedDocs();
			if (infoStream != null)
			{
				Message("setMergePolicy " + mp);
			}
		}
		
		/// <summary> Expert: returns the current MergePolicy in use by this writer.</summary>
		/// <seealso cref="setMergePolicy">
		/// </seealso>
		public virtual MergePolicy GetMergePolicy()
		{
			EnsureOpen();
			return mergePolicy;
		}
		
		/// <summary> Expert: set the merge scheduler used by this writer.</summary>
		public virtual void  SetMergeScheduler(MergeScheduler mergeScheduler)
		{
			lock (this)
			{
				EnsureOpen();
				if (mergeScheduler == null)
					throw new System.NullReferenceException("MergeScheduler must be non-null");
				
				if (this.mergeScheduler != mergeScheduler)
				{
					FinishMerges(true);
					this.mergeScheduler.Close();
				}
				this.mergeScheduler = mergeScheduler;
				if (infoStream != null)
				{
					Message("setMergeScheduler " + mergeScheduler);
				}
			}
		}
		
		/// <summary> Expert: returns the current MergePolicy in use by this
		/// writer.
		/// </summary>
		/// <seealso cref="setMergePolicy">
		/// </seealso>
		public virtual MergeScheduler GetMergeScheduler()
		{
			EnsureOpen();
			return mergeScheduler;
		}
		
		/// <summary><p/>Determines the largest segment (measured by
		/// document count) that may be merged with other segments.
		/// Small values (e.g., less than 10,000) are best for
		/// interactive indexing, as this limits the length of
		/// pauses while indexing to a few seconds.  Larger values
		/// are best for batched indexing and speedier
		/// searches.<p/>
		/// 
		/// <p/>The default value is {@link Integer#MAX_VALUE}.<p/>
		/// 
		/// <p/>Note that this method is a convenience method: it
		/// just calls mergePolicy.setMaxMergeDocs as long as
		/// mergePolicy is an instance of {@link LogMergePolicy}.
		/// Otherwise an IllegalArgumentException is thrown.<p/>
		/// 
		/// <p/>The default merge policy ({@link
		/// LogByteSizeMergePolicy}) also allows you to set this
		/// limit by net size (in MB) of the segment, using {@link
		/// LogByteSizeMergePolicy#setMaxMergeMB}.<p/>
		/// </summary>
		public virtual void  SetMaxMergeDocs(int maxMergeDocs)
		{
			GetLogMergePolicy().SetMaxMergeDocs(maxMergeDocs);
		}
		
		/// <summary> <p/>Returns the largest segment (measured by document
		/// count) that may be merged with other segments.<p/>
		/// 
		/// <p/>Note that this method is a convenience method: it
		/// just calls mergePolicy.getMaxMergeDocs as long as
		/// mergePolicy is an instance of {@link LogMergePolicy}.
		/// Otherwise an IllegalArgumentException is thrown.<p/>
		/// 
		/// </summary>
		/// <seealso cref="setMaxMergeDocs">
		/// </seealso>
		public virtual int GetMaxMergeDocs()
		{
			return GetLogMergePolicy().GetMaxMergeDocs();
		}
		
		/// <summary> The maximum number of terms that will be indexed for a single field in a
		/// document.  This limits the amount of memory required for indexing, so that
		/// collections with very large files will not crash the indexing process by
		/// running out of memory.  This setting refers to the number of running terms,
		/// not to the number of different terms.<p/>
		/// <strong>Note:</strong> this silently truncates large documents, excluding from the
		/// index all terms that occur further in the document.  If you know your source
		/// documents are large, be sure to set this value high enough to accomodate
		/// the expected size.  If you set it to Integer.MAX_VALUE, then the only limit
		/// is your memory, but you should anticipate an OutOfMemoryError.<p/>
		/// By default, no more than {@link #DEFAULT_MAX_FIELD_LENGTH} terms
		/// will be indexed for a field.
		/// </summary>
		public virtual void  SetMaxFieldLength(int maxFieldLength)
		{
			EnsureOpen();
			this.maxFieldLength = maxFieldLength;
			docWriter.SetMaxFieldLength(maxFieldLength);
			if (infoStream != null)
				Message("setMaxFieldLength " + maxFieldLength);
		}
		
		/// <summary> Returns the maximum number of terms that will be
		/// indexed for a single field in a document.
		/// </summary>
		/// <seealso cref="setMaxFieldLength">
		/// </seealso>
		public virtual int GetMaxFieldLength()
		{
			EnsureOpen();
			return maxFieldLength;
		}

        /** Sets the termsIndexDivisor passed to any readers that
        *  IndexWriter opens, for example when applying deletes
        *  or creating a near-real-time reader in {@link
        *  IndexWriter#getReader}.  Default value is {@link
        *  IndexReader#DEFAULT_TERMS_INDEX_DIVISOR}. */
        public void SetReaderTermsIndexDivisor(int divisor)
        {
            EnsureOpen();
            if (divisor <= 0)
            {
                throw new System.ArgumentException("divisor must be >= 1 (got " + divisor + ")");
            }
            readerTermsIndexDivisor = divisor;
            if (infoStream != null)
            {
                Message("setReaderTermsIndexDivisor " + readerTermsIndexDivisor);
            }
        }

        /** @see #setReaderTermsIndexDivisor */
        public int GetReaderTermsIndexDivisor()
        {
            EnsureOpen();
            return readerTermsIndexDivisor;
        }
		
		/// <summary>Determines the minimal number of documents required
		/// before the buffered in-memory documents are flushed as
		/// a new Segment.  Large values generally gives faster
		/// indexing.
		/// 
		/// <p/>When this is set, the writer will flush every
		/// maxBufferedDocs added documents.  Pass in {@link
		/// #DISABLE_AUTO_FLUSH} to prevent triggering a flush due
		/// to number of buffered documents.  Note that if flushing
		/// by RAM usage is also enabled, then the flush will be
		/// triggered by whichever comes first.<p/>
		/// 
		/// <p/>Disabled by default (writer flushes by RAM usage).<p/>
		/// 
		/// </summary>
		/// <throws>  IllegalArgumentException if maxBufferedDocs is </throws>
		/// <summary> enabled but smaller than 2, or it disables maxBufferedDocs
		/// when ramBufferSize is already disabled
		/// </summary>
		/// <seealso cref="setRAMBufferSizeMB">
		/// </seealso>
		public virtual void  SetMaxBufferedDocs(int maxBufferedDocs)
		{
			EnsureOpen();
			if (maxBufferedDocs != DISABLE_AUTO_FLUSH && maxBufferedDocs < 2)
				throw new System.ArgumentException("maxBufferedDocs must at least be 2 when enabled");
			if (maxBufferedDocs == DISABLE_AUTO_FLUSH && GetRAMBufferSizeMB() == DISABLE_AUTO_FLUSH)
				throw new System.ArgumentException("at least one of ramBufferSize and maxBufferedDocs must be enabled");
			docWriter.SetMaxBufferedDocs(maxBufferedDocs);
			PushMaxBufferedDocs();
			if (infoStream != null)
				Message("setMaxBufferedDocs " + maxBufferedDocs);
		}
		
		/// <summary> If we are flushing by doc count (not by RAM usage), and
		/// using LogDocMergePolicy then push maxBufferedDocs down
		/// as its minMergeDocs, to keep backwards compatibility.
		/// </summary>
		private void  PushMaxBufferedDocs()
		{
			if (docWriter.GetMaxBufferedDocs() != DISABLE_AUTO_FLUSH)
			{
				MergePolicy mp = mergePolicy;
				if (mp is LogDocMergePolicy)
				{
					LogDocMergePolicy lmp = (LogDocMergePolicy) mp;
					int maxBufferedDocs = docWriter.GetMaxBufferedDocs();
					if (lmp.GetMinMergeDocs() != maxBufferedDocs)
					{
						if (infoStream != null)
							Message("now push maxBufferedDocs " + maxBufferedDocs + " to LogDocMergePolicy");
						lmp.SetMinMergeDocs(maxBufferedDocs);
					}
				}
			}
		}
		
		/// <summary> Returns the number of buffered added documents that will
		/// trigger a flush if enabled.
		/// </summary>
		/// <seealso cref="setMaxBufferedDocs">
		/// </seealso>
		public virtual int GetMaxBufferedDocs()
		{
			EnsureOpen();
			return docWriter.GetMaxBufferedDocs();
		}
		
		/// <summary>Determines the amount of RAM that may be used for
		/// buffering added documents and deletions before they are
		/// flushed to the Directory.  Generally for faster
		/// indexing performance it's best to flush by RAM usage
		/// instead of document count and use as large a RAM buffer
		/// as you can.
		/// 
		/// <p/>When this is set, the writer will flush whenever
		/// buffered documents and deletions use this much RAM.
		/// Pass in {@link #DISABLE_AUTO_FLUSH} to prevent
		/// triggering a flush due to RAM usage.  Note that if
		/// flushing by document count is also enabled, then the
		/// flush will be triggered by whichever comes first.<p/>
		/// 
		/// <p/> <b>NOTE</b>: the account of RAM usage for pending
		/// deletions is only approximate.  Specifically, if you
		/// delete by Query, Lucene currently has no way to measure
		/// the RAM usage if individual Queries so the accounting
		/// will under-estimate and you should compensate by either
		/// calling commit() periodically yourself, or by using
		/// {@link #setMaxBufferedDeleteTerms} to flush by count
		/// instead of RAM usage (each buffered delete Query counts
		/// as one).
		/// 
		/// <p/>
		/// <b>NOTE</b>: because IndexWriter uses <code>int</code>s when managing its
		/// internal storage, the absolute maximum value for this setting is somewhat
		/// less than 2048 MB. The precise limit depends on various factors, such as
		/// how large your documents are, how many fields have norms, etc., so it's
		/// best to set this value comfortably under 2048.
		/// <p/>
		/// 
		/// <p/> The default value is {@link #DEFAULT_RAM_BUFFER_SIZE_MB}.<p/>
		/// 
		/// </summary>
		/// <throws>  IllegalArgumentException if ramBufferSize is </throws>
		/// <summary> enabled but non-positive, or it disables ramBufferSize
		/// when maxBufferedDocs is already disabled
		/// </summary>
		public virtual void  SetRAMBufferSizeMB(double mb)
		{
			if (mb > 2048.0)
			{
				throw new System.ArgumentException("ramBufferSize " + mb + " is too large; should be comfortably less than 2048");
			}
			if (mb != DISABLE_AUTO_FLUSH && mb <= 0.0)
				throw new System.ArgumentException("ramBufferSize should be > 0.0 MB when enabled");
			if (mb == DISABLE_AUTO_FLUSH && GetMaxBufferedDocs() == DISABLE_AUTO_FLUSH)
				throw new System.ArgumentException("at least one of ramBufferSize and maxBufferedDocs must be enabled");
			docWriter.SetRAMBufferSizeMB(mb);
			if (infoStream != null)
				Message("setRAMBufferSizeMB " + mb);
		}
		
		/// <summary> Returns the value set by {@link #setRAMBufferSizeMB} if enabled.</summary>
		public virtual double GetRAMBufferSizeMB()
		{
			return docWriter.GetRAMBufferSizeMB();
		}
		
		/// <summary> <p/>Determines the minimal number of delete terms required before the buffered
		/// in-memory delete terms are applied and flushed. If there are documents
		/// buffered in memory at the time, they are merged and a new segment is
		/// created.<p/>
		/// <p/>Disabled by default (writer flushes by RAM usage).<p/>
		/// 
		/// </summary>
		/// <throws>  IllegalArgumentException if maxBufferedDeleteTerms </throws>
		/// <summary> is enabled but smaller than 1
		/// </summary>
		/// <seealso cref="setRAMBufferSizeMB">
		/// </seealso>
		public virtual void  SetMaxBufferedDeleteTerms(int maxBufferedDeleteTerms)
		{
			EnsureOpen();
			if (maxBufferedDeleteTerms != DISABLE_AUTO_FLUSH && maxBufferedDeleteTerms < 1)
				throw new System.ArgumentException("maxBufferedDeleteTerms must at least be 1 when enabled");
			docWriter.SetMaxBufferedDeleteTerms(maxBufferedDeleteTerms);
			if (infoStream != null)
				Message("setMaxBufferedDeleteTerms " + maxBufferedDeleteTerms);
		}
		
		/// <summary> Returns the number of buffered deleted terms that will
		/// trigger a flush if enabled.
		/// </summary>
		/// <seealso cref="setMaxBufferedDeleteTerms">
		/// </seealso>
		public virtual int GetMaxBufferedDeleteTerms()
		{
			EnsureOpen();
			return docWriter.GetMaxBufferedDeleteTerms();
		}
		
		/// <summary>Determines how often segment indices are merged by addDocument().  With
		/// smaller values, less RAM is used while indexing, and searches on
		/// unoptimized indices are faster, but indexing speed is slower.  With larger
		/// values, more RAM is used during indexing, and while searches on unoptimized
		/// indices are slower, indexing is faster.  Thus larger values (> 10) are best
        /// for batch index creation, and smaller values (&lt; 10) for indices that are
		/// interactively maintained.
		/// 
		/// <p/>Note that this method is a convenience method: it
		/// just calls mergePolicy.setMergeFactor as long as
		/// mergePolicy is an instance of {@link LogMergePolicy}.
		/// Otherwise an IllegalArgumentException is thrown.<p/>
		/// 
		/// <p/>This must never be less than 2.  The default value is 10.
		/// </summary>
		public virtual void  SetMergeFactor(int mergeFactor)
		{
			GetLogMergePolicy().SetMergeFactor(mergeFactor);
		}
		
		/// <summary> <p/>Returns the number of segments that are merged at
		/// once and also controls the total number of segments
		/// allowed to accumulate in the index.<p/>
		/// 
		/// <p/>Note that this method is a convenience method: it
		/// just calls mergePolicy.getMergeFactor as long as
		/// mergePolicy is an instance of {@link LogMergePolicy}.
		/// Otherwise an IllegalArgumentException is thrown.<p/>
		/// 
		/// </summary>
		/// <seealso cref="setMergeFactor">
		/// </seealso>
		public virtual int GetMergeFactor()
		{
			return GetLogMergePolicy().GetMergeFactor();
		}
		
		/// <summary> Expert: returns max delay inserted before syncing a
		/// commit point.  On Windows, at least, pausing before
		/// syncing can increase net indexing throughput.  The
		/// delay is variable based on size of the segment's files,
		/// and is only inserted when using
		/// ConcurrentMergeScheduler for merges.
		/// </summary>
		/// <deprecated> This will be removed in 3.0, when
		/// autoCommit=true is removed from IndexWriter.
		/// </deprecated>
        [Obsolete("This will be removed in 3.0, when autoCommit=true is removed from IndexWriter.")]
		public virtual double GetMaxSyncPauseSeconds()
		{
			return maxSyncPauseSeconds;
		}
		
		/// <summary> Expert: sets the max delay before syncing a commit
		/// point.
		/// </summary>
		/// <seealso cref="getMaxSyncPauseSeconds">
		/// </seealso>
		/// <deprecated> This will be removed in 3.0, when
		/// autoCommit=true is removed from IndexWriter.
		/// </deprecated>
        [Obsolete("This will be removed in 3.0, when autoCommit=true is removed from IndexWriter.")]
		public virtual void  SetMaxSyncPauseSeconds(double seconds)
		{
			maxSyncPauseSeconds = seconds;
		}
		
		/// <summary>If non-null, this will be the default infoStream used
		/// by a newly instantiated IndexWriter.
		/// </summary>
		/// <seealso cref="setInfoStream">
		/// </seealso>
		public static void  SetDefaultInfoStream(System.IO.StreamWriter infoStream)
		{
			IndexWriter.defaultInfoStream = infoStream;
		}
		
		/// <summary> Returns the current default infoStream for newly
		/// instantiated IndexWriters.
		/// </summary>
		/// <seealso cref="setDefaultInfoStream">
		/// </seealso>
		public static System.IO.StreamWriter GetDefaultInfoStream()
		{
			return IndexWriter.defaultInfoStream;
		}
		
		/// <summary>If non-null, information about merges, deletes and a
		/// message when maxFieldLength is reached will be printed
		/// to this.
		/// </summary>
		public virtual void  SetInfoStream(System.IO.StreamWriter infoStream)
		{
			EnsureOpen();
			SetMessageID(infoStream);
			docWriter.SetInfoStream(infoStream);
			deleter.SetInfoStream(infoStream);
			if (infoStream != null)
				MessageState();
		}
		
		private void  MessageState()
		{
			Message("setInfoStream: dir=" + directory + " autoCommit=" + autoCommit + " mergePolicy=" + mergePolicy + " mergeScheduler=" + mergeScheduler + " ramBufferSizeMB=" + docWriter.GetRAMBufferSizeMB() + " maxBufferedDocs=" + docWriter.GetMaxBufferedDocs() + " maxBuffereDeleteTerms=" + docWriter.GetMaxBufferedDeleteTerms() + " maxFieldLength=" + maxFieldLength + " index=" + SegString());
		}
		
		/// <summary> Returns the current infoStream in use by this writer.</summary>
		/// <seealso cref="setInfoStream">
		/// </seealso>
		public virtual System.IO.StreamWriter GetInfoStream()
		{
			EnsureOpen();
			return infoStream;
		}
		
		/// <summary>Returns true if verbosing is enabled (i.e., infoStream != null). </summary>
		public virtual bool Verbose()
		{
			return infoStream != null;
		}
		
		/// <seealso cref="setDefaultWriteLockTimeout"> to change the default value for all instances of IndexWriter.
		/// </seealso>
		public virtual void  SetWriteLockTimeout(long writeLockTimeout)
		{
			EnsureOpen();
			this.writeLockTimeout = writeLockTimeout;
		}
		
		/// <summary> Returns allowed timeout when acquiring the write lock.</summary>
		/// <seealso cref="setWriteLockTimeout">
		/// </seealso>
		public virtual long GetWriteLockTimeout()
		{
			EnsureOpen();
			return writeLockTimeout;
		}
		
		/// <summary> Sets the default (for any instance of IndexWriter) maximum time to wait for a write lock (in
		/// milliseconds).
		/// </summary>
		public static void  SetDefaultWriteLockTimeout(long writeLockTimeout)
		{
			IndexWriter.WRITE_LOCK_TIMEOUT = writeLockTimeout;
		}
		
		/// <summary> Returns default write lock timeout for newly
		/// instantiated IndexWriters.
		/// </summary>
		/// <seealso cref="setDefaultWriteLockTimeout">
		/// </seealso>
		public static long GetDefaultWriteLockTimeout()
		{
			return IndexWriter.WRITE_LOCK_TIMEOUT;
		}
		
		/// <summary> Commits all changes to an index and closes all
		/// associated files.  Note that this may be a costly
		/// operation, so, try to re-use a single writer instead of
		/// closing and opening a new one.  See {@link #Commit()} for
		/// caveats about write caching done by some IO devices.
		/// 
		/// <p/> If an Exception is hit during close, eg due to disk
		/// full or some other reason, then both the on-disk index
		/// and the internal state of the IndexWriter instance will
		/// be consistent.  However, the close will not be complete
		/// even though part of it (flushing buffered documents)
		/// may have succeeded, so the write lock will still be
		/// held.<p/>
		/// 
		/// <p/> If you can correct the underlying cause (eg free up
		/// some disk space) then you can call close() again.
		/// Failing that, if you want to force the write lock to be
		/// released (dangerous, because you may then lose buffered
		/// docs in the IndexWriter instance) then you can do
		/// something like this:<p/>
		/// 
		/// <pre>
		/// try {
		/// writer.close();
		/// } finally {
		/// if (IndexWriter.isLocked(directory)) {
		/// IndexWriter.unlock(directory);
		/// }
		/// }
		/// </pre>
		/// 
		/// after which, you must be certain not to use the writer
		/// instance anymore.<p/>
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer, again.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  Close()
		{
			Close(true);
		}

        /// <summary>
        /// .NET
        /// </summary>
        public virtual void Dispose()
        {
            Close();
        }
		
		/// <summary> Closes the index with or without waiting for currently
		/// running merges to finish.  This is only meaningful when
		/// using a MergeScheduler that runs merges in background
		/// threads.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer, again.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// <p/><b>NOTE</b>: it is dangerous to always call
		/// close(false), especially when IndexWriter is not open
		/// for very long, because this can result in "merge
		/// starvation" whereby long merges will never have a
		/// chance to finish.  This will cause too many segments in
		/// your index over time.<p/>
		/// 
		/// </summary>
		/// <param name="waitForMerges">if true, this call will block
		/// until all merges complete; else, it will ask all
		/// running merges to abort, wait until those merges have
		/// finished (which should be at most a few seconds), and
		/// then return.
		/// </param>
		public virtual void  Close(bool waitForMerges)
		{
			
			// Ensure that only one thread actually gets to do the closing:
			if (ShouldClose())
			{
				// If any methods have hit OutOfMemoryError, then abort
				// on close, in case the internal state of IndexWriter
				// or DocumentsWriter is corrupt
				if (hitOOM)
					RollbackInternal();
				else
					CloseInternal(waitForMerges);
			}
		}
		
		// Returns true if this thread should attempt to close, or
		// false if IndexWriter is now closed; else, waits until
		// another thread finishes closing
		private bool ShouldClose()
		{
			lock (this)
			{
				while (true)
				{
					if (!closed)
					{
						if (!closing)
						{
							closing = true;
							return true;
						}
						else
						{
							// Another thread is presently trying to close;
							// wait until it finishes one way (closes
							// successfully) or another (fails to close)
							DoWait();
						}
					}
					else
						return false;
				}
			}
		}
		
		private void  CloseInternal(bool waitForMerges)
		{
			
			docWriter.PauseAllThreads();
			
			try
			{
				if (infoStream != null)
					Message("now flush at close");
				
				docWriter.Close();
				
				// Only allow a new merge to be triggered if we are
				// going to wait for merges:
				if (!hitOOM)
				{
					Flush(waitForMerges, true, true);
				}
				
				if (waitForMerges)
				// Give merge scheduler last chance to run, in case
				// any pending merges are waiting:
					mergeScheduler.Merge(this);
				
				mergePolicy.Close();
				
				FinishMerges(waitForMerges);
				stopMerges = true;
				
				mergeScheduler.Close();
				
				if (infoStream != null)
					Message("now call final commit()");
				
				if (!hitOOM)
				{
					Commit(0);
				}
				
				if (infoStream != null)
					Message("at close: " + SegString());
				
				lock (this)
				{
					readerPool.Close();
					docWriter = null;
					deleter.Close();
				}
				
				if (closeDir)
					directory.Close();
				
				if (writeLock != null)
				{
					writeLock.Release(); // release write lock
					writeLock = null;
				}
				lock (this)
				{
					closed = true;
				}
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "closeInternal");
			}
			finally
			{
				lock (this)
				{
					closing = false;
					System.Threading.Monitor.PulseAll(this);
					if (!closed)
					{
						if (docWriter != null)
							docWriter.ResumeAllThreads();
						if (infoStream != null)
							Message("hit exception while closing");
					}
				}
			}
		}
		
		/// <summary>Tells the docWriter to close its currently open shared
		/// doc stores (stored fields &amp; vectors files).
		/// Return value specifices whether new doc store files are compound or not.
		/// </summary>
		private bool FlushDocStores()
		{
			lock (this)
			{
                if (infoStream != null)
                {
                    Message("flushDocStores segment=" + docWriter.GetDocStoreSegment());
                }

				bool useCompoundDocStore = false;
                if (infoStream != null)
                {
                    Message("closeDocStores segment=" + docWriter.GetDocStoreSegment());
                }

				System.String docStoreSegment;
				
				bool success = false;
				try
				{
					docStoreSegment = docWriter.CloseDocStore();
					success = true;
				}
				finally
				{
					if (!success && infoStream != null)
					{
						Message("hit exception closing doc store segment");
					}
				}

                if (infoStream != null)
                {
                    Message("flushDocStores files=" + docWriter.ClosedFiles());
                }

				useCompoundDocStore = mergePolicy.UseCompoundDocStore(segmentInfos);
				
				if (useCompoundDocStore && docStoreSegment != null && docWriter.ClosedFiles().Count != 0)
				{
					// Now build compound doc store file
					
					if (infoStream != null)
					{
						Message("create compound file " + docStoreSegment + "." + IndexFileNames.COMPOUND_FILE_STORE_EXTENSION);
					}
					
					success = false;
					
					int numSegments = segmentInfos.Count;
					System.String compoundFileName = docStoreSegment + "." + IndexFileNames.COMPOUND_FILE_STORE_EXTENSION;
					
					try
					{
						CompoundFileWriter cfsWriter = new CompoundFileWriter(directory, compoundFileName);
						System.Collections.IEnumerator it = docWriter.ClosedFiles().GetEnumerator();
						while (it.MoveNext())
						{
							cfsWriter.AddFile((System.String) it.Current);
						}
						
						// Perform the merge
						cfsWriter.Close();
						success = true;
					}
					finally
					{
						if (!success)
						{
							if (infoStream != null)
								Message("hit exception building compound file doc store for segment " + docStoreSegment);
							deleter.DeleteFile(compoundFileName);
						}
					}
					
					for (int i = 0; i < numSegments; i++)
					{
						SegmentInfo si = segmentInfos.Info(i);
						if (si.GetDocStoreOffset() != - 1 && si.GetDocStoreSegment().Equals(docStoreSegment))
							si.SetDocStoreIsCompoundFile(true);
					}
					
					Checkpoint();
					
					// In case the files we just merged into a CFS were
					// not previously checkpointed:
					deleter.DeleteNewFiles(docWriter.ClosedFiles());
				}
				
				return useCompoundDocStore;
			}
		}
		
		/// <summary>Returns the Directory used by this index. </summary>
		public virtual Directory GetDirectory()
		{
			// Pass false because the flush during closing calls getDirectory
			EnsureOpen(false);
			return directory;
		}
		
		/// <summary>Returns the analyzer used by this index. </summary>
		public virtual Analyzer GetAnalyzer()
		{
			EnsureOpen();
			return analyzer;
		}
		
		/// <summary>Returns the number of documents currently in this
		/// index, not counting deletions.
		/// </summary>
		/// <deprecated> Please use {@link #MaxDoc()} (same as this
		/// method) or {@link #NumDocs()} (also takes deletions
		/// into account), instead. 
		/// </deprecated>
        [Obsolete("Please use MaxDoc() (same as this method) or NumDocs() (also takes deletions into account), instead. ")]
		public virtual int DocCount()
		{
			lock (this)
			{
				EnsureOpen();
				return MaxDoc();
			}
		}
		
		/// <summary>Returns total number of docs in this index, including
		/// docs not yet flushed (still in the RAM buffer),
		/// not counting deletions.
		/// </summary>
		/// <seealso cref="numDocs">
		/// </seealso>
		public virtual int MaxDoc()
		{
			lock (this)
			{
				int count;
				if (docWriter != null)
					count = docWriter.GetNumDocsInRAM();
				else
					count = 0;
				
				for (int i = 0; i < segmentInfos.Count; i++)
					count += segmentInfos.Info(i).docCount;
				return count;
			}
		}
		
		/// <summary>Returns total number of docs in this index, including
		/// docs not yet flushed (still in the RAM buffer), and
		/// including deletions.  <b>NOTE:</b> buffered deletions
		/// are not counted.  If you really need these to be
		/// counted you should call {@link #Commit()} first.
		/// </summary>
		/// <seealso cref="numDocs">
		/// </seealso>
		public virtual int NumDocs()
		{
			lock (this)
			{
				int count;
				if (docWriter != null)
					count = docWriter.GetNumDocsInRAM();
				else
					count = 0;
				
				for (int i = 0; i < segmentInfos.Count; i++)
				{
					SegmentInfo info = segmentInfos.Info(i);
					count += info.docCount - info.GetDelCount();
				}
				return count;
			}
		}
		
		public virtual bool HasDeletions()
		{
			lock (this)
			{
				EnsureOpen();
				if (docWriter.HasDeletes())
					return true;
				for (int i = 0; i < segmentInfos.Count; i++)
					if (segmentInfos.Info(i).HasDeletions())
						return true;
				return false;
			}
		}
		
		/// <summary> The maximum number of terms that will be indexed for a single field in a
		/// document.  This limits the amount of memory required for indexing, so that
		/// collections with very large files will not crash the indexing process by
		/// running out of memory.<p/>
		/// Note that this effectively truncates large documents, excluding from the
		/// index terms that occur further in the document.  If you know your source
		/// documents are large, be sure to set this value high enough to accomodate
		/// the expected size.  If you set it to Integer.MAX_VALUE, then the only limit
		/// is your memory, but you should anticipate an OutOfMemoryError.<p/>
		/// By default, no more than 10,000 terms will be indexed for a field.
		/// 
		/// </summary>
		/// <seealso cref="MaxFieldLength">
		/// </seealso>
		private int maxFieldLength;
		
		/// <summary> Adds a document to this index.  If the document contains more than
		/// {@link #SetMaxFieldLength(int)} terms for a given field, the remainder are
		/// discarded.
		/// 
		/// <p/> Note that if an Exception is hit (for example disk full)
		/// then the index will be consistent, but this document
		/// may not have been added.  Furthermore, it's possible
		/// the index will have one segment in non-compound format
		/// even when using compound files (when a merge has
		/// partially succeeded).<p/>
		/// 
		/// <p/> This method periodically flushes pending documents
		/// to the Directory (see <a href="#flush">above</a>), and
		/// also periodically triggers segment merges in the index
		/// according to the {@link MergePolicy} in use.<p/>
		/// 
		/// <p/>Merges temporarily consume space in the
		/// directory. The amount of space required is up to 1X the
		/// size of all segments being merged, when no
		/// readers/searchers are open against the index, and up to
		/// 2X the size of all segments being merged when
		/// readers/searchers are open against the index (see
		/// {@link #Optimize()} for details). The sequence of
		/// primitive merge operations performed is governed by the
		/// merge policy.
		/// 
		/// <p/>Note that each term in the document can be no longer
		/// than 16383 characters, otherwise an
		/// IllegalArgumentException will be thrown.<p/>
		/// 
		/// <p/>Note that it's possible to create an invalid Unicode
		/// string in java if a UTF16 surrogate pair is malformed.
		/// In this case, the invalid characters are silently
		/// replaced with the Unicode replacement character
		/// U+FFFD.<p/>
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  AddDocument(Document doc)
		{
			AddDocument(doc, analyzer);
		}
		
		/// <summary> Adds a document to this index, using the provided analyzer instead of the
		/// value of {@link #GetAnalyzer()}.  If the document contains more than
		/// {@link #SetMaxFieldLength(int)} terms for a given field, the remainder are
		/// discarded.
		/// 
		/// <p/>See {@link #AddDocument(Document)} for details on
		/// index and IndexWriter state after an Exception, and
		/// flushing/merging temporary free space requirements.<p/>
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  AddDocument(Document doc, Analyzer analyzer)
		{
			EnsureOpen();
			bool doFlush = false;
			bool success = false;
			try
			{
				try
				{
					doFlush = docWriter.AddDocument(doc, analyzer);
					success = true;
				}
				finally
				{
					if (!success)
					{
						
						if (infoStream != null)
							Message("hit exception adding document");
						
						lock (this)
						{
							// If docWriter has some aborted files that were
							// never incref'd, then we clean them up here
							if (docWriter != null)
							{
                                System.Collections.Generic.ICollection<string> files = docWriter.AbortedFiles();
								if (files != null)
									deleter.DeleteNewFiles(files);
							}
						}
					}
				}
				if (doFlush)
					Flush(true, false, false);
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "addDocument");
			}
		}
		
		/// <summary> Deletes the document(s) containing <code>term</code>.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <param name="term">the term to identify the documents to be deleted
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  DeleteDocuments(Term term)
		{
			EnsureOpen();
			try
			{
				bool doFlush = docWriter.BufferDeleteTerm(term);
				if (doFlush)
					Flush(true, false, false);
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "deleteDocuments(Term)");
			}
		}
		
		/// <summary> Deletes the document(s) containing any of the
		/// terms. All deletes are flushed at the same time.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <param name="terms">array of terms to identify the documents
		/// to be deleted
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  DeleteDocuments(Term[] terms)
		{
			EnsureOpen();
			try
			{
				bool doFlush = docWriter.BufferDeleteTerms(terms);
				if (doFlush)
					Flush(true, false, false);
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "deleteDocuments(Term[])");
			}
		}
		
		/// <summary> Deletes the document(s) matching the provided query.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <param name="query">the query to identify the documents to be deleted
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  DeleteDocuments(Query query)
		{
			EnsureOpen();
			bool doFlush = docWriter.BufferDeleteQuery(query);
			if (doFlush)
				Flush(true, false, false);
		}
		
		/// <summary> Deletes the document(s) matching any of the provided queries.
		/// All deletes are flushed at the same time.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <param name="queries">array of queries to identify the documents
		/// to be deleted
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  DeleteDocuments(Query[] queries)
		{
			EnsureOpen();
			bool doFlush = docWriter.BufferDeleteQueries(queries);
			if (doFlush)
				Flush(true, false, false);
		}
		
		/// <summary> Updates a document by first deleting the document(s)
		/// containing <code>term</code> and then adding the new
		/// document.  The delete and then add are atomic as seen
		/// by a reader on the same index (flush may happen only after
		/// the add).
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <param name="term">the term to identify the document(s) to be
		/// deleted
		/// </param>
		/// <param name="doc">the document to be added
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  UpdateDocument(Term term, Document doc)
		{
			EnsureOpen();
			UpdateDocument(term, doc, GetAnalyzer());
		}
		
		/// <summary> Updates a document by first deleting the document(s)
		/// containing <code>term</code> and then adding the new
		/// document.  The delete and then add are atomic as seen
		/// by a reader on the same index (flush may happen only after
		/// the add).
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <param name="term">the term to identify the document(s) to be
		/// deleted
		/// </param>
		/// <param name="doc">the document to be added
		/// </param>
		/// <param name="analyzer">the analyzer to use when analyzing the document
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  UpdateDocument(Term term, Document doc, Analyzer analyzer)
		{
			EnsureOpen();
			try
			{
				bool doFlush = false;
				bool success = false;
				try
				{
					doFlush = docWriter.UpdateDocument(term, doc, analyzer);
					success = true;
				}
				finally
				{
					if (!success)
					{
						
						if (infoStream != null)
							Message("hit exception updating document");
						
						lock (this)
						{
							// If docWriter has some aborted files that were
							// never incref'd, then we clean them up here
                            System.Collections.Generic.ICollection<string> files = docWriter.AbortedFiles();
							if (files != null)
								deleter.DeleteNewFiles(files);
						}
					}
				}
				if (doFlush)
					Flush(true, false, false);
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "updateDocument");
			}
		}
		
		// for test purpose
		public /*internal*/ int GetSegmentCount()
		{
			lock (this)
			{
				return segmentInfos.Count;
			}
		}
		
		// for test purpose
		public /*internal*/ int GetNumBufferedDocuments()
		{
			lock (this)
			{
				return docWriter.GetNumDocsInRAM();
			}
		}
		
		// for test purpose
		public /*internal*/ int GetDocCount(int i)
		{
			lock (this)
			{
				if (i >= 0 && i < segmentInfos.Count)
				{
					return segmentInfos.Info(i).docCount;
				}
				else
				{
					return - 1;
				}
			}
		}
		
		// for test purpose
		public /*internal*/ int GetFlushCount()
		{
			lock (this)
			{
				return flushCount;
			}
		}
		
		// for test purpose
		public /*internal*/ int GetFlushDeletesCount()
		{
			lock (this)
			{
				return flushDeletesCount;
			}
		}
		
		internal System.String NewSegmentName()
		{
			// Cannot synchronize on IndexWriter because that causes
			// deadlock
			lock (segmentInfos)
			{
				// Important to increment changeCount so that the
				// segmentInfos is written on close.  Otherwise we
				// could close, re-open and re-return the same segment
				// name that was previously returned which can cause
				// problems at least with ConcurrentMergeScheduler.
				changeCount++;
				return "_" + SupportClass.Number.ToString(segmentInfos.counter++);
			}
		}
		
		/// <summary>If non-null, information about merges will be printed to this.</summary>
		private System.IO.StreamWriter infoStream = null;
		private static System.IO.StreamWriter defaultInfoStream = null;
		
		/// <summary> Requests an "optimize" operation on an index, priming the index
		/// for the fastest available search. Traditionally this has meant
		/// merging all segments into a single segment as is done in the
		/// default merge policy, but individaul merge policies may implement
		/// optimize in different ways.
		/// 
		/// <p/>It is recommended that this method be called upon completion of indexing.  In
		/// environments with frequent updates, optimize is best done during low volume times, if at all. 
		/// 
		/// <p/>
		/// <p/>See http://www.gossamer-threads.com/lists/lucene/java-dev/47895 for more discussion. <p/>
		/// 
		/// <p/>Note that optimize requires 2X the index size free
		/// space in your Directory (3X if you're using compound
        /// file format).  For example, if your index
		/// size is 10 MB then you need 20 MB free for optimize to
        /// complete (30 MB if you're using compound fiel format).<p/>
		/// 
		/// <p/>If some but not all readers re-open while an
		/// optimize is underway, this will cause > 2X temporary
		/// space to be consumed as those new readers will then
		/// hold open the partially optimized segments at that
		/// time.  It is best not to re-open readers while optimize
		/// is running.<p/>
		/// 
		/// <p/>The actual temporary usage could be much less than
		/// these figures (it depends on many factors).<p/>
		/// 
		/// <p/>In general, once the optimize completes, the total size of the
		/// index will be less than the size of the starting index.
		/// It could be quite a bit smaller (if there were many
		/// pending deletes) or just slightly smaller.<p/>
		/// 
		/// <p/>If an Exception is hit during optimize(), for example
		/// due to disk full, the index will not be corrupt and no
		/// documents will have been lost.  However, it may have
		/// been partially optimized (some segments were merged but
		/// not all), and it's possible that one of the segments in
		/// the index will be in non-compound format even when
		/// using compound file format.  This will occur when the
		/// Exception is hit during conversion of the segment into
		/// compound format.<p/>
		/// 
		/// <p/>This call will optimize those segments present in
		/// the index when the call started.  If other threads are
		/// still adding documents and flushing segments, those
		/// newly created segments will not be optimized unless you
		/// call optimize again.<p/>
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <seealso cref="LogMergePolicy.findMergesForOptimize">
		/// </seealso>
		public virtual void  Optimize()
		{
			Optimize(true);
		}

        /// <summary> Optimize the index down to &lt;= maxNumSegments.  If
		/// maxNumSegments==1 then this is the same as {@link
		/// #Optimize()}.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <param name="maxNumSegments">maximum number of segments left
		/// in the index after optimization finishes
		/// </param>
		public virtual void  Optimize(int maxNumSegments)
		{
			Optimize(maxNumSegments, true);
		}
		
		/// <summary>Just like {@link #Optimize()}, except you can specify
		/// whether the call should block until the optimize
		/// completes.  This is only meaningful with a
		/// {@link MergeScheduler} that is able to run merges in
		/// background threads.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// </summary>
		public virtual void  Optimize(bool doWait)
		{
			Optimize(1, doWait);
		}
		
		/// <summary>Just like {@link #Optimize(int)}, except you can
		/// specify whether the call should block until the
		/// optimize completes.  This is only meaningful with a
		/// {@link MergeScheduler} that is able to run merges in
		/// background threads.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// </summary>
		public virtual void  Optimize(int maxNumSegments, bool doWait)
		{
			EnsureOpen();
			
			if (maxNumSegments < 1)
				throw new System.ArgumentException("maxNumSegments must be >= 1; got " + maxNumSegments);
			
			if (infoStream != null)
				Message("optimize: index now " + SegString());
			
			Flush(true, false, true);
			
			lock (this)
			{
				ResetMergeExceptions();
				segmentsToOptimize = new System.Collections.Hashtable();
                optimizeMaxNumSegments = maxNumSegments;
				int numSegments = segmentInfos.Count;
				for (int i = 0; i < numSegments; i++)
					SupportClass.CollectionsHelper.AddIfNotContains(segmentsToOptimize, segmentInfos.Info(i));
				
				// Now mark all pending & running merges as optimize
				// merge:
				System.Collections.IEnumerator it = pendingMerges.GetEnumerator();
				while (it.MoveNext())
				{
					MergePolicy.OneMerge merge = (MergePolicy.OneMerge) it.Current;
					merge.optimize = true;
					merge.maxNumSegmentsOptimize = maxNumSegments;
				}
				
				it = runningMerges.GetEnumerator();
				while (it.MoveNext())
				{
					MergePolicy.OneMerge merge = (MergePolicy.OneMerge) it.Current;
					merge.optimize = true;
					merge.maxNumSegmentsOptimize = maxNumSegments;
				}
			}
			
			MaybeMerge(maxNumSegments, true);
			
			if (doWait)
			{
				lock (this)
				{
					while (true)
					{
						
						if (hitOOM)
						{
							throw new System.SystemException("this writer hit an OutOfMemoryError; cannot complete optimize");
						}
						
						if (mergeExceptions.Count > 0)
						{
							// Forward any exceptions in background merge
							// threads to the current thread:
							int size = mergeExceptions.Count;
							for (int i = 0; i < size; i++)
							{
								MergePolicy.OneMerge merge = (MergePolicy.OneMerge) mergeExceptions[0];
								if (merge.optimize)
								{
                                    System.IO.IOException err;
									System.Exception t = merge.GetException();
                                    if (t != null)
									    err = new System.IO.IOException("background merge hit exception: " + merge.SegString(directory), t);
                                    else
                                        err = new System.IO.IOException("background merge hit exception: " + merge.SegString(directory));
									throw err;
								}
							}
						}
						
						if (OptimizeMergesPending())
							DoWait();
						else
							break;
					}
				}
				
				// If close is called while we are still
				// running, throw an exception so the calling
				// thread will know the optimize did not
				// complete
				EnsureOpen();
			}
			
			// NOTE: in the ConcurrentMergeScheduler case, when
			// doWait is false, we can return immediately while
			// background threads accomplish the optimization
		}
		
		/// <summary>Returns true if any merges in pendingMerges or
		/// runningMerges are optimization merges. 
		/// </summary>
		private bool OptimizeMergesPending()
		{
			lock (this)
			{
                System.Collections.Generic.LinkedList<MergePolicy.OneMerge>.Enumerator it =  pendingMerges.GetEnumerator();
                while (it.MoveNext())
                {
                    if (it.Current.optimize) return true;
                }

                System.Collections.Generic.List<MergePolicy.OneMerge>.Enumerator it2 = runningMerges.GetEnumerator();
                while (it2.MoveNext())
                {
                    if (it2.Current.optimize) return true;
                }
				
				return false;
			}
		}
		
		/// <summary>Just like {@link #ExpungeDeletes()}, except you can
		/// specify whether the call should block until the
		/// operation completes.  This is only meaningful with a
		/// {@link MergeScheduler} that is able to run merges in
		/// background threads.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// </summary>
		public virtual void  ExpungeDeletes(bool doWait)
		{
			EnsureOpen();
			
			if (infoStream != null)
				Message("expungeDeletes: index now " + SegString());
			
			MergePolicy.MergeSpecification spec;
			
			lock (this)
			{
				spec = mergePolicy.FindMergesToExpungeDeletes(segmentInfos);
				if (spec != null)
				{
					int numMerges = spec.merges.Count;
					for (int i = 0; i < numMerges; i++)
						RegisterMerge((MergePolicy.OneMerge) spec.merges[i]);
				}
			}
			
			mergeScheduler.Merge(this);
			
			if (spec != null && doWait)
			{
				int numMerges = spec.merges.Count;
				lock (this)
				{
					bool running = true;
					while (running)
					{
						
						if (hitOOM)
						{
							throw new System.SystemException("this writer hit an OutOfMemoryError; cannot complete expungeDeletes");
						}
						
						// Check each merge that MergePolicy asked us to
						// do, to see if any of them are still running and
						// if any of them have hit an exception.
						running = false;
						for (int i = 0; i < numMerges; i++)
						{
							MergePolicy.OneMerge merge = (MergePolicy.OneMerge) spec.merges[i];
							if (pendingMerges.Contains(merge) || runningMerges.Contains(merge))
								running = true;
							System.Exception t = merge.GetException();
							if (t != null)
							{
								System.IO.IOException ioe = new System.IO.IOException("background merge hit exception: " + merge.SegString(directory), t);
								throw ioe;
							}
						}
						
						// If any of our merges are still running, wait:
						if (running)
							DoWait();
					}
				}
			}
			
			// NOTE: in the ConcurrentMergeScheduler case, when
			// doWait is false, we can return immediately while
			// background threads accomplish the optimization
		}
		
		
		/// <summary>Expunges all deletes from the index.  When an index
		/// has many document deletions (or updates to existing
		/// documents), it's best to either call optimize or
		/// expungeDeletes to remove all unused data in the index
		/// associated with the deleted documents.  To see how
		/// many deletions you have pending in your index, call
		/// {@link IndexReader#numDeletedDocs}
		/// This saves disk space and memory usage while
		/// searching.  expungeDeletes should be somewhat faster
		/// than optimize since it does not insist on reducing the
		/// index to a single segment (though, this depends on the
		/// {@link MergePolicy}; see {@link
		/// MergePolicy#findMergesToExpungeDeletes}.). Note that
		/// this call does not first commit any buffered
		/// documents, so you must do so yourself if necessary.
		/// See also {@link #ExpungeDeletes(boolean)}
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// </summary>
		public virtual void  ExpungeDeletes()
		{
			ExpungeDeletes(true);
		}
		
		/// <summary> Expert: asks the mergePolicy whether any merges are
		/// necessary now and if so, runs the requested merges and
		/// then iterate (test again if merges are needed) until no
		/// more merges are returned by the mergePolicy.
		/// 
		/// Explicit calls to maybeMerge() are usually not
		/// necessary. The most common case is when merge policy
		/// parameters have changed.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// </summary>
		public void  MaybeMerge()
		{
			MaybeMerge(false);
		}
		
		private void  MaybeMerge(bool optimize)
		{
			MaybeMerge(1, optimize);
		}
		
		private void  MaybeMerge(int maxNumSegmentsOptimize, bool optimize)
		{
			UpdatePendingMerges(maxNumSegmentsOptimize, optimize);
			mergeScheduler.Merge(this);
		}
		
		private void  UpdatePendingMerges(int maxNumSegmentsOptimize, bool optimize)
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(!optimize || maxNumSegmentsOptimize > 0);

                if (stopMerges)
                {
                    return;
                }
				
				// Do not start new merges if we've hit OOME
				if (hitOOM)
				{
					return ;
				}
				
				MergePolicy.MergeSpecification spec;
				if (optimize)
				{
					spec = mergePolicy.FindMergesForOptimize(segmentInfos, maxNumSegmentsOptimize, segmentsToOptimize);
					
					if (spec != null)
					{
						int numMerges = spec.merges.Count;
						for (int i = 0; i < numMerges; i++)
						{
							MergePolicy.OneMerge merge = ((MergePolicy.OneMerge) spec.merges[i]);
							merge.optimize = true;
							merge.maxNumSegmentsOptimize = maxNumSegmentsOptimize;
						}
					}
				}
				else
					spec = mergePolicy.FindMerges(segmentInfos);
				
				if (spec != null)
				{
					int numMerges = spec.merges.Count;
					for (int i = 0; i < numMerges; i++)
						RegisterMerge((MergePolicy.OneMerge) spec.merges[i]);
				}
			}
		}

        public virtual MergePolicy.OneMerge GetNextMerge_forNUnit()
        {
            return GetNextMerge();
        }
		
		/// <summary>Expert: the {@link MergeScheduler} calls this method
		/// to retrieve the next merge requested by the
		/// MergePolicy 
		/// </summary>
		internal virtual MergePolicy.OneMerge GetNextMerge()
		{
			lock (this)
			{
				if (pendingMerges.Count == 0)
					return null;
				else
				{
                    // Advance the merge from pending to running
                    MergePolicy.OneMerge merge = (MergePolicy.OneMerge)pendingMerges.First.Value;
                    pendingMerges.RemoveFirst();
                    runningMerges.Add(merge);
                    return merge;
				}
			}
		}
		
		/// <summary>Like getNextMerge() except only returns a merge if it's
		/// external. 
		/// </summary>
		private MergePolicy.OneMerge GetNextExternalMerge()
		{
			lock (this)
			{
				if (pendingMerges.Count == 0)
					return null;
				else
				{
                    System.Collections.Generic.IEnumerator<MergePolicy.OneMerge> it = pendingMerges.GetEnumerator();
					while (it.MoveNext())
					{
                        MergePolicy.OneMerge merge = (MergePolicy.OneMerge) it.Current;
						if (merge.isExternal)
						{
							// Advance the merge from pending to running
                            pendingMerges.Remove(merge);  // {{Aroush-2.9}} From Mike Garski: this is an O(n) op... is that an issue?
                            runningMerges.Add(merge);
							return merge;
						}
					}
					
					// All existing merges do not involve external segments
					return null;
				}
			}
		}
		
		/*
		* Begin a transaction.  During a transaction, any segment
		* merges that happen (or ram segments flushed) will not
		* write a new segments file and will not remove any files
		* that were present at the start of the transaction.  You
		* must make a matched (try/finally) call to
		* commitTransaction() or rollbackTransaction() to finish
		* the transaction.
		*
		* Note that buffered documents and delete terms are not handled
		* within the transactions, so they must be flushed before the
		* transaction is started.
		*/
		private void  StartTransaction(bool haveReadLock)
		{
			lock (this)
			{
				
				bool success = false;
				try
				{
					if (infoStream != null)
						Message("now start transaction");
					
					System.Diagnostics.Debug.Assert(docWriter.GetNumBufferedDeleteTerms() == 0 , 
						"calling startTransaction with buffered delete terms not supported: numBufferedDeleteTerms=" + docWriter.GetNumBufferedDeleteTerms());
					System.Diagnostics.Debug.Assert(docWriter.GetNumDocsInRAM() == 0 , 
						"calling startTransaction with buffered documents not supported: numDocsInRAM=" + docWriter.GetNumDocsInRAM());
					
					EnsureOpen();
					
					// If a transaction is trying to roll back (because
					// addIndexes hit an exception) then wait here until
					// that's done:
					lock (this)
					{
						while (stopMerges)
							DoWait();
					}
					success = true;
				}
				finally
				{
					// Release the write lock if our caller held it, on
					// hitting an exception
					if (!success && haveReadLock)
						ReleaseRead();
				}
				
				if (haveReadLock)
				{
					UpgradeReadToWrite();
				}
				else
				{
					AcquireWrite();
				}
				
				success = false;
				try
				{
					localRollbackSegmentInfos = (SegmentInfos) segmentInfos.Clone();
					
					System.Diagnostics.Debug.Assert(!HasExternalSegments());
					
					localAutoCommit = autoCommit;
					localFlushedDocCount = docWriter.GetFlushedDocCount();
					
					if (localAutoCommit)
					{
						
						if (infoStream != null)
							Message("flush at startTransaction");
						
						Flush(true, false, false);
						
						// Turn off auto-commit during our local transaction:
						autoCommit = false;
					}
					// We must "protect" our files at this point from
					// deletion in case we need to rollback:
					else
						deleter.IncRef(segmentInfos, false);
					
					success = true;
				}
				finally
				{
					if (!success)
						FinishAddIndexes();
				}
			}
		}
		
		/*
		* Rolls back the transaction and restores state to where
		* we were at the start.
		*/
		private void  RollbackTransaction()
		{
			lock (this)
			{
				
				if (infoStream != null)
					Message("now rollback transaction");
				
				// First restore autoCommit in case we hit an exception below:
				autoCommit = localAutoCommit;
				if (docWriter != null)
				{
					docWriter.SetFlushedDocCount(localFlushedDocCount);
				}
				
				// Must finish merges before rolling back segmentInfos
				// so merges don't hit exceptions on trying to commit
				// themselves, don't get files deleted out from under
				// them, etc:
				FinishMerges(false);
				
				// Keep the same segmentInfos instance but replace all
				// of its SegmentInfo instances.  This is so the next
				// attempt to commit using this instance of IndexWriter
				// will always write to a new generation ("write once").
				segmentInfos.Clear();
				segmentInfos.AddRange(localRollbackSegmentInfos);
				localRollbackSegmentInfos = null;
				
				// This must come after we rollback segmentInfos, so
				// that if a commit() kicks off it does not see the
				// segmentInfos with external segments
				FinishAddIndexes();
				
				// Ask deleter to locate unreferenced files we had
				// created & remove them:
				deleter.Checkpoint(segmentInfos, false);
				
				if (!autoCommit)
				// Remove the incRef we did in startTransaction:
					deleter.DecRef(segmentInfos);
				
				// Also ask deleter to remove any newly created files
				// that were never incref'd; this "garbage" is created
				// when a merge kicks off but aborts part way through
				// before it had a chance to incRef the files it had
				// partially created
				deleter.Refresh();
				
				System.Threading.Monitor.PulseAll(this);
				
				System.Diagnostics.Debug.Assert(!HasExternalSegments());
			}
		}
		
		/*
		* Commits the transaction.  This will write the new
		* segments file and remove and pending deletions we have
		* accumulated during the transaction
		*/
		private void  CommitTransaction()
		{
			lock (this)
			{
				
				if (infoStream != null)
					Message("now commit transaction");
				
				// First restore autoCommit in case we hit an exception below:
				autoCommit = localAutoCommit;
				
				// Give deleter a chance to remove files now:
				Checkpoint();
				
				if (autoCommit)
				{
					bool success = false;
					try
					{
						Commit(0);
						success = true;
					}
					finally
					{
						if (!success)
						{
							if (infoStream != null)
								Message("hit exception committing transaction");
							RollbackTransaction();
						}
					}
				}
				// Remove the incRef we did in startTransaction.
				else
					deleter.DecRef(localRollbackSegmentInfos);
				
				localRollbackSegmentInfos = null;
				
				System.Diagnostics.Debug.Assert(!HasExternalSegments());
				
				FinishAddIndexes();
			}
		}
		
		/// <deprecated> Please use {@link #rollback} instead.
		/// </deprecated>
        [Obsolete("Please use Rollback instead.")]
		public virtual void  Abort()
		{
			Rollback();
		}
		
		/// <summary> Close the <code>IndexWriter</code> without committing
		/// any changes that have occurred since the last commit
		/// (or since it was opened, if commit hasn't been called).
		/// This removes any temporary files that had been created,
		/// after which the state of the index will be the same as
		/// it was when commit() was last called or when this
		/// writer was first opened.  This can only be called when
		/// this IndexWriter was opened with
		/// <code>autoCommit=false</code>.  This also clears a
		/// previous call to {@link #prepareCommit}.
		/// </summary>
		/// <throws>  IllegalStateException if this is called when </throws>
		/// <summary>  the writer was opened with <code>autoCommit=true</code>.
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  Rollback()
		{
			EnsureOpen();
			if (autoCommit)
				throw new System.SystemException("rollback() can only be called when IndexWriter was opened with autoCommit=false");
			
			// Ensure that only one thread actually gets to do the closing:
			if (ShouldClose())
				RollbackInternal();
		}
		
		private void  RollbackInternal()
		{
			
			bool success = false;

            if (infoStream != null)
            {
                Message("rollback");
            }
			
			docWriter.PauseAllThreads();
			
			try
			{
				FinishMerges(false);
				
				// Must pre-close these two, in case they increment
				// changeCount so that we can then set it to false
				// before calling closeInternal
				mergePolicy.Close();
				mergeScheduler.Close();
				
				lock (this)
				{
					
					if (pendingCommit != null)
					{
						pendingCommit.RollbackCommit(directory);
						deleter.DecRef(pendingCommit);
						pendingCommit = null;
						System.Threading.Monitor.PulseAll(this);
					}
					
					// Keep the same segmentInfos instance but replace all
					// of its SegmentInfo instances.  This is so the next
					// attempt to commit using this instance of IndexWriter
					// will always write to a new generation ("write
					// once").
					segmentInfos.Clear();
					segmentInfos.AddRange(rollbackSegmentInfos);
					
					System.Diagnostics.Debug.Assert(!HasExternalSegments());
					
					docWriter.Abort();
					
					System.Diagnostics.Debug.Assert(TestPoint("rollback before checkpoint"));
					
					// Ask deleter to locate unreferenced files & remove
					// them:
					deleter.Checkpoint(segmentInfos, false);
					deleter.Refresh();
				}
				
				// Don't bother saving any changes in our segmentInfos
				readerPool.Clear(null);
				
				lastCommitChangeCount = changeCount;
				
				success = true;
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "rollbackInternal");
			}
			finally
			{
				lock (this)
				{
					if (!success)
					{
						docWriter.ResumeAllThreads();
						closing = false;
						System.Threading.Monitor.PulseAll(this);
						if (infoStream != null)
							Message("hit exception during rollback");
					}
				}
			}
			
			CloseInternal(false);
		}
		
		/// <summary> Delete all documents in the index.
		/// 
		/// <p/>This method will drop all buffered documents and will 
		/// remove all segments from the index. This change will not be
		/// visible until a {@link #Commit()} has been called. This method
		/// can be rolled back using {@link #Rollback()}.<p/>
		/// 
		/// <p/>NOTE: this method is much faster than using deleteDocuments( new MatchAllDocsQuery() ).<p/>
		/// 
		/// <p/>NOTE: this method will forcefully abort all merges
		/// in progress.  If other threads are running {@link
		/// #Optimize()} or any of the addIndexes methods, they
		/// will receive {@link MergePolicy.MergeAbortedException}s.
		/// </summary>
		public virtual void  DeleteAll()
		{
			lock (this)
			{
				docWriter.PauseAllThreads();
				try
				{
					
					// Abort any running merges
					FinishMerges(false);
					
					// Remove any buffered docs
					docWriter.Abort();
					docWriter.SetFlushedDocCount(0);
					
					// Remove all segments
					segmentInfos.Clear();
					
					// Ask deleter to locate unreferenced files & remove them:
					deleter.Checkpoint(segmentInfos, false);
					deleter.Refresh();
					
					// Don't bother saving any changes in our segmentInfos
					readerPool.Clear(null);
					
					// Mark that the index has changed
					++changeCount;
				}
				catch (System.OutOfMemoryException oom)
				{
					HandleOOM(oom, "deleteAll");
				}
				finally
				{
					docWriter.ResumeAllThreads();
					if (infoStream != null)
					{
						Message("hit exception during deleteAll");
					}
				}
			}
		}
		
		private void  FinishMerges(bool waitForMerges)
		{
			lock (this)
			{
				if (!waitForMerges)
				{
					
					stopMerges = true;
					
					// Abort all pending & running merges:
					System.Collections.IEnumerator it = pendingMerges.GetEnumerator();
					while (it.MoveNext())
					{
						MergePolicy.OneMerge merge = (MergePolicy.OneMerge) it.Current;
						if (infoStream != null)
							Message("now abort pending merge " + merge.SegString(directory));
						merge.Abort();
						MergeFinish(merge);
					}
					pendingMerges.Clear();
					
					it = runningMerges.GetEnumerator();
					while (it.MoveNext())
					{
						MergePolicy.OneMerge merge = (MergePolicy.OneMerge) it.Current;
						if (infoStream != null)
							Message("now abort running merge " + merge.SegString(directory));
						merge.Abort();
					}
					
					// Ensure any running addIndexes finishes.  It's fine
					// if a new one attempts to start because its merges
					// will quickly see the stopMerges == true and abort.
					AcquireRead();
					ReleaseRead();
					
					// These merges periodically check whether they have
					// been aborted, and stop if so.  We wait here to make
					// sure they all stop.  It should not take very long
					// because the merge threads periodically check if
					// they are aborted.
					while (runningMerges.Count > 0)
					{
						if (infoStream != null)
							Message("now wait for " + runningMerges.Count + " running merge to abort");
						DoWait();
					}
					
					stopMerges = false;
					System.Threading.Monitor.PulseAll(this);
					
					System.Diagnostics.Debug.Assert(0 == mergingSegments.Count);
					
					if (infoStream != null)
						Message("all running merges have aborted");
				}
				else
				{
					// waitForMerges() will ensure any running addIndexes finishes.  
					// It's fine if a new one attempts to start because from our
					// caller above the call will see that we are in the
					// process of closing, and will throw an
					// AlreadyClosedException.
					WaitForMerges();
				}
			}
		}
		
		/// <summary> Wait for any currently outstanding merges to finish.
		/// 
		/// <p/>It is guaranteed that any merges started prior to calling this method 
		/// will have completed once this method completes.<p/>
		/// </summary>
		public virtual void  WaitForMerges()
		{
			lock (this)
			{
				// Ensure any running addIndexes finishes.
				AcquireRead();
				ReleaseRead();
				
				while (pendingMerges.Count > 0 || runningMerges.Count > 0)
				{
					DoWait();
				}
				
				// sanity check
				System.Diagnostics.Debug.Assert(0 == mergingSegments.Count);
			}
		}
		
		/*
		* Called whenever the SegmentInfos has been updated and
		* the index files referenced exist (correctly) in the
		* index directory.
		*/
		private void  Checkpoint()
		{
			lock (this)
			{
				changeCount++;
				deleter.Checkpoint(segmentInfos, false);
			}
		}
		
		private void  FinishAddIndexes()
		{
			ReleaseWrite();
		}
		
		private void  BlockAddIndexes(bool includePendingClose)
		{
			
			AcquireRead();
			
			bool success = false;
			try
			{
				
				// Make sure we are still open since we could have
				// waited quite a while for last addIndexes to finish
				EnsureOpen(includePendingClose);
				success = true;
			}
			finally
			{
				if (!success)
					ReleaseRead();
			}
		}
		
		private void  ResumeAddIndexes()
		{
			ReleaseRead();
		}
		
		/// <summary>Merges all segments from an array of indexes into this index.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <deprecated> Use {@link #addIndexesNoOptimize} instead,
		/// then separately call {@link #optimize} afterwards if
		/// you need to.
		/// 
		/// </deprecated>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
        [Obsolete("Use {@link #addIndexesNoOptimize} instead,then separately call {@link #optimize} afterwards if you need to.")]
		public virtual void  AddIndexes(Directory[] dirs)
		{
			
			EnsureOpen();
			
			NoDupDirs(dirs);
			
			// Do not allow add docs or deletes while we are running:
			docWriter.PauseAllThreads();
			
			try
			{
				
				if (infoStream != null)
					Message("flush at addIndexes");
				Flush(true, false, true);
				
				bool success = false;
				
				StartTransaction(false);
				
				try
				{
					
					int docCount = 0;
					lock (this)
					{
						EnsureOpen();
						for (int i = 0; i < dirs.Length; i++)
						{
							SegmentInfos sis = new SegmentInfos(); // read infos from dir
							sis.Read(dirs[i]);
							for (int j = 0; j < sis.Count; j++)
							{
								SegmentInfo info = sis.Info(j);
								docCount += info.docCount;
								System.Diagnostics.Debug.Assert(!segmentInfos.Contains(info));
								segmentInfos.Add(info); // add each info
							}
						}
					}
					
					// Notify DocumentsWriter that the flushed count just increased
					docWriter.UpdateFlushedDocCount(docCount);
					
					Optimize();
					
					success = true;
				}
				finally
				{
					if (success)
					{
						CommitTransaction();
					}
					else
					{
						RollbackTransaction();
					}
				}
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "addIndexes(Directory[])");
			}
			finally
			{
				if (docWriter != null)
				{
					docWriter.ResumeAllThreads();
				}
			}
		}
		
		private void  ResetMergeExceptions()
		{
			lock (this)
			{
				mergeExceptions = new System.Collections.ArrayList();
				mergeGen++;
			}
		}
		
		private void  NoDupDirs(Directory[] dirs)
		{
            System.Collections.Generic.Dictionary<Directory, Directory> dups = new System.Collections.Generic.Dictionary<Directory, Directory>();
			for (int i = 0; i < dirs.Length; i++)
			{
                if (dups.ContainsKey(dirs[i]))
				{
					throw new System.ArgumentException("Directory " + dirs[i] + " appears more than once");
				}
				if (dirs[i] == directory)
					throw new System.ArgumentException("Cannot add directory to itself");
                dups[dirs[i]] = dirs[i];
            }
		}
		
		/// <summary> Merges all segments from an array of indexes into this
		/// index.
		/// 
		/// <p/>This may be used to parallelize batch indexing.  A large document
		/// collection can be broken into sub-collections.  Each sub-collection can be
		/// indexed in parallel, on a different thread, process or machine.  The
		/// complete index can then be created by merging sub-collection indexes
		/// with this method.
		/// 
		/// <p/><b>NOTE:</b> the index in each Directory must not be
		/// changed (opened by a writer) while this method is
		/// running.  This method does not acquire a write lock in
		/// each input Directory, so it is up to the caller to
		/// enforce this.
		/// 
		/// <p/><b>NOTE:</b> while this is running, any attempts to
		/// add or delete documents (with another thread) will be
		/// paused until this method completes.
		/// 
		/// <p/>This method is transactional in how Exceptions are
		/// handled: it does not commit a new segments_N file until
		/// all indexes are added.  This means if an Exception
		/// occurs (for example disk full), then either no indexes
		/// will have been added or they all will have been.<p/>
		/// 
		/// <p/>Note that this requires temporary free space in the
		/// Directory up to 2X the sum of all input indexes
		/// (including the starting index).  If readers/searchers
		/// are open against the starting index, then temporary
		/// free space required will be higher by the size of the
		/// starting index (see {@link #Optimize()} for details).
		/// <p/>
		/// 
		/// <p/>Once this completes, the final size of the index
		/// will be less than the sum of all input index sizes
		/// (including the starting index).  It could be quite a
		/// bit smaller (if there were many pending deletes) or
		/// just slightly smaller.<p/>
		/// 
		/// <p/>
		/// This requires this index not be among those to be added.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  AddIndexesNoOptimize(Directory[] dirs)
		{
			
			EnsureOpen();
			
			NoDupDirs(dirs);
			
			// Do not allow add docs or deletes while we are running:
			docWriter.PauseAllThreads();
			
			try
			{
				if (infoStream != null)
					Message("flush at addIndexesNoOptimize");
				Flush(true, false, true);
				
				bool success = false;
				
				StartTransaction(false);
				
				try
				{
					
					int docCount = 0;
					lock (this)
					{
						EnsureOpen();
						
						for (int i = 0; i < dirs.Length; i++)
						{
							if (directory == dirs[i])
							{
								// cannot add this index: segments may be deleted in merge before added
								throw new System.ArgumentException("Cannot add this index to itself");
							}
							
							SegmentInfos sis = new SegmentInfos(); // read infos from dir
							sis.Read(dirs[i]);
							for (int j = 0; j < sis.Count; j++)
							{
								SegmentInfo info = sis.Info(j);
								System.Diagnostics.Debug.Assert(!segmentInfos.Contains(info), "dup info dir=" + info.dir + " name=" + info.name);
								docCount += info.docCount;
								segmentInfos.Add(info); // add each info
							}
						}
					}
					
					// Notify DocumentsWriter that the flushed count just increased
					docWriter.UpdateFlushedDocCount(docCount);
					
					MaybeMerge();
					
					EnsureOpen();
					
					// If after merging there remain segments in the index
					// that are in a different directory, just copy these
					// over into our index.  This is necessary (before
					// finishing the transaction) to avoid leaving the
					// index in an unusable (inconsistent) state.
					ResolveExternalSegments();
					
					EnsureOpen();
					
					success = true;
				}
				finally
				{
					if (success)
					{
						CommitTransaction();
					}
					else
					{
						RollbackTransaction();
					}
				}
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "addIndexesNoOptimize");
			}
			finally
			{
				if (docWriter != null)
				{
					docWriter.ResumeAllThreads();
				}
			}
		}
		
		private bool HasExternalSegments()
		{
			return segmentInfos.HasExternalSegments(directory);
		}
		
		/* If any of our segments are using a directory != ours
		* then we have to either copy them over one by one, merge
		* them (if merge policy has chosen to) or wait until
		* currently running merges (in the background) complete.
		* We don't return until the SegmentInfos has no more
		* external segments.  Currently this is only used by
		* addIndexesNoOptimize(). */
		private void  ResolveExternalSegments()
		{
			
			bool any = false;
			
			bool done = false;
			
			while (!done)
			{
				SegmentInfo info = null;
				MergePolicy.OneMerge merge = null;
				lock (this)
				{
					
					if (stopMerges)
						throw new MergePolicy.MergeAbortedException("rollback() was called or addIndexes* hit an unhandled exception");
					
					int numSegments = segmentInfos.Count;
					
					done = true;
					for (int i = 0; i < numSegments; i++)
					{
						info = segmentInfos.Info(i);
						if (info.dir != directory)
						{
							done = false;
							MergePolicy.OneMerge newMerge = new MergePolicy.OneMerge(segmentInfos.Range(i, 1 + i), mergePolicy is LogMergePolicy && GetUseCompoundFile());
							
							// Returns true if no running merge conflicts
							// with this one (and, records this merge as
							// pending), ie, this segment is not currently
							// being merged:
							if (RegisterMerge(newMerge))
							{
								merge = newMerge;
								
								// If this segment is not currently being
								// merged, then advance it to running & run
								// the merge ourself (below):
                                pendingMerges.Remove(merge);    // {{Aroush-2.9}} From Mike Garski: this is an O(n) op... is that an issue?
								runningMerges.Add(merge);
								break;
							}
						}
					}
					
					if (!done && merge == null)
					// We are not yet done (external segments still
					// exist in segmentInfos), yet, all such segments
					// are currently "covered" by a pending or running
					// merge.  We now try to grab any pending merge
					// that involves external segments:
						merge = GetNextExternalMerge();
					
					if (!done && merge == null)
					// We are not yet done, and, all external segments
					// fall under merges that the merge scheduler is
					// currently running.  So, we now wait and check
					// back to see if the merge has completed.
						DoWait();
				}
				
				if (merge != null)
				{
					any = true;
					Merge(merge);
				}
			}
			
			if (any)
			// Sometimes, on copying an external segment over,
			// more merges may become necessary:
				mergeScheduler.Merge(this);
		}
		
		/// <summary>Merges the provided indexes into this index.
		/// <p/>After this completes, the index is optimized. <p/>
		/// <p/>The provided IndexReaders are not closed.<p/>
		/// 
		/// <p/><b>NOTE:</b> while this is running, any attempts to
		/// add or delete documents (with another thread) will be
		/// paused until this method completes.
		/// 
		/// <p/>See {@link #AddIndexesNoOptimize(Directory[])} for
		/// details on transactional semantics, temporary free
		/// space required in the Directory, and non-CFS segments
		/// on an Exception.<p/>
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  AddIndexes(IndexReader[] readers)
		{
			
			EnsureOpen();
			
			// Do not allow add docs or deletes while we are running:
			docWriter.PauseAllThreads();
			
			// We must pre-acquire a read lock here (and upgrade to
			// write lock in startTransaction below) so that no
			// other addIndexes is allowed to start up after we have
			// flushed & optimized but before we then start our
			// transaction.  This is because the merging below
			// requires that only one segment is present in the
			// index:
			AcquireRead();
			
			try
			{
				
				SegmentInfo info = null;
				System.String mergedName = null;
				SegmentMerger merger = null;
				
				bool success = false;
				
				try
				{
					Flush(true, false, true);
					Optimize(); // start with zero or 1 seg
					success = true;
				}
				finally
				{
					// Take care to release the read lock if we hit an
					// exception before starting the transaction
					if (!success)
						ReleaseRead();
				}
				
				// true means we already have a read lock; if this
				// call hits an exception it will release the write
				// lock:
				StartTransaction(true);
				
				try
				{
					mergedName = NewSegmentName();
					merger = new SegmentMerger(this, mergedName, null);
					
					SegmentReader sReader = null;
					lock (this)
					{
						if (segmentInfos.Count == 1)
						{
							// add existing index, if any
							sReader = readerPool.Get(segmentInfos.Info(0), true, BufferedIndexInput.BUFFER_SIZE, - 1);
						}
					}
					
					success = false;
					
					try
					{
						if (sReader != null)
							merger.Add(sReader);
						
						for (int i = 0; i < readers.Length; i++)
						// add new indexes
							merger.Add(readers[i]);
						
						int docCount = merger.Merge(); // merge 'em
						
						lock (this)
						{
							segmentInfos.Clear(); // pop old infos & add new
							info = new SegmentInfo(mergedName, docCount, directory, false, true, - 1, null, false, merger.HasProx());
							SetDiagnostics(info, "addIndexes(IndexReader[])");
							segmentInfos.Add(info);
						}
						
						// Notify DocumentsWriter that the flushed count just increased
						docWriter.UpdateFlushedDocCount(docCount);
						
						success = true;
					}
					finally
					{
						if (sReader != null)
						{
							readerPool.Release(sReader);
						}
					}
				}
				finally
				{
					if (!success)
					{
						if (infoStream != null)
							Message("hit exception in addIndexes during merge");
						RollbackTransaction();
					}
					else
					{
						CommitTransaction();
					}
				}
				
				if (mergePolicy is LogMergePolicy && GetUseCompoundFile())
				{
					
					System.Collections.Generic.IList<string> files = null;
					
					lock (this)
					{
						// Must incRef our files so that if another thread
						// is running merge/optimize, it doesn't delete our
						// segment's files before we have a change to
						// finish making the compound file.
						if (segmentInfos.Contains(info))
						{
							files = info.Files();
							deleter.IncRef(files);
						}
					}
					
					if (files != null)
					{
						
						success = false;
						
						StartTransaction(false);
						
						try
						{
							merger.CreateCompoundFile(mergedName + ".cfs");
							lock (this)
							{
								info.SetUseCompoundFile(true);
							}
							
							success = true;
						}
						finally
						{
                            lock (this)
                            {
                                deleter.DecRef(files);
                            }
														
							if (!success)
							{
								if (infoStream != null)
									Message("hit exception building compound file in addIndexes during merge");
								
								RollbackTransaction();
							}
							else
							{
								CommitTransaction();
							}
						}
					}
				}
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "addIndexes(IndexReader[])");
			}
			finally
			{
				if (docWriter != null)
				{
					docWriter.ResumeAllThreads();
				}
			}
		}

        ///<summary>
        /// A hook for extending classes to execute operations after pending added and
        /// deleted documents have been flushed to the Directory but before the change
        /// is committed (new segments_N file written).
        ///</summary>   
		protected  virtual void  DoAfterFlush()
		{
		}
		
		/// <summary> Flush all in-memory buffered updates (adds and deletes)
		/// to the Directory. 
		/// <p/>Note: while this will force buffered docs to be
		/// pushed into the index, it will not make these docs
		/// visible to a reader.  Use {@link #Commit()} instead
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <deprecated> please call {@link #Commit()}) instead
		/// 
		/// </deprecated>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
        [Obsolete("please call Commit() instead")]
		public void  Flush()
		{
			if (hitOOM)
			{
				throw new System.SystemException("this writer hit an OutOfMemoryError; cannot flush");
			}
			
			Flush(true, false, true);
		}

        ///<summary>
        /// A hook for extending classes to execute operations before pending added and
        /// deleted documents are flushed to the Directory.
        ///</summary>
        protected virtual void DoBeforeFlush() 
        {
        }
		
		/// <summary>Expert: prepare for commit.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <seealso cref="PrepareCommit(Map)">
		/// </seealso>
		public void  PrepareCommit()
		{
			EnsureOpen();
			PrepareCommit(null);
		}
		
		/// <summary><p/>Expert: prepare for commit, specifying
		/// commitUserData Map (String -> String).  This does the
		/// first phase of 2-phase commit.  You can only call this
		/// when autoCommit is false.  This method does all steps
		/// necessary to commit changes since this writer was
		/// opened: flushes pending added and deleted docs, syncs
		/// the index files, writes most of next segments_N file.
		/// After calling this you must call either {@link
		/// #Commit()} to finish the commit, or {@link
		/// #Rollback()} to revert the commit and undo all changes
		/// done since the writer was opened.<p/>
		/// 
		/// You can also just call {@link #Commit(Map)} directly
		/// without prepareCommit first in which case that method
		/// will internally call prepareCommit.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <param name="commitUserData">Opaque Map (String->String)
		/// that's recorded into the segments file in the index,
		/// and retrievable by {@link
		/// IndexReader#getCommitUserData}.  Note that when
		/// IndexWriter commits itself, for example if open with
		/// autoCommit=true, or, during {@link #close}, the
		/// commitUserData is unchanged (just carried over from
		/// the prior commit).  If this is null then the previous
		/// commitUserData is kept.  Also, the commitUserData will
		/// only "stick" if there are actually changes in the
		/// index to commit.  Therefore it's best to use this
		/// feature only when autoCommit is false.
		/// </param>
        public void PrepareCommit(System.Collections.Generic.IDictionary<string, string> commitUserData)
		{
			PrepareCommit(commitUserData, false);
		}

        private void PrepareCommit(System.Collections.Generic.IDictionary<string, string> commitUserData, bool internal_Renamed)
		{
			
			if (hitOOM)
			{
				throw new System.SystemException("this writer hit an OutOfMemoryError; cannot commit");
			}
			
			if (autoCommit && !internal_Renamed)
				throw new System.SystemException("this method can only be used when autoCommit is false");
			
			if (!autoCommit && pendingCommit != null)
				throw new System.SystemException("prepareCommit was already called with no corresponding call to commit");
			
			if (infoStream != null)
				Message("prepareCommit: flush");
			
			Flush(true, true, true);
			
			StartCommit(0, commitUserData);
		}
		
        // Used only by commit, below; lock order is commitLock -> IW
        private Object commitLock = new Object();

		private void  Commit(long sizeInBytes)
		{
            lock(commitLock) {
                StartCommit(sizeInBytes, null);
                FinishCommit();
            }
		}
		
		/// <summary> <p/>Commits all pending changes (added &amp; deleted
		/// documents, optimizations, segment merges, added
		/// indexes, etc.) to the index, and syncs all referenced
		/// index files, such that a reader will see the changes
		/// and the index updates will survive an OS or machine
		/// crash or power loss.  Note that this does not wait for
		/// any running background merges to finish.  This may be a
		/// costly operation, so you should test the cost in your
		/// application and do it only when really necessary.<p/>
		/// 
		/// <p/> Note that this operation calls Directory.sync on
		/// the index files.  That call should not return until the
		/// file contents &amp; metadata are on stable storage.  For
		/// FSDirectory, this calls the OS's fsync.  But, beware:
		/// some hardware devices may in fact cache writes even
		/// during fsync, and return before the bits are actually
		/// on stable storage, to give the appearance of faster
		/// performance.  If you have such a device, and it does
		/// not have a battery backup (for example) then on power
		/// loss it may still lose data.  Lucene cannot guarantee
		/// consistency on such devices.  <p/>
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// 
		/// </summary>
		/// <seealso cref="prepareCommit">
		/// </seealso>
		/// <seealso cref="Commit(Map)">
		/// </seealso>
		public void  Commit()
		{
			Commit(null);
		}
		
		/// <summary>Commits all changes to the index, specifying a
		/// commitUserData Map (String -> String).  This just
		/// calls {@link #PrepareCommit(Map)} (if you didn't
		/// already call it) and then {@link #finishCommit}.
		/// 
		/// <p/><b>NOTE</b>: if this method hits an OutOfMemoryError
		/// you should immediately close the writer.  See <a
		/// href="#OOME">above</a> for details.<p/>
		/// </summary>
        public void Commit(System.Collections.Generic.IDictionary<string, string> commitUserData)
		{
			
			EnsureOpen();

            if (infoStream != null)
            {
                Message("commit: start");
            }

            lock (commitLock)
            {
                if (infoStream != null)
                {
                    Message("commit: enter lock");
                }
                if (autoCommit || pendingCommit == null)
                {
                    if (infoStream != null)
                        Message("commit: now prepare");
                    PrepareCommit(commitUserData, true);
                }
                else if (infoStream != null)
                {
                    Message("commit: already prepared");
                }

                FinishCommit();
            }
		}
		
		private void  FinishCommit()
		{
			lock (this)
			{
				
				if (pendingCommit != null)
				{
					try
					{
						if (infoStream != null)
							Message("commit: pendingCommit != null");
						pendingCommit.FinishCommit(directory);
						if (infoStream != null)
							Message("commit: wrote segments file \"" + pendingCommit.GetCurrentSegmentFileName() + "\"");
						lastCommitChangeCount = pendingCommitChangeCount;
						segmentInfos.UpdateGeneration(pendingCommit);
						segmentInfos.SetUserData(pendingCommit.GetUserData());
						SetRollbackSegmentInfos(pendingCommit);
						deleter.Checkpoint(pendingCommit, true);
					}
					finally
					{
						deleter.DecRef(pendingCommit);
						pendingCommit = null;
						System.Threading.Monitor.PulseAll(this);
					}
				}
                else if (infoStream != null)
                {
                    Message("commit: pendingCommit == null; skip");
                }

                if (infoStream != null)
                {
                    Message("commit: done");
                }
			}
		}
		
		/// <summary> Flush all in-memory buffered udpates (adds and deletes)
		/// to the Directory.
		/// </summary>
		/// <param name="triggerMerge">if true, we may merge segments (if
		/// deletes or docs were flushed) if necessary
		/// </param>
		/// <param name="flushDocStores">if false we are allowed to keep
		/// doc stores open to share with the next segment
		/// </param>
		/// <param name="flushDeletes">whether pending deletes should also
		/// be flushed
		/// </param>
		public /*protected internal*/ void  Flush(bool triggerMerge, bool flushDocStores, bool flushDeletes)
		{
			// We can be called during close, when closing==true, so we must pass false to ensureOpen:
			EnsureOpen(false);
			if (DoFlush(flushDocStores, flushDeletes) && triggerMerge)
				MaybeMerge();
		}
		
		// TODO: this method should not have to be entirely
		// synchronized, ie, merges should be allowed to commit
		// even while a flush is happening
		private bool DoFlush(bool flushDocStores, bool flushDeletes)
		{
			lock (this)
			{
				try
				{
					return DoFlushInternal(flushDocStores, flushDeletes);
				}
				finally
				{
                    if (docWriter.DoBalanceRAM())
                    {
                        docWriter.BalanceRAM();
                    }
					docWriter.ClearFlushPending();
				}
			}
		}
		
		// TODO: this method should not have to be entirely
		// synchronized, ie, merges should be allowed to commit
		// even while a flush is happening
		private bool DoFlushInternal(bool flushDocStores, bool flushDeletes)
		{
			lock (this)
			{
				
				if (hitOOM)
				{
					throw new System.SystemException("this writer hit an OutOfMemoryError; cannot flush");
				}
				
				EnsureOpen(false);
				
				System.Diagnostics.Debug.Assert(TestPoint("startDoFlush"));

                DoBeforeFlush();
				
				flushCount++;
				
				// If we are flushing because too many deletes
				// accumulated, then we should apply the deletes to free
				// RAM:
				flushDeletes |= docWriter.DoApplyDeletes();
				
				// When autoCommit=true we must always flush deletes
				// when flushing a segment; otherwise deletes may become
				// visible before their corresponding added document
				// from an updateDocument call
				flushDeletes |= autoCommit;
				
				// Make sure no threads are actively adding a document.
				// Returns true if docWriter is currently aborting, in
				// which case we skip flushing this segment
                if (infoStream != null)
                {
                    Message("flush: now pause all indexing threads");
                }
				if (docWriter.PauseAllThreads())
				{
					docWriter.ResumeAllThreads();
					return false;
				}
				
				try
				{
					
					SegmentInfo newSegment = null;
					
					int numDocs = docWriter.GetNumDocsInRAM();
					
					// Always flush docs if there are any
					bool flushDocs = numDocs > 0;
					
					// With autoCommit=true we always must flush the doc
					// stores when we flush
					flushDocStores |= autoCommit;
					System.String docStoreSegment = docWriter.GetDocStoreSegment();
					
					System.Diagnostics.Debug.Assert(docStoreSegment != null || numDocs == 0);
					
					if (docStoreSegment == null)
						flushDocStores = false;
					
					int docStoreOffset = docWriter.GetDocStoreOffset();
					
					// docStoreOffset should only be non-zero when
					// autoCommit == false
					System.Diagnostics.Debug.Assert(!autoCommit || 0 == docStoreOffset);
					
					bool docStoreIsCompoundFile = false;
					
					if (infoStream != null)
					{
						Message("  flush: segment=" + docWriter.GetSegment() + " docStoreSegment=" + docWriter.GetDocStoreSegment() + " docStoreOffset=" + docStoreOffset + " flushDocs=" + flushDocs + " flushDeletes=" + flushDeletes + " flushDocStores=" + flushDocStores + " numDocs=" + numDocs + " numBufDelTerms=" + docWriter.GetNumBufferedDeleteTerms());
						Message("  index before flush " + SegString());
					}
					
					// Check if the doc stores must be separately flushed
					// because other segments, besides the one we are about
					// to flush, reference it
					if (flushDocStores && (!flushDocs || !docWriter.GetSegment().Equals(docWriter.GetDocStoreSegment())))
					{
						// We must separately flush the doc store
						if (infoStream != null)
							Message("  flush shared docStore segment " + docStoreSegment);
						
						docStoreIsCompoundFile = FlushDocStores();
						flushDocStores = false;
					}
					
					System.String segment = docWriter.GetSegment();
					
					// If we are flushing docs, segment must not be null:
					System.Diagnostics.Debug.Assert(segment != null || !flushDocs);
					
					if (flushDocs)
					{
						
						bool success = false;
						int flushedDocCount;
						
						try
						{
							flushedDocCount = docWriter.Flush(flushDocStores);
                            if (infoStream != null)
                            {
                                Message("flushedFiles=" + docWriter.GetFlushedFiles());
                            }
							success = true;
						}
						finally
						{
							if (!success)
							{
								if (infoStream != null)
									Message("hit exception flushing segment " + segment);
								deleter.Refresh(segment);
							}
						}
						
						if (0 == docStoreOffset && flushDocStores)
						{
							// This means we are flushing private doc stores
							// with this segment, so it will not be shared
							// with other segments
							System.Diagnostics.Debug.Assert(docStoreSegment != null);
							System.Diagnostics.Debug.Assert(docStoreSegment.Equals(segment));
							docStoreOffset = - 1;
							docStoreIsCompoundFile = false;
							docStoreSegment = null;
						}
						
						// Create new SegmentInfo, but do not add to our
						// segmentInfos until deletes are flushed
						// successfully.
						newSegment = new SegmentInfo(segment, flushedDocCount, directory, false, true, docStoreOffset, docStoreSegment, docStoreIsCompoundFile, docWriter.HasProx());
						SetDiagnostics(newSegment, "flush");
					}
					
					docWriter.PushDeletes();
					
					if (flushDocs)
					{
						segmentInfos.Add(newSegment);
						Checkpoint();
					}
					
					if (flushDocs && mergePolicy.UseCompoundFile(segmentInfos, newSegment))
					{
						// Now build compound file
						bool success = false;
						try
						{
							docWriter.CreateCompoundFile(segment);
							success = true;
						}
						finally
						{
							if (!success)
							{
								if (infoStream != null)
									Message("hit exception creating compound file for newly flushed segment " + segment);
								deleter.DeleteFile(segment + "." + IndexFileNames.COMPOUND_FILE_EXTENSION);
							}
						}
						
						newSegment.SetUseCompoundFile(true);
						Checkpoint();
					}
					
					if (flushDeletes)
					{
						ApplyDeletes();
					}
					
					if (flushDocs)
						Checkpoint();
					
					DoAfterFlush();
					
					return flushDocs;
				}
				catch (System.OutOfMemoryException oom)
				{
					HandleOOM(oom, "doFlush");
					// never hit
					return false;
				}
				finally
				{
					docWriter.ResumeAllThreads();
				}
			}
		}
		
		/// <summary>Expert:  Return the total size of all index files currently cached in memory.
		/// Useful for size management with flushRamDocs()
		/// </summary>
		public long RamSizeInBytes()
		{
			EnsureOpen();
			return docWriter.GetRAMUsed();
		}
		
		/// <summary>Expert:  Return the number of documents currently
		/// buffered in RAM. 
		/// </summary>
		public int NumRamDocs()
		{
			lock (this)
			{
				EnsureOpen();
				return docWriter.GetNumDocsInRAM();
			}
		}
		
		private int EnsureContiguousMerge(MergePolicy.OneMerge merge)
		{
			
			int first = segmentInfos.IndexOf(merge.segments.Info(0));
			if (first == - 1)
				throw new MergePolicy.MergeException("could not find segment " + merge.segments.Info(0).name + " in current index " + SegString(), directory);
			
			int numSegments = segmentInfos.Count;
			
			int numSegmentsToMerge = merge.segments.Count;
			for (int i = 0; i < numSegmentsToMerge; i++)
			{
				SegmentInfo info = merge.segments.Info(i);
				
				if (first + i >= numSegments || !segmentInfos.Info(first + i).Equals(info))
				{
					if (segmentInfos.IndexOf(info) == - 1)
						throw new MergePolicy.MergeException("MergePolicy selected a segment (" + info.name + ") that is not in the current index " + SegString(), directory);
					else
						throw new MergePolicy.MergeException("MergePolicy selected non-contiguous segments to merge (" + merge.SegString(directory) + " vs " + SegString() + "), which IndexWriter (currently) cannot handle", directory);
				}
			}
			
			return first;
		}
		
		/// <summary>Carefully merges deletes for the segments we just
		/// merged.  This is tricky because, although merging will
		/// clear all deletes (compacts the documents), new
		/// deletes may have been flushed to the segments since
		/// the merge was started.  This method "carries over"
		/// such new deletes onto the newly merged segment, and
		/// saves the resulting deletes file (incrementing the
		/// delete generation for merge.info).  If no deletes were
		/// flushed, no new deletes file is saved. 
		/// </summary>
		private void  CommitMergedDeletes(MergePolicy.OneMerge merge, SegmentReader mergeReader)
		{
			lock (this)
			{
				
				System.Diagnostics.Debug.Assert(TestPoint("startCommitMergeDeletes"));
				
				SegmentInfos sourceSegments = merge.segments;
				
				if (infoStream != null)
					Message("commitMergeDeletes " + merge.SegString(directory));
				
				// Carefully merge deletes that occurred after we
				// started merging:
				int docUpto = 0;
				int delCount = 0;
				
				for (int i = 0; i < sourceSegments.Count; i++)
				{
					SegmentInfo info = sourceSegments.Info(i);
					int docCount = info.docCount;
					SegmentReader previousReader = merge.readersClone[i];
					SegmentReader currentReader = merge.readers[i];
					if (previousReader.HasDeletions())
					{
						
						// There were deletes on this segment when the merge
						// started.  The merge has collapsed away those
						// deletes, but, if new deletes were flushed since
						// the merge started, we must now carefully keep any
						// newly flushed deletes but mapping them to the new
						// docIDs.
						
						if (currentReader.NumDeletedDocs() > previousReader.NumDeletedDocs())
						{
							// This means this segment has had new deletes
							// committed since we started the merge, so we
							// must merge them:
							for (int j = 0; j < docCount; j++)
							{
								if (previousReader.IsDeleted(j))
								{
									System.Diagnostics.Debug.Assert(currentReader.IsDeleted(j));
                                }
								else
								{
									if (currentReader.IsDeleted(j))
									{
										mergeReader.DoDelete(docUpto);
										delCount++;
									}
									docUpto++;
								}
							}
						}
						else
						{
							docUpto += docCount - previousReader.NumDeletedDocs();
						}
					}
					else if (currentReader.HasDeletions())
					{
						// This segment had no deletes before but now it
						// does:
						for (int j = 0; j < docCount; j++)
						{
							if (currentReader.IsDeleted(j))
							{
								mergeReader.DoDelete(docUpto);
								delCount++;
							}
							docUpto++;
						}
					}
					// No deletes before or after
					else
						docUpto += info.docCount;
				}
				
				System.Diagnostics.Debug.Assert(mergeReader.NumDeletedDocs() == delCount);
				
				mergeReader.hasChanges = delCount > 0;
			}
		}
		
		/* FIXME if we want to support non-contiguous segment merges */
		private bool CommitMerge(MergePolicy.OneMerge merge, SegmentMerger merger, int mergedDocCount, SegmentReader mergedReader)
		{
			lock (this)
			{
				
				System.Diagnostics.Debug.Assert(TestPoint("startCommitMerge"));
				
				if (hitOOM)
				{
					throw new System.SystemException("this writer hit an OutOfMemoryError; cannot complete merge");
				}
				
				if (infoStream != null)
					Message("commitMerge: " + merge.SegString(directory) + " index=" + SegString());
				
				System.Diagnostics.Debug.Assert(merge.registerDone);
				
				// If merge was explicitly aborted, or, if rollback() or
				// rollbackTransaction() had been called since our merge
				// started (which results in an unqualified
				// deleter.refresh() call that will remove any index
				// file that current segments does not reference), we
				// abort this merge
				if (merge.IsAborted())
				{
					if (infoStream != null)
						Message("commitMerge: skipping merge " + merge.SegString(directory) + ": it was aborted");
					
					return false;
				}
				
				int start = EnsureContiguousMerge(merge);
				
				CommitMergedDeletes(merge, mergedReader);
				docWriter.RemapDeletes(segmentInfos, merger.GetDocMaps(), merger.GetDelCounts(), merge, mergedDocCount);

                // If the doc store we are using has been closed and
                // is in now compound format (but wasn't when we
                // started), then we will switch to the compound
                // format as well:
                SetMergeDocStoreIsCompoundFile(merge);
				
				merge.info.SetHasProx(merger.HasProx());
				
				((System.Collections.IList) ((System.Collections.ArrayList) segmentInfos).GetRange(start, start + merge.segments.Count - start)).Clear();
				System.Diagnostics.Debug.Assert(!segmentInfos.Contains(merge.info));
				segmentInfos.Insert(start, merge.info);

                CloseMergeReaders(merge, false);
				
				// Must note the change to segmentInfos so any commits
				// in-flight don't lose it:
				Checkpoint();
				
				// If the merged segments had pending changes, clear
				// them so that they don't bother writing them to
				// disk, updating SegmentInfo, etc.:
				readerPool.Clear(merge.segments);

                if (merge.optimize)
                {
                    // cascade the optimize:
                    segmentsToOptimize[merge.info] = merge.info;
                }
				return true;
			}
		}
		
		private void  HandleMergeException(System.Exception t, MergePolicy.OneMerge merge)
		{
			
			if (infoStream != null)
			{
				Message("handleMergeException: merge=" + merge.SegString(directory) + " exc=" + t);
			}
			
			// Set the exception on the merge, so if
			// optimize() is waiting on us it sees the root
			// cause exception:
			merge.SetException(t);
			AddMergeException(merge);
			
			if (t is MergePolicy.MergeAbortedException)
			{
				// We can ignore this exception (it happens when
				// close(false) or rollback is called), unless the
				// merge involves segments from external directories,
				// in which case we must throw it so, for example, the
				// rollbackTransaction code in addIndexes* is
				// executed.
				if (merge.isExternal)
					throw (MergePolicy.MergeAbortedException) t;
			}
			else if (t is System.IO.IOException)
				throw (System.IO.IOException) t;
			else if (t is System.SystemException)
				throw (System.SystemException) t;
			else if (t is System.ApplicationException)
				throw (System.ApplicationException) t;
			// Should not get here
			else
				throw new System.SystemException(null, t);
		}
		
		public void Merge_ForNUnit(MergePolicy.OneMerge merge)
        {
            Merge(merge);
        }
		/// <summary> Merges the indicated segments, replacing them in the stack with a
		/// single segment.
		/// </summary>
		internal void  Merge(MergePolicy.OneMerge merge)
		{
			
			bool success = false;
			
			try
			{
				try
				{
					try
					{
						MergeInit(merge);
						
						if (infoStream != null)
						{
							Message("now merge\n  merge=" + merge.SegString(directory) + "\n  merge=" + merge + "\n  index=" + SegString());
						}
						
						MergeMiddle(merge);
						MergeSuccess(merge);
						success = true;
					}
					catch (System.Exception t)
					{
						HandleMergeException(t, merge);
					}
				}
				finally
				{
					lock (this)
					{
						MergeFinish(merge);
						
						if (!success)
						{
							if (infoStream != null)
								Message("hit exception during merge");
							if (merge.info != null && !segmentInfos.Contains(merge.info))
								deleter.Refresh(merge.info.name);
						}
						
						// This merge (and, generally, any change to the
						// segments) may now enable new merges, so we call
						// merge policy & update pending merges.
						if (success && !merge.IsAborted() && !closed && !closing)
							UpdatePendingMerges(merge.maxNumSegmentsOptimize, merge.optimize);
					}
				}
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "merge");
			}
		}
		
		/// <summary>Hook that's called when the specified merge is complete. </summary>
		internal virtual void  MergeSuccess(MergePolicy.OneMerge merge)
		{
		}
		
		/// <summary>Checks whether this merge involves any segments
		/// already participating in a merge.  If not, this merge
		/// is "registered", meaning we record that its segments
		/// are now participating in a merge, and true is
		/// returned.  Else (the merge conflicts) false is
		/// returned. 
		/// </summary>
		internal bool RegisterMerge(MergePolicy.OneMerge merge)
		{
			lock (this)
			{
				
				if (merge.registerDone)
					return true;
				
				if (stopMerges)
				{
					merge.Abort();
					throw new MergePolicy.MergeAbortedException("merge is aborted: " + merge.SegString(directory));
				}
				
				int count = merge.segments.Count;
				bool isExternal = false;
				for (int i = 0; i < count; i++)
				{
					SegmentInfo info = merge.segments.Info(i);
                    if (mergingSegments.Contains(info))
                    {
                        return false;
                    }
                    if (segmentInfos.IndexOf(info) == -1)
                    {
                        return false;
                    }
                    if (info.dir != directory)
                    {
                        isExternal = true;
                    }
                    if (segmentsToOptimize.Contains(info))
                    {
                        merge.optimize = true;
                        merge.maxNumSegmentsOptimize = optimizeMaxNumSegments;
                    }
				}
				
				EnsureContiguousMerge(merge);
				
				pendingMerges.AddLast(merge);
				
				if (infoStream != null)
					Message("add merge to pendingMerges: " + merge.SegString(directory) + " [total " + pendingMerges.Count + " pending]");
				
				merge.mergeGen = mergeGen;
				merge.isExternal = isExternal;
				
				// OK it does not conflict; now record that this merge
				// is running (while synchronized) to avoid race
				// condition where two conflicting merges from different
				// threads, start
                for (int i = 0; i < count; i++)
                {
                    SegmentInfo si = merge.segments.Info(i);
                    mergingSegments[si] = si;
                }
				
				// Merge is now registered
				merge.registerDone = true;
				return true;
			}
		}
		
		/// <summary>Does initial setup for a merge, which is fast but holds
		/// the synchronized lock on IndexWriter instance.  
		/// </summary>
		internal void  MergeInit(MergePolicy.OneMerge merge)
		{
			lock (this)
			{
				bool success = false;
				try
				{
					_MergeInit(merge);
					success = true;
				}
				finally
				{
					if (!success)
					{
						MergeFinish(merge);
					}
				}
			}
		}
		
		private void  _MergeInit(MergePolicy.OneMerge merge)
		{
			lock (this)
			{
				
				System.Diagnostics.Debug.Assert(TestPoint("startMergeInit"));
				
				System.Diagnostics.Debug.Assert(merge.registerDone);
				System.Diagnostics.Debug.Assert(!merge.optimize || merge.maxNumSegmentsOptimize > 0);
				
				if (hitOOM)
				{
					throw new System.SystemException("this writer hit an OutOfMemoryError; cannot merge");
				}
				
				if (merge.info != null)
				// mergeInit already done
					return ;
				
				if (merge.IsAborted())
					return ;
				
				bool changed = ApplyDeletes();
				
				// If autoCommit == true then all deletes should have
				// been flushed when we flushed the last segment
				System.Diagnostics.Debug.Assert(!changed || !autoCommit);
				
				SegmentInfos sourceSegments = merge.segments;
				int end = sourceSegments.Count;
				
				// Check whether this merge will allow us to skip
				// merging the doc stores (stored field & vectors).
				// This is a very substantial optimization (saves tons
				// of IO) that can only be applied with
				// autoCommit=false.
				
				Directory lastDir = directory;
				System.String lastDocStoreSegment = null;
				int next = - 1;
				
				bool mergeDocStores = false;
				bool doFlushDocStore = false;
				System.String currentDocStoreSegment = docWriter.GetDocStoreSegment();
				
				// Test each segment to be merged: check if we need to
				// flush/merge doc stores
				for (int i = 0; i < end; i++)
				{
					SegmentInfo si = sourceSegments.Info(i);
					
					// If it has deletions we must merge the doc stores
					if (si.HasDeletions())
						mergeDocStores = true;
					
					// If it has its own (private) doc stores we must
					// merge the doc stores
					if (- 1 == si.GetDocStoreOffset())
						mergeDocStores = true;
					
					// If it has a different doc store segment than
					// previous segments, we must merge the doc stores
					System.String docStoreSegment = si.GetDocStoreSegment();
					if (docStoreSegment == null)
						mergeDocStores = true;
					else if (lastDocStoreSegment == null)
						lastDocStoreSegment = docStoreSegment;
					else if (!lastDocStoreSegment.Equals(docStoreSegment))
						mergeDocStores = true;
					
					// Segments' docScoreOffsets must be in-order,
					// contiguous.  For the default merge policy now
					// this will always be the case but for an arbitrary
					// merge policy this may not be the case
					if (- 1 == next)
						next = si.GetDocStoreOffset() + si.docCount;
					else if (next != si.GetDocStoreOffset())
						mergeDocStores = true;
					else
						next = si.GetDocStoreOffset() + si.docCount;
					
					// If the segment comes from a different directory
					// we must merge
					if (lastDir != si.dir)
						mergeDocStores = true;
					
					// If the segment is referencing the current "live"
					// doc store outputs then we must merge
					if (si.GetDocStoreOffset() != - 1 && currentDocStoreSegment != null && si.GetDocStoreSegment().Equals(currentDocStoreSegment))
					{
						doFlushDocStore = true;
					}
				}

                // if a mergedSegmentWarmer is installed, we must merge
                // the doc stores because we will open a full
                // SegmentReader on the merged segment:
                if (!mergeDocStores && mergedSegmentWarmer != null && currentDocStoreSegment != null && lastDocStoreSegment != null && lastDocStoreSegment.Equals(currentDocStoreSegment))
                {
                    mergeDocStores = true;
                }

				int docStoreOffset;
				System.String docStoreSegment2;
				bool docStoreIsCompoundFile;
				
				if (mergeDocStores)
				{
					docStoreOffset = - 1;
					docStoreSegment2 = null;
					docStoreIsCompoundFile = false;
				}
				else
				{
					SegmentInfo si = sourceSegments.Info(0);
					docStoreOffset = si.GetDocStoreOffset();
					docStoreSegment2 = si.GetDocStoreSegment();
					docStoreIsCompoundFile = si.GetDocStoreIsCompoundFile();
				}
				
				if (mergeDocStores && doFlushDocStore)
				{
					// SegmentMerger intends to merge the doc stores
					// (stored fields, vectors), and at least one of the
					// segments to be merged refers to the currently
					// live doc stores.
					
					// TODO: if we know we are about to merge away these
					// newly flushed doc store files then we should not
					// make compound file out of them...
					if (infoStream != null)
						Message("now flush at merge");
					DoFlush(true, false);
				}
				
				merge.mergeDocStores = mergeDocStores;
				
				// Bind a new segment name here so even with
				// ConcurrentMergePolicy we keep deterministic segment
				// names.
				merge.info = new SegmentInfo(NewSegmentName(), 0, directory, false, true, docStoreOffset, docStoreSegment2, docStoreIsCompoundFile, false);


                System.Collections.Generic.IDictionary<string, string> details = new System.Collections.Generic.Dictionary<string, string>();
				details["optimize"] = merge.optimize + "";
				details["mergeFactor"] = end + "";
				details["mergeDocStores"] = mergeDocStores + "";
				SetDiagnostics(merge.info, "merge", details);
				
				// Also enroll the merged segment into mergingSegments;
				// this prevents it from getting selected for a merge
				// after our merge is done but while we are building the
				// CFS:
                mergingSegments[merge.info] = merge.info;
			}
		}
		
		private void  SetDiagnostics(SegmentInfo info, System.String source)
		{
			SetDiagnostics(info, source, null);
		}

        private void SetDiagnostics(SegmentInfo info, System.String source, System.Collections.Generic.IDictionary<string, string> details)
		{
            System.Collections.Generic.IDictionary<string, string> diagnostics = new System.Collections.Generic.Dictionary<string,string>();
			diagnostics["source"] = source;
			diagnostics["lucene.version"] = Constants.LUCENE_VERSION;
			diagnostics["os"] = Constants.OS_NAME + "";
			diagnostics["os.arch"] = Constants.OS_ARCH + "";
			diagnostics["os.version"] = Constants.OS_VERSION + "";
			diagnostics["java.version"] = Constants.JAVA_VERSION + "";
			diagnostics["java.vendor"] = Constants.JAVA_VENDOR + "";
			if (details != null)
			{
				//System.Collections.ArrayList keys = new System.Collections.ArrayList(details.Keys);
				//System.Collections.ArrayList values = new System.Collections.ArrayList(details.Values);
                foreach (string key in details.Keys)
                {
                    diagnostics[key] = details[key];
                }
			}
			info.SetDiagnostics(diagnostics);
		}
		
		/// <summary>This is called after merging a segment and before
		/// building its CFS.  Return true if the files should be
		/// sync'd.  If you return false, then the source segment
		/// files that were merged cannot be deleted until the CFS
		/// file is built &amp; sync'd.  So, returning false consumes
		/// more transient disk space, but saves performance of
		/// not having to sync files which will shortly be deleted
		/// anyway.
		/// </summary>
		/// <deprecated> -- this will be removed in 3.0 when
		/// autoCommit is hardwired to false 
		/// </deprecated>
        [Obsolete("-- this will be removed in 3.0 when autoCommit is hardwired to false ")]
		private bool DoCommitBeforeMergeCFS(MergePolicy.OneMerge merge)
		{
			lock (this)
			{
				long freeableBytes = 0;
				int size = merge.segments.Count;
				for (int i = 0; i < size; i++)
				{
					SegmentInfo info = merge.segments.Info(i);
					// It's only important to sync if the most recent
					// commit actually references this segment, because if
					// it doesn't, even without syncing we will free up
					// the disk space:
                    bool exist = rollbackSegments.ContainsKey(info);
                    if (exist)
					{
						int loc = (System.Int32) rollbackSegments[info];
						SegmentInfo oldInfo = rollbackSegmentInfos.Info(loc);
						if (oldInfo.GetUseCompoundFile() != info.GetUseCompoundFile())
							freeableBytes += info.SizeInBytes();
					}
				}
				// If we would free up more than 1/3rd of the index by
				// committing now, then do so:
				long totalBytes = 0;
				int numSegments = segmentInfos.Count;
				for (int i = 0; i < numSegments; i++)
					totalBytes += segmentInfos.Info(i).SizeInBytes();
				if (3 * freeableBytes > totalBytes)
					return true;
				else
					return false;
			}
		}
		
		/// <summary>Does fininishing for a merge, which is fast but holds
		/// the synchronized lock on IndexWriter instance. 
		/// </summary>
		internal void  MergeFinish(MergePolicy.OneMerge merge)
		{
			lock (this)
			{
				
				// Optimize, addIndexes or finishMerges may be waiting
				// on merges to finish.
				System.Threading.Monitor.PulseAll(this);
				
				// It's possible we are called twice, eg if there was an
				// exception inside mergeInit
				if (merge.registerDone)
				{
					SegmentInfos sourceSegments = merge.segments;
					int end = sourceSegments.Count;
					for (int i = 0; i < end; i++)
						mergingSegments.Remove(sourceSegments.Info(i));
                    if(merge.info != null)
					    mergingSegments.Remove(merge.info);
					merge.registerDone = false;
				}
				
				runningMerges.Remove(merge);
			}
		}
		
        private void SetMergeDocStoreIsCompoundFile(MergePolicy.OneMerge merge)
        {
            lock (this)
            {
                string mergeDocStoreSegment = merge.info.GetDocStoreSegment();
                if (mergeDocStoreSegment != null && !merge.info.GetDocStoreIsCompoundFile())
                {
                    int size = segmentInfos.Count;
                    for (int i = 0; i < size; i++)
                    {
                        SegmentInfo info = segmentInfos.Info(i);
                        string docStoreSegment = info.GetDocStoreSegment();
                        if (docStoreSegment != null &&
                            docStoreSegment.Equals(mergeDocStoreSegment) &&
                            info.GetDocStoreIsCompoundFile())
                        {
                            merge.info.SetDocStoreIsCompoundFile(true);
                            break;
                        }
                    }
                }
            }
        }

        private void CloseMergeReaders(MergePolicy.OneMerge merge, bool suppressExceptions)
        {
            lock (this)
            {
                int numSegments = merge.segments.Count;
                if (suppressExceptions)
                {
                    // Suppress any new exceptions so we throw the
                    // original cause
                    for (int i = 0; i < numSegments; i++)
                    {
                        if (merge.readers[i] != null)
                        {
                            try
                            {
                                readerPool.Release(merge.readers[i], false);
                            }
                            catch (Exception t)
                            {
                            }
                            merge.readers[i] = null;
                        }

                        if (merge.readersClone[i] != null)
                        {
                            try
                            {
                                merge.readersClone[i].Close();
                            }
                            catch (Exception t)
                            {
                            }
                            // This was a private clone and we had the
                            // only reference
                            System.Diagnostics.Debug.Assert(merge.readersClone[i].GetRefCount() == 0); //: "refCount should be 0 but is " + merge.readersClone[i].getRefCount();
                            merge.readersClone[i] = null;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < numSegments; i++)
                    {
                        if (merge.readers[i] != null)
                        {
                            readerPool.Release(merge.readers[i], true);
                            merge.readers[i] = null;
                        }

                        if (merge.readersClone[i] != null)
                        {
                            merge.readersClone[i].Close();
                            // This was a private clone and we had the only reference
                            System.Diagnostics.Debug.Assert(merge.readersClone[i].GetRefCount() == 0);
                            merge.readersClone[i] = null;
                        }
                    }
                }
            }
        }


		/// <summary>Does the actual (time-consuming) work of the merge,
		/// but without holding synchronized lock on IndexWriter
		/// instance 
		/// </summary>
		private int MergeMiddle(MergePolicy.OneMerge merge)
		{
			
			merge.CheckAborted(directory);
			
			System.String mergedName = merge.info.name;
			
			SegmentMerger merger = null;
			
			int mergedDocCount = 0;
			
			SegmentInfos sourceSegments = merge.segments;
			int numSegments = sourceSegments.Count;
			
			if (infoStream != null)
				Message("merging " + merge.SegString(directory));
			
			merger = new SegmentMerger(this, mergedName, merge);
			
			merge.readers = new SegmentReader[numSegments];
			merge.readersClone = new SegmentReader[numSegments];
			
			bool mergeDocStores = false;

            System.Collections.Hashtable dss = new System.Collections.Hashtable();
			
            String currentDocStoreSegment;
            lock(this) {
                currentDocStoreSegment = docWriter.GetDocStoreSegment();
            }
            bool currentDSSMerged = false;

			// This is try/finally to make sure merger's readers are
			// closed:
			bool success = false;
            try
            {
                int totDocCount = 0;

                for (int i = 0; i < numSegments; i++)
                {

                    SegmentInfo info = sourceSegments.Info(i);

                    // Hold onto the "live" reader; we will use this to
                    // commit merged deletes
                    SegmentReader reader = merge.readers[i] = readerPool.Get(info, merge.mergeDocStores, MERGE_READ_BUFFER_SIZE, -1);

                    // We clone the segment readers because other
                    // deletes may come in while we're merging so we
                    // need readers that will not change
                    SegmentReader clone = merge.readersClone[i] = (SegmentReader)reader.Clone(true);
                    merger.Add(clone);

                    if (clone.HasDeletions())
                    {
                        mergeDocStores = true;
                    }

                    if (info.GetDocStoreOffset() != -1 && currentDocStoreSegment != null)
                    {
                        currentDSSMerged |= currentDocStoreSegment.Equals(info.GetDocStoreSegment());
                    }

                    totDocCount += clone.NumDocs();
                }

                if (infoStream != null)
                {
                    Message("merge: total " + totDocCount + " docs");
                }

                merge.CheckAborted(directory);

                // If deletions have arrived and it has now become
                // necessary to merge doc stores, go and open them:
                if (mergeDocStores && !merge.mergeDocStores)
                {
                    merge.mergeDocStores = true;
                    lock (this)
                    {
                        if (currentDSSMerged)
                        {
                            if (infoStream != null)
                            {
                                Message("now flush at mergeMiddle");
                            }
                            DoFlush(true, false);
                        }
                    }

                    for (int i = 0; i < numSegments; i++)
                    {
                        merge.readersClone[i].OpenDocStores();
                    }

                    // Clear DSS
                    merge.info.SetDocStore(-1, null, false);

                }

                // This is where all the work happens:
                mergedDocCount = merge.info.docCount = merger.Merge(merge.mergeDocStores);

                System.Diagnostics.Debug.Assert(mergedDocCount == totDocCount);

                if (merge.useCompoundFile)
                {

                    success = false;
                    string compoundFileName = IndexFileNames.SegmentFileName(mergedName, IndexFileNames.COMPOUND_FILE_EXTENSION);

                    try
                    {
                        if (infoStream != null)
                        {
                            Message("create compound file " + compoundFileName);
                        }
                        merger.CreateCompoundFile(compoundFileName);
                        success = true;
                    }
                    catch (System.IO.IOException ioe)
                    {
                        lock (this)
                        {
                            if (merge.IsAborted())
                            {
                                // This can happen if rollback or close(false)
                                // is called -- fall through to logic below to
                                // remove the partially created CFS:
                            }
                            else
                            {
                                HandleMergeException(ioe, merge);
                            }
                        }
                    }
                    catch (Exception t)
                    {
                        HandleMergeException(t, merge);
                    }
                    finally
                    {
                        if (!success)
                        {
                            if (infoStream != null)
                            {
                                Message("hit exception creating compound file during merge");
                            }

                            lock (this)
                            {
                                deleter.DeleteFile(compoundFileName);
                                deleter.DeleteNewFiles(merger.GetMergedFiles());
                            }
                        }
                    }

                    success = false;

                    lock (this)
                    {

                        // delete new non cfs files directly: they were never
                        // registered with IFD
                        deleter.DeleteNewFiles(merger.GetMergedFiles());

                        if (merge.IsAborted())
                        {
                            if (infoStream != null)
                            {
                                Message("abort merge after building CFS");
                            }
                            deleter.DeleteFile(compoundFileName);
                            return 0;
                        }
                    }

                    merge.info.SetUseCompoundFile(true);
                }

                int termsIndexDivisor;
                bool loadDocStores;

                // if the merged segment warmer was not installed when
                // this merge was started, causing us to not force
                // the docStores to close, we can't warm it now
                bool canWarm = merge.info.GetDocStoreSegment() == null || currentDocStoreSegment == null || !merge.info.GetDocStoreSegment().Equals(currentDocStoreSegment);

                if (poolReaders && mergedSegmentWarmer != null && canWarm)
                {
                    // Load terms index & doc stores so the segment
                    // warmer can run searches, load documents/term
                    // vectors
                    termsIndexDivisor = readerTermsIndexDivisor;
                    loadDocStores = true;
                }
                else
                {
                    termsIndexDivisor = -1;
                    loadDocStores = false;
                }

                // TODO: in the non-realtime case, we may want to only
                // keep deletes (it's costly to open entire reader
                // when we just need deletes)

                SegmentReader mergedReader = readerPool.Get(merge.info, loadDocStores, BufferedIndexInput.BUFFER_SIZE, termsIndexDivisor);
                try
                {
                    if (poolReaders && mergedSegmentWarmer != null)
                    {
                        mergedSegmentWarmer.Warm(mergedReader);
                    }
                    if (!CommitMerge(merge, merger, mergedDocCount, mergedReader))
                    {
                        // commitMerge will return false if this merge was aborted
                        return 0;
                    }
                }
                finally
                {
                    lock (this)
                    {
                        readerPool.Release(mergedReader);
                    }
                }

                success = true;
            }
            finally
            {
                // Readers are already closed in commitMerge if we didn't hit
                // an exc:
                if (!success)
                {
                    CloseMergeReaders(merge, true);
                }
            }

            merge.mergeDone = true;

            lock (mergeScheduler)
            {
                System.Threading.Monitor.PulseAll(mergeScheduler); 
            }

			// Force a sync after commiting the merge.  Once this
			// sync completes then all index files referenced by the
			// current segmentInfos are on stable storage so if the
			// OS/machine crashes, or power cord is yanked, the
			// index will be intact.  Note that this is just one
			// (somewhat arbitrary) policy; we could try other
			// policies like only sync if it's been > X minutes or
			// more than Y bytes have been written, etc.
			if (autoCommit)
			{
				long size;
				lock (this)
				{
					size = merge.info.SizeInBytes();
				}
				Commit(size);
			}
			
			return mergedDocCount;
		}
		
		internal virtual void  AddMergeException(MergePolicy.OneMerge merge)
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(merge.GetException() != null);
				if (!mergeExceptions.Contains(merge) && mergeGen == merge.mergeGen)
					mergeExceptions.Add(merge);
			}
		}
		
		// Apply buffered deletes to all segments.
		private bool ApplyDeletes()
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(TestPoint("startApplyDeletes"));
                flushDeletesCount++;
				
				bool success = false;
				bool changed;
				try
				{
					changed = docWriter.ApplyDeletes(segmentInfos);
					success = true;
				}
				finally
				{
                    if (!success && infoStream != null)
                    {
                        Message("hit exception flushing deletes");
                    }
				}
				
				if (changed)
					Checkpoint();
				return changed;
			}
		}
		
		// For test purposes.
		public /*internal*/ int GetBufferedDeleteTermsSize()
		{
			lock (this)
			{
				return docWriter.GetBufferedDeleteTerms().Count;
			}
		}
		
		// For test purposes.
		public /*internal*/ int GetNumBufferedDeleteTerms()
		{
			lock (this)
			{
				return docWriter.GetNumBufferedDeleteTerms();
			}
		}
		
		// utility routines for tests
		public /*internal*/ virtual SegmentInfo NewestSegment()
		{
            return segmentInfos.Count > 0 ? segmentInfos.Info(segmentInfos.Count - 1) : null;
		}
		
		public virtual System.String SegString()
		{
			lock (this)
			{
				return SegString(segmentInfos);
			}
		}
		
		private System.String SegString(SegmentInfos infos)
		{
			lock (this)
			{
				System.Text.StringBuilder buffer = new System.Text.StringBuilder();
				int count = infos.Count;
				for (int i = 0; i < count; i++)
				{
					if (i > 0)
					{
						buffer.Append(' ');
					}
					SegmentInfo info = infos.Info(i);
					buffer.Append(info.SegString(directory));
					if (info.dir != directory)
						buffer.Append("**");
				}
				return buffer.ToString();
			}
		}
		
		// Files that have been sync'd already
        private System.Collections.Generic.Dictionary<string, string> synced = new System.Collections.Generic.Dictionary<string, string>();
		
		// Files that are now being sync'd
        private System.Collections.Hashtable syncing = new System.Collections.Hashtable();
		
		private bool StartSync(System.String fileName, System.Collections.Generic.ICollection<System.String> pending)
		{
			lock (synced)
			{
				if (!synced.ContainsKey(fileName))
				{
					if (!syncing.Contains(fileName))
					{
						syncing[fileName] = fileName;
						return true;
					}
					else
					{
						pending.Add(fileName);
						return false;
					}
				}
				else
					return false;
			}
		}
		
		private void  FinishSync(System.String fileName, bool success)
		{
			lock (synced)
			{
				System.Diagnostics.Debug.Assert(syncing.ContainsKey(fileName));
				syncing.Remove(fileName);
				if (success)
                    synced[fileName] = fileName;
				System.Threading.Monitor.PulseAll(synced);
			}
		}
		
		/// <summary>Blocks until all files in syncing are sync'd </summary>
		private bool WaitForAllSynced(System.Collections.Generic.ICollection<System.String> syncing)
		{
			lock (synced)
			{
				System.Collections.Generic.IEnumerator<System.String> it = syncing.GetEnumerator();
				while (it.MoveNext())
				{
					System.String fileName = (System.String) it.Current;
					while (!synced.ContainsKey(fileName))
					{
						if (!syncing.Contains(fileName))
						// There was an error because a file that was
						// previously syncing failed to appear in synced
							return false;
						else
							try
							{
								System.Threading.Monitor.Wait(synced);
							}
							catch (System.Threading.ThreadInterruptedException ie)
							{
								// In 3.0 we will change this to throw
								// InterruptedException instead
								SupportClass.ThreadClass.Current().Interrupt();
								throw new System.SystemException(ie.Message, ie);
							}
					}
				}
				return true;
			}
		}
		
		/// <summary>Pauses before syncing.  On Windows, at least, it's
		/// best (performance-wise) to pause in order to let OS
		/// flush writes to disk on its own, before forcing a
		/// sync.
		/// </summary>
		/// <deprecated> -- this will be removed in 3.0 when
		/// autoCommit is hardwired to false 
		/// </deprecated>
        [Obsolete("-- this will be removed in 3.0 when autoCommit is hardwired to false ")]
		private void  SyncPause(long sizeInBytes)
		{
			if (mergeScheduler is ConcurrentMergeScheduler && maxSyncPauseSeconds > 0)
			{
				// Rough heuristic: for every 10 MB, we pause for 1
				// second, up until the max
				long pauseTime = (long) (1000 * sizeInBytes / 10 / 1024 / 1024);
				long maxPauseTime = (long) (maxSyncPauseSeconds * 1000);
				if (pauseTime > maxPauseTime)
					pauseTime = maxPauseTime;
				int sleepCount = (int) (pauseTime / 100);
				for (int i = 0; i < sleepCount; i++)
				{
					lock (this)
					{
						if (stopMerges || closing)
							break;
					}
					try
					{
						System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * 100));
					}
					catch (System.Threading.ThreadInterruptedException ie)
					{
						// In 3.0 we will change this to throw
						// InterruptedException instead
						SupportClass.ThreadClass.Current().Interrupt();
						throw new System.SystemException(ie.Message, ie);
					}
				}
			}
		}
		
		private void  DoWait()
		{
			lock (this)
			{
				// NOTE: the callers of this method should in theory
				// be able to do simply wait(), but, as a defense
				// against thread timing hazards where notifyAll()
				// falls to be called, we wait for at most 1 second
				// and then return so caller can check if wait
				// conditions are satisified:
				try
				{
					System.Threading.Monitor.Wait(this, TimeSpan.FromMilliseconds(1000));
				}
				catch (System.Threading.ThreadInterruptedException ie)
				{
					// In 3.0 we will change this to throw
					// InterruptedException instead
					SupportClass.ThreadClass.Current().Interrupt();
					throw new System.SystemException(ie.Message, ie);
				}
			}
		}
		
		/// <summary>Walk through all files referenced by the current
		/// segmentInfos and ask the Directory to sync each file,
		/// if it wasn't already.  If that succeeds, then we
		/// prepare a new segments_N file but do not fully commit
		/// it. 
		/// </summary>
        private void StartCommit(long sizeInBytes, System.Collections.Generic.IDictionary<string, string> commitUserData)
		{
			
			System.Diagnostics.Debug.Assert(TestPoint("startStartCommit"));

            // TODO: as of LUCENE-2095, we can simplify this method,
            // since only 1 thread can be in here at once
			
			if (hitOOM)
			{
				throw new System.SystemException("this writer hit an OutOfMemoryError; cannot commit");
			}
			
			try
			{
				
				if (infoStream != null)
					Message("startCommit(): start sizeInBytes=" + sizeInBytes);
				
				if (sizeInBytes > 0)
					SyncPause(sizeInBytes);
				
				SegmentInfos toSync = null;
				long myChangeCount;
				
				lock (this)
				{
					
					// sizeInBytes > 0 means this is an autoCommit at
					// the end of a merge.  If at this point stopMerges
					// is true (which means a rollback() or
					// rollbackTransaction() is waiting for us to
					// finish), we skip the commit to avoid deadlock
					if (sizeInBytes > 0 && stopMerges)
						return ;
					
					// Wait for any running addIndexes to complete
					// first, then block any from running until we've
					// copied the segmentInfos we intend to sync:
					BlockAddIndexes(false);
					
					// On commit the segmentInfos must never
					// reference a segment in another directory:
					System.Diagnostics.Debug.Assert(!HasExternalSegments());
					
					try
					{
						
						System.Diagnostics.Debug.Assert(lastCommitChangeCount <= changeCount);
                        myChangeCount = changeCount;
						
						if (changeCount == lastCommitChangeCount)
						{
							if (infoStream != null)
								Message("  skip startCommit(): no changes pending");
							return ;
						}
						
						// First, we clone & incref the segmentInfos we intend
						// to sync, then, without locking, we sync() each file
						// referenced by toSync, in the background.  Multiple
						// threads can be doing this at once, if say a large
						// merge and a small merge finish at the same time:
						
						if (infoStream != null)
							Message("startCommit index=" + SegString(segmentInfos) + " changeCount=" + changeCount);

                        readerPool.Commit();
						
						// It's possible another flush (that did not close
                        // the open do stores) snuck in after the flush we
                        // just did, so we remove any tail segments
                        // referencing the open doc store from the
                        // SegmentInfos we are about to sync (the main
                        // SegmentInfos will keep them):
                        toSync = (SegmentInfos) segmentInfos.Clone();
                        string dss = docWriter.GetDocStoreSegment();
                        if (dss != null)
                        {
                            while (true)
                            {
                                String dss2 = toSync.Info(toSync.Count - 1).GetDocStoreSegment();
                                if (dss2 == null || !dss2.Equals(dss))
                                {
                                    break;
                                }
                                toSync.RemoveAt(toSync.Count - 1);
                                changeCount++;
                            }
                        }
						
						if (commitUserData != null)
							toSync.SetUserData(commitUserData);
						
						deleter.IncRef(toSync, false);
												
						System.Collections.Generic.IEnumerator<string> it = toSync.Files(directory, false).GetEnumerator();
						while (it.MoveNext())
						{
							System.String fileName = it.Current;
							System.Diagnostics.Debug.Assert(directory.FileExists(fileName), "file " + fileName + " does not exist");
                            // If this trips it means we are missing a call to
                            // .checkpoint somewhere, because by the time we
                            // are called, deleter should know about every
                            // file referenced by the current head
                            // segmentInfos:
                            System.Diagnostics.Debug.Assert(deleter.Exists(fileName));
						}
					}
					finally
					{
						ResumeAddIndexes();
					}
				}
				
				System.Diagnostics.Debug.Assert(TestPoint("midStartCommit"));
				
				bool setPending = false;
				
				try
				{
					
					// Loop until all files toSync references are sync'd:
					while (true)
					{
						
						System.Collections.Generic.ICollection<System.String> pending = new System.Collections.Generic.List<System.String>();
						
						System.Collections.Generic.IEnumerator<string> it = toSync.Files(directory, false).GetEnumerator();
						while (it.MoveNext())
						{
							System.String fileName = it.Current;
							if (StartSync(fileName, pending))
							{
								bool success = false;
								try
								{
									// Because we incRef'd this commit point, above,
									// the file had better exist:
									System.Diagnostics.Debug.Assert(directory.FileExists(fileName), "file '" + fileName + "' does not exist dir=" + directory);
									if (infoStream != null)
										Message("now sync " + fileName);
									directory.Sync(fileName);
									success = true;
								}
								finally
								{
									FinishSync(fileName, success);
								}
							}
						}
						
						// All files that I require are either synced or being
						// synced by other threads.  If they are being synced,
						// we must at this point block until they are done.
						// If this returns false, that means an error in
						// another thread resulted in failing to actually
						// sync one of our files, so we repeat:
						if (WaitForAllSynced(pending))
							break;
					}
					
					System.Diagnostics.Debug.Assert(TestPoint("midStartCommit2"));
					
					lock (this)
					{
						// If someone saved a newer version of segments file
						// since I first started syncing my version, I can
						// safely skip saving myself since I've been
						// superseded:
						
						while (true)
						{
							if (myChangeCount <= lastCommitChangeCount)
							{
								if (infoStream != null)
								{
									Message("sync superseded by newer infos");
								}
								break;
							}
							else if (pendingCommit == null)
							{
								// My turn to commit
								
								if (segmentInfos.GetGeneration() > toSync.GetGeneration())
									toSync.UpdateGeneration(segmentInfos);
								
								bool success = false;
								try
								{
									
									// Exception here means nothing is prepared
									// (this method unwinds everything it did on
									// an exception)
									try
									{
										toSync.PrepareCommit(directory);
									}
									finally
									{
										// Have our master segmentInfos record the
										// generations we just prepared.  We do this
										// on error or success so we don't
										// double-write a segments_N file.
										segmentInfos.UpdateGeneration(toSync);
									}
									
									System.Diagnostics.Debug.Assert(pendingCommit == null);
									setPending = true;
									pendingCommit = toSync;
									pendingCommitChangeCount = (uint) myChangeCount;
									success = true;
								}
								finally
								{
									if (!success && infoStream != null)
										Message("hit exception committing segments file");
								}
								break;
							}
							else
							{
								// Must wait for other commit to complete
								DoWait();
							}
						}
					}
					
					if (infoStream != null)
						Message("done all syncs");
					
					System.Diagnostics.Debug.Assert(TestPoint("midStartCommitSuccess"));
				}
				finally
				{
					lock (this)
					{
						if (!setPending)
							deleter.DecRef(toSync);
					}
				}
			}
			catch (System.OutOfMemoryException oom)
			{
				HandleOOM(oom, "startCommit");
			}
			System.Diagnostics.Debug.Assert(TestPoint("finishStartCommit"));
		}
		
		/// <summary> Returns <code>true</code> iff the index in the named directory is
		/// currently locked.
		/// </summary>
		/// <param name="directory">the directory to check for a lock
		/// </param>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static bool IsLocked(Directory directory)
		{
			return directory.MakeLock(WRITE_LOCK_NAME).IsLocked();
		}
		
		/// <summary> Returns <code>true</code> iff the index in the named directory is
		/// currently locked.
		/// </summary>
		/// <param name="directory">the directory to check for a lock
		/// </param>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #IsLocked(Directory)}
		/// </deprecated>
        [Obsolete("Use IsLocked(Directory)")]
		public static bool IsLocked(System.String directory)
		{
			Directory dir = FSDirectory.GetDirectory(directory);
			try
			{
				return IsLocked(dir);
			}
			finally
			{
				dir.Close();
			}
		}
		
		/// <summary> Forcibly unlocks the index in the named directory.
		/// <p/>
		/// Caution: this should only be used by failure recovery code,
		/// when it is known that no other process nor thread is in fact
		/// currently accessing this index.
		/// </summary>
		public static void  Unlock(Directory directory)
		{
			directory.MakeLock(IndexWriter.WRITE_LOCK_NAME).Release();
		}
		
		/// <summary> Specifies maximum field length (in number of tokens/terms) in {@link IndexWriter} constructors.
		/// {@link #SetMaxFieldLength(int)} overrides the value set by
		/// the constructor.
		/// </summary>
		public sealed class MaxFieldLength
		{
			
			private int limit;
			private System.String name;
			
			/// <summary> Private type-safe-enum-pattern constructor.
			/// 
			/// </summary>
			/// <param name="name">instance name
			/// </param>
			/// <param name="limit">maximum field length
			/// </param>
			internal MaxFieldLength(System.String name, int limit)
			{
				this.name = name;
				this.limit = limit;
			}
			
			/// <summary> Public constructor to allow users to specify the maximum field size limit.
			/// 
			/// </summary>
			/// <param name="limit">The maximum field length
			/// </param>
			public MaxFieldLength(int limit):this("User-specified", limit)
			{
			}
			
			public int GetLimit()
			{
				return limit;
			}
			
			public override System.String ToString()
			{
				return name + ":" + limit;
			}
			
			/// <summary>Sets the maximum field length to {@link Integer#MAX_VALUE}. </summary>
			public static readonly MaxFieldLength UNLIMITED = new MaxFieldLength("UNLIMITED", System.Int32.MaxValue);
			
			/// <summary>  Sets the maximum field length to 
			/// {@link #DEFAULT_MAX_FIELD_LENGTH} 
			/// 
			/// </summary>
			public static readonly MaxFieldLength LIMITED;
			static MaxFieldLength()
			{
				LIMITED = new MaxFieldLength("LIMITED", Mono.Lucene.Net.Index.IndexWriter.DEFAULT_MAX_FIELD_LENGTH);
			}
		}
		
		/// <summary>If {@link #getReader} has been called (ie, this writer
		/// is in near real-time mode), then after a merge
		/// completes, this class can be invoked to warm the
		/// reader on the newly merged segment, before the merge
		/// commits.  This is not required for near real-time
		/// search, but will reduce search latency on opening a
		/// new near real-time reader after a merge completes.
		/// 
		/// <p/><b>NOTE:</b> This API is experimental and might
		/// change in incompatible ways in the next release.<p/>
		/// 
		/// <p/><b>NOTE</b>: warm is called before any deletes have
		/// been carried over to the merged segment. 
		/// </summary>
		public abstract class IndexReaderWarmer
		{
			public abstract void  Warm(IndexReader reader);
		}
		
		private IndexReaderWarmer mergedSegmentWarmer;
		
		/// <summary>Set the merged segment warmer.  See {@link
		/// IndexReaderWarmer}. 
		/// </summary>
		public virtual void  SetMergedSegmentWarmer(IndexReaderWarmer warmer)
		{
			mergedSegmentWarmer = warmer;
		}
		
		/// <summary>Returns the current merged segment warmer.  See {@link
		/// IndexReaderWarmer}. 
		/// </summary>
		public virtual IndexReaderWarmer GetMergedSegmentWarmer()
		{
			return mergedSegmentWarmer;
		}
		
		private void  HandleOOM(System.OutOfMemoryException oom, System.String location)
		{
			if (infoStream != null)
			{
				Message("hit OutOfMemoryError inside " + location);
			}
			hitOOM = true;
			throw oom;
		}
		
		// deprecated
        [Obsolete]
		private bool allowMinus1Position;
		
		/// <summary>Deprecated: emulates IndexWriter's buggy behavior when
		/// first token(s) have positionIncrement==0 (ie, prior to
		/// fixing LUCENE-1542) 
		/// </summary>
		public virtual void  SetAllowMinus1Position()
		{
			allowMinus1Position = true;
			docWriter.SetAllowMinus1Position();
		}
		
		// deprecated
        [Obsolete]
		internal virtual bool GetAllowMinus1Position()
		{
			return allowMinus1Position;
		}
		
		// Used only by assert for testing.  Current points:
		//   startDoFlush
		//   startCommitMerge
		//   startStartCommit
		//   midStartCommit
		//   midStartCommit2
		//   midStartCommitSuccess
		//   finishStartCommit
		//   startCommitMergeDeletes
		//   startMergeInit
		//   startApplyDeletes
		//   DocumentsWriter.ThreadState.init start
		public /*internal*/ virtual bool TestPoint(System.String name)
		{
			return true;
		}
		
		internal virtual bool NrtIsCurrent(SegmentInfos infos)
		{
			lock (this)
			{
				if (!infos.Equals(segmentInfos))
				{
					// if any structural changes (new segments), we are
					// stale
					return false;
                }
                else if (infos.GetGeneration() != segmentInfos.GetGeneration())
                {
                    // if any commit took place since we were opened, we
                    // are stale
                    return false;
                }
                else
                {
                    return !docWriter.AnyChanges();
                }
			}
		}
		
		internal virtual bool IsClosed()
		{
			lock (this)
			{
				return closed;
			}
		}
		static IndexWriter()
		{
			DEFAULT_MERGE_FACTOR = LogMergePolicy.DEFAULT_MERGE_FACTOR;
			DEFAULT_MAX_MERGE_DOCS = LogDocMergePolicy.DEFAULT_MAX_MERGE_DOCS;
			MAX_TERM_LENGTH = DocumentsWriter.MAX_TERM_LENGTH;
			{
				if (Constants.WINDOWS)
					DEFAULT_MAX_SYNC_PAUSE_SECONDS = 10.0;
				else
					DEFAULT_MAX_SYNC_PAUSE_SECONDS = 0.0;
			}
		}
	}
}
