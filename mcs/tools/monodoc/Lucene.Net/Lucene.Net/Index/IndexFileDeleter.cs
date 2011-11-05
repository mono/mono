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
	
	/*
	* This class keeps track of each SegmentInfos instance that
	* is still "live", either because it corresponds to a
	* segments_N file in the Directory (a "commit", i.e. a
	* committed SegmentInfos) or because it's an in-memory
	* SegmentInfos that a writer is actively updating but has
	* not yet committed.  This class uses simple reference
	* counting to map the live SegmentInfos instances to
	* individual files in the Directory.
	*
	* When autoCommit=true, IndexWriter currently commits only
	* on completion of a merge (though this may change with
	* time: it is not a guarantee).  When autoCommit=false,
	* IndexWriter only commits when it is closed.  Regardless
	* of autoCommit, the user may call IndexWriter.commit() to
	* force a blocking commit.
	* 
	* The same directory file may be referenced by more than
	* one IndexCommit, i.e. more than one SegmentInfos.
	* Therefore we count how many commits reference each file.
	* When all the commits referencing a certain file have been
	* deleted, the refcount for that file becomes zero, and the
	* file is deleted.
	*
	* A separate deletion policy interface
	* (IndexDeletionPolicy) is consulted on creation (onInit)
	* and once per commit (onCommit), to decide when a commit
	* should be removed.
	* 
	* It is the business of the IndexDeletionPolicy to choose
	* when to delete commit points.  The actual mechanics of
	* file deletion, retrying, etc, derived from the deletion
	* of commit points is the business of the IndexFileDeleter.
	* 
	* The current default deletion policy is {@link
	* KeepOnlyLastCommitDeletionPolicy}, which removes all
	* prior commits when a new commit has completed.  This
	* matches the behavior before 2.2.
	*
	* Note that you must hold the write.lock before
	* instantiating this class.  It opens segments_N file(s)
	* directly with no retry logic.
	*/
	
	public sealed class IndexFileDeleter
	{
		
		/* Files that we tried to delete but failed (likely
		* because they are open and we are running on Windows),
		* so we will retry them again later: */
		private System.Collections.Generic.IList<string> deletable;
		
		/* Reference count for all files in the index.  
		* Counts how many existing commits reference a file.
		* Maps String to RefCount (class below) instances: */
		private System.Collections.Generic.Dictionary<System.String, RefCount> refCounts = new System.Collections.Generic.Dictionary<System.String, RefCount>();
		
		/* Holds all commits (segments_N) currently in the index.
		* This will have just 1 commit if you are using the
		* default delete policy (KeepOnlyLastCommitDeletionPolicy).
		* Other policies may leave commit points live for longer
		* in which case this list would be longer than 1: */
		private System.Collections.ArrayList commits = new System.Collections.ArrayList();
		
		/* Holds files we had incref'd from the previous
		* non-commit checkpoint: */
        private System.Collections.Generic.IList<string> lastFiles = new System.Collections.Generic.List<string>();
		
		/* Commits that the IndexDeletionPolicy have decided to delete: */
		private System.Collections.ArrayList commitsToDelete = new System.Collections.ArrayList();
		
		private System.IO.StreamWriter infoStream;
		private Directory directory;
		private IndexDeletionPolicy policy;
		private DocumentsWriter docWriter;
		
		internal bool startingCommitDeleted;
        private SegmentInfos lastSegmentInfos;

        private System.Collections.Generic.Dictionary<string, string> synced;
		
		/// <summary>Change to true to see details of reference counts when
		/// infoStream != null 
		/// </summary>
		public static bool VERBOSE_REF_COUNTS = false;
		
		internal void  SetInfoStream(System.IO.StreamWriter infoStream)
		{
			this.infoStream = infoStream;
			if (infoStream != null)
			{
				Message("setInfoStream deletionPolicy=" + policy);
			}
		}
		
		private void  Message(System.String message)
		{
            infoStream.WriteLine("IFD [" + new DateTime().ToString() + "; " + SupportClass.ThreadClass.Current().Name + "]: " + message);
		}
		
		/// <summary> Initialize the deleter: find all previous commits in
		/// the Directory, incref the files they reference, call
		/// the policy to let it delete commits.  This will remove
		/// any files not referenced by any of the commits.
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
        public IndexFileDeleter(Directory directory, IndexDeletionPolicy policy, SegmentInfos segmentInfos, System.IO.StreamWriter infoStream, DocumentsWriter docWriter, System.Collections.Generic.Dictionary<string, string> synced)
		{
			
			this.docWriter = docWriter;
			this.infoStream = infoStream;
            this.synced = synced;
			
			if (infoStream != null)
			{
				Message("init: current segments file is \"" + segmentInfos.GetCurrentSegmentFileName() + "\"; deletionPolicy=" + policy);
			}
			
			this.policy = policy;
			this.directory = directory;
			
			// First pass: walk the files and initialize our ref
			// counts:
			long currentGen = segmentInfos.GetGeneration();
			IndexFileNameFilter filter = IndexFileNameFilter.GetFilter();
			
			System.String[] files = directory.ListAll();
			
			CommitPoint currentCommitPoint = null;
			
			for (int i = 0; i < files.Length; i++)
			{
				
				System.String fileName = files[i];
				
				if (filter.Accept(null, fileName) && !fileName.Equals(IndexFileNames.SEGMENTS_GEN))
				{
					
					// Add this file to refCounts with initial count 0:
					GetRefCount(fileName);
					
					if (fileName.StartsWith(IndexFileNames.SEGMENTS))
					{
						
						// This is a commit (segments or segments_N), and
						// it's valid (<= the max gen).  Load it, then
						// incref all files it refers to:
                        if (infoStream != null)
                        {
                            Message("init: load commit \"" + fileName + "\"");
                        }
                        SegmentInfos sis = new SegmentInfos();
                        try
                        {
                            sis.Read(directory, fileName);
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            // LUCENE-948: on NFS (and maybe others), if
                            // you have writers switching back and forth
                            // between machines, it's very likely that the
                            // dir listing will be stale and will claim a
                            // file segments_X exists when in fact it
                            // doesn't.  So, we catch this and handle it
                            // as if the file does not exist
                            if (infoStream != null)
                            {
                                Message("init: hit FileNotFoundException when loading commit \"" + fileName + "\"; skipping this commit point");
                            }
                            sis = null;
                        }
                        catch (System.IO.IOException e)
                        {
                            if (SegmentInfos.GenerationFromSegmentsFileName(fileName) <= currentGen)
                            {
                                throw e;
                            }
                            else
                            {
                                // Most likely we are opening an index that
                                // has an aborted "future" commit, so suppress
                                // exc in this case
                                sis = null;
                            }
                        }
                        if (sis != null)
                        {
                            CommitPoint commitPoint = new CommitPoint(this,commitsToDelete, directory, sis);
                            if (sis.GetGeneration() == segmentInfos.GetGeneration())
                            {
                                currentCommitPoint = commitPoint;
                            }
                            commits.Add(commitPoint);
                            IncRef(sis, true);

                            if (lastSegmentInfos == null || sis.GetGeneration() > lastSegmentInfos.GetGeneration())
                            {
                                lastSegmentInfos = sis;
                            }
						}
					}
				}
			}
			
			if (currentCommitPoint == null)
			{
				// We did not in fact see the segments_N file
				// corresponding to the segmentInfos that was passed
				// in.  Yet, it must exist, because our caller holds
				// the write lock.  This can happen when the directory
				// listing was stale (eg when index accessed via NFS
				// client with stale directory listing cache).  So we
				// try now to explicitly open this commit point:
				SegmentInfos sis = new SegmentInfos();
				try
				{
					sis.Read(directory, segmentInfos.GetCurrentSegmentFileName());
				}
				catch (System.IO.IOException e)
				{
					throw new CorruptIndexException("failed to locate current segments_N file");
				}
				if (infoStream != null)
					Message("forced open of current segments file " + segmentInfos.GetCurrentSegmentFileName());
				currentCommitPoint = new CommitPoint(this, commitsToDelete, directory, sis);
				commits.Add(currentCommitPoint);
				IncRef(sis, true);
			}
			
			// We keep commits list in sorted order (oldest to newest):
			commits.Sort();
			
			// Now delete anything with ref count at 0.  These are
			// presumably abandoned files eg due to crash of
			// IndexWriter.
			System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.String, RefCount>> it = refCounts.GetEnumerator();
			while (it.MoveNext())
			{
				System.String fileName = (System.String) it.Current.Key;
				RefCount rc = (RefCount) refCounts[fileName];
				if (0 == rc.count)
				{
					if (infoStream != null)
					{
						Message("init: removing unreferenced file \"" + fileName + "\"");
					}
					DeleteFile(fileName);
				}
			}
			
			// Finally, give policy a chance to remove things on
			// startup:
			policy.OnInit(commits);
			
			// Always protect the incoming segmentInfos since
			// sometime it may not be the most recent commit
			Checkpoint(segmentInfos, false);
			
			startingCommitDeleted = currentCommitPoint.IsDeleted();
			
			DeleteCommits();
		}

        public SegmentInfos GetLastSegmentInfos()
        {
            return lastSegmentInfos;
        }
		
		/// <summary> Remove the CommitPoints in the commitsToDelete List by
		/// DecRef'ing all files from each SegmentInfos.
		/// </summary>
		private void  DeleteCommits()
		{
			
			int size = commitsToDelete.Count;
			
			if (size > 0)
			{
				
				// First decref all files that had been referred to by
				// the now-deleted commits:
				for (int i = 0; i < size; i++)
				{
					CommitPoint commit = (CommitPoint) commitsToDelete[i];
					if (infoStream != null)
					{
						Message("deleteCommits: now decRef commit \"" + commit.GetSegmentsFileName() + "\"");
					}
					System.Collections.Generic.IEnumerator<string> it = commit.files.GetEnumerator();
					while (it.MoveNext())
					{
						DecRef(it.Current);
					}
				}
				commitsToDelete.Clear();
				
				// Now compact commits to remove deleted ones (preserving the sort):
				size = commits.Count;
				int readFrom = 0;
				int writeTo = 0;
				while (readFrom < size)
				{
					CommitPoint commit = (CommitPoint) commits[readFrom];
					if (!commit.deleted)
					{
						if (writeTo != readFrom)
						{
							commits[writeTo] = commits[readFrom];
						}
						writeTo++;
					}
					readFrom++;
				}
				
				while (size > writeTo)
				{
					commits.RemoveAt(size - 1);
					size--;
				}
			}
		}
		
		/// <summary> Writer calls this when it has hit an error and had to
		/// roll back, to tell us that there may now be
		/// unreferenced files in the filesystem.  So we re-list
		/// the filesystem and delete such files.  If segmentName
		/// is non-null, we will only delete files corresponding to
		/// that segment.
		/// </summary>
		public void  Refresh(System.String segmentName)
		{
			System.String[] files = directory.ListAll();
			IndexFileNameFilter filter = IndexFileNameFilter.GetFilter();
			System.String segmentPrefix1;
			System.String segmentPrefix2;
			if (segmentName != null)
			{
				segmentPrefix1 = segmentName + ".";
				segmentPrefix2 = segmentName + "_";
			}
			else
			{
				segmentPrefix1 = null;
				segmentPrefix2 = null;
			}
			
			for (int i = 0; i < files.Length; i++)
			{
				System.String fileName = files[i];
				if (filter.Accept(null, fileName) && (segmentName == null || fileName.StartsWith(segmentPrefix1) || fileName.StartsWith(segmentPrefix2)) && !refCounts.ContainsKey(fileName) && !fileName.Equals(IndexFileNames.SEGMENTS_GEN))
				{
					// Unreferenced file, so remove it
					if (infoStream != null)
					{
						Message("refresh [prefix=" + segmentName + "]: removing newly created unreferenced file \"" + fileName + "\"");
					}
					DeleteFile(fileName);
				}
			}
		}
		
		public void  Refresh()
		{
			Refresh(null);
		}
		
		public void  Close()
		{
			// DecRef old files from the last checkpoint, if any:
			int size = lastFiles.Count;
			if (size > 0)
			{
				for (int i = 0; i < size; i++)
					DecRef(lastFiles[i]);
				lastFiles.Clear();
			}
			
			DeletePendingFiles();
		}
		
		private void  DeletePendingFiles()
		{
			if (deletable != null)
			{
				System.Collections.Generic.IList<string> oldDeletable = deletable;
				deletable = null;
				int size = oldDeletable.Count;
				for (int i = 0; i < size; i++)
				{
					if (infoStream != null)
					{
						Message("delete pending file " + oldDeletable[i]);
					}
					DeleteFile(oldDeletable[i]);
				}
			}
		}
		
		/// <summary> For definition of "check point" see IndexWriter comments:
		/// "Clarification: Check Points (and commits)".
		/// 
		/// Writer calls this when it has made a "consistent
		/// change" to the index, meaning new files are written to
		/// the index and the in-memory SegmentInfos have been
		/// modified to point to those files.
		/// 
		/// This may or may not be a commit (segments_N may or may
		/// not have been written).
		/// 
		/// We simply incref the files referenced by the new
		/// SegmentInfos and decref the files we had previously
		/// seen (if any).
		/// 
		/// If this is a commit, we also call the policy to give it
		/// a chance to remove other commits.  If any commits are
		/// removed, we decref their files as well.
		/// </summary>
		public void  Checkpoint(SegmentInfos segmentInfos, bool isCommit)
		{
			
			if (infoStream != null)
			{
				Message("now checkpoint \"" + segmentInfos.GetCurrentSegmentFileName() + "\" [" + segmentInfos.Count + " segments " + "; isCommit = " + isCommit + "]");
			}
			
			// Try again now to delete any previously un-deletable
			// files (because they were in use, on Windows):
			DeletePendingFiles();
			
			// Incref the files:
			IncRef(segmentInfos, isCommit);
			
			if (isCommit)
			{
				// Append to our commits list:
				commits.Add(new CommitPoint(this, commitsToDelete, directory, segmentInfos));
				
				// Tell policy so it can remove commits:
				policy.OnCommit(commits);
				
				// Decref files for commits that were deleted by the policy:
				DeleteCommits();
			}
			else
			{
				
				System.Collections.Generic.IList<string> docWriterFiles;
				if (docWriter != null)
				{
					docWriterFiles = docWriter.OpenFiles();
					if (docWriterFiles != null)
					// We must incRef these files before decRef'ing
					// last files to make sure we don't accidentally
					// delete them:
						IncRef(docWriterFiles);
				}
				else
					docWriterFiles = null;
				
				// DecRef old files from the last checkpoint, if any:
				int size = lastFiles.Count;
				if (size > 0)
				{
					for (int i = 0; i < size; i++)
						DecRef(lastFiles[i]);
					lastFiles.Clear();
				}
				
				// Save files so we can decr on next checkpoint/commit:
                foreach (string fname in segmentInfos.Files(directory, false))
                {
                    lastFiles.Add(fname);
                }
				
                if (docWriterFiles != null)
                {
                    foreach (string fname in docWriterFiles)
                    {
                        lastFiles.Add(fname);
                    }
                }
			}
		}
		
		internal void  IncRef(SegmentInfos segmentInfos, bool isCommit)
		{
			// If this is a commit point, also incRef the
			// segments_N file:
			System.Collections.Generic.IEnumerator<string> it = segmentInfos.Files(directory, isCommit).GetEnumerator();
			while (it.MoveNext())
			{
				IncRef(it.Current);
			}
		}
		
		internal void  IncRef(System.Collections.Generic.IList<string> files)
		{
			int size = files.Count;
			for (int i = 0; i < size; i++)
			{
				IncRef((System.String) files[i]);
			}
		}
		
		internal void  IncRef(System.String fileName)
		{
			RefCount rc = GetRefCount(fileName);
			if (infoStream != null && VERBOSE_REF_COUNTS)
			{
				Message("  IncRef \"" + fileName + "\": pre-incr count is " + rc.count);
			}
			rc.IncRef();
		}
		
		internal void  DecRef(System.Collections.Generic.ICollection<string> files)
		{
            System.Collections.Generic.IEnumerator<string> it = files.GetEnumerator();
            while (it.MoveNext())
            {
                DecRef(it.Current);
            }
		}
		
		internal void  DecRef(System.String fileName)
		{
			RefCount rc = GetRefCount(fileName);
			if (infoStream != null && VERBOSE_REF_COUNTS)
			{
				Message("  DecRef \"" + fileName + "\": pre-decr count is " + rc.count);
			}
			if (0 == rc.DecRef())
			{
				// This file is no longer referenced by any past
				// commit points nor by the in-memory SegmentInfos:
				DeleteFile(fileName);
				refCounts.Remove(fileName);

                if (synced != null) {
                    lock(synced) 
                    {
                      synced.Remove(fileName);
                    }
                }
			}
		}
		
		internal void  DecRef(SegmentInfos segmentInfos)
		{
			System.Collections.Generic.IEnumerator<string> it = segmentInfos.Files(directory, false).GetEnumerator();
			while (it.MoveNext())
			{
				DecRef(it.Current);
			}
		}

        public bool Exists(String fileName)
        {
            if (!refCounts.ContainsKey(fileName))
            {
                return false;
            }
            else
            {
                return GetRefCount(fileName).count > 0;
            }
        }
		
		private RefCount GetRefCount(System.String fileName)
		{
			RefCount rc;
			if (!refCounts.ContainsKey(fileName))
			{
				rc = new RefCount(fileName);
				refCounts[fileName] = rc;
			}
			else
			{
				rc = (RefCount) refCounts[fileName];
			}
			return rc;
		}
		
		internal void  DeleteFiles(System.Collections.IList files)
		{
			int size = files.Count;
			for (int i = 0; i < size; i++)
				DeleteFile((System.String) files[i]);
		}
		
		/// <summary>Deletes the specified files, but only if they are new
		/// (have not yet been incref'd). 
		/// </summary>
        internal void DeleteNewFiles(System.Collections.Generic.ICollection<string> files)
		{
			System.Collections.IEnumerator it = files.GetEnumerator();
			while (it.MoveNext())
			{
				System.String fileName = (System.String) it.Current;
                if (!refCounts.ContainsKey(fileName))
                {
                    if (infoStream != null)
                    {
                        Message("delete new file \"" + fileName + "\"");
                    }
                    DeleteFile(fileName);
                }
			}
		}
		
		internal void  DeleteFile(System.String fileName)
		{
			try
			{
				if (infoStream != null)
				{
					Message("delete \"" + fileName + "\"");
				}
				directory.DeleteFile(fileName);
			}
			catch (System.IO.IOException e)
			{
				// if delete fails
				if (directory.FileExists(fileName))
				{
					
					// Some operating systems (e.g. Windows) don't
					// permit a file to be deleted while it is opened
					// for read (e.g. by another process or thread). So
					// we assume that when a delete fails it is because
					// the file is open in another process, and queue
					// the file for subsequent deletion.
					
					if (infoStream != null)
					{
						Message("IndexFileDeleter: unable to remove file \"" + fileName + "\": " + e.ToString() + "; Will re-try later.");
					}
					if (deletable == null)
					{
                        deletable = new System.Collections.Generic.List<string>();
					}
					deletable.Add(fileName); // add to deletable
				}
			}
		}
		
		/// <summary> Tracks the reference count for a single index file:</summary>
		sealed private class RefCount
		{
			
			// fileName used only for better assert error messages
			internal System.String fileName;
			internal bool initDone;
			internal RefCount(System.String fileName)
			{
				this.fileName = fileName;
			}
			
			internal int count;
			
			public int IncRef()
			{
				if (!initDone)
				{
					initDone = true;
				}
				else
				{
					System.Diagnostics.Debug.Assert(count > 0, "RefCount is 0 pre-increment for file " + fileName);
				}
				return ++count;
			}
			
			public int DecRef()
			{
				System.Diagnostics.Debug.Assert(count > 0, "RefCount is 0 pre-decrement for file " + fileName);
				return --count;
			}
		}
		
		/// <summary> Holds details for each commit point.  This class is
		/// also passed to the deletion policy.  Note: this class
		/// has a natural ordering that is inconsistent with
		/// equals.
		/// </summary>
		
		sealed private class CommitPoint:IndexCommit, System.IComparable
		{
            private void InitBlock(IndexFileDeleter enclosingInstance)
            {
                this.enclosingInstance = enclosingInstance;
            }
            private IndexFileDeleter enclosingInstance;
            public IndexFileDeleter Enclosing_Instance
            {
                get
                {
                    return enclosingInstance;
                }

            }
			
			internal long gen;
            internal System.Collections.Generic.ICollection<string> files;
			internal System.String segmentsFileName;
			internal bool deleted;
			internal Directory directory;
			internal System.Collections.ICollection commitsToDelete;
			internal long version;
			internal long generation;
			internal bool isOptimized;
            internal System.Collections.Generic.IDictionary<string, string> userData;
			
			public CommitPoint(IndexFileDeleter enclosingInstance, System.Collections.ICollection commitsToDelete, Directory directory, SegmentInfos segmentInfos)
			{
				InitBlock(enclosingInstance);
				this.directory = directory;
				this.commitsToDelete = commitsToDelete;
				userData = segmentInfos.GetUserData();
				segmentsFileName = segmentInfos.GetCurrentSegmentFileName();
				version = segmentInfos.GetVersion();
				generation = segmentInfos.GetGeneration();
                files = segmentInfos.Files(directory, true);
				gen = segmentInfos.GetGeneration();
				isOptimized = segmentInfos.Count == 1 && !segmentInfos.Info(0).HasDeletions();
				
				System.Diagnostics.Debug.Assert(!segmentInfos.HasExternalSegments(directory));
			}

            public override string ToString()
            {
                return "IndexFileDeleter.CommitPoint(" + segmentsFileName + ")";
            }

			public override bool IsOptimized()
			{
				return isOptimized;
			}
			
			public override System.String GetSegmentsFileName()
			{
				return segmentsFileName;
			}

            public override System.Collections.Generic.ICollection<string> GetFileNames()
			{
				return files;
			}
			
			public override Directory GetDirectory()
			{
				return directory;
			}
			
			public override long GetVersion()
			{
				return version;
			}
			
			public override long GetGeneration()
			{
				return generation;
			}

            public override System.Collections.Generic.IDictionary<string, string> GetUserData()
			{
				return userData;
			}
			
			/// <summary> Called only be the deletion policy, to remove this
			/// commit point from the index.
			/// </summary>
			public override void  Delete()
			{
				if (!deleted)
				{
					deleted = true;
					Enclosing_Instance.commitsToDelete.Add(this);
				}
			}
			
			public override bool IsDeleted()
			{
				return deleted;
			}
			
			public int CompareTo(System.Object obj)
			{
				CommitPoint commit = (CommitPoint) obj;
				if (gen < commit.gen)
				{
					return - 1;
				}
				else if (gen > commit.gen)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}
	}
}
