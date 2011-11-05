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
using BufferedIndexInput = Mono.Lucene.Net.Store.BufferedIndexInput;
using Directory = Mono.Lucene.Net.Store.Directory;
using IndexInput = Mono.Lucene.Net.Store.IndexInput;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using BitVector = Mono.Lucene.Net.Util.BitVector;
using CloseableThreadLocal = Mono.Lucene.Net.Util.CloseableThreadLocal;
using DefaultSimilarity = Mono.Lucene.Net.Search.DefaultSimilarity;

namespace Mono.Lucene.Net.Index
{
	
	/// <version>  $Id 
	/// </version>
	/// <summary> <p/><b>NOTE:</b> This API is new and still experimental
	/// (subject to change suddenly in the next release)<p/>
	/// </summary>
	public class SegmentReader:IndexReader, System.ICloneable
	{
		public SegmentReader()
		{
			InitBlock();
		}
		private void  InitBlock()
		{
			fieldsReaderLocal = new FieldsReaderLocal(this);
		}
		protected internal bool readOnly;
		
		private SegmentInfo si;
		private int readBufferSize;
		
		internal CloseableThreadLocal fieldsReaderLocal;
		internal CloseableThreadLocal termVectorsLocal = new CloseableThreadLocal();
		
		internal BitVector deletedDocs = null;
		internal Ref deletedDocsRef = null;
		private bool deletedDocsDirty = false;
		private bool normsDirty = false;
		private int pendingDeleteCount;
		
		private bool rollbackHasChanges = false;
		private bool rollbackDeletedDocsDirty = false;
		private bool rollbackNormsDirty = false;
        private SegmentInfo rollbackSegmentInfo;
		private int rollbackPendingDeleteCount;
		
		// optionally used for the .nrm file shared by multiple norms
		private IndexInput singleNormStream;
		private Ref singleNormRef;
		
		internal CoreReaders core;
		
		// Holds core readers that are shared (unchanged) when
		// SegmentReader is cloned or reopened
		public /*internal*/ sealed class CoreReaders
		{
			
			// Counts how many other reader share the core objects
			// (freqStream, proxStream, tis, etc.) of this reader;
			// when coreRef drops to 0, these core objects may be
			// closed.  A given insance of SegmentReader may be
			// closed, even those it shares core objects with other
			// SegmentReaders:
			private Ref ref_Renamed = new Ref();
			
			internal System.String segment;
			internal FieldInfos fieldInfos;
			internal IndexInput freqStream;
			internal IndexInput proxStream;
			internal TermInfosReader tisNoIndex;
			
			internal Directory dir;
			internal Directory cfsDir;
			internal int readBufferSize;
			internal int termsIndexDivisor;

            internal SegmentReader origInstance;
			
			internal TermInfosReader tis;
			internal FieldsReader fieldsReaderOrig;
			internal TermVectorsReader termVectorsReaderOrig;
			internal CompoundFileReader cfsReader;
			internal CompoundFileReader storeCFSReader;

            internal CoreReaders(SegmentReader origInstance, Directory dir, SegmentInfo si, int readBufferSize, int termsIndexDivisor)
			{
				segment = si.name;
				this.readBufferSize = readBufferSize;
				this.dir = dir;
				
				bool success = false;
				
				try
				{
					Directory dir0 = dir;
					if (si.GetUseCompoundFile())
					{
						cfsReader = new CompoundFileReader(dir, segment + "." + IndexFileNames.COMPOUND_FILE_EXTENSION, readBufferSize);
						dir0 = cfsReader;
					}
					cfsDir = dir0;
					
					fieldInfos = new FieldInfos(cfsDir, segment + "." + IndexFileNames.FIELD_INFOS_EXTENSION);
					
					this.termsIndexDivisor = termsIndexDivisor;
					TermInfosReader reader = new TermInfosReader(cfsDir, segment, fieldInfos, readBufferSize, termsIndexDivisor);
					if (termsIndexDivisor == - 1)
					{
						tisNoIndex = reader;
					}
					else
					{
						tis = reader;
						tisNoIndex = null;
					}
					
					// make sure that all index files have been read or are kept open
					// so that if an index update removes them we'll still have them
					freqStream = cfsDir.OpenInput(segment + "." + IndexFileNames.FREQ_EXTENSION, readBufferSize);
					
					if (fieldInfos.HasProx())
					{
						proxStream = cfsDir.OpenInput(segment + "." + IndexFileNames.PROX_EXTENSION, readBufferSize);
					}
					else
					{
						proxStream = null;
					}
					success = true;
				}
				finally
				{
					if (!success)
					{
						DecRef();
					}
				}


                // Must assign this at the end -- if we hit an
                // exception above core, we don't want to attempt to
                // purge the FieldCache (will hit NPE because core is
                // not assigned yet).
                this.origInstance = origInstance;
			}
			
			internal TermVectorsReader GetTermVectorsReaderOrig()
			{
				lock (this)
				{
					return termVectorsReaderOrig;
				}
			}
			
			internal FieldsReader GetFieldsReaderOrig()
			{
				lock (this)
				{
					return fieldsReaderOrig;
				}
			}
			
			internal void  IncRef()
			{
				lock (this)
				{
					ref_Renamed.IncRef();
				}
			}
			
			internal Directory GetCFSReader()
			{
				lock (this)
				{
					return cfsReader;
				}
			}
			
			internal TermInfosReader GetTermsReader()
			{
				lock (this)
				{
					if (tis != null)
					{
						return tis;
					}
					else
					{
						return tisNoIndex;
					}
				}
			}
			
			internal bool TermsIndexIsLoaded()
			{
				lock (this)
				{
					return tis != null;
				}
			}
			
			// NOTE: only called from IndexWriter when a near
			// real-time reader is opened, or applyDeletes is run,
			// sharing a segment that's still being merged.  This
			// method is not fully thread safe, and relies on the
			// synchronization in IndexWriter
			internal void  LoadTermsIndex(SegmentInfo si, int termsIndexDivisor)
			{
				lock (this)
				{
					if (tis == null)
					{
						Directory dir0;
						if (si.GetUseCompoundFile())
						{
							// In some cases, we were originally opened when CFS
							// was not used, but then we are asked to open the
							// terms reader with index, the segment has switched
							// to CFS
							if (cfsReader == null)
							{
								cfsReader = new CompoundFileReader(dir, segment + "." + IndexFileNames.COMPOUND_FILE_EXTENSION, readBufferSize);
							}
							dir0 = cfsReader;
						}
						else
						{
							dir0 = dir;
						}
						
						tis = new TermInfosReader(dir0, segment, fieldInfos, readBufferSize, termsIndexDivisor);
					}
				}
			}
			
			internal void  DecRef()
			{
				lock (this)
				{
					
					if (ref_Renamed.DecRef() == 0)
					{
						
						// close everything, nothing is shared anymore with other readers
						if (tis != null)
						{
							tis.Close();
							// null so if an app hangs on to us we still free most ram
							tis = null;
						}
						
						if (tisNoIndex != null)
						{
							tisNoIndex.Close();
						}
						
						if (freqStream != null)
						{
							freqStream.Close();
						}
						
						if (proxStream != null)
						{
							proxStream.Close();
						}
						
						if (termVectorsReaderOrig != null)
						{
							termVectorsReaderOrig.Close();
						}
						
						if (fieldsReaderOrig != null)
						{
							fieldsReaderOrig.Close();
						}
						
						if (cfsReader != null)
						{
							cfsReader.Close();
						}
						
						if (storeCFSReader != null)
						{
							storeCFSReader.Close();
						}

                        // Force FieldCache to evict our entries at this point
                        if (origInstance != null)
                        {
                            Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.Purge(origInstance);
                        }
					}
				}
			}
			
			internal void  OpenDocStores(SegmentInfo si)
			{
				lock (this)
				{
					
					System.Diagnostics.Debug.Assert(si.name.Equals(segment));
					
					if (fieldsReaderOrig == null)
					{
						Directory storeDir;
						if (si.GetDocStoreOffset() != - 1)
						{
							if (si.GetDocStoreIsCompoundFile())
							{
								System.Diagnostics.Debug.Assert(storeCFSReader == null);
								storeCFSReader = new CompoundFileReader(dir, si.GetDocStoreSegment() + "." + IndexFileNames.COMPOUND_FILE_STORE_EXTENSION, readBufferSize);
								storeDir = storeCFSReader;
								System.Diagnostics.Debug.Assert(storeDir != null);
							}
							else
							{
								storeDir = dir;
								System.Diagnostics.Debug.Assert(storeDir != null);
							}
						}
						else if (si.GetUseCompoundFile())
						{
							// In some cases, we were originally opened when CFS
							// was not used, but then we are asked to open doc
							// stores after the segment has switched to CFS
							if (cfsReader == null)
							{
								cfsReader = new CompoundFileReader(dir, segment + "." + IndexFileNames.COMPOUND_FILE_EXTENSION, readBufferSize);
							}
							storeDir = cfsReader;
							System.Diagnostics.Debug.Assert(storeDir != null);
						}
						else
						{
							storeDir = dir;
							System.Diagnostics.Debug.Assert(storeDir != null);
						}
						
						System.String storesSegment;
						if (si.GetDocStoreOffset() != - 1)
						{
							storesSegment = si.GetDocStoreSegment();
						}
						else
						{
							storesSegment = segment;
						}
						
						fieldsReaderOrig = new FieldsReader(storeDir, storesSegment, fieldInfos, readBufferSize, si.GetDocStoreOffset(), si.docCount);
						
						// Verify two sources of "maxDoc" agree:
						if (si.GetDocStoreOffset() == - 1 && fieldsReaderOrig.Size() != si.docCount)
						{
							throw new CorruptIndexException("doc counts differ for segment " + segment + ": fieldsReader shows " + fieldsReaderOrig.Size() + " but segmentInfo shows " + si.docCount);
						}
						
						if (fieldInfos.HasVectors())
						{
							// open term vector files only as needed
							termVectorsReaderOrig = new TermVectorsReader(storeDir, storesSegment, fieldInfos, readBufferSize, si.GetDocStoreOffset(), si.docCount);
						}
					}
				}
			}

            public FieldInfos fieldInfos_ForNUnit
            {
                get { return fieldInfos; }
            }
		}
		
		/// <summary> Sets the initial value </summary>
		private class FieldsReaderLocal:CloseableThreadLocal
		{
			public FieldsReaderLocal(SegmentReader enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(SegmentReader enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SegmentReader enclosingInstance;
			public SegmentReader Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public /*protected internal*/ override System.Object InitialValue()
			{
				return Enclosing_Instance.core.GetFieldsReaderOrig().Clone();
			}
		}
		
		public /*internal*/ class Ref
		{
			private int refCount = 1;
			
			public override System.String ToString()
			{
				return "refcount: " + refCount;
			}
			
			public virtual int RefCount()
			{
				lock (this)
				{
					return refCount;
				}
			}
			
			public virtual int IncRef()
			{
				lock (this)
				{
					System.Diagnostics.Debug.Assert(refCount > 0);
					refCount++;
					return refCount;
				}
			}
			
			public virtual int DecRef()
			{
				lock (this)
				{
					System.Diagnostics.Debug.Assert(refCount > 0);
					refCount--;
					return refCount;
				}
			}
		}
		
		/// <summary> Byte[] referencing is used because a new norm object needs 
		/// to be created for each clone, and the byte array is all 
		/// that is needed for sharing between cloned readers.  The 
		/// current norm referencing is for sharing between readers 
		/// whereas the byte[] referencing is for copy on write which 
		/// is independent of reader references (i.e. incRef, decRef).
		/// </summary>
		
		public /*internal*/ sealed class Norm : System.ICloneable
		{
			private void  InitBlock(SegmentReader enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SegmentReader enclosingInstance;
			public SegmentReader Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal /*private*/ int refCount = 1;
			
			// If this instance is a clone, the originalNorm
			// references the Norm that has a real open IndexInput:
			private Norm origNorm;
			
			private IndexInput in_Renamed;
			private long normSeek;
			
			// null until bytes is set
			private Ref bytesRef;
			internal /*private*/ byte[] bytes;
			internal /*private*/ bool dirty;
			internal /*private*/ int number;
			internal /*private*/ bool rollbackDirty;
			
			public Norm(SegmentReader enclosingInstance, IndexInput in_Renamed, int number, long normSeek)
			{
				InitBlock(enclosingInstance);
				this.in_Renamed = in_Renamed;
				this.number = number;
				this.normSeek = normSeek;
			}
			
			public void  IncRef()
			{
				lock (this)
				{
					System.Diagnostics.Debug.Assert(refCount > 0 &&(origNorm == null || origNorm.refCount > 0));
					refCount++;
				}
			}
			
			private void  CloseInput()
			{
				if (in_Renamed != null)
				{
					if (in_Renamed != Enclosing_Instance.singleNormStream)
					{
						// It's private to us -- just close it
						in_Renamed.Close();
					}
					else
					{
						// We are sharing this with others -- decRef and
						// maybe close the shared norm stream
						if (Enclosing_Instance.singleNormRef.DecRef() == 0)
						{
							Enclosing_Instance.singleNormStream.Close();
							Enclosing_Instance.singleNormStream = null;
						}
					}
					
					in_Renamed = null;
				}
			}
			
			public void  DecRef()
			{
				lock (this)
				{
					System.Diagnostics.Debug.Assert(refCount > 0 &&(origNorm == null || origNorm.refCount > 0));
					
					if (--refCount == 0)
					{
						if (origNorm != null)
						{
							origNorm.DecRef();
							origNorm = null;
						}
						else
						{
							CloseInput();
						}
						
						if (bytes != null)
						{
							System.Diagnostics.Debug.Assert(bytesRef != null);
							bytesRef.DecRef();
							bytes = null;
							bytesRef = null;
						}
						else
						{
							System.Diagnostics.Debug.Assert(bytesRef == null);
						}
					}
				}
			}
			
			// Load bytes but do not cache them if they were not
			// already cached
			public void  Bytes(byte[] bytesOut, int offset, int len)
			{
				lock (this)
				{
					System.Diagnostics.Debug.Assert(refCount > 0 &&(origNorm == null || origNorm.refCount > 0));
					if (bytes != null)
					{
						// Already cached -- copy from cache:
						System.Diagnostics.Debug.Assert(len <= Enclosing_Instance.MaxDoc());
						Array.Copy(bytes, 0, bytesOut, offset, len);
					}
					else
					{
						// Not cached
						if (origNorm != null)
						{
							// Ask origNorm to load
							origNorm.Bytes(bytesOut, offset, len);
						}
						else
						{
							// We are orig -- read ourselves from disk:
							lock (in_Renamed)
							{
								in_Renamed.Seek(normSeek);
								in_Renamed.ReadBytes(bytesOut, offset, len, false);
							}
						}
					}
				}
			}
			
			// Load & cache full bytes array.  Returns bytes.
			public byte[] Bytes()
			{
				lock (this)
				{
					System.Diagnostics.Debug.Assert(refCount > 0 &&(origNorm == null || origNorm.refCount > 0));
					if (bytes == null)
					{
						// value not yet read
						System.Diagnostics.Debug.Assert(bytesRef == null);
						if (origNorm != null)
						{
							// Ask origNorm to load so that for a series of
							// reopened readers we share a single read-only
							// byte[]
							bytes = origNorm.Bytes();
							bytesRef = origNorm.bytesRef;
							bytesRef.IncRef();
							
							// Once we've loaded the bytes we no longer need
							// origNorm:
							origNorm.DecRef();
							origNorm = null;
						}
						else
						{
							// We are the origNorm, so load the bytes for real
							// ourself:
							int count = Enclosing_Instance.MaxDoc();
							bytes = new byte[count];
							
							// Since we are orig, in must not be null
							System.Diagnostics.Debug.Assert(in_Renamed != null);
							
							// Read from disk.
							lock (in_Renamed)
							{
								in_Renamed.Seek(normSeek);
								in_Renamed.ReadBytes(bytes, 0, count, false);
							}
							
							bytesRef = new Ref();
							CloseInput();
						}
					}
					
					return bytes;
				}
			}
			
			// Only for testing
			public /*internal*/ Ref BytesRef()
			{
				return bytesRef;
			}
			
			// Called if we intend to change a norm value.  We make a
			// private copy of bytes if it's shared with others:
			public byte[] CopyOnWrite()
			{
				lock (this)
				{
					System.Diagnostics.Debug.Assert(refCount > 0 &&(origNorm == null || origNorm.refCount > 0));
					Bytes();
					System.Diagnostics.Debug.Assert(bytes != null);
					System.Diagnostics.Debug.Assert(bytesRef != null);
					if (bytesRef.RefCount() > 1)
					{
						// I cannot be the origNorm for another norm
						// instance if I'm being changed.  Ie, only the
						// "head Norm" can be changed:
						System.Diagnostics.Debug.Assert(refCount == 1);
						Ref oldRef = bytesRef;
						bytes = Enclosing_Instance.CloneNormBytes(bytes);
						bytesRef = new Ref();
						oldRef.DecRef();
					}
					dirty = true;
					return bytes;
				}
			}
			
			// Returns a copy of this Norm instance that shares
			// IndexInput & bytes with the original one
			public System.Object Clone()
			{
                lock (this) //LUCENENET-375
                {
                    System.Diagnostics.Debug.Assert(refCount > 0 && (origNorm == null || origNorm.refCount > 0));

                    Norm clone;
                    try
                    {
                        clone = (Norm)base.MemberwiseClone();
                    }
                    catch (System.Exception cnse)
                    {
                        // Cannot happen
                        throw new System.SystemException("unexpected CloneNotSupportedException", cnse);
                    }
                    clone.refCount = 1;

                    if (bytes != null)
                    {
                        System.Diagnostics.Debug.Assert(bytesRef != null);
                        System.Diagnostics.Debug.Assert(origNorm == null);

                        // Clone holds a reference to my bytes:
                        clone.bytesRef.IncRef();
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(bytesRef == null);
                        if (origNorm == null)
                        {
                            // I become the origNorm for the clone:
                            clone.origNorm = this;
                        }
                        clone.origNorm.IncRef();
                    }

                    // Only the origNorm will actually readBytes from in:
                    clone.in_Renamed = null;

                    return clone;
                }
			}
			
			// Flush all pending changes to the next generation
			// separate norms file.
			public void  ReWrite(SegmentInfo si)
			{
				System.Diagnostics.Debug.Assert(refCount > 0 && (origNorm == null || origNorm.refCount > 0), "refCount=" + refCount + " origNorm=" + origNorm);
				
				// NOTE: norms are re-written in regular directory, not cfs
				si.AdvanceNormGen(this.number);
				string normFileName = si.GetNormFileName(this.number);
                IndexOutput @out = enclosingInstance.Directory().CreateOutput(normFileName);
                bool success = false;
				try
				{
					try {
                        @out.WriteBytes(bytes, enclosingInstance.MaxDoc());
                    } finally {
                        @out.Close();
                    }
                    success = true;
				}
				finally
				{
                    if (!success)
                    {
                        try
                        {
                            enclosingInstance.Directory().DeleteFile(normFileName);
                        }
                        catch (Exception t)
                        {
                            // suppress this so we keep throwing the
                            // original exception
                        }
                    }
				}
				this.dirty = false;
			}
		}
		
		internal System.Collections.IDictionary norms = new System.Collections.Hashtable();
		
		/// <summary>The class which implements SegmentReader. </summary>
		// @deprecated (LUCENE-1677)
		private static System.Type IMPL;
		
		// @deprecated (LUCENE-1677)
		private static System.Type READONLY_IMPL;
		
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated>
		/// </deprecated>
        [Obsolete]
		public static SegmentReader Get(SegmentInfo si)
		{
			return Get(false, si.dir, si, BufferedIndexInput.BUFFER_SIZE, true, IndexReader.DEFAULT_TERMS_INDEX_DIVISOR);
		}
		
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static SegmentReader Get(bool readOnly, SegmentInfo si, int termInfosIndexDivisor)
		{
			return Get(readOnly, si.dir, si, BufferedIndexInput.BUFFER_SIZE, true, termInfosIndexDivisor);
		}
		
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated>
		/// </deprecated>
        [Obsolete]
		internal static SegmentReader Get(SegmentInfo si, int readBufferSize, bool doOpenStores, int termInfosIndexDivisor)
		{
			return Get(false, si.dir, si, readBufferSize, doOpenStores, termInfosIndexDivisor);
		}
		
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public static SegmentReader Get(bool readOnly, Directory dir, SegmentInfo si, int readBufferSize, bool doOpenStores, int termInfosIndexDivisor)
		{
			SegmentReader instance;
			try
			{
				if (readOnly)
					instance = (SegmentReader) System.Activator.CreateInstance(READONLY_IMPL);
				else
					instance = (SegmentReader) System.Activator.CreateInstance(IMPL);
			}
			catch (System.Exception e)
			{
				throw new System.SystemException("cannot load SegmentReader class: " + e, e);
			}
			instance.readOnly = readOnly;
			instance.si = si;
			instance.readBufferSize = readBufferSize;
			
			bool success = false;
			
			try
			{
				instance.core = new CoreReaders(instance, dir, si, readBufferSize, termInfosIndexDivisor);
				if (doOpenStores)
				{
					instance.core.OpenDocStores(si);
				}
				instance.LoadDeletedDocs();
				instance.OpenNorms(instance.core.cfsDir, readBufferSize);
				success = true;
			}
			finally
			{
				
				// With lock-less commits, it's entirely possible (and
				// fine) to hit a FileNotFound exception above.  In
				// this case, we want to explicitly close any subset
				// of things that were opened so that we don't have to
				// wait for a GC to do so.
				if (!success)
				{
					instance.DoClose();
				}
			}
			return instance;
		}
		
		internal virtual void  OpenDocStores()
		{
			core.OpenDocStores(si);
		}

        private bool CheckDeletedCounts()
        {
            int recomputedCount = deletedDocs.GetRecomputedCount();

            System.Diagnostics.Debug.Assert(deletedDocs.Count() == recomputedCount, "deleted count=" + deletedDocs.Count() + " vs recomputed count=" + recomputedCount);

            System.Diagnostics.Debug.Assert(si.GetDelCount() == recomputedCount, "delete count mismatch: info=" + si.GetDelCount() + " vs BitVector=" + recomputedCount);

            // Verify # deletes does not exceed maxDoc for this
            // segment:
            System.Diagnostics.Debug.Assert(si.GetDelCount() <= MaxDoc(), "delete count mismatch: " + recomputedCount + ") exceeds max doc (" + MaxDoc() + ") for segment " + si.name);

            return true;
        }
		
		private void  LoadDeletedDocs()
		{
			// NOTE: the bitvector is stored using the regular directory, not cfs
			if (HasDeletions(si))
			{
				deletedDocs = new BitVector(Directory(), si.GetDelFileName());
				deletedDocsRef = new Ref();

                System.Diagnostics.Debug.Assert(CheckDeletedCounts());
			}
			else 
				System.Diagnostics.Debug.Assert(si.GetDelCount() == 0);
		}
		
		/// <summary> Clones the norm bytes.  May be overridden by subclasses.  New and experimental.</summary>
		/// <param name="bytes">Byte array to clone
		/// </param>
		/// <returns> New BitVector
		/// </returns>
		protected internal virtual byte[] CloneNormBytes(byte[] bytes)
		{
			byte[] cloneBytes = new byte[bytes.Length];
			Array.Copy(bytes, 0, cloneBytes, 0, bytes.Length);
			return cloneBytes;
		}
		
		/// <summary> Clones the deleteDocs BitVector.  May be overridden by subclasses. New and experimental.</summary>
		/// <param name="bv">BitVector to clone
		/// </param>
		/// <returns> New BitVector
		/// </returns>
		protected internal virtual BitVector CloneDeletedDocs(BitVector bv)
		{
			return (BitVector) bv.Clone();
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
				return ReopenSegment(si, true, openReadOnly);
			}
		}
		
		internal virtual SegmentReader ReopenSegment(SegmentInfo si, bool doClone, bool openReadOnly)
		{
			lock (this)
			{
				bool deletionsUpToDate = (this.si.HasDeletions() == si.HasDeletions()) && (!si.HasDeletions() || this.si.GetDelFileName().Equals(si.GetDelFileName()));
				bool normsUpToDate = true;
				
				bool[] fieldNormsChanged = new bool[core.fieldInfos.Size()];
				int fieldCount = core.fieldInfos.Size();
				for (int i = 0; i < fieldCount; i++)
				{
					if (!this.si.GetNormFileName(i).Equals(si.GetNormFileName(i)))
					{
						normsUpToDate = false;
						fieldNormsChanged[i] = true;
					}
				}
				
				// if we're cloning we need to run through the reopenSegment logic
				// also if both old and new readers aren't readonly, we clone to avoid sharing modifications
				if (normsUpToDate && deletionsUpToDate && !doClone && openReadOnly && readOnly)
				{
					return this;
				}
				
				// When cloning, the incoming SegmentInfos should not
				// have any changes in it:
				System.Diagnostics.Debug.Assert(!doClone ||(normsUpToDate && deletionsUpToDate));
				
				// clone reader
				SegmentReader clone;
				try
				{
					if (openReadOnly)
						clone = (SegmentReader) System.Activator.CreateInstance(READONLY_IMPL);
					else
						clone = (SegmentReader) System.Activator.CreateInstance(IMPL);
				}
				catch (System.Exception e)
				{
					throw new System.SystemException("cannot load SegmentReader class: " + e, e);
				}
				
				bool success = false;
				try
				{
					core.IncRef();
					clone.core = core;
					clone.readOnly = openReadOnly;
					clone.si = si;
					clone.readBufferSize = readBufferSize;
					
					if (!openReadOnly && hasChanges)
					{
						// My pending changes transfer to the new reader
						clone.pendingDeleteCount = pendingDeleteCount;
						clone.deletedDocsDirty = deletedDocsDirty;
						clone.normsDirty = normsDirty;
						clone.hasChanges = hasChanges;
						hasChanges = false;
					}
					
					if (doClone)
					{
						if (deletedDocs != null)
						{
							deletedDocsRef.IncRef();
							clone.deletedDocs = deletedDocs;
							clone.deletedDocsRef = deletedDocsRef;
						}
					}
					else
					{
						if (!deletionsUpToDate)
						{
							// load deleted docs
							System.Diagnostics.Debug.Assert(clone.deletedDocs == null);
							clone.LoadDeletedDocs();
						}
						else if (deletedDocs != null)
						{
							deletedDocsRef.IncRef();
							clone.deletedDocs = deletedDocs;
							clone.deletedDocsRef = deletedDocsRef;
						}
					}
					
					clone.SetDisableFakeNorms(GetDisableFakeNorms());
					clone.norms = new System.Collections.Hashtable();
					
					// Clone norms
					for (int i = 0; i < fieldNormsChanged.Length; i++)
					{
						
						// Clone unchanged norms to the cloned reader
						if (doClone || !fieldNormsChanged[i])
						{
							System.String curField = core.fieldInfos.FieldInfo(i).name;
							Norm norm = (Norm) this.norms[curField];
							if (norm != null)
								clone.norms[curField] = norm.Clone();
						}
					}
					
					// If we are not cloning, then this will open anew
					// any norms that have changed:
					clone.OpenNorms(si.GetUseCompoundFile()?core.GetCFSReader():Directory(), readBufferSize);
					
					success = true;
				}
				finally
				{
					if (!success)
					{
						// An exception occured during reopen, we have to decRef the norms
						// that we incRef'ed already and close singleNormsStream and FieldsReader
						clone.DecRef();
					}
				}
				
				return clone;
			}
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
            if (hasChanges)
            {
                StartCommit();
                bool success = false;
                try
                {
                    CommitChanges(commitUserData);
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        RollbackCommit();
                    }
                }
            }
        }

        private void CommitChanges(System.Collections.Generic.IDictionary<string, string> commitUserData)
        {
            if (deletedDocsDirty)
            {               // re-write deleted
                si.AdvanceDelGen();

                // We can write directly to the actual name (vs to a
                // .tmp & renaming it) because the file is not live
                // until segments file is written:
                string delFileName = si.GetDelFileName();
                bool success = false;
                try
                {
                    deletedDocs.Write(Directory(), delFileName);
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        try
                        {
                            Directory().DeleteFile(delFileName);
                        }
                        catch (Exception t)
                        {
                            // suppress this so we keep throwing the
                            // original exception
                        }
                    }
                }

                si.SetDelCount(si.GetDelCount() + pendingDeleteCount);
                pendingDeleteCount = 0;
                System.Diagnostics.Debug.Assert(deletedDocs.Count() == si.GetDelCount(), "delete count mismatch during commit: info=" + si.GetDelCount() + " vs BitVector=" + deletedDocs.Count());
            }
            else
            {
                System.Diagnostics.Debug.Assert(pendingDeleteCount == 0);
            }

            if (normsDirty)
            {               // re-write norms
                si.SetNumFields(core.fieldInfos.Size());
                System.Collections.IEnumerator it = norms.Values.GetEnumerator();
                while (it.MoveNext())
                {
                    Norm norm = (Norm)it.Current;
                    if (norm.dirty)
                    {
                        norm.ReWrite(si);
                    }
                }
            }

            deletedDocsDirty = false;
            normsDirty = false;
            hasChanges = false;
        }
        
		internal virtual FieldsReader GetFieldsReader()
		{
			return (FieldsReader) fieldsReaderLocal.Get();
		}
		
		protected internal override void  DoClose()
		{
			termVectorsLocal.Close();
			fieldsReaderLocal.Close();
			
			if (deletedDocs != null)
			{
				deletedDocsRef.DecRef();
				// null so if an app hangs on to us we still free most ram
				deletedDocs = null;
			}
			
			System.Collections.IEnumerator it = norms.Values.GetEnumerator();
			while (it.MoveNext())
			{
				((Norm) it.Current).DecRef();
			}
			if (core != null)
			{
				core.DecRef();
			}
		}
		
		internal static bool HasDeletions(SegmentInfo si)
		{
			// Don't call ensureOpen() here (it could affect performance)
			return si.HasDeletions();
		}
		
		public override bool HasDeletions()
		{
			// Don't call ensureOpen() here (it could affect performance)
			return deletedDocs != null;
		}
		
		internal static bool UsesCompoundFile(SegmentInfo si)
		{
			return si.GetUseCompoundFile();
		}
		
		internal static bool HasSeparateNorms(SegmentInfo si)
		{
			return si.HasSeparateNorms();
		}
		
		protected internal override void  DoDelete(int docNum)
		{
			if (deletedDocs == null)
			{
				deletedDocs = new BitVector(MaxDoc());
				deletedDocsRef = new Ref();
			}
			// there is more than 1 SegmentReader with a reference to this
			// deletedDocs BitVector so decRef the current deletedDocsRef,
			// clone the BitVector, create a new deletedDocsRef
			if (deletedDocsRef.RefCount() > 1)
			{
				Ref oldRef = deletedDocsRef;
				deletedDocs = CloneDeletedDocs(deletedDocs);
				deletedDocsRef = new Ref();
				oldRef.DecRef();
			}
			deletedDocsDirty = true;
			if (!deletedDocs.GetAndSet(docNum))
				pendingDeleteCount++;
		}
		
		protected internal override void  DoUndeleteAll()
		{
			deletedDocsDirty = false;
			if (deletedDocs != null)
			{
				System.Diagnostics.Debug.Assert(deletedDocsRef != null);
				deletedDocsRef.DecRef();
				deletedDocs = null;
				deletedDocsRef = null;
				pendingDeleteCount = 0;
				si.ClearDelGen();
				si.SetDelCount(0);
			}
			else
			{
				System.Diagnostics.Debug.Assert(deletedDocsRef == null);
				System.Diagnostics.Debug.Assert(pendingDeleteCount == 0);
			}
		}
		
		internal virtual System.Collections.Generic.IList<string> Files()
		{
			return si.Files();
		}
		
		public override TermEnum Terms()
		{
			EnsureOpen();
			return core.GetTermsReader().Terms();
		}
		
		public override TermEnum Terms(Term t)
		{
			EnsureOpen();
			return core.GetTermsReader().Terms(t);
		}
		
		public /*internal*/ virtual FieldInfos FieldInfos()
		{
			return core.fieldInfos;
		}
		
		public override Document Document(int n, FieldSelector fieldSelector)
		{
			EnsureOpen();
			return GetFieldsReader().Doc(n, fieldSelector);
		}
		
		public override bool IsDeleted(int n)
		{
			lock (this)
			{
				return (deletedDocs != null && deletedDocs.Get(n));
			}
		}
		
		public override TermDocs TermDocs(Term term)
		{
			if (term == null)
			{
				return new AllTermDocs(this);
			}
			else
			{
				return base.TermDocs(term);
			}
		}
		
		public override TermDocs TermDocs()
		{
			EnsureOpen();
			return new SegmentTermDocs(this);
		}
		
		public override TermPositions TermPositions()
		{
			EnsureOpen();
			return new SegmentTermPositions(this);
		}
		
		public override int DocFreq(Term t)
		{
			EnsureOpen();
			TermInfo ti = core.GetTermsReader().Get(t);
			if (ti != null)
				return ti.docFreq;
			else
				return 0;
		}
		
		public override int NumDocs()
		{
			// Don't call ensureOpen() here (it could affect performance)
			int n = MaxDoc();
			if (deletedDocs != null)
				n -= deletedDocs.Count();
			return n;
		}
		
		public override int MaxDoc()
		{
			// Don't call ensureOpen() here (it could affect performance)
			return si.docCount;
		}
		
		/// <seealso cref="IndexReader.GetFieldNames(IndexReader.FieldOption)">
		/// </seealso>
        public override System.Collections.Generic.ICollection<string> GetFieldNames(IndexReader.FieldOption fieldOption)
		{
			EnsureOpen();

            System.Collections.Generic.IDictionary<string, string> fieldSet = new System.Collections.Generic.Dictionary<string, string>();
			for (int i = 0; i < core.fieldInfos.Size(); i++)
			{
				FieldInfo fi = core.fieldInfos.FieldInfo(i);
				if (fieldOption == IndexReader.FieldOption.ALL)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (!fi.isIndexed && fieldOption == IndexReader.FieldOption.UNINDEXED)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (fi.omitTermFreqAndPositions && fieldOption == IndexReader.FieldOption.OMIT_TERM_FREQ_AND_POSITIONS)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (fi.storePayloads && fieldOption == IndexReader.FieldOption.STORES_PAYLOADS)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (fi.isIndexed && fieldOption == IndexReader.FieldOption.INDEXED)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (fi.isIndexed && fi.storeTermVector == false && fieldOption == IndexReader.FieldOption.INDEXED_NO_TERMVECTOR)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (fi.storeTermVector == true && fi.storePositionWithTermVector == false && fi.storeOffsetWithTermVector == false && fieldOption == IndexReader.FieldOption.TERMVECTOR)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (fi.isIndexed && fi.storeTermVector && fieldOption == IndexReader.FieldOption.INDEXED_WITH_TERMVECTOR)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (fi.storePositionWithTermVector && fi.storeOffsetWithTermVector == false && fieldOption == IndexReader.FieldOption.TERMVECTOR_WITH_POSITION)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if (fi.storeOffsetWithTermVector && fi.storePositionWithTermVector == false && fieldOption == IndexReader.FieldOption.TERMVECTOR_WITH_OFFSET)
				{
					fieldSet[fi.name] = fi.name;
				}
				else if ((fi.storeOffsetWithTermVector && fi.storePositionWithTermVector) && fieldOption == IndexReader.FieldOption.TERMVECTOR_WITH_POSITION_OFFSET)
				{
					fieldSet[fi.name] = fi.name;
				}
			}
			return fieldSet.Keys;
		}
		
		
		public override bool HasNorms(System.String field)
		{
			lock (this)
			{
				EnsureOpen();
				return norms.Contains(field);
			}
		}
		
		internal static byte[] CreateFakeNorms(int size)
		{
			byte[] ones = new byte[size];
			byte val = (byte) DefaultSimilarity.EncodeNorm(1.0f);
            for (int i = 0; i < ones.Length; i++)
            {
                ones[i] = val;
            }
			return ones;
		}
		
		private byte[] ones;
		private byte[] FakeNorms()
		{
			System.Diagnostics.Debug.Assert(!GetDisableFakeNorms());
			if (ones == null)
				ones = CreateFakeNorms(MaxDoc());
			return ones;
		}
		
		// can return null if norms aren't stored
		protected internal virtual byte[] GetNorms(System.String field)
		{
			lock (this)
			{
				Norm norm = (Norm) norms[field];
				if (norm == null)
					return null; // not indexed, or norms not stored
				return norm.Bytes();
			}
		}
		
		// returns fake norms if norms aren't available
		public override byte[] Norms(System.String field)
		{
			lock (this)
			{
				EnsureOpen();
				byte[] bytes = GetNorms(field);
				if (bytes == null && !GetDisableFakeNorms())
					bytes = FakeNorms();
				return bytes;
			}
		}
		
		protected internal override void  DoSetNorm(int doc, System.String field, byte value_Renamed)
		{
			Norm norm = (Norm) norms[field];
			if (norm == null)
			// not an indexed field
				return ;
			
			normsDirty = true;
			norm.CopyOnWrite()[doc] = value_Renamed; // set the value
		}
		
		/// <summary>Read norms into a pre-allocated array. </summary>
		public override void  Norms(System.String field, byte[] bytes, int offset)
		{
			lock (this)
			{
				
				EnsureOpen();
				Norm norm = (Norm) norms[field];
				if (norm == null)
				{
                    for (int i = offset; i < bytes.Length; i++)
                    {
                        bytes[i] = (byte) DefaultSimilarity.EncodeNorm(1.0f);
                    }
					return ;
				}
				
				norm.Bytes(bytes, offset, MaxDoc());
			}
		}
		
		
		private void  OpenNorms(Directory cfsDir, int readBufferSize)
		{
			long nextNormSeek = SegmentMerger.NORMS_HEADER.Length; //skip header (header unused for now)
			int maxDoc = MaxDoc();
			for (int i = 0; i < core.fieldInfos.Size(); i++)
			{
				FieldInfo fi = core.fieldInfos.FieldInfo(i);
				if (norms.Contains(fi.name))
				{
					// in case this SegmentReader is being re-opened, we might be able to
					// reuse some norm instances and skip loading them here
					continue;
				}
				if (fi.isIndexed && !fi.omitNorms)
				{
					Directory d = Directory();
					System.String fileName = si.GetNormFileName(fi.number);
					if (!si.HasSeparateNorms(fi.number))
					{
						d = cfsDir;
					}
					
					// singleNormFile means multiple norms share this file
					bool singleNormFile = fileName.EndsWith("." + IndexFileNames.NORMS_EXTENSION);
					IndexInput normInput = null;
					long normSeek;
					
					if (singleNormFile)
					{
						normSeek = nextNormSeek;
						if (singleNormStream == null)
						{
							singleNormStream = d.OpenInput(fileName, readBufferSize);
							singleNormRef = new Ref();
						}
						else
						{
							singleNormRef.IncRef();
						}
						// All norms in the .nrm file can share a single IndexInput since
						// they are only used in a synchronized context.
						// If this were to change in the future, a clone could be done here.
						normInput = singleNormStream;
					}
					else
					{
						normSeek = 0;
						normInput = d.OpenInput(fileName);
					}
					
					norms[fi.name] = new Norm(this, normInput, fi.number, normSeek);
					nextNormSeek += maxDoc; // increment also if some norms are separate
				}
			}
		}
		
		public /*internal*/ virtual bool TermsIndexLoaded()
		{
			return core.TermsIndexIsLoaded();
		}
		
		// NOTE: only called from IndexWriter when a near
		// real-time reader is opened, or applyDeletes is run,
		// sharing a segment that's still being merged.  This
		// method is not thread safe, and relies on the
		// synchronization in IndexWriter
		internal virtual void  LoadTermsIndex(int termsIndexDivisor)
		{
			core.LoadTermsIndex(si, termsIndexDivisor);
		}
		
		// for testing only
		public /*internal*/ virtual bool NormsClosed()
		{
			if (singleNormStream != null)
			{
				return false;
			}
			System.Collections.IEnumerator it = norms.Values.GetEnumerator();
			while (it.MoveNext())
			{
				Norm norm = (Norm) it.Current;
				if (norm.refCount > 0)
				{
					return false;
				}
			}
			return true;
		}
		
		// for testing only
		public /*internal*/ virtual bool NormsClosed(System.String field)
		{
			Norm norm = (Norm) norms[field];
			return norm.refCount == 0;
		}
		
		/// <summary> Create a clone from the initial TermVectorsReader and store it in the ThreadLocal.</summary>
		/// <returns> TermVectorsReader
		/// </returns>
		internal virtual TermVectorsReader GetTermVectorsReader()
		{
			TermVectorsReader tvReader = (TermVectorsReader) termVectorsLocal.Get();
			if (tvReader == null)
			{
				TermVectorsReader orig = core.GetTermVectorsReaderOrig();
				if (orig == null)
				{
					return null;
				}
				else
				{
					try
					{
						tvReader = (TermVectorsReader) orig.Clone();
					}
					catch (System.Exception cnse)
					{
						return null;
					}
				}
				termVectorsLocal.Set(tvReader);
			}
			return tvReader;
		}
		
		internal virtual TermVectorsReader GetTermVectorsReaderOrig()
		{
			return core.GetTermVectorsReaderOrig();
		}
		
		/// <summary>Return a term frequency vector for the specified document and field. The
		/// vector returned contains term numbers and frequencies for all terms in
		/// the specified field of this document, if the field had storeTermVector
		/// flag set.  If the flag was not set, the method returns null.
		/// </summary>
		/// <throws>  IOException </throws>
		public override TermFreqVector GetTermFreqVector(int docNumber, System.String field)
		{
			// Check if this field is invalid or has no stored term vector
			EnsureOpen();
			FieldInfo fi = core.fieldInfos.FieldInfo(field);
			if (fi == null || !fi.storeTermVector)
				return null;
			
			TermVectorsReader termVectorsReader = GetTermVectorsReader();
			if (termVectorsReader == null)
				return null;
			
			return termVectorsReader.Get(docNumber, field);
		}
		
		
		public override void  GetTermFreqVector(int docNumber, System.String field, TermVectorMapper mapper)
		{
			EnsureOpen();
			FieldInfo fi = core.fieldInfos.FieldInfo(field);
			if (fi == null || !fi.storeTermVector)
				return ;
			
			TermVectorsReader termVectorsReader = GetTermVectorsReader();
			if (termVectorsReader == null)
			{
				return ;
			}
			
			
			termVectorsReader.Get(docNumber, field, mapper);
		}
		
		
		public override void  GetTermFreqVector(int docNumber, TermVectorMapper mapper)
		{
			EnsureOpen();
			
			TermVectorsReader termVectorsReader = GetTermVectorsReader();
			if (termVectorsReader == null)
				return ;
			
			termVectorsReader.Get(docNumber, mapper);
		}
		
		/// <summary>Return an array of term frequency vectors for the specified document.
		/// The array contains a vector for each vectorized field in the document.
		/// Each vector vector contains term numbers and frequencies for all terms
		/// in a given vectorized field.
		/// If no such fields existed, the method returns null.
		/// </summary>
		/// <throws>  IOException </throws>
		public override TermFreqVector[] GetTermFreqVectors(int docNumber)
		{
			EnsureOpen();
			
			TermVectorsReader termVectorsReader = GetTermVectorsReader();
			if (termVectorsReader == null)
				return null;
			
			return termVectorsReader.Get(docNumber);
		}
		
		/// <summary> Return the name of the segment this reader is reading.</summary>
		public virtual System.String GetSegmentName()
		{
			return core.segment;
		}
		
		/// <summary> Return the SegmentInfo of the segment this reader is reading.</summary>
		internal virtual SegmentInfo GetSegmentInfo()
		{
			return si;
		}
		
		internal virtual void  SetSegmentInfo(SegmentInfo info)
		{
			si = info;
		}
		
		internal virtual void  StartCommit()
		{
            rollbackSegmentInfo = (SegmentInfo)si.Clone();
			rollbackHasChanges = hasChanges;
			rollbackDeletedDocsDirty = deletedDocsDirty;
			rollbackNormsDirty = normsDirty;
			rollbackPendingDeleteCount = pendingDeleteCount;
			System.Collections.IEnumerator it = norms.Values.GetEnumerator();
			while (it.MoveNext())
			{
				Norm norm = (Norm) it.Current;
				norm.rollbackDirty = norm.dirty;
			}
		}
		
		internal virtual void  RollbackCommit()
		{
            si.Reset(rollbackSegmentInfo);
			hasChanges = rollbackHasChanges;
			deletedDocsDirty = rollbackDeletedDocsDirty;
			normsDirty = rollbackNormsDirty;
			pendingDeleteCount = rollbackPendingDeleteCount;
			System.Collections.IEnumerator it = norms.Values.GetEnumerator();
			while (it.MoveNext())
			{
				Norm norm = (Norm) it.Current;
				norm.dirty = norm.rollbackDirty;
			}
		}
		
		/// <summary>Returns the directory this index resides in. </summary>
		public override Directory Directory()
		{
			// Don't ensureOpen here -- in certain cases, when a
			// cloned/reopened reader needs to commit, it may call
			// this method on the closed original reader
			return core.dir;
		}
		
		// This is necessary so that cloned SegmentReaders (which
		// share the underlying postings data) will map to the
		// same entry in the FieldCache.  See LUCENE-1579.
        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Index.IndexReader.GetFieldCacheKey()")]
		public override System.Object GetFieldCacheKey()
		{
			return core.freqStream;
		}

		public override object GetDeletesCacheKey() 
        {
            return deletedDocs;
        }


		public override long GetUniqueTermCount()
		{
			return core.GetTermsReader().Size();
		}
		
		/// <summary> Lotsa tests did hacks like:<br/>
		/// SegmentReader reader = (SegmentReader) IndexReader.open(dir);<br/>
		/// They broke. This method serves as a hack to keep hacks working
		/// </summary>
		public /*internal*/ static SegmentReader GetOnlySegmentReader(Directory dir)
		{
			return GetOnlySegmentReader(IndexReader.Open(dir));
		}
		
		public /*internal*/ static SegmentReader GetOnlySegmentReader(IndexReader reader)
		{
			if (reader is SegmentReader)
				return (SegmentReader) reader;
			
			if (reader is DirectoryReader)
			{
				IndexReader[] subReaders = reader.GetSequentialSubReaders();
				if (subReaders.Length != 1)
				{
					throw new System.ArgumentException(reader + " has " + subReaders.Length + " segments instead of exactly one");
				}
				
				return (SegmentReader) subReaders[0];
			}
			
			throw new System.ArgumentException(reader + " is not a SegmentReader or a single-segment DirectoryReader");
		}
		
		public override int GetTermInfosIndexDivisor()
		{
			return core.termsIndexDivisor;
		}
		static SegmentReader()
		{
			{
				try
				{
					System.String name = SupportClass.AppSettings.Get("Mono.Lucene.Net.SegmentReader.class", typeof(SegmentReader).FullName);
					IMPL = System.Type.GetType(name);
				}
				catch (System.Security.SecurityException se)
				{
					try
					{
						IMPL = System.Type.GetType(typeof(SegmentReader).FullName);
					}
					catch (System.Exception e)
					{
						throw new System.SystemException("cannot load default SegmentReader class: " + e, e);
					}
				}
				catch (System.Exception e)
				{
					throw new System.SystemException("cannot load SegmentReader class: " + e, e);
				}
			}
			{
				try
				{
					System.String name = SupportClass.AppSettings.Get("Mono.Lucene.Net.ReadOnlySegmentReader.class", typeof(ReadOnlySegmentReader).FullName);
					READONLY_IMPL = System.Type.GetType(name);
				}
				catch (System.Security.SecurityException se)
				{
					try
					{
						READONLY_IMPL = System.Type.GetType(typeof(ReadOnlySegmentReader).FullName);
					}
					catch (System.Exception e)
					{
						throw new System.SystemException("cannot load default ReadOnlySegmentReader class: " + e, e);
					}
				}
				catch (System.Exception e)
				{
					throw new System.SystemException("cannot load ReadOnlySegmentReader class: " + e, e);
				}
			}
		}

        public System.Collections.IDictionary norms_ForNUnit
        {
            get { return norms; }
        }

        public BitVector deletedDocs_ForNUnit
        {
            get { return deletedDocs; }
        }

        public CoreReaders core_ForNUnit
        {
            get { return core; }
        }

        public Ref deletedDocsRef_ForNUnit
        {
            get { return deletedDocsRef; }
        }
	}
}
