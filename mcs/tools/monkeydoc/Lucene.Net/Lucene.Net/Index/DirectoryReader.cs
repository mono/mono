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
using AlreadyClosedException = Mono.Lucene.Net.Store.AlreadyClosedException;
using Directory = Mono.Lucene.Net.Store.Directory;
using Lock = Mono.Lucene.Net.Store.Lock;
using LockObtainFailedException = Mono.Lucene.Net.Store.LockObtainFailedException;
using DefaultSimilarity = Mono.Lucene.Net.Search.DefaultSimilarity;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> An IndexReader which reads indexes with multiple segments.</summary>
	public class DirectoryReader:IndexReader, System.ICloneable
	{
		/*new*/ private class AnonymousClassFindSegmentsFile:SegmentInfos.FindSegmentsFile
		{
			private void  InitBlock(bool readOnly, Mono.Lucene.Net.Index.IndexDeletionPolicy deletionPolicy, int termInfosIndexDivisor)
			{
				this.readOnly = readOnly;
				this.deletionPolicy = deletionPolicy;
				this.termInfosIndexDivisor = termInfosIndexDivisor;
			}
			private bool readOnly;
			private Mono.Lucene.Net.Index.IndexDeletionPolicy deletionPolicy;
			private int termInfosIndexDivisor;
			internal AnonymousClassFindSegmentsFile(bool readOnly, Mono.Lucene.Net.Index.IndexDeletionPolicy deletionPolicy, int termInfosIndexDivisor, Mono.Lucene.Net.Store.Directory Param1):base(Param1)
			{
				InitBlock(readOnly, deletionPolicy, termInfosIndexDivisor);
			}
			public /*protected internal*/ override System.Object DoBody(System.String segmentFileName)
			{
				SegmentInfos infos = new SegmentInfos();
				infos.Read(directory, segmentFileName);
				if (readOnly)
					return new ReadOnlyDirectoryReader(directory, infos, deletionPolicy, termInfosIndexDivisor);
				else
					return new DirectoryReader(directory, infos, deletionPolicy, false, termInfosIndexDivisor);
			}
		}
		private class AnonymousClassFindSegmentsFile1:SegmentInfos.FindSegmentsFile
		{
			private void  InitBlock(bool openReadOnly, DirectoryReader enclosingInstance)
			{
				this.openReadOnly = openReadOnly;
				this.enclosingInstance = enclosingInstance;
			}
			private bool openReadOnly;
			private DirectoryReader enclosingInstance;
			public DirectoryReader Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal AnonymousClassFindSegmentsFile1(bool openReadOnly, DirectoryReader enclosingInstance, Mono.Lucene.Net.Store.Directory Param1):base(Param1)
			{
				InitBlock(openReadOnly, enclosingInstance);
			}
			public /*protected internal*/ override System.Object DoBody(System.String segmentFileName)
			{
				SegmentInfos infos = new SegmentInfos();
				infos.Read(directory, segmentFileName);
				return Enclosing_Instance.DoReopen(infos, false, openReadOnly);
			}
		}
		protected internal Directory directory;
		protected internal bool readOnly;
		
		internal IndexWriter writer;
		
		private IndexDeletionPolicy deletionPolicy;
        private System.Collections.Generic.Dictionary<string, string> synced = new System.Collections.Generic.Dictionary<string, string>();
		private Lock writeLock;
		private SegmentInfos segmentInfos;
		private SegmentInfos segmentInfosStart;
		private bool stale;
		private int termInfosIndexDivisor;
		
		private bool rollbackHasChanges;
				
		private SegmentReader[] subReaders;
		private int[] starts; // 1st docno for each segment
		private System.Collections.IDictionary normsCache = new System.Collections.Hashtable();
		private int maxDoc = 0;
		private int numDocs = - 1;
		private bool hasDeletions = false;
        
        // Max version in index as of when we opened; this can be
        // > our current segmentInfos version in case we were
        // opened on a past IndexCommit:
        private long maxIndexVersion;
		
		internal static IndexReader Open(Directory directory, IndexDeletionPolicy deletionPolicy, IndexCommit commit, bool readOnly, int termInfosIndexDivisor)
		{
			return (IndexReader) new AnonymousClassFindSegmentsFile(readOnly, deletionPolicy, termInfosIndexDivisor, directory).Run(commit);
		}
		
		/// <summary>Construct reading the named set of readers. </summary>
		internal DirectoryReader(Directory directory, SegmentInfos sis, IndexDeletionPolicy deletionPolicy, bool readOnly, int termInfosIndexDivisor)
		{
			this.directory = directory;
			this.readOnly = readOnly;
			this.segmentInfos = sis;
			this.deletionPolicy = deletionPolicy;
			this.termInfosIndexDivisor = termInfosIndexDivisor;
			
			if (!readOnly)
			{
				// We assume that this segments_N was previously
				// properly sync'd:
				SupportClass.CollectionsHelper.AddAllIfNotContains(synced, sis.Files(directory, true));
			}
			
			// To reduce the chance of hitting FileNotFound
			// (and having to retry), we open segments in
			// reverse because IndexWriter merges & deletes
			// the newest segments first.
			
			SegmentReader[] readers = new SegmentReader[sis.Count];
			for (int i = sis.Count - 1; i >= 0; i--)
			{
				bool success = false;
				try
				{
					readers[i] = SegmentReader.Get(readOnly, sis.Info(i), termInfosIndexDivisor);
					success = true;
				}
				finally
				{
					if (!success)
					{
						// Close all readers we had opened:
						for (i++; i < sis.Count; i++)
						{
							try
							{
								readers[i].Close();
							}
							catch (System.Exception ignore)
							{
								// keep going - we want to clean up as much as possible
							}
						}
					}
				}
			}
			
			Initialize(readers);
		}
		
		// Used by near real-time search
		internal DirectoryReader(IndexWriter writer, SegmentInfos infos, int termInfosIndexDivisor)
		{
			this.directory = writer.GetDirectory();
			this.readOnly = true;
			segmentInfos = infos;
			segmentInfosStart = (SegmentInfos) infos.Clone();
			this.termInfosIndexDivisor = termInfosIndexDivisor;
			if (!readOnly)
			{
				// We assume that this segments_N was previously
				// properly sync'd:
				SupportClass.CollectionsHelper.AddAllIfNotContains(synced, infos.Files(directory, true));
			}
			
			// IndexWriter synchronizes externally before calling
			// us, which ensures infos will not change; so there's
			// no need to process segments in reverse order
			int numSegments = infos.Count;
			SegmentReader[] readers = new SegmentReader[numSegments];
			Directory dir = writer.GetDirectory();
			int upto = 0;
			
			for (int i = 0; i < numSegments; i++)
			{
				bool success = false;
				try
				{
					SegmentInfo info = infos.Info(i);
					if (info.dir == dir)
					{
						readers[upto++] = writer.readerPool.GetReadOnlyClone(info, true, termInfosIndexDivisor);
					}
					success = true;
				}
				finally
				{
					if (!success)
					{
						// Close all readers we had opened:
						for (upto--; upto >= 0; upto--)
						{
							try
							{
								readers[upto].Close();
							}
							catch (System.Exception ignore)
							{
								// keep going - we want to clean up as much as possible
							}
						}
					}
				}
			}
			
			this.writer = writer;
			
			if (upto < readers.Length)
			{
				// This means some segments were in a foreign Directory
				SegmentReader[] newReaders = new SegmentReader[upto];
				Array.Copy(readers, 0, newReaders, 0, upto);
				readers = newReaders;
			}
			
			Initialize(readers);
		}
		
		/// <summary>This constructor is only used for {@link #Reopen()} </summary>
		internal DirectoryReader(Directory directory, SegmentInfos infos, SegmentReader[] oldReaders, int[] oldStarts, System.Collections.IDictionary oldNormsCache, bool readOnly, bool doClone, int termInfosIndexDivisor)
		{
			this.directory = directory;
			this.readOnly = readOnly;
			this.segmentInfos = infos;
			this.termInfosIndexDivisor = termInfosIndexDivisor;
			if (!readOnly)
			{
				// We assume that this segments_N was previously
				// properly sync'd:
				SupportClass.CollectionsHelper.AddAllIfNotContains(synced, infos.Files(directory, true));
			}
			
			// we put the old SegmentReaders in a map, that allows us
			// to lookup a reader using its segment name
			System.Collections.IDictionary segmentReaders = new System.Collections.Hashtable();
			
			if (oldReaders != null)
			{
				// create a Map SegmentName->SegmentReader
				for (int i = 0; i < oldReaders.Length; i++)
				{
					segmentReaders[oldReaders[i].GetSegmentName()] = (System.Int32) i;
				}
			}
			
			SegmentReader[] newReaders = new SegmentReader[infos.Count];
			
			// remember which readers are shared between the old and the re-opened
			// DirectoryReader - we have to incRef those readers
			bool[] readerShared = new bool[infos.Count];
			
			for (int i = infos.Count - 1; i >= 0; i--)
			{
				// find SegmentReader for this segment
                int? oldReaderIndex = (int?)segmentReaders[infos.Info(i).name];
                if (oldReaderIndex.HasValue == false)
                {
                    // this is a new segment, no old SegmentReader can be reused
                    newReaders[i] = null;
                }
                else
                {
                    // there is an old reader for this segment - we'll try to reopen it
                    newReaders[i] = oldReaders[oldReaderIndex.Value];
                }
				
				bool success = false;
				try
				{
					SegmentReader newReader;
					if (newReaders[i] == null || infos.Info(i).GetUseCompoundFile() != newReaders[i].GetSegmentInfo().GetUseCompoundFile())
					{
						
						// We should never see a totally new segment during cloning
						System.Diagnostics.Debug.Assert(!doClone);
						
						// this is a new reader; in case we hit an exception we can close it safely
						newReader = SegmentReader.Get(readOnly, infos.Info(i), termInfosIndexDivisor);
					}
					else
					{
						newReader = newReaders[i].ReopenSegment(infos.Info(i), doClone, readOnly);
					}
					if (newReader == newReaders[i])
					{
						// this reader will be shared between the old and the new one,
						// so we must incRef it
						readerShared[i] = true;
						newReader.IncRef();
					}
					else
					{
						readerShared[i] = false;
						newReaders[i] = newReader;
					}
					success = true;
				}
				finally
				{
					if (!success)
					{
						for (i++; i < infos.Count; i++)
						{
							if (newReaders[i] != null)
							{
								try
								{
									if (!readerShared[i])
									{
										// this is a new subReader that is not used by the old one,
										// we can close it
										newReaders[i].Close();
									}
									else
									{
										// this subReader is also used by the old reader, so instead
										// closing we must decRef it
										newReaders[i].DecRef();
									}
								}
								catch (System.IO.IOException ignore)
								{
									// keep going - we want to clean up as much as possible
								}
							}
						}
					}
				}
			}
			
			// initialize the readers to calculate maxDoc before we try to reuse the old normsCache
			Initialize(newReaders);
			
			// try to copy unchanged norms from the old normsCache to the new one
			if (oldNormsCache != null)
			{
				System.Collections.IEnumerator it = new System.Collections.Hashtable(oldNormsCache).GetEnumerator();
				while (it.MoveNext())
				{
					System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) it.Current;
					System.String field = (System.String) entry.Key;
					if (!HasNorms(field))
					{
						continue;
					}
					
					byte[] oldBytes = (byte[]) entry.Value;
					
					byte[] bytes = new byte[MaxDoc()];
					
					for (int i = 0; i < subReaders.Length; i++)
					{
                        int? oldReaderIndex = (int?)segmentReaders[subReaders[i].GetSegmentName()];

                        // this SegmentReader was not re-opened, we can copy all of its norms 
                        if (oldReaderIndex.HasValue &&
                             (oldReaders[oldReaderIndex.Value] == subReaders[i]
                               || oldReaders[oldReaderIndex.Value].norms[field] == subReaders[i].norms[field]))
                        {
                            // we don't have to synchronize here: either this constructor is called from a SegmentReader,
                            // in which case no old norms cache is present, or it is called from MultiReader.reopen(),
                            // which is synchronized
                            Array.Copy(oldBytes, oldStarts[oldReaderIndex.Value], bytes, starts[i], starts[i + 1] - starts[i]);
                        }
                        else
                        {
                            subReaders[i].Norms(field, bytes, starts[i]);
                        }
					}
					
					normsCache[field] = bytes; // update cache
				}
			}
		}
		
		private void  Initialize(SegmentReader[] subReaders)
		{
			this.subReaders = subReaders;
			starts = new int[subReaders.Length + 1]; // build starts array
			for (int i = 0; i < subReaders.Length; i++)
			{
				starts[i] = maxDoc;
				maxDoc += subReaders[i].MaxDoc(); // compute maxDocs
				
				if (subReaders[i].HasDeletions())
					hasDeletions = true;
			}
			starts[subReaders.Length] = maxDoc;

            if (!readOnly)
            {
                maxIndexVersion = SegmentInfos.ReadCurrentVersion(directory);
            }
		}
		
		public override System.Object Clone()
		{
            lock (this)
            {
                try
                {
                    return Clone(readOnly); // Preserve current readOnly
                }
                catch (System.Exception ex)
                {
                    throw new System.SystemException(ex.Message, ex);
                }
            }
		}
		
		public override IndexReader Clone(bool openReadOnly)
		{
			lock (this)
			{
				DirectoryReader newReader = DoReopen((SegmentInfos) segmentInfos.Clone(), true, openReadOnly);
				
				if (this != newReader)
				{
					newReader.deletionPolicy = deletionPolicy;
				}
				newReader.writer = writer;
				// If we're cloning a non-readOnly reader, move the
				// writeLock (if there is one) to the new reader:
				if (!openReadOnly && writeLock != null)
				{
					// In near real-time search, reader is always readonly
					System.Diagnostics.Debug.Assert(writer == null);
					newReader.writeLock = writeLock;
					newReader.hasChanges = hasChanges;
					newReader.hasDeletions = hasDeletions;
					writeLock = null;
					hasChanges = false;
				}
				
				return newReader;
			}
		}
		
		public override IndexReader Reopen()
		{
	        // Preserve current readOnly
			return DoReopen(readOnly, null);
		}
		
		public override IndexReader Reopen(bool openReadOnly)
		{
			return DoReopen(openReadOnly, null);
		}
		
		public override IndexReader Reopen(IndexCommit commit)
		{
			return DoReopen(true, commit);
		}

        private IndexReader DoReopenFromWriter(bool openReadOnly, IndexCommit commit)
        {
            System.Diagnostics.Debug.Assert(readOnly);

            if (!openReadOnly)
            {
                throw new System.ArgumentException("a reader obtained from IndexWriter.getReader() can only be reopened with openReadOnly=true (got false)");
            }

            if (commit != null)
            {
                throw new System.ArgumentException("a reader obtained from IndexWriter.getReader() cannot currently accept a commit");
            }

            // TODO: right now we *always* make a new reader; in
            // the future we could have write make some effort to
            // detect that no changes have occurred
            return writer.GetReader();
        }

        internal virtual IndexReader DoReopen(bool openReadOnly, IndexCommit commit)
        {
            EnsureOpen();

            System.Diagnostics.Debug.Assert(commit == null || openReadOnly);

            // If we were obtained by writer.getReader(), re-ask the
            // writer to get a new reader.
            if (writer != null)
            {
                return DoReopenFromWriter(openReadOnly, commit);
            }
            else
            {
                return DoReopenNoWriter(openReadOnly, commit);
            }
        }
                
        private IndexReader DoReopenNoWriter(bool openReadOnly, IndexCommit commit)
        {
            lock (this)
            {
                if (commit == null)
                {
                    if (hasChanges)
                    {
                        // We have changes, which means we are not readOnly:
                        System.Diagnostics.Debug.Assert(readOnly == false);
                        // and we hold the write lock:
                        System.Diagnostics.Debug.Assert(writeLock != null);
                        // so no other writer holds the write lock, which
                        // means no changes could have been done to the index:
                        System.Diagnostics.Debug.Assert(IsCurrent());

                        if (openReadOnly)
                        {
                            return (IndexReader)Clone(openReadOnly);
                        }
                        else
                        {
                            return this;
                        }
                    }
                    else if (IsCurrent())
                    {
                        if (openReadOnly != readOnly)
                        {
                            // Just fallback to clone
                            return (IndexReader)Clone(openReadOnly);
                        }
                        else
                        {
                            return this;
                        }
                    }
                }
                else
                {
                    if (directory != commit.GetDirectory())
                        throw new System.IO.IOException("the specified commit does not match the specified Directory");
                    if (segmentInfos != null && commit.GetSegmentsFileName().Equals(segmentInfos.GetCurrentSegmentFileName()))
                    {
                        if (readOnly != openReadOnly)
                        {
                            // Just fallback to clone
                            return (IndexReader)Clone(openReadOnly);
                        }
                        else
                        {
                            return this;
                        }
                    }
                }

                return (IndexReader)new AnonymousFindSegmentsFile(directory, openReadOnly, this).Run(commit);
            }
        }

        class AnonymousFindSegmentsFile : SegmentInfos.FindSegmentsFile
        {
            DirectoryReader enclosingInstance;
            bool openReadOnly;
            Directory dir;
            public AnonymousFindSegmentsFile(Directory directory, bool openReadOnly, DirectoryReader dirReader) : base(directory)
            {
                this.dir = directory;
                this.openReadOnly = openReadOnly;
                enclosingInstance = dirReader;
            }

            public override object DoBody(string segmentFileName)
            {
                SegmentInfos infos = new SegmentInfos();
                infos.Read(this.dir, segmentFileName);
                return enclosingInstance.DoReopen(infos, false, openReadOnly);
            }
        }

        private DirectoryReader DoReopen(SegmentInfos infos, bool doClone, bool openReadOnly)
        {
            lock (this)
            {
                DirectoryReader reader;
                if (openReadOnly)
                {
                    reader = new ReadOnlyDirectoryReader(directory, infos, subReaders, starts, normsCache, doClone, termInfosIndexDivisor);
                }
                else
                {
                    reader = new DirectoryReader(directory, infos, subReaders, starts, normsCache, false, doClone, termInfosIndexDivisor);
                }
                reader.SetDisableFakeNorms(GetDisableFakeNorms());
                return reader;
            }
        }


		
		/// <summary>Version number when this IndexReader was opened. </summary>
		public override long GetVersion()
		{
			EnsureOpen();
			return segmentInfos.GetVersion();
		}
		
		public override TermFreqVector[] GetTermFreqVectors(int n)
		{
			EnsureOpen();
			int i = ReaderIndex(n); // find segment num
			return subReaders[i].GetTermFreqVectors(n - starts[i]); // dispatch to segment
		}
		
		public override TermFreqVector GetTermFreqVector(int n, System.String field)
		{
			EnsureOpen();
			int i = ReaderIndex(n); // find segment num
			return subReaders[i].GetTermFreqVector(n - starts[i], field);
		}
		
		
		public override void  GetTermFreqVector(int docNumber, System.String field, TermVectorMapper mapper)
		{
			EnsureOpen();
			int i = ReaderIndex(docNumber); // find segment num
			subReaders[i].GetTermFreqVector(docNumber - starts[i], field, mapper);
		}
		
		public override void  GetTermFreqVector(int docNumber, TermVectorMapper mapper)
		{
			EnsureOpen();
			int i = ReaderIndex(docNumber); // find segment num
			subReaders[i].GetTermFreqVector(docNumber - starts[i], mapper);
		}
		
		/// <summary> Checks is the index is optimized (if it has a single segment and no deletions)</summary>
		/// <returns> <code>true</code> if the index is optimized; <code>false</code> otherwise
		/// </returns>
		public override bool IsOptimized()
		{
			EnsureOpen();
			return segmentInfos.Count == 1 && !HasDeletions();
		}
		
		public override int NumDocs()
		{
			// Don't call ensureOpen() here (it could affect performance)
            // NOTE: multiple threads may wind up init'ing
            // numDocs... but that's harmless
			if (numDocs == - 1)
			{
				// check cache
				int n = 0; // cache miss--recompute
				for (int i = 0; i < subReaders.Length; i++)
					n += subReaders[i].NumDocs(); // sum from readers
				numDocs = n;
			}
			return numDocs;
		}
		
		public override int MaxDoc()
		{
			// Don't call ensureOpen() here (it could affect performance)
			return maxDoc;
		}
		
		// inherit javadoc
		public override Document Document(int n, FieldSelector fieldSelector)
		{
			EnsureOpen();
			int i = ReaderIndex(n); // find segment num
			return subReaders[i].Document(n - starts[i], fieldSelector); // dispatch to segment reader
		}
		
		public override bool IsDeleted(int n)
		{
			// Don't call ensureOpen() here (it could affect performance)
			int i = ReaderIndex(n); // find segment num
			return subReaders[i].IsDeleted(n - starts[i]); // dispatch to segment reader
		}
		
		public override bool HasDeletions()
		{
			// Don't call ensureOpen() here (it could affect performance)
			return hasDeletions;
		}
		
		protected internal override void  DoDelete(int n)
		{
			numDocs = - 1; // invalidate cache
			int i = ReaderIndex(n); // find segment num
			subReaders[i].DeleteDocument(n - starts[i]); // dispatch to segment reader
			hasDeletions = true;
		}
		
		protected internal override void  DoUndeleteAll()
		{
			for (int i = 0; i < subReaders.Length; i++)
				subReaders[i].UndeleteAll();
			
			hasDeletions = false;
			numDocs = - 1; // invalidate cache
		}
		
		private int ReaderIndex(int n)
		{
			// find reader for doc n:
			return ReaderIndex(n, this.starts, this.subReaders.Length);
		}
		
		internal static int ReaderIndex(int n, int[] starts, int numSubReaders)
		{
			// find reader for doc n:
			int lo = 0; // search starts array
			int hi = numSubReaders - 1; // for first element less
			
			while (hi >= lo)
			{
				int mid = SupportClass.Number.URShift((lo + hi), 1);
				int midValue = starts[mid];
				if (n < midValue)
					hi = mid - 1;
				else if (n > midValue)
					lo = mid + 1;
				else
				{
					// found a match
					while (mid + 1 < numSubReaders && starts[mid + 1] == midValue)
					{
						mid++; // scan to last match
					}
					return mid;
				}
			}
			return hi;
		}
		
		public override bool HasNorms(System.String field)
		{
			EnsureOpen();
			for (int i = 0; i < subReaders.Length; i++)
			{
				if (subReaders[i].HasNorms(field))
					return true;
			}
			return false;
		}
		
		private byte[] ones;
		private byte[] FakeNorms()
		{
			if (ones == null)
				ones = SegmentReader.CreateFakeNorms(MaxDoc());
			return ones;
		}
		
		public override byte[] Norms(System.String field)
		{
			lock (this)
			{
				EnsureOpen();
				byte[] bytes = (byte[]) normsCache[field];
				if (bytes != null)
					return bytes; // cache hit
				if (!HasNorms(field))
					return GetDisableFakeNorms()?null:FakeNorms();
				
				bytes = new byte[MaxDoc()];
				for (int i = 0; i < subReaders.Length; i++)
					subReaders[i].Norms(field, bytes, starts[i]);
				normsCache[field] = bytes; // update cache
				return bytes;
			}
		}
		
		public override void  Norms(System.String field, byte[] result, int offset)
		{
			lock (this)
			{
				EnsureOpen();
				byte[] bytes = (byte[]) normsCache[field];
				if (bytes == null && !HasNorms(field))
				{
                    byte val = DefaultSimilarity.EncodeNorm(1.0f);
			        for (int index = offset; index < result.Length; index++)
				        result.SetValue(val, index);
				}
				else if (bytes != null)
				{
					// cache hit
					Array.Copy(bytes, 0, result, offset, MaxDoc());
				}
				else
				{
					for (int i = 0; i < subReaders.Length; i++)
					{
						// read from segments
						subReaders[i].Norms(field, result, offset + starts[i]);
					}
				}
			}
		}
		
		protected internal override void  DoSetNorm(int n, System.String field, byte value_Renamed)
		{
			lock (normsCache.SyncRoot)
			{
				normsCache.Remove(field); // clear cache      
			}
			int i = ReaderIndex(n); // find segment num
			subReaders[i].SetNorm(n - starts[i], field, value_Renamed); // dispatch
		}
		
		public override TermEnum Terms()
		{
			EnsureOpen();
			return new MultiTermEnum(this, subReaders, starts, null);
		}
		
		public override TermEnum Terms(Term term)
		{
			EnsureOpen();
			return new MultiTermEnum(this, subReaders, starts, term);
		}
		
		public override int DocFreq(Term t)
		{
			EnsureOpen();
			int total = 0; // sum freqs in segments
			for (int i = 0; i < subReaders.Length; i++)
				total += subReaders[i].DocFreq(t);
			return total;
		}
		
		public override TermDocs TermDocs()
		{
			EnsureOpen();
			return new MultiTermDocs(this, subReaders, starts);
		}
		
		public override TermPositions TermPositions()
		{
			EnsureOpen();
			return new MultiTermPositions(this, subReaders, starts);
		}
		
		/// <summary> Tries to acquire the WriteLock on this directory. this method is only valid if this IndexReader is directory
		/// owner.
		/// 
		/// </summary>
		/// <throws>  StaleReaderException  if the index has changed since this reader was opened </throws>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  Mono.Lucene.Net.Store.LockObtainFailedException </throws>
		/// <summary>                               if another writer has this index open (<code>write.lock</code> could not be
		/// obtained)
		/// </summary>
		/// <throws>  IOException           if there is a low-level IO error </throws>
		protected internal override void  AcquireWriteLock()
		{
			
			if (readOnly)
			{
				// NOTE: we should not reach this code w/ the core
				// IndexReader classes; however, an external subclass
				// of IndexReader could reach this.
				ReadOnlySegmentReader.NoWrite();
			}
			
			if (segmentInfos != null)
			{
				EnsureOpen();
				if (stale)
					throw new StaleReaderException("IndexReader out of date and no longer valid for delete, undelete, or setNorm operations");
				
				if (this.writeLock == null)
				{
					Lock writeLock = directory.MakeLock(IndexWriter.WRITE_LOCK_NAME);
					if (!writeLock.Obtain(IndexWriter.WRITE_LOCK_TIMEOUT))
					// obtain write lock
					{
						throw new LockObtainFailedException("Index locked for write: " + writeLock);
					}
					this.writeLock = writeLock;
					
                    // we have to check whether index has changed since this reader was opened.
                    // if so, this reader is no longer valid for
                    // deletion
                    if (SegmentInfos.ReadCurrentVersion(directory) > maxIndexVersion)
					{
						stale = true;
						this.writeLock.Release();
						this.writeLock = null;
						throw new StaleReaderException("IndexReader out of date and no longer valid for delete, undelete, or setNorm operations");
					}
				}
			}
		}
		
		/// <deprecated>  
		/// </deprecated>
        [Obsolete]
		protected internal override void  DoCommit()
		{
			DoCommit(null);
		}
		
		/// <summary> Commit changes resulting from delete, undeleteAll, or setNorm operations
		/// <p/>
		/// If an exception is hit, then either no changes or all changes will have been committed to the index (transactional
		/// semantics).
		/// 
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
        protected internal override void DoCommit(System.Collections.Generic.IDictionary<string, string> commitUserData)
		{
			if (hasChanges)
			{
				segmentInfos.SetUserData(commitUserData);
				// Default deleter (for backwards compatibility) is
				// KeepOnlyLastCommitDeleter:
				IndexFileDeleter deleter = new IndexFileDeleter(directory, deletionPolicy == null?new KeepOnlyLastCommitDeletionPolicy():deletionPolicy, segmentInfos, null, null, synced);

                segmentInfos.UpdateGeneration(deleter.GetLastSegmentInfos());

				// Checkpoint the state we are about to change, in
				// case we have to roll back:
				StartCommit();
				
				bool success = false;
				try
				{
					for (int i = 0; i < subReaders.Length; i++)
						subReaders[i].Commit();

                    // Sync all files we just wrote
                    foreach(string fileName in segmentInfos.Files(directory, false))
                    {
                        if(!synced.ContainsKey(fileName))
                        {
                            System.Diagnostics.Debug.Assert(directory.FileExists(fileName));
							directory.Sync(fileName);
                            synced[fileName]=fileName;
                        }   
                    }
					
					segmentInfos.Commit(directory);
					success = true;
				}
				finally
				{
					
					if (!success)
					{
						
						// Rollback changes that were made to
						// SegmentInfos but failed to get [fully]
						// committed.  This way this reader instance
						// remains consistent (matched to what's
						// actually in the index):
						RollbackCommit();
						
						// Recompute deletable files & remove them (so
						// partially written .del files, etc, are
						// removed):
						deleter.Refresh();
					}
				}
				
				// Have the deleter remove any now unreferenced
				// files due to this commit:
				deleter.Checkpoint(segmentInfos, true);
				deleter.Close();

                maxIndexVersion = segmentInfos.GetVersion();
				
				if (writeLock != null)
				{
					writeLock.Release(); // release write lock
					writeLock = null;
				}
			}
			hasChanges = false;
		}
		
		internal virtual void  StartCommit()
		{
			rollbackHasChanges = hasChanges;
			for (int i = 0; i < subReaders.Length; i++)
			{
				subReaders[i].StartCommit();
			}
		}
		
		internal virtual void  RollbackCommit()
		{
            hasChanges = rollbackHasChanges;
            for (int i = 0; i < subReaders.Length; i++)
            {
                subReaders[i].RollbackCommit();
            }
		}

        public override System.Collections.Generic.IDictionary<string, string> GetCommitUserData()
		{
			EnsureOpen();
			return segmentInfos.GetUserData();
		}
		
		public override bool IsCurrent()
		{
			EnsureOpen();
			if (writer == null || writer.IsClosed())
			{
				// we loaded SegmentInfos from the directory
				return SegmentInfos.ReadCurrentVersion(directory) == segmentInfos.GetVersion();
			}
			else
			{
				return writer.NrtIsCurrent(segmentInfosStart);
			}
		}
		
		protected internal override void  DoClose()
		{
			lock (this)
			{
				System.IO.IOException ioe = null;
				normsCache = null;
				for (int i = 0; i < subReaders.Length; i++)
				{
					// try to close each reader, even if an exception is thrown
					try
					{
						subReaders[i].DecRef();
					}
					catch (System.IO.IOException e)
					{
						if (ioe == null)
							ioe = e;
					}
				}

                // NOTE: only needed in case someone had asked for
                // FieldCache for top-level reader (which is generally
                // not a good idea):
                Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.Purge(this);

				// throw the first exception
				if (ioe != null)
					throw ioe;
			}
		}

        public override System.Collections.Generic.ICollection<string> GetFieldNames(IndexReader.FieldOption fieldNames)
		{
			EnsureOpen();
			return GetFieldNames(fieldNames, this.subReaders);
		}

        internal static System.Collections.Generic.ICollection<string> GetFieldNames(IndexReader.FieldOption fieldNames, IndexReader[] subReaders)
		{
			// maintain a unique set of field names
            System.Collections.Generic.Dictionary<string,string> fieldSet = new System.Collections.Generic.Dictionary<string,string>();
			for (int i = 0; i < subReaders.Length; i++)
			{
				IndexReader reader = subReaders[i];
                System.Collections.Generic.ICollection<string> names = reader.GetFieldNames(fieldNames);
				SupportClass.CollectionsHelper.AddAllIfNotContains(fieldSet, names);
			}
			return fieldSet.Keys;
		}
		
		public override IndexReader[] GetSequentialSubReaders()
		{
			return subReaders;
		}

        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Index.IndexReader.SetDisableFakeNorms(bool)")]
		public override void  SetDisableFakeNorms(bool disableFakeNorms)
		{
			base.SetDisableFakeNorms(disableFakeNorms);
			for (int i = 0; i < subReaders.Length; i++)
				subReaders[i].SetDisableFakeNorms(disableFakeNorms);
		}
		
		/// <summary>Returns the directory this index resides in. </summary>
		public override Directory Directory()
		{
			// Don't ensureOpen here -- in certain cases, when a
			// cloned/reopened reader needs to commit, it may call
			// this method on the closed original reader
			return directory;
		}
		
		public override int GetTermInfosIndexDivisor()
		{
			return termInfosIndexDivisor;
		}
		
		/// <summary> Expert: return the IndexCommit that this reader has opened.
		/// <p/>
		/// <p/><b>WARNING</b>: this API is new and experimental and may suddenly change.<p/>
		/// </summary>
		public override IndexCommit GetIndexCommit()
		{
			return new ReaderCommit(segmentInfos, directory);
		}
		
		/// <seealso cref="Mono.Lucene.Net.Index.IndexReader.listCommits">
		/// </seealso>
		public static new System.Collections.ICollection ListCommits(Directory dir)
		{
			System.String[] files = dir.ListAll();
			
			System.Collections.ArrayList commits = new System.Collections.ArrayList();
			
			SegmentInfos latest = new SegmentInfos();
			latest.Read(dir);
			long currentGen = latest.GetGeneration();
			
			commits.Add(new ReaderCommit(latest, dir));
			
			for (int i = 0; i < files.Length; i++)
			{
				
				System.String fileName = files[i];
				
				if (fileName.StartsWith(IndexFileNames.SEGMENTS) && !fileName.Equals(IndexFileNames.SEGMENTS_GEN) && SegmentInfos.GenerationFromSegmentsFileName(fileName) < currentGen)
				{
					
					SegmentInfos sis = new SegmentInfos();
					try
					{
						// IOException allowed to throw there, in case
						// segments_N is corrupt
						sis.Read(dir, fileName);
					}
					catch (System.IO.FileNotFoundException fnfe)
					{
						// LUCENE-948: on NFS (and maybe others), if
						// you have writers switching back and forth
						// between machines, it's very likely that the
						// dir listing will be stale and will claim a
						// file segments_X exists when in fact it
						// doesn't.  So, we catch this and handle it
						// as if the file does not exist
						sis = null;
					}
					
					if (sis != null)
						commits.Add(new ReaderCommit(sis, dir));
				}
			}
			
			return commits;
		}
		
		private sealed class ReaderCommit:IndexCommit
		{
			private System.String segmentsFileName;
			internal System.Collections.Generic.ICollection<string> files;
			internal Directory dir;
			internal long generation;
			internal long version;
			internal bool isOptimized;
            internal System.Collections.Generic.IDictionary<string, string> userData;
			
			internal ReaderCommit(SegmentInfos infos, Directory dir)
			{
				segmentsFileName = infos.GetCurrentSegmentFileName();
				this.dir = dir;
				userData = infos.GetUserData();
                files = infos.Files(dir, true);
				version = infos.GetVersion();
				generation = infos.GetGeneration();
				isOptimized = infos.Count == 1 && !infos.Info(0).HasDeletions();
			}
            public override string ToString()
            {
                return "DirectoryReader.ReaderCommit(" + segmentsFileName + ")";
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
				return dir;
			}
			
			public override long GetVersion()
			{
				return version;
			}
			
			public override long GetGeneration()
			{
				return generation;
			}
			
			public override bool IsDeleted()
			{
				return false;
			}

            public override System.Collections.Generic.IDictionary<string, string> GetUserData()
			{
				return userData;
			}

            public override void Delete()
            {
                throw new System.NotSupportedException("This IndexCommit does not support deletions");
            }
		}
		
		internal class MultiTermEnum:TermEnum
		{
			internal IndexReader topReader; // used for matching TermEnum to TermDocs
			private SegmentMergeQueue queue;
			
			private Term term;
			private int docFreq;
			internal SegmentMergeInfo[] matchingSegments; // null terminated array of matching segments
			
			public MultiTermEnum(IndexReader topReader, IndexReader[] readers, int[] starts, Term t)
			{
				this.topReader = topReader;
				queue = new SegmentMergeQueue(readers.Length);
				matchingSegments = new SegmentMergeInfo[readers.Length + 1];
				for (int i = 0; i < readers.Length; i++)
				{
					IndexReader reader = readers[i];
					TermEnum termEnum;
					
					if (t != null)
					{
						termEnum = reader.Terms(t);
					}
					else
						termEnum = reader.Terms();
					
					SegmentMergeInfo smi = new SegmentMergeInfo(starts[i], termEnum, reader);
					smi.ord = i;
					if (t == null?smi.Next():termEnum.Term() != null)
						queue.Put(smi);
					// initialize queue
					else
						smi.Close();
				}
				
				if (t != null && queue.Size() > 0)
				{
					Next();
				}
			}
			
			public override bool Next()
			{
				for (int i = 0; i < matchingSegments.Length; i++)
				{
					SegmentMergeInfo smi = matchingSegments[i];
					if (smi == null)
						break;
					if (smi.Next())
						queue.Put(smi);
					else
						smi.Close(); // done with segment
				}
				
				int numMatchingSegments = 0;
				matchingSegments[0] = null;
				
				SegmentMergeInfo top = (SegmentMergeInfo) queue.Top();
				
				if (top == null)
				{
					term = null;
					return false;
				}
				
				term = top.term;
				docFreq = 0;
				
				while (top != null && term.CompareTo(top.term) == 0)
				{
					matchingSegments[numMatchingSegments++] = top;
					queue.Pop();
					docFreq += top.termEnum.DocFreq(); // increment freq
					top = (SegmentMergeInfo) queue.Top();
				}
				
				matchingSegments[numMatchingSegments] = null;
				return true;
			}
			
			public override Term Term()
			{
				return term;
			}
			
			public override int DocFreq()
			{
				return docFreq;
			}
			
			public override void  Close()
			{
				queue.Close();
			}
		}
		
		internal class MultiTermDocs : TermDocs
		{
			internal IndexReader topReader; // used for matching TermEnum to TermDocs
			protected internal IndexReader[] readers;
			protected internal int[] starts;
			protected internal Term term;
			
			protected internal int base_Renamed = 0;
			protected internal int pointer = 0;
			
			private TermDocs[] readerTermDocs;
			protected internal TermDocs current; // == readerTermDocs[pointer]
			
			private MultiTermEnum tenum; // the term enum used for seeking... can be null
			internal int matchingSegmentPos; // position into the matching segments from tenum
			internal SegmentMergeInfo smi; // current segment mere info... can be null
			
			public MultiTermDocs(IndexReader topReader, IndexReader[] r, int[] s)
			{
				this.topReader = topReader;
				readers = r;
				starts = s;
				
				readerTermDocs = new TermDocs[r.Length];
			}
			
			public virtual int Doc()
			{
				return base_Renamed + current.Doc();
			}
			public virtual int Freq()
			{
				return current.Freq();
			}
			
			public virtual void  Seek(Term term)
			{
				this.term = term;
				this.base_Renamed = 0;
				this.pointer = 0;
				this.current = null;
				this.tenum = null;
				this.smi = null;
				this.matchingSegmentPos = 0;
			}
			
			public virtual void  Seek(TermEnum termEnum)
			{
				Seek(termEnum.Term());
				if (termEnum is MultiTermEnum)
				{
					tenum = (MultiTermEnum) termEnum;
					if (topReader != tenum.topReader)
						tenum = null;
				}
			}
			
			public virtual bool Next()
			{
				for (; ; )
				{
					if (current != null && current.Next())
					{
						return true;
					}
					else if (pointer < readers.Length)
					{
						if (tenum != null)
						{
							smi = tenum.matchingSegments[matchingSegmentPos++];
							if (smi == null)
							{
								pointer = readers.Length;
								return false;
							}
							pointer = smi.ord;
						}
						base_Renamed = starts[pointer];
						current = TermDocs(pointer++);
					}
					else
					{
						return false;
					}
				}
			}
			
			/// <summary>Optimized implementation. </summary>
			public virtual int Read(int[] docs, int[] freqs)
			{
				while (true)
				{
					while (current == null)
					{
						if (pointer < readers.Length)
						{
							// try next segment
							if (tenum != null)
							{
								smi = tenum.matchingSegments[matchingSegmentPos++];
								if (smi == null)
								{
									pointer = readers.Length;
									return 0;
								}
								pointer = smi.ord;
							}
							base_Renamed = starts[pointer];
							current = TermDocs(pointer++);
						}
						else
						{
							return 0;
						}
					}
					int end = current.Read(docs, freqs);
					if (end == 0)
					{
						// none left in segment
						current = null;
					}
					else
					{
						// got some
						int b = base_Renamed; // adjust doc numbers
						for (int i = 0; i < end; i++)
							docs[i] += b;
						return end;
					}
				}
			}
			
			/* A Possible future optimization could skip entire segments */
			public virtual bool SkipTo(int target)
			{
				for (; ; )
				{
					if (current != null && current.SkipTo(target - base_Renamed))
					{
						return true;
					}
					else if (pointer < readers.Length)
					{
						if (tenum != null)
						{
							SegmentMergeInfo smi = tenum.matchingSegments[matchingSegmentPos++];
							if (smi == null)
							{
								pointer = readers.Length;
								return false;
							}
							pointer = smi.ord;
						}
						base_Renamed = starts[pointer];
						current = TermDocs(pointer++);
					}
					else
						return false;
				}
			}
			
			private TermDocs TermDocs(int i)
			{
				TermDocs result = readerTermDocs[i];
				if (result == null)
					result = readerTermDocs[i] = TermDocs(readers[i]);
				if (smi != null)
				{
					System.Diagnostics.Debug.Assert((smi.ord == i));
					System.Diagnostics.Debug.Assert((smi.termEnum.Term().Equals(term)));
					result.Seek(smi.termEnum);
				}
				else
				{
					result.Seek(term);
				}
				return result;
			}
			
			protected internal virtual TermDocs TermDocs(IndexReader reader)
			{
				return term == null?reader.TermDocs(null):reader.TermDocs();
			}
			
			public virtual void  Close()
			{
				for (int i = 0; i < readerTermDocs.Length; i++)
				{
					if (readerTermDocs[i] != null)
						readerTermDocs[i].Close();
				}
			}
		}
		
		internal class MultiTermPositions:MultiTermDocs, TermPositions
		{
			public MultiTermPositions(IndexReader topReader, IndexReader[] r, int[] s):base(topReader, r, s)
			{
			}
			
			protected internal override TermDocs TermDocs(IndexReader reader)
			{
				return (TermDocs) reader.TermPositions();
			}
			
			public virtual int NextPosition()
			{
				return ((TermPositions) current).NextPosition();
			}
			
			public virtual int GetPayloadLength()
			{
				return ((TermPositions) current).GetPayloadLength();
			}
			
			public virtual byte[] GetPayload(byte[] data, int offset)
			{
				return ((TermPositions) current).GetPayload(data, offset);
			}
			
			
			// TODO: Remove warning after API has been finalized
			public virtual bool IsPayloadAvailable()
			{
				return ((TermPositions) current).IsPayloadAvailable();
			}
		}
	}
}
