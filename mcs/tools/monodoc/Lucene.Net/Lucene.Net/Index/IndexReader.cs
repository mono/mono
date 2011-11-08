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

using Document = Mono.Lucene.Net.Documents.Document;
using FieldSelector = Mono.Lucene.Net.Documents.FieldSelector;
using Mono.Lucene.Net.Store;
using Similarity = Mono.Lucene.Net.Search.Similarity;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>IndexReader is an abstract class, providing an interface for accessing an
	/// index.  Search of an index is done entirely through this abstract interface,
	/// so that any subclass which implements it is searchable.
	/// <p/> Concrete subclasses of IndexReader are usually constructed with a call to
	/// one of the static <code>open()</code> methods, e.g. {@link
	/// #Open(String, boolean)}.
	/// <p/> For efficiency, in this API documents are often referred to via
	/// <i>document numbers</i>, non-negative integers which each name a unique
	/// document in the index.  These document numbers are ephemeral--they may change
	/// as documents are added to and deleted from an index.  Clients should thus not
	/// rely on a given document having the same number between sessions.
	/// <p/> An IndexReader can be opened on a directory for which an IndexWriter is
	/// opened already, but it cannot be used to delete documents from the index then.
	/// <p/>
	/// <b>NOTE</b>: for backwards API compatibility, several methods are not listed 
	/// as abstract, but have no useful implementations in this base class and 
	/// instead always throw UnsupportedOperationException.  Subclasses are 
	/// strongly encouraged to override these methods, but in many cases may not 
	/// need to.
	/// <p/>
	/// <p/>
	/// <b>NOTE</b>: as of 2.4, it's possible to open a read-only
	/// IndexReader using one of the static open methods that
	/// accepts the boolean readOnly parameter.  Such a reader has
	/// better concurrency as it's not necessary to synchronize on
	/// the isDeleted method.  Currently the default for readOnly
	/// is false, meaning if not specified you will get a
	/// read/write IndexReader.  But in 3.0 this default will
	/// change to true, meaning you must explicitly specify false
	/// if you want to make changes with the resulting IndexReader.
	/// <p/>
	/// <a name="thread-safety"></a><p/><b>NOTE</b>: {@link
	/// <code>IndexReader</code>} instances are completely thread
	/// safe, meaning multiple threads can call any of its methods,
	/// concurrently.  If your application requires external
	/// synchronization, you should <b>not</b> synchronize on the
	/// <code>IndexReader</code> instance; use your own
	/// (non-Lucene) objects instead.
	/// </summary>
	/// <version>  $Id: IndexReader.java 826049 2009-10-16 19:28:55Z mikemccand $
	/// </version>
	public abstract class IndexReader : System.ICloneable, System.IDisposable
	{
		private class AnonymousClassFindSegmentsFile:SegmentInfos.FindSegmentsFile
		{
			private void  InitBlock(Mono.Lucene.Net.Store.Directory directory2)
			{
				this.directory2 = directory2;
			}
			private Mono.Lucene.Net.Store.Directory directory2;
			internal AnonymousClassFindSegmentsFile(Mono.Lucene.Net.Store.Directory directory2, Mono.Lucene.Net.Store.Directory Param1):base(Param1)
			{
				InitBlock(directory2);
			}
			public override System.Object DoBody(System.String segmentFileName)
			{
				return (long) directory2.FileModified(segmentFileName);
			}
		}
		
		/// <summary> Constants describing field properties, for example used for
		/// {@link IndexReader#GetFieldNames(FieldOption)}.
		/// </summary>
		public sealed class FieldOption
		{
			private System.String option;
			internal FieldOption()
			{
			}
			internal FieldOption(System.String option)
			{
				this.option = option;
			}
			public override System.String ToString()
			{
				return this.option;
			}
			/// <summary>All fields </summary>
			public static readonly FieldOption ALL = new FieldOption("ALL");
			/// <summary>All indexed fields </summary>
			public static readonly FieldOption INDEXED = new FieldOption("INDEXED");
			/// <summary>All fields that store payloads </summary>
			public static readonly FieldOption STORES_PAYLOADS = new FieldOption("STORES_PAYLOADS");
			/// <summary>All fields that omit tf </summary>
			public static readonly FieldOption OMIT_TERM_FREQ_AND_POSITIONS = new FieldOption("OMIT_TERM_FREQ_AND_POSITIONS");
			/// <deprecated> Renamed to {@link #OMIT_TERM_FREQ_AND_POSITIONS} 
			/// </deprecated>
            [Obsolete("Renamed to OMIT_TERM_FREQ_AND_POSITIONS")]
			public static readonly FieldOption OMIT_TF;
			/// <summary>All fields which are not indexed </summary>
			public static readonly FieldOption UNINDEXED = new FieldOption("UNINDEXED");
			/// <summary>All fields which are indexed with termvectors enabled </summary>
			public static readonly FieldOption INDEXED_WITH_TERMVECTOR = new FieldOption("INDEXED_WITH_TERMVECTOR");
			/// <summary>All fields which are indexed but don't have termvectors enabled </summary>
			public static readonly FieldOption INDEXED_NO_TERMVECTOR = new FieldOption("INDEXED_NO_TERMVECTOR");
			/// <summary>All fields with termvectors enabled. Please note that only standard termvector fields are returned </summary>
			public static readonly FieldOption TERMVECTOR = new FieldOption("TERMVECTOR");
			/// <summary>All fields with termvectors with position values enabled </summary>
			public static readonly FieldOption TERMVECTOR_WITH_POSITION = new FieldOption("TERMVECTOR_WITH_POSITION");
			/// <summary>All fields with termvectors with offset values enabled </summary>
			public static readonly FieldOption TERMVECTOR_WITH_OFFSET = new FieldOption("TERMVECTOR_WITH_OFFSET");
			/// <summary>All fields with termvectors with offset values and position values enabled </summary>
			public static readonly FieldOption TERMVECTOR_WITH_POSITION_OFFSET = new FieldOption("TERMVECTOR_WITH_POSITION_OFFSET");
			static FieldOption()
			{
				OMIT_TF = OMIT_TERM_FREQ_AND_POSITIONS;
			}
		}
		
		private bool closed;
		protected internal bool hasChanges;
		
		private int refCount;
		
		internal static int DEFAULT_TERMS_INDEX_DIVISOR = 1;
		
		private bool disableFakeNorms = false;
		
		/// <summary>Expert: returns the current refCount for this reader </summary>
		public virtual int GetRefCount()
		{
			lock (this)
			{
				return refCount;
			}
		}
		
		/// <summary> Expert: increments the refCount of this IndexReader
		/// instance.  RefCounts are used to determine when a
		/// reader can be closed safely, i.e. as soon as there are
		/// no more references.  Be sure to always call a
		/// corresponding {@link #decRef}, in a finally clause;
		/// otherwise the reader may never be closed.  Note that
		/// {@link #close} simply calls decRef(), which means that
		/// the IndexReader will not really be closed until {@link
		/// #decRef} has been called for all outstanding
		/// references.
		/// 
		/// </summary>
		/// <seealso cref="decRef">
		/// </seealso>
		public virtual void  IncRef()
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(refCount > 0);
				EnsureOpen();
				refCount++;
			}
		}
		
		/// <summary> Expert: decreases the refCount of this IndexReader
		/// instance.  If the refCount drops to 0, then pending
		/// changes (if any) are committed to the index and this
		/// reader is closed.
		/// 
		/// </summary>
		/// <throws>  IOException in case an IOException occurs in commit() or doClose() </throws>
		/// <summary> 
		/// </summary>
		/// <seealso cref="incRef">
		/// </seealso>
		public virtual void  DecRef()
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(refCount > 0);
				EnsureOpen();
				if (refCount == 1)
				{
					Commit();
					DoClose();
				}
				refCount--;
			}
		}
		
		/// <deprecated> will be deleted when IndexReader(Directory) is deleted
		/// </deprecated>
		/// <seealso cref="Directory()">
		/// </seealso>
        [Obsolete("will be deleted when IndexReader(Directory) is deleted")]
		private Directory directory;
		
		/// <summary> Legacy Constructor for backwards compatibility.
		/// 
		/// <p/>
		/// This Constructor should not be used, it exists for backwards 
		/// compatibility only to support legacy subclasses that did not "own" 
		/// a specific directory, but needed to specify something to be returned 
		/// by the directory() method.  Future subclasses should delegate to the 
		/// no arg constructor and implement the directory() method as appropriate.
		/// 
		/// </summary>
		/// <param name="directory">Directory to be returned by the directory() method
		/// </param>
		/// <seealso cref="Directory()">
		/// </seealso>
		/// <deprecated> - use IndexReader()
		/// </deprecated>
        [Obsolete("- use IndexReader()")]
		protected internal IndexReader(Directory directory):this()
		{
			this.directory = directory;
		}
		
		protected internal IndexReader()
		{
			refCount = 1;
		}
		
		/// <throws>  AlreadyClosedException if this IndexReader is closed </throws>
		protected internal void  EnsureOpen()
		{
			if (refCount <= 0)
			{
				throw new AlreadyClosedException("this IndexReader is closed");
			}
		}
		
		/// <summary>Returns a read/write IndexReader reading the index in an FSDirectory in the named
		/// path.
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #Open(Directory, boolean)} instead. 
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
		/// <param name="path">the path to the index directory 
		/// </param>
        [Obsolete("Use Open(Directory, boolean) instead. This method will be removed in the 3.0 release.")]
		public static IndexReader Open(System.String path)
		{
			return Open(path, false);
		}
		
		/// <summary>Returns an IndexReader reading the index in an
		/// FSDirectory in the named path.  You should pass
		/// readOnly=true, since it gives much better concurrent
		/// performance, unless you intend to do write operations
		/// (delete documents or change norms) with the reader.
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="readOnly">true if this should be a readOnly
		/// reader
		/// </param>
		/// <deprecated> Use {@link #Open(Directory, boolean)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Use Open(Directory, bool) instead. This method will be removed in the 3.0 release.")]
		public static IndexReader Open(System.String path, bool readOnly)
		{
			Directory dir = FSDirectory.GetDirectory(path);
			IndexReader r = null;
			try
			{
				r = Open(dir, null, null, readOnly, DEFAULT_TERMS_INDEX_DIVISOR);
			}
			finally
			{
				if (r == null)
					dir.Close();
			}
			return new DirectoryOwningReader(r);
		}
		
		/// <summary>Returns a read/write IndexReader reading the index in an FSDirectory in the named
		/// path.
		/// </summary>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #Open(Directory, boolean)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Use Open(Directory, bool) instead.This method will be removed in the 3.0 release.")]
		public static IndexReader Open(System.IO.FileInfo path)
		{
			return Open(path, false);
		}
		
		/// <summary>Returns an IndexReader reading the index in an
		/// FSDirectory in the named path.  You should pass
		/// readOnly=true, since it gives much better concurrent
		/// performance, unless you intend to do write operations
		/// (delete documents or change norms) with the reader.
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <param name="path">the path to the index directory
		/// </param>
		/// <param name="readOnly">true if this should be a readOnly
		/// reader
		/// </param>
		/// <deprecated> Use {@link #Open(Directory, boolean)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Use Open(Directory, bool) instead. This method will be removed in the 3.0 release.")]
		public static IndexReader Open(System.IO.FileInfo path, bool readOnly)
		{
			Directory dir = FSDirectory.GetDirectory(path);
			IndexReader r = null;
			try
			{
				r = Open(dir, null, null, readOnly, DEFAULT_TERMS_INDEX_DIVISOR);
			}
			finally
			{
				if (r == null)
					dir.Close();
			}
			return new DirectoryOwningReader(r);
		}
		
		/// <summary>Returns a read/write IndexReader reading the index in
		/// the given Directory.
		/// </summary>
		/// <param name="directory">the index directory
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #Open(Directory, boolean)} instead
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Use Open(Directory, bool) instead. This method will be removed in the 3.0 release.")]
		public static IndexReader Open(Directory directory)
		{
			return Open(directory, null, null, false, DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <summary>Returns an IndexReader reading the index in the given
		/// Directory.  You should pass readOnly=true, since it
		/// gives much better concurrent performance, unless you
		/// intend to do write operations (delete documents or
		/// change norms) with the reader.
		/// </summary>
		/// <param name="directory">the index directory
		/// </param>
		/// <param name="readOnly">true if no changes (deletions, norms) will be made with this IndexReader
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static IndexReader Open(Directory directory, bool readOnly)
		{
			return Open(directory, null, null, readOnly, DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <summary>Expert: returns a read/write IndexReader reading the index in the given
		/// {@link IndexCommit}.
		/// </summary>
		/// <param name="commit">the commit point to open
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <deprecated> Use {@link #Open(IndexCommit, boolean)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
		/// <throws>  IOException if there is a low-level IO error </throws>
        [Obsolete("Use Open(IndexCommit, bool) instead. This method will be removed in the 3.0 release.")]
		public static IndexReader Open(IndexCommit commit)
		{
			return Open(commit.GetDirectory(), null, commit, false, DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <summary>Expert: returns an IndexReader reading the index in the given
		/// {@link IndexCommit}.  You should pass readOnly=true, since it
		/// gives much better concurrent performance, unless you
		/// intend to do write operations (delete documents or
		/// change norms) with the reader.
		/// </summary>
		/// <param name="commit">the commit point to open
		/// </param>
		/// <param name="readOnly">true if no changes (deletions, norms) will be made with this IndexReader
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static IndexReader Open(IndexCommit commit, bool readOnly)
		{
			return Open(commit.GetDirectory(), null, commit, readOnly, DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <summary>Expert: returns a read/write IndexReader reading the index in the given
		/// Directory, with a custom {@link IndexDeletionPolicy}.
		/// </summary>
		/// <param name="directory">the index directory
		/// </param>
		/// <param name="deletionPolicy">a custom deletion policy (only used
		/// if you use this reader to perform deletes or to set
		/// norms); see {@link IndexWriter} for details.
		/// </param>
		/// <deprecated> Use {@link #Open(Directory, IndexDeletionPolicy, boolean)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
        [Obsolete("Use Open(Directory, IndexDeletionPolicy, bool) instead. This method will be removed in the 3.0 release.")]
		public static IndexReader Open(Directory directory, IndexDeletionPolicy deletionPolicy)
		{
			return Open(directory, deletionPolicy, null, false, DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <summary>Expert: returns an IndexReader reading the index in
		/// the given Directory, with a custom {@link
		/// IndexDeletionPolicy}.  You should pass readOnly=true,
		/// since it gives much better concurrent performance,
		/// unless you intend to do write operations (delete
		/// documents or change norms) with the reader.
		/// </summary>
		/// <param name="directory">the index directory
		/// </param>
		/// <param name="deletionPolicy">a custom deletion policy (only used
		/// if you use this reader to perform deletes or to set
		/// norms); see {@link IndexWriter} for details.
		/// </param>
		/// <param name="readOnly">true if no changes (deletions, norms) will be made with this IndexReader
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static IndexReader Open(Directory directory, IndexDeletionPolicy deletionPolicy, bool readOnly)
		{
			return Open(directory, deletionPolicy, null, readOnly, DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <summary>Expert: returns an IndexReader reading the index in
		/// the given Directory, with a custom {@link
		/// IndexDeletionPolicy}.  You should pass readOnly=true,
		/// since it gives much better concurrent performance,
		/// unless you intend to do write operations (delete
		/// documents or change norms) with the reader.
		/// </summary>
		/// <param name="directory">the index directory
		/// </param>
		/// <param name="deletionPolicy">a custom deletion policy (only used
		/// if you use this reader to perform deletes or to set
		/// norms); see {@link IndexWriter} for details.
		/// </param>
		/// <param name="readOnly">true if no changes (deletions, norms) will be made with this IndexReader
		/// </param>
		/// <param name="termInfosIndexDivisor">Subsamples which indexed
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
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static IndexReader Open(Directory directory, IndexDeletionPolicy deletionPolicy, bool readOnly, int termInfosIndexDivisor)
		{
			return Open(directory, deletionPolicy, null, readOnly, termInfosIndexDivisor);
		}
		
		/// <summary>Expert: returns a read/write IndexReader reading the index in the given
		/// Directory, using a specific commit and with a custom
		/// {@link IndexDeletionPolicy}.
		/// </summary>
		/// <param name="commit">the specific {@link IndexCommit} to open;
		/// see {@link IndexReader#listCommits} to list all commits
		/// in a directory
		/// </param>
		/// <param name="deletionPolicy">a custom deletion policy (only used
		/// if you use this reader to perform deletes or to set
		/// norms); see {@link IndexWriter} for details.
		/// </param>
		/// <deprecated> Use {@link #Open(IndexCommit, IndexDeletionPolicy, boolean)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
        [Obsolete("Use Open(IndexCommit, IndexDeletionPolicy, bool) instead. This method will be removed in the 3.0 release.")]
		public static IndexReader Open(IndexCommit commit, IndexDeletionPolicy deletionPolicy)
		{
			return Open(commit.GetDirectory(), deletionPolicy, commit, false, DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <summary>Expert: returns an IndexReader reading the index in
		/// the given Directory, using a specific commit and with
		/// a custom {@link IndexDeletionPolicy}.  You should pass
		/// readOnly=true, since it gives much better concurrent
		/// performance, unless you intend to do write operations
		/// (delete documents or change norms) with the reader.
		/// </summary>
		/// <param name="commit">the specific {@link IndexCommit} to open;
		/// see {@link IndexReader#listCommits} to list all commits
		/// in a directory
		/// </param>
		/// <param name="deletionPolicy">a custom deletion policy (only used
		/// if you use this reader to perform deletes or to set
		/// norms); see {@link IndexWriter} for details.
		/// </param>
		/// <param name="readOnly">true if no changes (deletions, norms) will be made with this IndexReader
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static IndexReader Open(IndexCommit commit, IndexDeletionPolicy deletionPolicy, bool readOnly)
		{
			return Open(commit.GetDirectory(), deletionPolicy, commit, readOnly, DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <summary>Expert: returns an IndexReader reading the index in
		/// the given Directory, using a specific commit and with
		/// a custom {@link IndexDeletionPolicy}.  You should pass
		/// readOnly=true, since it gives much better concurrent
		/// performance, unless you intend to do write operations
		/// (delete documents or change norms) with the reader.
		/// </summary>
		/// <param name="commit">the specific {@link IndexCommit} to open;
		/// see {@link IndexReader#listCommits} to list all commits
		/// in a directory
		/// </param>
		/// <param name="deletionPolicy">a custom deletion policy (only used
		/// if you use this reader to perform deletes or to set
		/// norms); see {@link IndexWriter} for details.
		/// </param>
		/// <param name="readOnly">true if no changes (deletions, norms) will be made with this IndexReader
		/// </param>
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
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static IndexReader Open(IndexCommit commit, IndexDeletionPolicy deletionPolicy, bool readOnly, int termInfosIndexDivisor)
		{
			return Open(commit.GetDirectory(), deletionPolicy, commit, readOnly, termInfosIndexDivisor);
		}
		
		private static IndexReader Open(Directory directory, IndexDeletionPolicy deletionPolicy, IndexCommit commit, bool readOnly, int termInfosIndexDivisor)
		{
			return DirectoryReader.Open(directory, deletionPolicy, commit, readOnly, termInfosIndexDivisor);
		}
		
		/// <summary> Refreshes an IndexReader if the index has changed since this instance 
		/// was (re)opened. 
		/// <p/>
		/// Opening an IndexReader is an expensive operation. This method can be used
		/// to refresh an existing IndexReader to reduce these costs. This method 
		/// tries to only load segments that have changed or were created after the 
		/// IndexReader was (re)opened.
		/// <p/>
		/// If the index has not changed since this instance was (re)opened, then this
		/// call is a NOOP and returns this instance. Otherwise, a new instance is 
		/// returned. The old instance is <b>not</b> closed and remains usable.<br/>
		/// <p/>   
		/// If the reader is reopened, even though they share
		/// resources internally, it's safe to make changes
		/// (deletions, norms) with the new reader.  All shared
		/// mutable state obeys "copy on write" semantics to ensure
		/// the changes are not seen by other readers.
		/// <p/>
		/// You can determine whether a reader was actually reopened by comparing the
		/// old instance with the instance returned by this method: 
		/// <pre>
		/// IndexReader reader = ... 
		/// ...
		/// IndexReader newReader = r.reopen();
		/// if (newReader != reader) {
		/// ...     // reader was reopened
		/// reader.close(); 
		/// }
		/// reader = newReader;
		/// ...
		/// </pre>
		/// 
		/// Be sure to synchronize that code so that other threads,
		/// if present, can never use reader after it has been
		/// closed and before it's switched to newReader.
		/// 
		/// <p/><b>NOTE</b>: If this reader is a near real-time
		/// reader (obtained from {@link IndexWriter#GetReader()},
		/// reopen() will simply call writer.getReader() again for
		/// you, though this may change in the future.
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual IndexReader Reopen()
		{
			lock (this)
			{
				throw new System.NotSupportedException("This reader does not support reopen().");
			}
		}
		
		
		/// <summary>Just like {@link #Reopen()}, except you can change the
		/// readOnly of the original reader.  If the index is
		/// unchanged but readOnly is different then a new reader
		/// will be returned. 
		/// </summary>
		public virtual IndexReader Reopen(bool openReadOnly)
		{
			lock (this)
			{
				throw new System.NotSupportedException("This reader does not support reopen().");
			}
		}
		
		/// <summary>Expert: reopen this reader on a specific commit point.
		/// This always returns a readOnly reader.  If the
		/// specified commit point matches what this reader is
		/// already on, and this reader is already readOnly, then
		/// this same instance is returned; if it is not already
		/// readOnly, a readOnly clone is returned. 
		/// </summary>
		public virtual IndexReader Reopen(IndexCommit commit)
		{
			lock (this)
			{
				throw new System.NotSupportedException("This reader does not support reopen(IndexCommit).");
			}
		}
		
		/// <summary> Efficiently clones the IndexReader (sharing most
		/// internal state).
		/// <p/>
		/// On cloning a reader with pending changes (deletions,
		/// norms), the original reader transfers its write lock to
		/// the cloned reader.  This means only the cloned reader
		/// may make further changes to the index, and commit the
		/// changes to the index on close, but the old reader still
		/// reflects all changes made up until it was cloned.
		/// <p/>
		/// Like {@link #Reopen()}, it's safe to make changes to
		/// either the original or the cloned reader: all shared
		/// mutable state obeys "copy on write" semantics to ensure
		/// the changes are not seen by other readers.
		/// <p/>
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual System.Object Clone()
		{
			throw new System.NotSupportedException("This reader does not implement clone()");
		}
		
		/// <summary> Clones the IndexReader and optionally changes readOnly.  A readOnly 
		/// reader cannot open a writeable reader.  
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual IndexReader Clone(bool openReadOnly)
		{
			lock (this)
			{
				throw new System.NotSupportedException("This reader does not implement clone()");
			}
		}
		
		/// <summary> Returns the directory associated with this index.  The Default 
		/// implementation returns the directory specified by subclasses when 
		/// delegating to the IndexReader(Directory) constructor, or throws an 
		/// UnsupportedOperationException if one was not specified.
		/// </summary>
		/// <throws>  UnsupportedOperationException if no directory </throws>
		public virtual Directory Directory()
		{
			EnsureOpen();
			if (null != directory)
			{
				return directory;
			}
			else
			{
				throw new System.NotSupportedException("This reader does not support this method.");
			}
		}
		
		/// <summary> Returns the time the index in the named directory was last modified.
		/// Do not use this to check whether the reader is still up-to-date, use
		/// {@link #IsCurrent()} instead. 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #LastModified(Directory)} instead.
		/// This method will be removed in the 3.0 release.
		/// </deprecated>
        [Obsolete("Use LastModified(Directory) instead. This method will be removed in the 3.0 release.")]
		public static long LastModified(System.String directory)
		{
			return LastModified(new System.IO.FileInfo(directory));
		}
		
		/// <summary> Returns the time the index in the named directory was last modified. 
		/// Do not use this to check whether the reader is still up-to-date, use
		/// {@link #IsCurrent()} instead. 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #LastModified(Directory)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Use LastModified(Directory) instead. This method will be removed in the 3.0 release.")]
		public static long LastModified(System.IO.FileInfo fileDirectory)
		{
			Directory dir = FSDirectory.GetDirectory(fileDirectory); // use new static method here
			try
			{
				return LastModified(dir);
			}
			finally
			{
				dir.Close();
			}
		}
		
		/// <summary> Returns the time the index in the named directory was last modified. 
		/// Do not use this to check whether the reader is still up-to-date, use
		/// {@link #IsCurrent()} instead. 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static long LastModified(Directory directory2)
		{
			return (long) ((System.Int64) new AnonymousClassFindSegmentsFile(directory2, directory2).Run());
		}
		
		/// <summary> Reads version number from segments files. The version number is
		/// initialized with a timestamp and then increased by one for each change of
		/// the index.
		/// 
		/// </summary>
		/// <param name="directory">where the index resides.
		/// </param>
		/// <returns> version number.
		/// </returns>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #GetCurrentVersion(Directory)} instead.
		/// This method will be removed in the 3.0 release.
		/// </deprecated>
        [Obsolete("Use GetCurrentVersion(Directory) instead. This method will be removed in the 3.0 release.")]
		public static long GetCurrentVersion(System.String directory)
		{
			return GetCurrentVersion(new System.IO.FileInfo(directory));
		}
		
		/// <summary> Reads version number from segments files. The version number is
		/// initialized with a timestamp and then increased by one for each change of
		/// the index.
		/// 
		/// </summary>
		/// <param name="directory">where the index resides.
		/// </param>
		/// <returns> version number.
		/// </returns>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #GetCurrentVersion(Directory)} instead.
		/// This method will be removed in the 3.0 release.
		/// </deprecated>
        [Obsolete("Use GetCurrentVersion(Directory) instead. This method will be removed in the 3.0 release.")]
		public static long GetCurrentVersion(System.IO.FileInfo directory)
		{
			Directory dir = FSDirectory.GetDirectory(directory);
			try
			{
				return GetCurrentVersion(dir);
			}
			finally
			{
				dir.Close();
			}
		}
		
		/// <summary> Reads version number from segments files. The version number is
		/// initialized with a timestamp and then increased by one for each change of
		/// the index.
		/// 
		/// </summary>
		/// <param name="directory">where the index resides.
		/// </param>
		/// <returns> version number.
		/// </returns>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static long GetCurrentVersion(Directory directory)
		{
			return SegmentInfos.ReadCurrentVersion(directory);
		}
		
		/// <summary> Reads commitUserData, previously passed to {@link
		/// IndexWriter#Commit(Map)}, from current index
		/// segments file.  This will return null if {@link
		/// IndexWriter#Commit(Map)} has never been called for
		/// this index.
		/// 
		/// </summary>
		/// <param name="directory">where the index resides.
		/// </param>
		/// <returns> commit userData.
		/// </returns>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <summary> 
		/// </summary>
		/// <seealso cref="GetCommitUserData()">
		/// </seealso>
        public static System.Collections.Generic.IDictionary<string, string> GetCommitUserData(Directory directory)
		{
			return SegmentInfos.ReadCurrentUserData(directory);
		}
		
		/// <summary> Version number when this IndexReader was opened. Not implemented in the
		/// IndexReader base class.
		/// 
		/// <p/>
		/// If this reader is based on a Directory (ie, was created by calling
		/// {@link #Open}, or {@link #Reopen} on a reader based on a Directory), then
		/// this method returns the version recorded in the commit that the reader
		/// opened. This version is advanced every time {@link IndexWriter#Commit} is
		/// called.
		/// <p/>
		/// 
		/// <p/>
		/// If instead this reader is a near real-time reader (ie, obtained by a call
		/// to {@link IndexWriter#GetReader}, or by calling {@link #Reopen} on a near
		/// real-time reader), then this method returns the version of the last
		/// commit done by the writer. Note that even as further changes are made
		/// with the writer, the version will not changed until a commit is
		/// completed. Thus, you should not rely on this method to determine when a
		/// near real-time reader should be opened. Use {@link #IsCurrent} instead.
		/// <p/>
		/// 
		/// </summary>
		/// <throws>  UnsupportedOperationException </throws>
		/// <summary>             unless overridden in subclass
		/// </summary>
		public virtual long GetVersion()
		{
			throw new System.NotSupportedException("This reader does not support this method.");
		}
		
		/// <summary> Retrieve the String userData optionally passed to
		/// IndexWriter#commit.  This will return null if {@link
		/// IndexWriter#Commit(Map)} has never been called for
		/// this index.
		/// 
		/// </summary>
		/// <seealso cref="GetCommitUserData(Directory)">
		/// </seealso>
        public virtual System.Collections.Generic.IDictionary<string, string> GetCommitUserData()
		{
			throw new System.NotSupportedException("This reader does not support this method.");
		}
		
		/// <summary><p/>For IndexReader implementations that use
		/// TermInfosReader to read terms, this sets the
		/// indexDivisor to subsample the number of indexed terms
		/// loaded into memory.  This has the same effect as {@link
		/// IndexWriter#setTermIndexInterval} except that setting
		/// must be done at indexing time while this setting can be
		/// set per reader.  When set to N, then one in every
		/// N*termIndexInterval terms in the index is loaded into
		/// memory.  By setting this to a value > 1 you can reduce
		/// memory usage, at the expense of higher latency when
		/// loading a TermInfo.  The default value is 1.<p/>
		/// 
		/// <b>NOTE:</b> you must call this before the term
		/// index is loaded.  If the index is already loaded, 
		/// an IllegalStateException is thrown.
		/// </summary>
		/// <throws>  IllegalStateException if the term index has already been loaded into memory </throws>
		/// <deprecated> Please use {@link IndexReader#Open(Directory, IndexDeletionPolicy, boolean, int)} to specify the required TermInfos index divisor instead.
		/// </deprecated>
        [Obsolete("Please use IndexReader.Open(Directory, IndexDeletionPolicy, bool, int) to specify the required TermInfos index divisor instead.")]
		public virtual void  SetTermInfosIndexDivisor(int indexDivisor)
		{
			throw new System.NotSupportedException("Please pass termInfosIndexDivisor up-front when opening IndexReader");
		}
		
		/// <summary><p/>For IndexReader implementations that use
		/// TermInfosReader to read terms, this returns the
		/// current indexDivisor as specified when the reader was
		/// opened.
		/// </summary>
		public virtual int GetTermInfosIndexDivisor()
		{
			throw new System.NotSupportedException("This reader does not support this method.");
		}
		
		/// <summary> Check whether any new changes have occurred to the index since this
		/// reader was opened.
		/// 
		/// <p/>
		/// If this reader is based on a Directory (ie, was created by calling
		/// {@link #open}, or {@link #reopen} on a reader based on a Directory), then
		/// this method checks if any further commits (see {@link IndexWriter#commit}
		/// have occurred in that directory).
		/// <p/>
		/// 
		/// <p/>
		/// If instead this reader is a near real-time reader (ie, obtained by a call
		/// to {@link IndexWriter#getReader}, or by calling {@link #reopen} on a near
		/// real-time reader), then this method checks if either a new commmit has
		/// occurred, or any new uncommitted changes have taken place via the writer.
		/// Note that even if the writer has only performed merging, this method will
		/// still return false.
		/// <p/>
		/// 
		/// <p/>
		/// In any event, if this returns false, you should call {@link #reopen} to
		/// get a new reader that sees the changes.
		/// <p/>
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <throws>  UnsupportedOperationException unless overridden in subclass </throws>
		public virtual bool IsCurrent()
		{
			throw new System.NotSupportedException("This reader does not support this method.");
		}
		
		/// <summary> Checks is the index is optimized (if it has a single segment and 
		/// no deletions).  Not implemented in the IndexReader base class.
		/// </summary>
		/// <returns> <code>true</code> if the index is optimized; <code>false</code> otherwise
		/// </returns>
		/// <throws>  UnsupportedOperationException unless overridden in subclass </throws>
		public virtual bool IsOptimized()
		{
			throw new System.NotSupportedException("This reader does not support this method.");
		}
		
		/// <summary> Return an array of term frequency vectors for the specified document.
		/// The array contains a vector for each vectorized field in the document.
		/// Each vector contains terms and frequencies for all terms in a given vectorized field.
		/// If no such fields existed, the method returns null. The term vectors that are
		/// returned may either be of type {@link TermFreqVector}
		/// or of type {@link TermPositionVector} if
		/// positions or offsets have been stored.
		/// 
		/// </summary>
		/// <param name="docNumber">document for which term frequency vectors are returned
		/// </param>
		/// <returns> array of term frequency vectors. May be null if no term vectors have been
		/// stored for the specified document.
		/// </returns>
		/// <throws>  IOException if index cannot be accessed </throws>
		/// <seealso cref="Mono.Lucene.Net.Documents.Field.TermVector">
		/// </seealso>
		abstract public TermFreqVector[] GetTermFreqVectors(int docNumber);
		
		
		/// <summary> Return a term frequency vector for the specified document and field. The
		/// returned vector contains terms and frequencies for the terms in
		/// the specified field of this document, if the field had the storeTermVector
		/// flag set. If termvectors had been stored with positions or offsets, a 
		/// {@link TermPositionVector} is returned.
		/// 
		/// </summary>
		/// <param name="docNumber">document for which the term frequency vector is returned
		/// </param>
		/// <param name="field">field for which the term frequency vector is returned.
		/// </param>
		/// <returns> term frequency vector May be null if field does not exist in the specified
		/// document or term vector was not stored.
		/// </returns>
		/// <throws>  IOException if index cannot be accessed </throws>
		/// <seealso cref="Mono.Lucene.Net.Documents.Field.TermVector">
		/// </seealso>
		abstract public TermFreqVector GetTermFreqVector(int docNumber, System.String field);
		
		/// <summary> Load the Term Vector into a user-defined data structure instead of relying on the parallel arrays of
		/// the {@link TermFreqVector}.
		/// </summary>
		/// <param name="docNumber">The number of the document to load the vector for
		/// </param>
		/// <param name="field">The name of the field to load
		/// </param>
		/// <param name="mapper">The {@link TermVectorMapper} to process the vector.  Must not be null
		/// </param>
		/// <throws>  IOException if term vectors cannot be accessed or if they do not exist on the field and doc. specified. </throws>
		/// <summary> 
		/// </summary>
		abstract public void  GetTermFreqVector(int docNumber, System.String field, TermVectorMapper mapper);
		
		/// <summary> Map all the term vectors for all fields in a Document</summary>
		/// <param name="docNumber">The number of the document to load the vector for
		/// </param>
		/// <param name="mapper">The {@link TermVectorMapper} to process the vector.  Must not be null
		/// </param>
		/// <throws>  IOException if term vectors cannot be accessed or if they do not exist on the field and doc. specified. </throws>
		abstract public void  GetTermFreqVector(int docNumber, TermVectorMapper mapper);
		
		/// <summary> Returns <code>true</code> if an index exists at the specified directory.
		/// If the directory does not exist or if there is no index in it.
		/// <code>false</code> is returned.
		/// </summary>
		/// <param name="directory">the directory to check for an index
		/// </param>
		/// <returns> <code>true</code> if an index exists; <code>false</code> otherwise
		/// </returns>
		/// <deprecated> Use {@link #IndexExists(Directory)} instead
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Use IndexExists(Directory) instead. This method will be removed in the 3.0 release.")]
		public static bool IndexExists(System.String directory)
		{
			return IndexExists(new System.IO.FileInfo(directory));
		}
		
		/// <summary> Returns <code>true</code> if an index exists at the specified directory.
		/// If the directory does not exist or if there is no index in it.
		/// </summary>
		/// <param name="directory">the directory to check for an index
		/// </param>
		/// <returns> <code>true</code> if an index exists; <code>false</code> otherwise
		/// </returns>
		/// <deprecated> Use {@link #IndexExists(Directory)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Use IndexExists(Directory) instead. This method will be removed in the 3.0 release.")]
		public static bool IndexExists(System.IO.FileInfo directory)
		{
            System.String[] list = null;
            if (System.IO.Directory.Exists(directory.FullName))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(directory.FullName);
                System.IO.FileInfo[] fi = di.GetFiles();
                if (fi.Length > 0)
                {
                    list = new System.String[fi.Length];
                    for (int i = 0; i < fi.Length; i++)
                    {
                        list[i] = fi[i].Name;
                    }
                }
            }
			return SegmentInfos.GetCurrentSegmentGeneration(list) != - 1;
		}
		
		/// <summary> Returns <code>true</code> if an index exists at the specified directory.
		/// If the directory does not exist or if there is no index in it.
		/// </summary>
		/// <param name="directory">the directory to check for an index
		/// </param>
		/// <returns> <code>true</code> if an index exists; <code>false</code> otherwise
		/// </returns>
		/// <throws>  IOException if there is a problem with accessing the index </throws>
		public static bool IndexExists(Directory directory)
		{
			return SegmentInfos.GetCurrentSegmentGeneration(directory) != - 1;
		}
		
		/// <summary>Returns the number of documents in this index. </summary>
		public abstract int NumDocs();
		
		/// <summary>Returns one greater than the largest possible document number.
		/// This may be used to, e.g., determine how big to allocate an array which
		/// will have an element for every document number in an index.
		/// </summary>
		public abstract int MaxDoc();
		
		/// <summary>Returns the number of deleted documents. </summary>
		public virtual int NumDeletedDocs()
		{
			return MaxDoc() - NumDocs();
		}
		
		/// <summary> Returns the stored fields of the <code>n</code><sup>th</sup>
		/// <code>Document</code> in this index.
		/// <p/>
		/// <b>NOTE:</b> for performance reasons, this method does not check if the
		/// requested document is deleted, and therefore asking for a deleted document
		/// may yield unspecified results. Usually this is not required, however you
		/// can call {@link #IsDeleted(int)} with the requested document ID to verify
		/// the document is not deleted.
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual Document Document(int n)
		{
			EnsureOpen();
			return Document(n, null);
		}
		
		/// <summary> Get the {@link Mono.Lucene.Net.Documents.Document} at the <code>n</code>
		/// <sup>th</sup> position. The {@link FieldSelector} may be used to determine
		/// what {@link Mono.Lucene.Net.Documents.Field}s to load and how they should
		/// be loaded. <b>NOTE:</b> If this Reader (more specifically, the underlying
		/// <code>FieldsReader</code>) is closed before the lazy
		/// {@link Mono.Lucene.Net.Documents.Field} is loaded an exception may be
		/// thrown. If you want the value of a lazy
		/// {@link Mono.Lucene.Net.Documents.Field} to be available after closing you
		/// must explicitly load it or fetch the Document again with a new loader.
		/// <p/>
		/// <b>NOTE:</b> for performance reasons, this method does not check if the
		/// requested document is deleted, and therefore asking for a deleted document
		/// may yield unspecified results. Usually this is not required, however you
		/// can call {@link #IsDeleted(int)} with the requested document ID to verify
		/// the document is not deleted.
		/// 
		/// </summary>
		/// <param name="n">Get the document at the <code>n</code><sup>th</sup> position
		/// </param>
		/// <param name="fieldSelector">The {@link FieldSelector} to use to determine what
		/// Fields should be loaded on the Document. May be null, in which case
		/// all Fields will be loaded.
		/// </param>
		/// <returns> The stored fields of the
		/// {@link Mono.Lucene.Net.Documents.Document} at the nth position
		/// </returns>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <seealso cref="Mono.Lucene.Net.Documents.Fieldable">
		/// </seealso>
		/// <seealso cref="Mono.Lucene.Net.Documents.FieldSelector">
		/// </seealso>
		/// <seealso cref="Mono.Lucene.Net.Documents.SetBasedFieldSelector">
		/// </seealso>
		/// <seealso cref="Mono.Lucene.Net.Documents.LoadFirstFieldSelector">
		/// </seealso>
		// TODO (1.5): When we convert to JDK 1.5 make this Set<String>
		public abstract Document Document(int n, FieldSelector fieldSelector);
		
		/// <summary>Returns true if document <i>n</i> has been deleted </summary>
		public abstract bool IsDeleted(int n);
		
		/// <summary>Returns true if any documents have been deleted </summary>
		public abstract bool HasDeletions();
		
		/// <summary>Returns true if there are norms stored for this field. </summary>
		public virtual bool HasNorms(System.String field)
		{
			// backward compatible implementation.
			// SegmentReader has an efficient implementation.
			EnsureOpen();
			return Norms(field) != null;
		}
		
		/// <summary>Returns the byte-encoded normalization factor for the named field of
		/// every document.  This is used by the search code to score documents.
		/// 
		/// </summary>
		/// <seealso cref="Mono.Lucene.Net.Documents.Field.SetBoost(float)">
		/// </seealso>
		public abstract byte[] Norms(System.String field);
		
		/// <summary>Reads the byte-encoded normalization factor for the named field of every
		/// document.  This is used by the search code to score documents.
		/// 
		/// </summary>
		/// <seealso cref="Mono.Lucene.Net.Documents.Field.SetBoost(float)">
		/// </seealso>
		public abstract void  Norms(System.String field, byte[] bytes, int offset);
		
		/// <summary>Expert: Resets the normalization factor for the named field of the named
		/// document.  The norm represents the product of the field's {@link
		/// Mono.Lucene.Net.Documents.Fieldable#SetBoost(float) boost} and its {@link Similarity#LengthNorm(String,
		/// int) length normalization}.  Thus, to preserve the length normalization
		/// values when resetting this, one should base the new value upon the old.
		/// 
		/// <b>NOTE:</b> If this field does not store norms, then
		/// this method call will silently do nothing.
		/// 
		/// </summary>
		/// <seealso cref="Norms(String)">
		/// </seealso>
		/// <seealso cref="Similarity.DecodeNorm(byte)">
		/// </seealso>
		/// <throws>  StaleReaderException if the index has changed </throws>
		/// <summary>  since this reader was opened
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  SetNorm(int doc, System.String field, byte value_Renamed)
		{
			lock (this)
			{
				EnsureOpen();
				AcquireWriteLock();
				hasChanges = true;
				DoSetNorm(doc, field, value_Renamed);
			}
		}
		
		/// <summary>Implements setNorm in subclass.</summary>
		protected internal abstract void  DoSetNorm(int doc, System.String field, byte value_Renamed);
		
		/// <summary>Expert: Resets the normalization factor for the named field of the named
		/// document.
		/// 
		/// </summary>
		/// <seealso cref="Norms(String)">
		/// </seealso>
		/// <seealso cref="Similarity.DecodeNorm(byte)">
		/// 
		/// </seealso>
		/// <throws>  StaleReaderException if the index has changed </throws>
		/// <summary>  since this reader was opened
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  SetNorm(int doc, System.String field, float value_Renamed)
		{
			EnsureOpen();
			SetNorm(doc, field, Similarity.EncodeNorm(value_Renamed));
		}
		
		/// <summary>Returns an enumeration of all the terms in the index. The
		/// enumeration is ordered by Term.compareTo(). Each term is greater
		/// than all that precede it in the enumeration. Note that after
		/// calling terms(), {@link TermEnum#Next()} must be called
		/// on the resulting enumeration before calling other methods such as
		/// {@link TermEnum#Term()}.
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public abstract TermEnum Terms();
		
		/// <summary>Returns an enumeration of all terms starting at a given term. If
		/// the given term does not exist, the enumeration is positioned at the
		/// first term greater than the supplied term. The enumeration is
		/// ordered by Term.compareTo(). Each term is greater than all that
		/// precede it in the enumeration.
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public abstract TermEnum Terms(Term t);
		
		/// <summary>Returns the number of documents containing the term <code>t</code>.</summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public abstract int DocFreq(Term t);
		
		/// <summary>Returns an enumeration of all the documents which contain
		/// <code>term</code>. For each document, the document number, the frequency of
		/// the term in that document is also provided, for use in
		/// search scoring.  If term is null, then all non-deleted
		/// docs are returned with freq=1.
		/// Thus, this method implements the mapping:
		/// <p/><ul>
		/// Term &#160;&#160; =&gt; &#160;&#160; &lt;docNum, freq&gt;<sup>*</sup>
		/// </ul>
		/// <p/>The enumeration is ordered by document number.  Each document number
		/// is greater than all that precede it in the enumeration.
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual TermDocs TermDocs(Term term)
		{
			EnsureOpen();
			TermDocs termDocs = TermDocs();
			termDocs.Seek(term);
			return termDocs;
		}
		
		/// <summary>Returns an unpositioned {@link TermDocs} enumerator.</summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public abstract TermDocs TermDocs();
		
		/// <summary>Returns an enumeration of all the documents which contain
		/// <code>term</code>.  For each document, in addition to the document number
		/// and frequency of the term in that document, a list of all of the ordinal
		/// positions of the term in the document is available.  Thus, this method
		/// implements the mapping:
		/// 
		/// <p/><ul>
		/// Term &#160;&#160; =&gt; &#160;&#160; &lt;docNum, freq,
		/// &lt;pos<sub>1</sub>, pos<sub>2</sub>, ...
		/// pos<sub>freq-1</sub>&gt;
		/// &gt;<sup>*</sup>
		/// </ul>
		/// <p/> This positional information facilitates phrase and proximity searching.
		/// <p/>The enumeration is ordered by document number.  Each document number is
		/// greater than all that precede it in the enumeration.
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual TermPositions TermPositions(Term term)
		{
			EnsureOpen();
			TermPositions termPositions = TermPositions();
			termPositions.Seek(term);
			return termPositions;
		}
		
		/// <summary>Returns an unpositioned {@link TermPositions} enumerator.</summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public abstract TermPositions TermPositions();
		
		
		
		/// <summary>Deletes the document numbered <code>docNum</code>.  Once a document is
		/// deleted it will not appear in TermDocs or TermPostitions enumerations.
		/// Attempts to read its field with the {@link #document}
		/// method will result in an error.  The presence of this document may still be
		/// reflected in the {@link #docFreq} statistic, though
		/// this will be corrected eventually as the index is further modified.
		/// 
		/// </summary>
		/// <throws>  StaleReaderException if the index has changed </throws>
		/// <summary> since this reader was opened
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  DeleteDocument(int docNum)
		{
			lock (this)
			{
				EnsureOpen();
				AcquireWriteLock();
				hasChanges = true;
				DoDelete(docNum);
			}
		}
		
		
		/// <summary>Implements deletion of the document numbered <code>docNum</code>.
		/// Applications should call {@link #DeleteDocument(int)} or {@link #DeleteDocuments(Term)}.
		/// </summary>
		protected internal abstract void  DoDelete(int docNum);
		
		
		/// <summary>Deletes all documents that have a given <code>term</code> indexed.
		/// This is useful if one uses a document field to hold a unique ID string for
		/// the document.  Then to delete such a document, one merely constructs a
		/// term with the appropriate field and the unique ID string as its text and
		/// passes it to this method.
		/// See {@link #DeleteDocument(int)} for information about when this deletion will 
		/// become effective.
		/// 
		/// </summary>
		/// <returns> the number of documents deleted
		/// </returns>
		/// <throws>  StaleReaderException if the index has changed </throws>
		/// <summary>  since this reader was opened
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual int DeleteDocuments(Term term)
		{
			EnsureOpen();
			TermDocs docs = TermDocs(term);
			if (docs == null)
				return 0;
			int n = 0;
			try
			{
				while (docs.Next())
				{
					DeleteDocument(docs.Doc());
					n++;
				}
			}
			finally
			{
				docs.Close();
			}
			return n;
		}
		
		/// <summary>Undeletes all documents currently marked as deleted in this index.
		/// 
		/// </summary>
		/// <throws>  StaleReaderException if the index has changed </throws>
		/// <summary>  since this reader was opened
		/// </summary>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  UndeleteAll()
		{
			lock (this)
			{
				EnsureOpen();
				AcquireWriteLock();
				hasChanges = true;
				DoUndeleteAll();
			}
		}
		
		/// <summary>Implements actual undeleteAll() in subclass. </summary>
		protected internal abstract void  DoUndeleteAll();
		
		/// <summary>Does nothing by default. Subclasses that require a write lock for
		/// index modifications must implement this method. 
		/// </summary>
		protected internal virtual void  AcquireWriteLock()
		{
			lock (this)
			{
				/* NOOP */
			}
		}
		
		/// <summary> </summary>
		/// <throws>  IOException </throws>
		public void  Flush()
		{
			lock (this)
			{
				EnsureOpen();
				Commit();
			}
		}
		
		/// <param name="commitUserData">Opaque Map (String -> String)
		/// that's recorded into the segments file in the index,
		/// and retrievable by {@link
		/// IndexReader#getCommitUserData}.
		/// </param>
		/// <throws>  IOException </throws>
        public void Flush(System.Collections.Generic.IDictionary<string, string> commitUserData)
		{
			lock (this)
			{
				EnsureOpen();
				Commit(commitUserData);
			}
		}
		
		/// <summary> Commit changes resulting from delete, undeleteAll, or
		/// setNorm operations
		/// 
		/// If an exception is hit, then either no changes or all
		/// changes will have been committed to the index
		/// (transactional semantics).
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public /*protected internal*/ void  Commit()
		{
			lock (this)
			{
				Commit(null);
			}
		}
		
		/// <summary> Commit changes resulting from delete, undeleteAll, or
		/// setNorm operations
		/// 
		/// If an exception is hit, then either no changes or all
		/// changes will have been committed to the index
		/// (transactional semantics).
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
        public void Commit(System.Collections.Generic.IDictionary<string, string> commitUserData)
		{
			lock (this)
			{
				if (hasChanges)
				{
					DoCommit(commitUserData);
				}
				hasChanges = false;
			}
		}
		
		/// <summary>Implements commit.</summary>
		/// <deprecated> Please implement {@link #DoCommit(Map)
		/// instead}. 
		/// </deprecated>
        [Obsolete("Please implement DoCommit(IDictionary<string, string>) instead")]
		protected internal abstract void  DoCommit();
		
		/// <summary>Implements commit.  NOTE: subclasses should override
		/// this.  In 3.0 this will become an abstract method. 
		/// </summary>
        protected internal virtual void DoCommit(System.Collections.Generic.IDictionary<string, string> commitUserData)
		{
			// Default impl discards commitUserData; all Lucene
			// subclasses override this (do not discard it).
			DoCommit();
		}
		
		/// <summary> Closes files associated with this index.
		/// Also saves any new deletions to disk.
		/// No other methods should be called after this has been called.
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public void  Close()
		{
			lock (this)
			{
				if (!closed)
				{
					DecRef();
					closed = true;
				}
			}
		}

        /// <summary>
        /// .NET
        /// </summary>
        public void Dispose()
        {
            Close();
        }
		
		/// <summary>Implements close. </summary>
		protected internal abstract void  DoClose();
		
		
		/// <summary> Get a list of unique field names that exist in this index and have the specified
		/// field option information.
		/// </summary>
		/// <param name="fldOption">specifies which field option should be available for the returned fields
		/// </param>
		/// <returns> Collection of Strings indicating the names of the fields.
		/// </returns>
		/// <seealso cref="IndexReader.FieldOption">
		/// </seealso>
		public abstract System.Collections.Generic.ICollection<string> GetFieldNames(FieldOption fldOption);
		
		/// <summary> Returns <code>true</code> iff the index in the named directory is
		/// currently locked.
		/// </summary>
		/// <param name="directory">the directory to check for a lock
		/// </param>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Please use {@link IndexWriter#IsLocked(Directory)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Please use IndexWriter.IsLocked(Directory) instead. This method will be removed in the 3.0 release.")]
		public static bool IsLocked(Directory directory)
		{
			return directory.MakeLock(IndexWriter.WRITE_LOCK_NAME).IsLocked();
		}
		
		/// <summary> Returns <code>true</code> iff the index in the named directory is
		/// currently locked.
		/// </summary>
		/// <param name="directory">the directory to check for a lock
		/// </param>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #IsLocked(Directory)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Use IsLocked(Directory) instead. This method will be removed in the 3.0 release.")]
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
		/// <deprecated> Please use {@link IndexWriter#Unlock(Directory)} instead.
		/// This method will be removed in the 3.0 release.
		/// 
		/// </deprecated>
        [Obsolete("Please use IndexWriter.Unlock(Directory) instead. This method will be removed in the 3.0 release.")]
		public static void  Unlock(Directory directory)
		{
			directory.MakeLock(IndexWriter.WRITE_LOCK_NAME).Release();
		}
		
		/// <summary> Expert: return the IndexCommit that this reader has
		/// opened.  This method is only implemented by those
		/// readers that correspond to a Directory with its own
		/// segments_N file.
		/// 
		/// <p/><b>WARNING</b>: this API is new and experimental and
		/// may suddenly change.<p/>
		/// </summary>
		public virtual IndexCommit GetIndexCommit()
		{
			throw new System.NotSupportedException("This reader does not support this method.");
		}
		
		/// <summary> Prints the filename and size of each file within a given compound file.
		/// Add the -extract flag to extract files to the current working directory.
		/// In order to make the extracted version of the index work, you have to copy
		/// the segments file from the compound index into the directory where the extracted files are stored.
		/// </summary>
		/// <param name="args">Usage: Mono.Lucene.Net.Index.IndexReader [-extract] &lt;cfsfile&gt;
		/// </param>
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = null;
			bool extract = false;
			
			for (int i = 0; i < args.Length; ++i)
			{
				if (args[i].Equals("-extract"))
				{
					extract = true;
				}
				else if (filename == null)
				{
					filename = args[i];
				}
			}
			
			if (filename == null)
			{
				System.Console.Out.WriteLine("Usage: Mono.Lucene.Net.Index.IndexReader [-extract] <cfsfile>");
				return ;
			}
			
			Directory dir = null;
			CompoundFileReader cfr = null;
			
			try
			{
				System.IO.FileInfo file = new System.IO.FileInfo(filename);
				System.String dirname = new System.IO.FileInfo(file.FullName).DirectoryName;
				filename = file.Name;
				dir = FSDirectory.Open(new System.IO.FileInfo(dirname));
				cfr = new CompoundFileReader(dir, filename);
				
				System.String[] files = cfr.List();
				System.Array.Sort(files); // sort the array of filename so that the output is more readable
				
				for (int i = 0; i < files.Length; ++i)
				{
					long len = cfr.FileLength(files[i]);
					
					if (extract)
					{
						System.Console.Out.WriteLine("extract " + files[i] + " with " + len + " bytes to local directory...");
						IndexInput ii = cfr.OpenInput(files[i]);
						
						System.IO.FileStream f = new System.IO.FileStream(files[i], System.IO.FileMode.Create);
						
						// read and write with a small buffer, which is more effectiv than reading byte by byte
						byte[] buffer = new byte[1024];
						int chunk = buffer.Length;
						while (len > 0)
						{
							int bufLen = (int) System.Math.Min(chunk, len);
							ii.ReadBytes(buffer, 0, bufLen);
							f.Write(buffer, 0, bufLen);
							len -= bufLen;
						}
						
						f.Close();
						ii.Close();
					}
					else
						System.Console.Out.WriteLine(files[i] + ": " + len + " bytes");
				}
			}
			catch (System.IO.IOException ioe)
			{
				System.Console.Error.WriteLine(ioe.StackTrace);
			}
			finally
			{
				try
				{
					if (dir != null)
						dir.Close();
					if (cfr != null)
						cfr.Close();
				}
				catch (System.IO.IOException ioe)
				{
					System.Console.Error.WriteLine(ioe.StackTrace);
				}
			}
		}
		
		/// <summary>Returns all commit points that exist in the Directory.
		/// Normally, because the default is {@link
		/// KeepOnlyLastCommitDeletionPolicy}, there would be only
		/// one commit point.  But if you're using a custom {@link
		/// IndexDeletionPolicy} then there could be many commits.
		/// Once you have a given commit, you can open a reader on
		/// it by calling {@link IndexReader#Open(IndexCommit)}
		/// There must be at least one commit in
		/// the Directory, else this method throws {@link
		/// java.io.IOException}.  Note that if a commit is in
		/// progress while this method is running, that commit
		/// may or may not be returned array.  
		/// </summary>
		public static System.Collections.ICollection ListCommits(Directory dir)
		{
			return DirectoryReader.ListCommits(dir);
		}
		
		/// <summary>Expert: returns the sequential sub readers that this
		/// reader is logically composed of.  For example,
		/// IndexSearcher uses this API to drive searching by one
		/// sub reader at a time.  If this reader is not composed
		/// of sequential child readers, it should return null.
		/// If this method returns an empty array, that means this
		/// reader is a null reader (for example a MultiReader
		/// that has no sub readers).
		/// <p/>
		/// NOTE: You should not try using sub-readers returned by
		/// this method to make any changes (setNorm, deleteDocument,
		/// etc.). While this might succeed for one composite reader
		/// (like MultiReader), it will most likely lead to index
		/// corruption for other readers (like DirectoryReader obtained
		/// through {@link #open}. Use the parent reader directly. 
		/// </summary>
		public virtual IndexReader[] GetSequentialSubReaders()
		{
			return null;
		}
		
		/// <summary>Expert    </summary>
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		public virtual System.Object GetFieldCacheKey()
		{
			return this;
		}

        /** Expert.  Warning: this returns null if the reader has
          *  no deletions 
          */
        public virtual object GetDeletesCacheKey()
        {
            return this;
        }
		
		/// <summary>Returns the number of unique terms (across all fields)
		/// in this reader.
		/// 
		/// This method returns long, even though internally
		/// Lucene cannot handle more than 2^31 unique terms, for
		/// a possible future when this limitation is removed.
		/// 
		/// </summary>
		/// <throws>  UnsupportedOperationException if this count </throws>
		/// <summary>  cannot be easily determined (eg Multi*Readers).
		/// Instead, you should call {@link
		/// #getSequentialSubReaders} and ask each sub reader for
		/// its unique term count. 
		/// </summary>
		public virtual long GetUniqueTermCount()
		{
			throw new System.NotSupportedException("this reader does not implement getUniqueTermCount()");
		}
		
		/// <summary>Expert: Return the state of the flag that disables fakes norms in favor of representing the absence of field norms with null.</summary>
		/// <returns> true if fake norms are disabled
		/// </returns>
		/// <deprecated> This currently defaults to false (to remain
		/// back-compatible), but in 3.0 it will be hardwired to
		/// true, meaning the norms() methods will return null for
		/// fields that had disabled norms.
		/// </deprecated>
        [Obsolete("This currently defaults to false (to remain back-compatible), but in 3.0 it will be hardwired to true, meaning the norms() methods will return null for fields that had disabled norms.")]
		public virtual bool GetDisableFakeNorms()
		{
			return disableFakeNorms;
		}
		
		/// <summary>Expert: Set the state of the flag that disables fakes norms in favor of representing the absence of field norms with null.</summary>
		/// <param name="disableFakeNorms">true to disable fake norms, false to preserve the legacy behavior
		/// </param>
		/// <deprecated> This currently defaults to false (to remain
		/// back-compatible), but in 3.0 it will be hardwired to
		/// true, meaning the norms() methods will return null for
		/// fields that had disabled norms.
		/// </deprecated>
        [Obsolete("This currently defaults to false (to remain back-compatible), but in 3.0 it will be hardwired to true, meaning the norms() methods will return null for fields that had disabled norms.")]
		public virtual void  SetDisableFakeNorms(bool disableFakeNorms)
		{
			this.disableFakeNorms = disableFakeNorms;
		}

        public bool hasChanges_ForNUnit
        {
            get { return hasChanges; }
        }
	}
}
