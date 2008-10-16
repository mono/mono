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
using Analyzer = Monodoc.Lucene.Net.Analysis.Analyzer;
using Document = Monodoc.Lucene.Net.Documents.Document;
using Similarity = Monodoc.Lucene.Net.Search.Similarity;
using Directory = Monodoc.Lucene.Net.Store.Directory;
using FSDirectory = Monodoc.Lucene.Net.Store.FSDirectory;
using InputStream = Monodoc.Lucene.Net.Store.InputStream;
using Lock = Monodoc.Lucene.Net.Store.Lock;
using OutputStream = Monodoc.Lucene.Net.Store.OutputStream;
using RAMDirectory = Monodoc.Lucene.Net.Store.RAMDirectory;
namespace Monodoc.Lucene.Net.Index
{
	
	
	/// <summary>An IndexWriter creates and maintains an index.
	/// The third argument to the <a href="#IndexWriter"><b>constructor</b></a>
	/// determines whether a new index is created, or whether an existing index is
	/// opened for the addition of new documents.
	/// In either case, documents are added with the <a
	/// href="#addDocument"><b>addDocument</b></a> method.  When finished adding
	/// documents, <a href="#close"><b>close</b></a> should be called.
	/// If an index will not have more documents added for a while and optimal search
	/// performance is desired, then the <a href="#optimize"><b>optimize</b></a>
	/// method should be called before the index is closed.
	/// </summary>
	
	public class IndexWriter
	{
		private class AnonymousClassWith : Lock.With
		{
			private void  InitBlock(bool create, IndexWriter enclosingInstance)
			{
				this.create = create;
				this.enclosingInstance = enclosingInstance;
			}
			private bool create;
			private IndexWriter enclosingInstance;
			public IndexWriter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal AnonymousClassWith(bool create, IndexWriter enclosingInstance, Monodoc.Lucene.Net.Store.Lock Param1, long Param2):base(Param1, Param2)
			{
				InitBlock(create, enclosingInstance);
			}
			public override System.Object DoBody()
			{
				if (create)
					Enclosing_Instance.segmentInfos.Write(Enclosing_Instance.directory);
				else
					Enclosing_Instance.segmentInfos.Read(Enclosing_Instance.directory);
				return null;
			}
		}
		private class AnonymousClassWith1 : Lock.With
		{
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
			internal AnonymousClassWith1(IndexWriter enclosingInstance, Monodoc.Lucene.Net.Store.Lock Param1, long Param2):base(Param1, Param2)
			{
				InitBlock(enclosingInstance);
			}
			public override System.Object DoBody()
			{
				Enclosing_Instance.segmentInfos.Write(Enclosing_Instance.directory); // commit changes
				return null;
			}
		}
		private class AnonymousClassWith2 : Lock.With
		{
			private void  InitBlock(System.Collections.ArrayList segmentsToDelete, IndexWriter enclosingInstance)
			{
				this.segmentsToDelete = segmentsToDelete;
				this.enclosingInstance = enclosingInstance;
			}
			private System.Collections.ArrayList segmentsToDelete;
			private IndexWriter enclosingInstance;
			public IndexWriter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal AnonymousClassWith2(System.Collections.ArrayList segmentsToDelete, IndexWriter enclosingInstance, Monodoc.Lucene.Net.Store.Lock Param1, long Param2):base(Param1, Param2)
			{
				InitBlock(segmentsToDelete, enclosingInstance);
			}
			public override System.Object DoBody()
			{
				Enclosing_Instance.segmentInfos.Write(Enclosing_Instance.directory); // commit before deleting
				Enclosing_Instance.DeleteSegments(segmentsToDelete); // delete now-unused segments
				return null;
			}
		}
		private void  InitBlock()
		{
			similarity = Similarity.GetDefault();
		}
		
		/// <summary> Default value is 1000.  Use <code>Monodoc.Lucene.Net.writeLockTimeout</code>
		/// system property to override.
		/// </summary>
		public static long WRITE_LOCK_TIMEOUT = SupportClass.AppSettings.Get("Monodoc.Lucene.Net.writeLockTimeout", 1000L);
		
		/// <summary> Default value is 10000.  Use <code>Monodoc.Lucene.Net.commitLockTimeout</code>
		/// system property to override.
		/// </summary>
		public static long COMMIT_LOCK_TIMEOUT = System.Int32.Parse(SupportClass.AppSettings.Get("Monodoc.Lucene.Net.commitLockTimeout", "10000"));
		
		public const System.String WRITE_LOCK_NAME = "write.lock";
		public const System.String COMMIT_LOCK_NAME = "commit.lock";
		
		/// <summary> Default value is 10.  Use <code>Monodoc.Lucene.Net.mergeFactor</code>
		/// system property to override.
		/// </summary>
		public static readonly int DEFAULT_MERGE_FACTOR = System.Int32.Parse(SupportClass.AppSettings.Get("Monodoc.Lucene.Net.mergeFactor", "10"));
		
		/// <summary> Default value is 10.  Use <code>Monodoc.Lucene.Net.minMergeDocs</code>
		/// system property to override.
		/// </summary>
		public static readonly int DEFAULT_MIN_MERGE_DOCS = System.Int32.Parse(SupportClass.AppSettings.Get("Monodoc.Lucene.Net.minMergeDocs", "10"));
		
		/// <summary> Default value is {@link Integer#MAX_VALUE}.
		/// Use <code>Monodoc.Lucene.Net.maxMergeDocs</code> system property to override.
		/// </summary>
		public static readonly int DEFAULT_MAX_MERGE_DOCS = System.Int32.Parse(SupportClass.AppSettings.Get("Monodoc.Lucene.Net.maxMergeDocs", System.Convert.ToString(System.Int32.MaxValue)));
		
		/// <summary> Default value is 10000.  Use <code>Monodoc.Lucene.Net.maxFieldLength</code>
		/// system property to override.
		/// </summary>
		public static readonly int DEFAULT_MAX_FIELD_LENGTH = System.Int32.Parse(SupportClass.AppSettings.Get("Monodoc.Lucene.Net.maxFieldLength", "10000")); //// "5000000")); // "2147483647"));
		
		
		private Directory directory; // where this index resides
		private Analyzer analyzer; // how to analyze text
		
		private Similarity similarity; // how to normalize
		
		private SegmentInfos segmentInfos = new SegmentInfos(); // the segments
		private Directory ramDirectory = new RAMDirectory(); // for temp segs
		
		private Lock writeLock;
		
		/// <summary>Use compound file setting. Defaults to true, minimizing the number of
		/// files used.  Setting this to false may improve indexing performance, but
		/// may also cause file handle problems.
		/// </summary>
		private bool useCompoundFile = true;
		
		private bool closeDir;
		
		/// <summary>Setting to turn on usage of a compound file. When on, multiple files
		/// for each segment are merged into a single file once the segment creation
		/// is finished. This is done regardless of what directory is in use.
		/// </summary>
		public virtual bool GetUseCompoundFile()
		{
			return useCompoundFile;
		}
		
		/// <summary>Setting to turn on usage of a compound file. When on, multiple files
		/// for each segment are merged into a single file once the segment creation
		/// is finished. This is done regardless of what directory is in use.
		/// </summary>
		public virtual void  SetUseCompoundFile(bool value_Renamed)
		{
			useCompoundFile = value_Renamed;
		}
		
		
		/// <summary>Expert: Set the Similarity implementation used by this IndexWriter.
		/// 
		/// </summary>
		/// <seealso cref="Similarity#SetDefault(Similarity)">
		/// </seealso>
		public virtual void  SetSimilarity(Similarity similarity)
		{
			this.similarity = similarity;
		}
		
		/// <summary>Expert: Return the Similarity implementation used by this IndexWriter.
		/// 
		/// <p>This defaults to the current value of {@link Similarity#GetDefault()}.
		/// </summary>
		public virtual Similarity GetSimilarity()
		{
			return this.similarity;
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
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist, and <code>create</code> is
		/// <code>false</code>
		/// </summary>
		public IndexWriter(System.String path, Analyzer a, bool create) :this(FSDirectory.GetDirectory(path, create), a, create, true)
		{
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
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist, and <code>create</code> is
		/// <code>false</code>
		/// </summary>
		public IndexWriter(System.IO.FileInfo path, Analyzer a, bool create):this(FSDirectory.GetDirectory(path, create), a, create, true)
		{
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
		/// <throws>  IOException if the directory cannot be read/written to, or </throws>
		/// <summary>  if it does not exist, and <code>create</code> is
		/// <code>false</code>
		/// </summary>
		public IndexWriter(Directory d, Analyzer a, bool create):this(d, a, create, false)
		{
		}
		
		private IndexWriter(Directory d, Analyzer a, bool create, bool closeDir)
		{
			InitBlock();
			this.closeDir = closeDir;
			directory = d;
			analyzer = a;
			
			Lock writeLock = directory.MakeLock(IndexWriter.WRITE_LOCK_NAME);
			if (!writeLock.Obtain(WRITE_LOCK_TIMEOUT))
			// obtain write lock
			{
				throw new System.IO.IOException("Index locked for write: " + writeLock);
			}
			this.writeLock = writeLock; // save it
			
			lock (directory)
			{
				// in- & inter-process sync
				new AnonymousClassWith(create, this, directory.MakeLock(IndexWriter.COMMIT_LOCK_NAME), COMMIT_LOCK_TIMEOUT).Run();
			}
		}
		
		/// <summary>Flushes all changes to an index and closes all associated files. </summary>
		public virtual void  Close()
		{
			lock (this)
			{
				FlushRamSegments();
				ramDirectory.Close();
				writeLock.Release(); // release write lock
				writeLock = null;
				if (closeDir)
					directory.Close();
				System.GC.SuppressFinalize(this);
			}
		}
		
		/// <summary>Release the write lock, if needed. </summary>
		~IndexWriter()
		{
			if (writeLock != null)
			{
				writeLock.Release(); // release write lock
				writeLock = null;
			}
		}
		
		/// <summary>Returns the analyzer used by this index. </summary>
		public virtual Analyzer GetAnalyzer()
		{
			return analyzer;
		}
		
		
		/// <summary>Returns the number of documents currently in this index. </summary>
		public virtual int DocCount()
		{
			lock (this)
			{
				int count = 0;
				for (int i = 0; i < segmentInfos.Count; i++)
				{
					SegmentInfo si = segmentInfos.Info(i);
					count += si.docCount;
				}
				return count;
			}
		}
		
		/// <summary> The maximum number of terms that will be indexed for a single Field in a
		/// document.  This limits the amount of memory required for indexing, so that
		/// collections with very large files will not crash the indexing process by
		/// running out of memory.<p/>
		/// Note that this effectively truncates large documents, excluding from the
		/// index terms that occur further in the document.  If you know your source
		/// documents are large, be sure to set this value high enough to accomodate
		/// the expected size.  If you set it to Integer.MAX_VALUE, then the only limit
		/// is your memory, but you should anticipate an OutOfMemoryError.<p/>
		/// By default, no more than 10,000 terms will be indexed for a Field.
		/// </summary>
		public int maxFieldLength = DEFAULT_MAX_FIELD_LENGTH;
		
		/// <summary> Adds a document to this index.  If the document contains more than
		/// {@link #maxFieldLength} terms for a given Field, the remainder are
		/// discarded.
		/// </summary>
		public virtual void  AddDocument(Document doc)
		{
			AddDocument(doc, analyzer);
		}
		
		/// <summary> Adds a document to this index, using the provided analyzer instead of the
		/// value of {@link #GetAnalyzer()}.  If the document contains more than
		/// {@link #maxFieldLength} terms for a given Field, the remainder are
		/// discarded.
		/// </summary>
		public virtual void  AddDocument(Document doc, Analyzer analyzer)
		{
			DocumentWriter dw = new DocumentWriter(ramDirectory, analyzer, similarity, maxFieldLength);
			System.String segmentName = NewSegmentName();
			dw.AddDocument(segmentName, doc);
			lock (this)
			{
				segmentInfos.Add(new SegmentInfo(segmentName, 1, ramDirectory));
				MaybeMergeSegments();
			}
		}
		
		internal int GetSegmentsCounter()
		{
			return segmentInfos.counter;
		}
		
		private System.String NewSegmentName()
		{
			lock (this)
			{
				return "_" + SupportClass.Number.ToString(segmentInfos.counter++, SupportClass.Number.MAX_RADIX);
			}
		}
		
		/// <summary>Determines how often segment indices are merged by addDocument().  With
		/// smaller values, less RAM is used while indexing, and searches on
		/// unoptimized indices are faster, but indexing speed is slower.  With larger
		/// values, more RAM is used during indexing, and while searches on unoptimized
		/// indices are slower, indexing is faster.  Thus larger values (> 10) are best
		/// for batch index creation, and smaller values (< 10) for indices that are
		/// interactively maintained.
		/// 
		/// <p>This must never be less than 2.  The default value is 10.
		/// </summary>
		public int mergeFactor = DEFAULT_MERGE_FACTOR;
		
		/// <summary>Determines the minimal number of documents required before the buffered
		/// in-memory documents are merging and a new Segment is created.
		/// Since Documents are merged in a {@link Monodoc.Lucene.Net.Store.RAMDirectory},
		/// large value gives faster indexing.  At the same time, mergeFactor limits
		/// the number of files open in a FSDirectory.
		/// 
		/// <p> The default value is 10.
		/// </summary>
		public int minMergeDocs = DEFAULT_MIN_MERGE_DOCS;
		
		
		/// <summary>Determines the largest number of documents ever merged by addDocument().
		/// Small values (e.g., less than 10,000) are best for interactive indexing,
		/// as this limits the length of pauses while indexing to a few seconds.
		/// Larger values are best for batched indexing and speedier searches.
		/// 
		/// <p>The default value is {@link Integer#MAX_VALUE}. 
		/// </summary>
		public int maxMergeDocs = DEFAULT_MAX_MERGE_DOCS;
		
		/// <summary>If non-null, information about merges will be printed to this. </summary>
		public System.IO.TextWriter infoStream = null;
		
		/// <summary>Merges all segments together into a single segment, optimizing an index
		/// for search. 
		/// </summary>
		public virtual void  Optimize()
		{
			lock (this)
			{
				FlushRamSegments();
				while (segmentInfos.Count > 1 || (segmentInfos.Count == 1 && (SegmentReader.HasDeletions(segmentInfos.Info(0)) || segmentInfos.Info(0).dir != directory || (useCompoundFile && (!SegmentReader.UsesCompoundFile(segmentInfos.Info(0)) || SegmentReader.HasSeparateNorms(segmentInfos.Info(0)))))))
				{
					int minSegment = segmentInfos.Count - mergeFactor;
					MergeSegments(minSegment < 0?0:minSegment);
				}
			}
		}
		
		/// <summary>Merges all segments from an array of indexes into this index.
		/// 
		/// <p>This may be used to parallelize batch indexing.  A large document
		/// collection can be broken into sub-collections.  Each sub-collection can be
		/// indexed in parallel, on a different thread, process or machine.  The
		/// complete index can then be created by merging sub-collection indexes
		/// with this method.
		/// 
		/// <p>After this completes, the index is optimized. 
		/// </summary>
		public virtual void  AddIndexes(Directory[] dirs)
		{
			lock (this)
			{
				Optimize(); // start with zero or 1 seg
				for (int i = 0; i < dirs.Length; i++)
				{
					SegmentInfos sis = new SegmentInfos(); // read infos from dir
					sis.Read(dirs[i]);
					for (int j = 0; j < sis.Count; j++)
					{
						segmentInfos.Add(sis.Info(j)); // add each info
					}
				}
				Optimize(); // final cleanup
			}
		}
		
		/// <summary>Merges the provided indexes into this index.
		/// <p>After this completes, the index is optimized. </p>
		/// <p>The provided Monodoc.Lucene.Net.Index.IndexReaders are not closed.</p>
		/// </summary>
		public virtual void  AddIndexes(Monodoc.Lucene.Net.Index.IndexReader[] readers)
		{
			lock (this)
			{
				
				Optimize(); // start with zero or 1 seg
				
				System.String mergedName = NewSegmentName();
				SegmentMerger merger = new SegmentMerger(directory, mergedName, false);
				
				if (segmentInfos.Count == 1)
				// add existing index, if any
					merger.Add(new SegmentReader(segmentInfos.Info(0)));
				
				for (int i = 0; i < readers.Length; i++)
				// add new indexes
					merger.Add(readers[i]);
				
				int docCount = merger.Merge(); // merge 'em
				
				segmentInfos.Clear(); // pop old infos & add new
				segmentInfos.Add(new SegmentInfo(mergedName, docCount, directory));
				
				lock (directory)
				{
					// in- & inter-process sync
					new AnonymousClassWith1(this, directory.MakeLock("commit.lock"), COMMIT_LOCK_TIMEOUT).Run();
				}
			}
		}
		
		/// <summary>Merges all RAM-resident segments. </summary>
		private void  FlushRamSegments()
		{
			int minSegment = segmentInfos.Count - 1;
			int docCount = 0;
			while (minSegment >= 0 && (segmentInfos.Info(minSegment)).dir == ramDirectory)
			{
				docCount += segmentInfos.Info(minSegment).docCount;
				minSegment--;
			}
			if (minSegment < 0 || (docCount + segmentInfos.Info(minSegment).docCount) > mergeFactor || !(segmentInfos.Info(segmentInfos.Count - 1).dir == ramDirectory))
				minSegment++;
			if (minSegment >= segmentInfos.Count)
				return ; // none to merge
			MergeSegments(minSegment);
		}
		
		/// <summary>Incremental segment merger.  </summary>
		private void  MaybeMergeSegments()
		{
			long targetMergeDocs = minMergeDocs;
			while (targetMergeDocs <= maxMergeDocs)
			{
				// find segments smaller than current target size
				int minSegment = segmentInfos.Count;
				int mergeDocs = 0;
				while (--minSegment >= 0)
				{
					SegmentInfo si = segmentInfos.Info(minSegment);
					if (si.docCount >= targetMergeDocs)
						break;
					mergeDocs += si.docCount;
				}
				
				if (mergeDocs >= targetMergeDocs)
				// found a merge to do
					MergeSegments(minSegment + 1);
				else
					break;
				
				targetMergeDocs *= mergeFactor; // increase target size
			}
		}
		
		/// <summary>Pops segments off of segmentInfos stack down to minSegment, merges them,
		/// and pushes the merged index onto the top of the segmentInfos stack. 
		/// </summary>
		private void  MergeSegments(int minSegment)
		{
			System.String mergedName = NewSegmentName();
			if (infoStream != null)
				infoStream.Write("merging segments");
			SegmentMerger merger = new SegmentMerger(directory, mergedName, useCompoundFile);
			
			System.Collections.ArrayList segmentsToDelete = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
			for (int i = minSegment; i < segmentInfos.Count; i++)
			{
				SegmentInfo si = segmentInfos.Info(i);
				if (infoStream != null)
					infoStream.Write(" " + si.name + " (" + si.docCount + " docs)");
				Monodoc.Lucene.Net.Index.IndexReader reader = new SegmentReader(si);
				merger.Add(reader);
				if ((reader.Directory() == this.directory) || (reader.Directory() == this.ramDirectory))
					segmentsToDelete.Add(reader); // queue segment for deletion
			}
			
			int mergedDocCount = merger.Merge();
			
			if (infoStream != null)
			{
				infoStream.WriteLine(" into " + mergedName + " (" + mergedDocCount + " docs)");
			}
			
			segmentInfos.RemoveRange(minSegment, segmentInfos.Count - minSegment); // pop old infos & add new
			segmentInfos.Add(new SegmentInfo(mergedName, mergedDocCount, directory));
			
			// close readers before we attempt to delete now-obsolete segments
			merger.CloseReaders();
			
			lock (directory)
			{
				// in- & inter-process sync
				new AnonymousClassWith2(segmentsToDelete, this, directory.MakeLock(IndexWriter.COMMIT_LOCK_NAME), COMMIT_LOCK_TIMEOUT).Run();
			}
		}
		
		/* Some operating systems (e.g. Windows) don't permit a file to be deleted
		while it is opened for read (e.g. by another process or thread).  So we
		assume that when a delete fails it is because the file is open in another
		process, and queue the file for subsequent deletion. */
		
		private void  DeleteSegments(System.Collections.ArrayList segments)
		{
			System.Collections.ArrayList deletable = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
			
			DeleteFiles(ReadDeleteableFiles(), deletable); // try to delete deleteable
			
			for (int i = 0; i < segments.Count; i++)
			{
				SegmentReader reader = (SegmentReader) segments[i];
				if (reader.Directory() == this.directory)
					DeleteFiles(reader.Files(), deletable);
				// try to delete our files
				else
					DeleteFiles(reader.Files(), reader.Directory()); // delete other files
			}
			
			WriteDeleteableFiles(deletable); // note files we can't delete
		}
		
		private void  DeleteFiles(System.Collections.ArrayList files, Directory directory)
		{
			for (int i = 0; i < files.Count; i++)
				directory.DeleteFile((System.String) files[i]);
		}
		
		private void  DeleteFiles(System.Collections.ArrayList files, System.Collections.ArrayList deletable)
		{
			for (int i = 0; i < files.Count; i++)
			{
				System.String file = (System.String) files[i];
				try
				{
					directory.DeleteFile(file); // try to delete each file
				}
				catch (System.IO.IOException e)
				{
					// if delete fails
					if (directory.FileExists(file))
					{
						if (infoStream != null)
						{
							infoStream.WriteLine(e.Message + "; Will re-try later.");
						}
						deletable.Add(file); // add to deletable
					}
				}
			}
		}
		
		private System.Collections.ArrayList ReadDeleteableFiles()
		{
			System.Collections.ArrayList result = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
			if (!directory.FileExists("deletable"))
				return result;
			
			InputStream input = directory.OpenFile("deletable");
			try
			{
				for (int i = input.ReadInt(); i > 0; i--)
				// read file names
					result.Add(input.ReadString());
			}
			finally
			{
				input.Close();
			}
			return result;
		}
		
		private void  WriteDeleteableFiles(System.Collections.ArrayList files)
		{
			OutputStream output = directory.CreateFile("deleteable.new");
			try
			{
				output.WriteInt(files.Count);
				for (int i = 0; i < files.Count; i++)
					output.WriteString((System.String) files[i]);
			}
			finally
			{
				output.Close();
			}
			directory.RenameFile("deleteable.new", "deletable");
		}
	}
}