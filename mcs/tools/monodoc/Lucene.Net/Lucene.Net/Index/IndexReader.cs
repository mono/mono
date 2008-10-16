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
using Document = Monodoc.Lucene.Net.Documents.Document;
using Field = Monodoc.Lucene.Net.Documents.Field;
using Similarity = Monodoc.Lucene.Net.Search.Similarity;
using Directory = Monodoc.Lucene.Net.Store.Directory;
using FSDirectory = Monodoc.Lucene.Net.Store.FSDirectory;
using Lock = Monodoc.Lucene.Net.Store.Lock;
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary>IndexReader is an abstract class, providing an interface for accessing an
	/// index.  Search of an index is done entirely through this abstract interface,
	/// so that any subclass which implements it is searchable.
	/// <p> Concrete subclasses of IndexReader are usually constructed with a call to
	/// the static method {@link #open}.
	/// <p> For efficiency, in this API documents are often referred to via
	/// <i>document numbers</i>, non-negative integers which each name a unique
	/// document in the index.  These document numbers are ephemeral--they may change
	/// as documents are added to and deleted from an index.  Clients should thus not
	/// rely on a given document having the same number between sessions.
	/// </summary>
	/// <author>  Doug Cutting
	/// </author>
	/// <version>  $Id: IndexReader.java,v 1.32 2004/04/21 16:46:30 goller Exp $
	/// </version>
	public abstract class IndexReader
	{
		private class AnonymousClassWith : Lock.With
		{
			private void  InitBlock(Lucene.Net.Store.Directory directory, bool closeDirectory)
			{
				this.directory = directory;
				this.closeDirectory = closeDirectory;
			}
			private Lucene.Net.Store.Directory directory;
			private bool closeDirectory;
			internal AnonymousClassWith(Lucene.Net.Store.Directory directory, bool closeDirectory, Lucene.Net.Store.Lock Param1, long Param2) : base(Param1, Param2)
			{
				InitBlock(directory, closeDirectory);
			}
			public override System.Object DoBody()
			{
				SegmentInfos infos = new SegmentInfos();
				infos.Read(directory);
				if (infos.Count == 1)
				{
					// index is optimized
					return new SegmentReader(infos, infos.Info(0), closeDirectory);
				}
				else
				{
					Monodoc.Lucene.Net.Index.IndexReader[] readers = new Monodoc.Lucene.Net.Index.IndexReader[infos.Count];
					for (int i = 0; i < infos.Count; i++)
						readers[i] = new SegmentReader(infos.Info(i));
					return new MultiReader(directory, infos, closeDirectory, readers);
				}
			}
		}
		private class AnonymousClassWith1 : Lock.With
		{
			private void  InitBlock(Monodoc.Lucene.Net.Index.IndexReader enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private Monodoc.Lucene.Net.Index.IndexReader enclosingInstance;
			public Monodoc.Lucene.Net.Index.IndexReader Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal AnonymousClassWith1(Monodoc.Lucene.Net.Index.IndexReader enclosingInstance, Lucene.Net.Store.Lock Param1, long Param2) : base(Param1, Param2)
			{
				InitBlock(enclosingInstance);
			}
			public override System.Object DoBody()
			{
				Enclosing_Instance.DoCommit();
				Enclosing_Instance.segmentInfos.Write(Enclosing_Instance.directory);
				return null;
			}
		}
		
		/// <summary> Constructor used if IndexReader is not owner of its directory. 
		/// This is used for IndexReaders that are used within other IndexReaders that take care or locking directories.
		/// 
		/// </summary>
		/// <param name="directory">Directory where IndexReader files reside.
		/// </param>
		protected internal IndexReader(Directory directory)
		{
			this.directory = directory;
			segmentInfos = null;
			directoryOwner = false;
			closeDirectory = false;
			stale = false;
			hasChanges = false;
			writeLock = null;
		}
		
		/// <summary> Constructor used if IndexReader is owner of its directory.
		/// If IndexReader is owner of its directory, it locks its directory in case of write operations.
		/// 
		/// </summary>
		/// <param name="directory">Directory where IndexReader files reside.
		/// </param>
		/// <param name="segmentInfos">Used for write-l
		/// </param>
		/// <param name="">closeDirectory
		/// </param>
		internal IndexReader(Directory directory, SegmentInfos segmentInfos, bool closeDirectory)
		{
			this.directory = directory;
			this.segmentInfos = segmentInfos;
			directoryOwner = true;
			this.closeDirectory = closeDirectory;
			stale = false;
			hasChanges = false;
			writeLock = null;
		}
		
		private Directory directory;
		
		private bool directoryOwner;
		private SegmentInfos segmentInfos;
		private Lock writeLock;
		private bool stale;
		private bool hasChanges;
		
		private bool closeDirectory;
		
		/// <summary>Returns an IndexReader reading the index in an FSDirectory in the named
		/// path. 
		/// </summary>
		public static Monodoc.Lucene.Net.Index.IndexReader Open(System.String path)
		{
			return Open(FSDirectory.GetDirectory(path, false), true);
		}
		
		/// <summary>Returns an IndexReader reading the index in an FSDirectory in the named
		/// path. 
		/// </summary>
		public static Monodoc.Lucene.Net.Index.IndexReader Open(System.IO.FileInfo path)
		{
			return Open(FSDirectory.GetDirectory(path, false), true);
		}
		
		/// <summary>Returns an IndexReader reading the index in the given Directory. </summary>
		public static Monodoc.Lucene.Net.Index.IndexReader Open(Directory directory)
		{
			return Open(directory, false);
		}
		
		private static Monodoc.Lucene.Net.Index.IndexReader Open(Directory directory, bool closeDirectory)
		{
			lock (directory)
			{
				// in- & inter-process sync
				return (Monodoc.Lucene.Net.Index.IndexReader) new AnonymousClassWith(directory, closeDirectory, directory.MakeLock(IndexWriter.COMMIT_LOCK_NAME), IndexWriter.COMMIT_LOCK_TIMEOUT).Run();
			}
		}
		
		/// <summary>Returns the directory this index resides in. </summary>
		public virtual Directory Directory()
		{
			return directory;
		}
		
		/// <summary> Returns the time the index in the named directory was last modified. 
		/// 
		/// <p>Synchronization of IndexReader and IndexWriter instances is 
		/// no longer done via time stamps of the segments file since the time resolution 
		/// depends on the hardware platform. Instead, a version number is maintained
		/// within the segments file, which is incremented everytime when the index is
		/// changed.</p>
		/// 
		/// </summary>
		/// <deprecated>  Replaced by {@link #GetCurrentVersion(String)}
		/// 
		/// </deprecated>
		public static long LastModified(System.String directory)
		{
			return LastModified(new System.IO.FileInfo(directory));
		}
		
		/// <summary> Returns the time the index in the named directory was last modified. 
		/// 
		/// <p>Synchronization of IndexReader and IndexWriter instances is 
		/// no longer done via time stamps of the segments file since the time resolution 
		/// depends on the hardware platform. Instead, a version number is maintained
		/// within the segments file, which is incremented everytime when the index is
		/// changed.</p>
		/// 
		/// </summary>
		/// <deprecated>  Replaced by {@link #GetCurrentVersion(File)}
		/// 
		/// </deprecated>
		public static long LastModified(System.IO.FileInfo directory)
		{
			return FSDirectory.FileModified(directory, "segments");
		}
		
		/// <summary> Returns the time the index in the named directory was last modified. 
		/// 
		/// <p>Synchronization of IndexReader and IndexWriter instances is 
		/// no longer done via time stamps of the segments file since the time resolution 
		/// depends on the hardware platform. Instead, a version number is maintained
		/// within the segments file, which is incremented everytime when the index is
		/// changed.</p>
		/// 
		/// </summary>
		/// <deprecated>  Replaced by {@link #GetCurrentVersion(Directory)}
		/// 
		/// </deprecated>
		public static long LastModified(Directory directory)
		{
			return directory.FileModified("segments");
		}
		
		/// <summary> Reads version number from segments files. The version number counts the
		/// number of changes of the index.
		/// 
		/// </summary>
		/// <param name="directory">where the index resides.
		/// </param>
		/// <returns> version number.
		/// </returns>
		/// <throws>  IOException if segments file cannot be read </throws>
		public static long GetCurrentVersion(System.String directory)
		{
			return GetCurrentVersion(new System.IO.FileInfo(directory));
		}
		
		/// <summary> Reads version number from segments files. The version number counts the
		/// number of changes of the index.
		/// 
		/// </summary>
		/// <param name="directory">where the index resides.
		/// </param>
		/// <returns> version number.
		/// </returns>
		/// <throws>  IOException if segments file cannot be read </throws>
		public static long GetCurrentVersion(System.IO.FileInfo directory)
		{
			Directory dir = FSDirectory.GetDirectory(directory, false);
			long version = GetCurrentVersion(dir);
			dir.Close();
			return version;
		}
		
		/// <summary> Reads version number from segments files. The version number counts the
		/// number of changes of the index.
		/// 
		/// </summary>
		/// <param name="directory">where the index resides.
		/// </param>
		/// <returns> version number.
		/// </returns>
		/// <throws>  IOException if segments file cannot be read. </throws>
		public static long GetCurrentVersion(Directory directory)
		{
			return SegmentInfos.ReadCurrentVersion(directory);
		}
		
		/// <summary>Return an array of term frequency vectors for the specified document.
		/// The array contains a vector for each vectorized Field in the document.
		/// Each vector contains terms and frequencies for all terms
		/// in a given vectorized Field.
		/// If no such fields existed, the method returns null.
		/// 
		/// </summary>
		/// <seealso cref="Field#IsTermVectorStored()">
		/// </seealso>
		abstract public TermFreqVector[] GetTermFreqVectors(int docNumber);
		
		/// <summary>Return a term frequency vector for the specified document and Field. The
		/// vector returned contains terms and frequencies for those terms in
		/// the specified Field of this document, if the Field had storeTermVector
		/// flag set.  If the flag was not set, the method returns null.
		/// 
		/// </summary>
		/// <seealso cref="Field#IsTermVectorStored()">
		/// </seealso>
		abstract public TermFreqVector GetTermFreqVector(int docNumber, System.String field);
		
		/// <summary> Returns <code>true</code> if an index exists at the specified directory.
		/// If the directory does not exist or if there is no index in it.
		/// <code>false</code> is returned.
		/// </summary>
		/// <param name="directory">the directory to check for an index
		/// </param>
		/// <returns> <code>true</code> if an index exists; <code>false</code> otherwise
		/// </returns>
		public static bool IndexExists(System.String directory)
		{
			bool tmpBool;
			if (System.IO.File.Exists((new System.IO.FileInfo(System.IO.Path.Combine(directory, "segments"))).FullName))
				tmpBool = true;
			else
				tmpBool = System.IO.Directory.Exists((new System.IO.FileInfo(System.IO.Path.Combine(directory, "segments"))).FullName);
			return tmpBool;
		}
		
		/// <summary> Returns <code>true</code> if an index exists at the specified directory.
		/// If the directory does not exist or if there is no index in it.
		/// </summary>
		/// <param name="directory">the directory to check for an index
		/// </param>
		/// <returns> <code>true</code> if an index exists; <code>false</code> otherwise
		/// </returns>
		public static bool IndexExists(System.IO.FileInfo directory)
		{
			bool tmpBool;
			if (System.IO.File.Exists((new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, "segments"))).FullName))
				tmpBool = true;
			else
				tmpBool = System.IO.Directory.Exists((new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, "segments"))).FullName);
			return tmpBool;
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
			return directory.FileExists("segments");
		}
		
		/// <summary>Returns the number of documents in this index. </summary>
		public abstract int NumDocs();
		
		/// <summary>Returns one greater than the largest possible document number.
		/// This may be used to, e.g., determine how big to allocate an array which
		/// will have an element for every document number in an index.
		/// </summary>
		public abstract int MaxDoc();
		
		/// <summary>Returns the stored fields of the <code>n</code><sup>th</sup>
		/// <code>Document</code> in this index. 
		/// </summary>
		public abstract Document Document(int n);
		
		/// <summary>Returns true if document <i>n</i> has been deleted </summary>
		public abstract bool IsDeleted(int n);
		
		/// <summary>Returns true if any documents have been deleted </summary>
		public abstract bool HasDeletions();
		
		/// <summary>Returns the byte-encoded normalization factor for the named Field of
		/// every document.  This is used by the search code to score documents.
		/// 
		/// </summary>
		/// <seealso cref="Field#SetBoost(float)">
		/// </seealso>
		public abstract byte[] Norms(System.String field);
		
		/// <summary>Reads the byte-encoded normalization factor for the named Field of every
		/// document.  This is used by the search code to score documents.
		/// 
		/// </summary>
		/// <seealso cref="Field#SetBoost(float)">
		/// </seealso>
		public abstract void  Norms(System.String field, byte[] bytes, int offset);
		
		/// <summary>Expert: Resets the normalization factor for the named Field of the named
		/// document.  The norm represents the product of the Field's {@link
		/// Field#SetBoost(float) boost} and its {@link Similarity#LengthNorm(String,
		/// int) length normalization}.  Thus, to preserve the length normalization
		/// values when resetting this, one should base the new value upon the old.
		/// 
		/// </summary>
		/// <seealso cref="#Norms(String)">
		/// </seealso>
		/// <seealso cref="Similarity#DecodeNorm(byte)">
		/// </seealso>
		public void  SetNorm(int doc, System.String field, byte value_Renamed)
		{
			lock (this)
			{
				if (directoryOwner)
					AquireWriteLock();
				DoSetNorm(doc, field, value_Renamed);
				hasChanges = true;
			}
		}
		
		/// <summary>Implements setNorm in subclass.</summary>
		protected internal abstract void  DoSetNorm(int doc, System.String field, byte value_Renamed);
		
		/// <summary>Expert: Resets the normalization factor for the named Field of the named
		/// document.
		/// 
		/// </summary>
		/// <seealso cref="#Norms(String)">
		/// </seealso>
		/// <seealso cref="Similarity#DecodeNorm(byte)">
		/// </seealso>
		public virtual void  SetNorm(int doc, System.String field, float value_Renamed)
		{
			SetNorm(doc, field, Similarity.EncodeNorm(value_Renamed));
		}
		
		
		/// <summary>Returns an enumeration of all the terms in the index.
		/// The enumeration is ordered by Term.compareTo().  Each term
		/// is greater than all that precede it in the enumeration.
		/// </summary>
		public abstract TermEnum Terms();
		
		/// <summary>Returns an enumeration of all terms after a given term.
		/// The enumeration is ordered by Term.compareTo().  Each term
		/// is greater than all that precede it in the enumeration.
		/// </summary>
		public abstract TermEnum Terms(Term t);
		
		/// <summary>Returns the number of documents containing the term <code>t</code>. </summary>
		public abstract int DocFreq(Term t);
		
		/// <summary>Returns an enumeration of all the documents which contain
		/// <code>term</code>. For each document, the document number, the frequency of
		/// the term in that document is also provided, for use in search scoring.
		/// Thus, this method implements the mapping:
		/// <p><ul>
		/// Term &nbsp;&nbsp; =&gt; &nbsp;&nbsp; &lt;docNum, freq&gt;<sup>*</sup>
		/// </ul>
		/// <p>The enumeration is ordered by document number.  Each document number
		/// is greater than all that precede it in the enumeration.
		/// </summary>
		public virtual TermDocs TermDocs(Term term)
		{
			TermDocs termDocs = TermDocs();
			termDocs.Seek(term);
			return termDocs;
		}
		
		/// <summary>Returns an unpositioned {@link TermDocs} enumerator. </summary>
		public abstract TermDocs TermDocs();
		
		/// <summary>Returns an enumeration of all the documents which contain
		/// <code>term</code>.  For each document, in addition to the document number
		/// and frequency of the term in that document, a list of all of the ordinal
		/// positions of the term in the document is available.  Thus, this method
		/// implements the mapping:
		/// <p><ul>
		/// Term &nbsp;&nbsp; =&gt; &nbsp;&nbsp; &lt;docNum, freq,
		/// &lt;pos<sub>1</sub>, pos<sub>2</sub>, ...
		/// pos<sub>freq-1</sub>&gt;
		/// &gt;<sup>*</sup>
		/// </ul>
		/// <p> This positional information faciliates phrase and proximity searching.
		/// <p>The enumeration is ordered by document number.  Each document number is
		/// greater than all that precede it in the enumeration.
		/// </summary>
		public virtual TermPositions TermPositions(Term term)
		{
			TermPositions termPositions = TermPositions();
			termPositions.Seek(term);
			return termPositions;
		}
		
		/// <summary>Returns an unpositioned {@link TermPositions} enumerator. </summary>
		public abstract TermPositions TermPositions();
		
		/// <summary> Trys to acquire the WriteLock on this directory.
		/// this method is only valid if this IndexReader is directory owner.
		/// 
		/// </summary>
		/// <throws>  IOException If WriteLock cannot be acquired. </throws>
		private void  AquireWriteLock()
		{
			if (stale)
				throw new System.IO.IOException("IndexReader out of date and no longer valid for delete, undelete, or setNorm operations");
			
			if (this.writeLock == null)
			{
				Lock writeLock = directory.MakeLock(IndexWriter.WRITE_LOCK_NAME);
				if (!writeLock.Obtain(IndexWriter.WRITE_LOCK_TIMEOUT))
				// obtain write lock
				{
					throw new System.IO.IOException("Index locked for write: " + writeLock);
				}
				this.writeLock = writeLock;
				
				// we have to check whether index has changed since this reader was opened.
				// if so, this reader is no longer valid for deletion
				if (SegmentInfos.ReadCurrentVersion(directory) > segmentInfos.GetVersion())
				{
					stale = true;
					this.writeLock.Release();
					this.writeLock = null;
					throw new System.IO.IOException("IndexReader out of date and no longer valid for delete, undelete, or setNorm operations");
				}
			}
		}
		
		/// <summary>Deletes the document numbered <code>docNum</code>.  Once a document is
		/// deleted it will not appear in TermDocs or TermPostitions enumerations.
		/// Attempts to read its Field with the {@link #document}
		/// method will result in an error.  The presence of this document may still be
		/// reflected in the {@link #docFreq} statistic, though
		/// this will be corrected eventually as the index is further modified.
		/// </summary>
		public void  Delete(int docNum)
		{
			lock (this)
			{
				if (directoryOwner)
					AquireWriteLock();
				DoDelete(docNum);
				hasChanges = true;
			}
		}
		
		/// <summary>Implements deletion of the document numbered <code>docNum</code>.
		/// Applications should call {@link #Delete(int)} or {@link #Delete(Term)}.
		/// </summary>
		protected internal abstract void  DoDelete(int docNum);
		
		/// <summary>Deletes all documents containing <code>term</code>.
		/// This is useful if one uses a document Field to hold a unique ID string for
		/// the document.  Then to delete such a document, one merely constructs a
		/// term with the appropriate Field and the unique ID string as its text and
		/// passes it to this method.  Returns the number of documents deleted.
		/// </summary>
		public int Delete(Term term)
		{
			TermDocs docs = TermDocs(term);
			if (docs == null)
				return 0;
			int n = 0;
			try
			{
				while (docs.Next())
				{
					Delete(docs.Doc());
					n++;
				}
			}
			finally
			{
				docs.Close();
			}
			return n;
		}
		
		/// <summary>Undeletes all documents currently marked as deleted in this index.</summary>
		public void  UndeleteAll()
		{
			lock (this)
			{
				if (directoryOwner)
					AquireWriteLock();
				DoUndeleteAll();
				hasChanges = true;
			}
		}
		
		/// <summary>Implements actual undeleteAll() in subclass. </summary>
		protected internal abstract void  DoUndeleteAll();
		
		/// <summary> Commit changes resulting from delete, undeleteAll, or setNorm operations
		/// 
		/// </summary>
		/// <throws>  IOException </throws>
		protected internal void  Commit()
		{
			lock (this)
			{
				if (hasChanges)
				{
					if (directoryOwner)
					{
						lock (directory)
						{
							// in- & inter-process sync
							new AnonymousClassWith1(this, directory.MakeLock(IndexWriter.COMMIT_LOCK_NAME), IndexWriter.COMMIT_LOCK_TIMEOUT).Run();
						}
						if (writeLock != null)
						{
							writeLock.Release(); // release write lock
							writeLock = null;
						}
					}
					else
						DoCommit();
				}
				hasChanges = false;
			}
		}
		
		/// <summary>Implements commit. </summary>
		protected internal abstract void  DoCommit();
		
		/// <summary> Closes files associated with this index.
		/// Also saves any new deletions to disk.
		/// No other methods should be called after this has been called.
		/// </summary>
		public void  Close()
		{
            lock (this)
            {
                Commit();
                DoClose();
                if (closeDirectory)
                    directory.Close();
                System.GC.SuppressFinalize(this);
            }
		}
		
		/// <summary>Implements close. </summary>
		protected internal abstract void  DoClose();
		
		/// <summary>Release the write lock, if needed. </summary>
		~IndexReader()
		{
			if (writeLock != null)
			{
				writeLock.Release(); // release write lock
				writeLock = null;
			}
		}
		
		/// <summary> Returns a list of all unique Field names that exist in the index pointed
		/// to by this IndexReader.
		/// </summary>
		/// <returns> Collection of Strings indicating the names of the fields
		/// </returns>
		/// <throws>  IOException if there is a problem with accessing the index </throws>
		public abstract System.Collections.ICollection GetFieldNames();
		
		/// <summary> Returns a list of all unique Field names that exist in the index pointed
		/// to by this IndexReader.  The boolean argument specifies whether the fields
		/// returned are indexed or not.
		/// </summary>
		/// <param name="indexed"><code>true</code> if only indexed fields should be returned;
		/// <code>false</code> if only unindexed fields should be returned.
		/// </param>
		/// <returns> Collection of Strings indicating the names of the fields
		/// </returns>
		/// <throws>  IOException if there is a problem with accessing the index </throws>
		public abstract System.Collections.ICollection GetFieldNames(bool indexed);
		
		/// <summary> </summary>
		/// <param name="storedTermVector">if true, returns only Indexed fields that have term vector info, 
		/// else only indexed fields without term vector info 
		/// </param>
		/// <returns> Collection of Strings indicating the names of the fields
		/// </returns>
		public abstract System.Collections.ICollection GetIndexedFieldNames(bool storedTermVector);
		
		/// <summary> Returns <code>true</code> iff the index in the named directory is
		/// currently locked.
		/// </summary>
		/// <param name="directory">the directory to check for a lock
		/// </param>
		/// <throws>  IOException if there is a problem with accessing the index </throws>
		public static bool IsLocked(Directory directory)
		{
			return directory.MakeLock(IndexWriter.WRITE_LOCK_NAME).IsLocked() || directory.MakeLock(IndexWriter.COMMIT_LOCK_NAME).IsLocked();
		}
		
		/// <summary> Returns <code>true</code> iff the index in the named directory is
		/// currently locked.
		/// </summary>
		/// <param name="directory">the directory to check for a lock
		/// </param>
		/// <throws>  IOException if there is a problem with accessing the index </throws>
		public static bool IsLocked(System.String directory)
		{
			Directory dir = FSDirectory.GetDirectory(directory, false);
			bool result = IsLocked(dir);
			dir.Close();
			return result;
		}
		
		/// <summary> Forcibly unlocks the index in the named directory.
		/// <P>
		/// Caution: this should only be used by failure recovery code,
		/// when it is known that no other process nor thread is in fact
		/// currently accessing this index.
		/// </summary>
		public static void  Unlock(Directory directory)
		{
			directory.MakeLock(IndexWriter.WRITE_LOCK_NAME).Release();
			directory.MakeLock(IndexWriter.COMMIT_LOCK_NAME).Release();
		}
	}
}
