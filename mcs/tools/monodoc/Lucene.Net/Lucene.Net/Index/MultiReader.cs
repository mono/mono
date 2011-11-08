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
using MultiTermDocs = Mono.Lucene.Net.Index.DirectoryReader.MultiTermDocs;
using MultiTermEnum = Mono.Lucene.Net.Index.DirectoryReader.MultiTermEnum;
using MultiTermPositions = Mono.Lucene.Net.Index.DirectoryReader.MultiTermPositions;
using DefaultSimilarity = Mono.Lucene.Net.Search.DefaultSimilarity;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary>An IndexReader which reads multiple indexes, appending their content.
	/// 
	/// </summary>
	/// <version>  $Id: MultiReader.java 782406 2009-06-07 16:31:18Z mikemccand $
	/// </version>
	public class MultiReader:IndexReader, System.ICloneable
	{
		protected internal IndexReader[] subReaders;
		private int[] starts; // 1st docno for each segment
		private bool[] decrefOnClose; // remember which subreaders to decRef on close
		private System.Collections.IDictionary normsCache = new System.Collections.Hashtable();
		private int maxDoc = 0;
		private int numDocs = - 1;
		private bool hasDeletions = false;
		
		/// <summary> <p/>Construct a MultiReader aggregating the named set of (sub)readers.
		/// Directory locking for delete, undeleteAll, and setNorm operations is
		/// left to the subreaders. <p/>
		/// <p/>Note that all subreaders are closed if this Multireader is closed.<p/>
		/// </summary>
		/// <param name="subReaders">set of (sub)readers
		/// </param>
		/// <throws>  IOException </throws>
		public MultiReader(IndexReader[] subReaders)
		{
			Initialize(subReaders, true);
		}
		
		/// <summary> <p/>Construct a MultiReader aggregating the named set of (sub)readers.
		/// Directory locking for delete, undeleteAll, and setNorm operations is
		/// left to the subreaders. <p/>
		/// </summary>
		/// <param name="closeSubReaders">indicates whether the subreaders should be closed
		/// when this MultiReader is closed
		/// </param>
		/// <param name="subReaders">set of (sub)readers
		/// </param>
		/// <throws>  IOException </throws>
		public MultiReader(IndexReader[] subReaders, bool closeSubReaders)
		{
			Initialize(subReaders, closeSubReaders);
		}
		
		private void  Initialize(IndexReader[] subReaders, bool closeSubReaders)
		{
			this.subReaders = new IndexReader[subReaders.Length];
			subReaders.CopyTo(this.subReaders, 0);
			starts = new int[subReaders.Length + 1]; // build starts array
			decrefOnClose = new bool[subReaders.Length];
			for (int i = 0; i < subReaders.Length; i++)
			{
				starts[i] = maxDoc;
				maxDoc += subReaders[i].MaxDoc(); // compute maxDocs
				
				if (!closeSubReaders)
				{
					subReaders[i].IncRef();
					decrefOnClose[i] = true;
				}
				else
				{
					decrefOnClose[i] = false;
				}
				
				if (subReaders[i].HasDeletions())
					hasDeletions = true;
			}
			starts[subReaders.Length] = maxDoc;
		}
		
		/// <summary> Tries to reopen the subreaders.
		/// <br/>
		/// If one or more subreaders could be re-opened (i. e. subReader.reopen() 
		/// returned a new instance != subReader), then a new MultiReader instance 
		/// is returned, otherwise this instance is returned.
		/// <p/>
		/// A re-opened instance might share one or more subreaders with the old 
		/// instance. Index modification operations result in undefined behavior
		/// when performed before the old instance is closed.
		/// (see {@link IndexReader#Reopen()}).
		/// <p/>
		/// If subreaders are shared, then the reference count of those
		/// readers is increased to ensure that the subreaders remain open
		/// until the last referring reader is closed.
		/// 
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error  </throws>
		public override IndexReader Reopen()
		{
			lock (this)
			{
				return DoReopen(false);
			}
		}
		
		/// <summary> Clones the subreaders.
		/// (see {@link IndexReader#clone()}).
		/// <br/>
		/// <p/>
		/// If subreaders are shared, then the reference count of those
		/// readers is increased to ensure that the subreaders remain open
		/// until the last referring reader is closed.
		/// </summary>
		public override System.Object Clone()
		{
			try
			{
				return DoReopen(true);
			}
			catch (System.Exception ex)
			{
				throw new System.SystemException(ex.Message, ex);
			}
		}
		
		/// <summary> If clone is true then we clone each of the subreaders</summary>
		/// <param name="doClone">
		/// </param>
		/// <returns> New IndexReader, or same one (this) if
		/// reopen/clone is not necessary
		/// </returns>
		/// <throws>  CorruptIndexException </throws>
		/// <throws>  IOException </throws>
		protected internal virtual IndexReader DoReopen(bool doClone)
		{
			EnsureOpen();
			
			bool reopened = false;
			IndexReader[] newSubReaders = new IndexReader[subReaders.Length];
			
			bool success = false;
			try
			{
				for (int i = 0; i < subReaders.Length; i++)
				{
					if (doClone)
						newSubReaders[i] = (IndexReader) subReaders[i].Clone();
					else
						newSubReaders[i] = subReaders[i].Reopen();
					// if at least one of the subreaders was updated we remember that
					// and return a new MultiReader
					if (newSubReaders[i] != subReaders[i])
					{
						reopened = true;
					}
				}
				success = true;
			}
			finally
			{
				if (!success && reopened)
				{
					for (int i = 0; i < newSubReaders.Length; i++)
					{
						if (newSubReaders[i] != subReaders[i])
						{
							try
							{
								newSubReaders[i].Close();
							}
							catch (System.IO.IOException ignore)
							{
								// keep going - we want to clean up as much as possible
							}
						}
					}
				}
			}
			
			if (reopened)
			{
				bool[] newDecrefOnClose = new bool[subReaders.Length];
				for (int i = 0; i < subReaders.Length; i++)
				{
					if (newSubReaders[i] == subReaders[i])
					{
						newSubReaders[i].IncRef();
						newDecrefOnClose[i] = true;
					}
				}
				MultiReader mr = new MultiReader(newSubReaders);
				mr.decrefOnClose = newDecrefOnClose;
				mr.SetDisableFakeNorms(GetDisableFakeNorms());
				return mr;
			}
			else
			{
				return this;
			}
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
		
		public override bool IsOptimized()
		{
			return false;
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
			return DirectoryReader.ReaderIndex(n, this.starts, this.subReaders.Length);
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
				for (int i = 0; i < subReaders.Length; i++)
				// read from segments
					subReaders[i].Norms(field, result, offset + starts[i]);
				
				if (bytes == null && !HasNorms(field))
				{
                    for (int i = offset; i < result.Length; i++)
                    {
                        result[i] = (byte) DefaultSimilarity.EncodeNorm(1.0f);
                    }
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
		
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		protected internal override void  DoCommit()
		{
			DoCommit(null);
		}

        protected internal override void DoCommit(System.Collections.Generic.IDictionary<string, string> commitUserData)
		{
			for (int i = 0; i < subReaders.Length; i++)
				subReaders[i].Commit(commitUserData);
		}
		
		protected internal override void  DoClose()
		{
			lock (this)
			{
				for (int i = 0; i < subReaders.Length; i++)
				{
					if (decrefOnClose[i])
					{
						subReaders[i].DecRef();
					}
					else
					{
						subReaders[i].Close();
					}
				}
			}

            // NOTE: only needed in case someone had asked for
            // FieldCache for top-level reader (which is generally
            // not a good idea):
            Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.Purge(this);
		}

        public override System.Collections.Generic.ICollection<string> GetFieldNames(IndexReader.FieldOption fieldNames)
		{
			EnsureOpen();
			return DirectoryReader.GetFieldNames(fieldNames, this.subReaders);
		}
		
		/// <summary> Checks recursively if all subreaders are up to date. </summary>
		public override bool IsCurrent()
		{
			for (int i = 0; i < subReaders.Length; i++)
			{
				if (!subReaders[i].IsCurrent())
				{
					return false;
				}
			}
			
			// all subreaders are up to date
			return true;
		}
		
		/// <summary>Not implemented.</summary>
		/// <throws>  UnsupportedOperationException </throws>
		public override long GetVersion()
		{
			throw new System.NotSupportedException("MultiReader does not support this method.");
		}
		
		public override IndexReader[] GetSequentialSubReaders()
		{
			return subReaders;
		}
	}
}
