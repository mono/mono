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
using AlreadyClosedException = Mono.Lucene.Net.Store.AlreadyClosedException;
using Directory = Mono.Lucene.Net.Store.Directory;
using ArrayUtil = Mono.Lucene.Net.Util.ArrayUtil;
using Constants = Mono.Lucene.Net.Util.Constants;
using IndexSearcher = Mono.Lucene.Net.Search.IndexSearcher;
using Query = Mono.Lucene.Net.Search.Query;
using Scorer = Mono.Lucene.Net.Search.Scorer;
using Similarity = Mono.Lucene.Net.Search.Similarity;
using Weight = Mono.Lucene.Net.Search.Weight;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> This class accepts multiple added documents and directly
	/// writes a single segment file.  It does this more
	/// efficiently than creating a single segment per document
	/// (with DocumentWriter) and doing standard merges on those
	/// segments.
	/// 
	/// Each added document is passed to the {@link DocConsumer},
	/// which in turn processes the document and interacts with
	/// other consumers in the indexing chain.  Certain
	/// consumers, like {@link StoredFieldsWriter} and {@link
	/// TermVectorsTermsWriter}, digest a document and
	/// immediately write bytes to the "doc store" files (ie,
	/// they do not consume RAM per document, except while they
	/// are processing the document).
	/// 
	/// Other consumers, eg {@link FreqProxTermsWriter} and
	/// {@link NormsWriter}, buffer bytes in RAM and flush only
	/// when a new segment is produced.
	/// Once we have used our allowed RAM buffer, or the number
	/// of added docs is large enough (in the case we are
	/// flushing by doc count instead of RAM usage), we create a
	/// real segment and flush it to the Directory.
	/// 
	/// Threads:
	/// 
	/// Multiple threads are allowed into addDocument at once.
	/// There is an initial synchronized call to getThreadState
	/// which allocates a ThreadState for this thread.  The same
	/// thread will get the same ThreadState over time (thread
	/// affinity) so that if there are consistent patterns (for
	/// example each thread is indexing a different content
	/// source) then we make better use of RAM.  Then
	/// processDocument is called on that ThreadState without
	/// synchronization (most of the "heavy lifting" is in this
	/// call).  Finally the synchronized "finishDocument" is
	/// called to flush changes to the directory.
	/// 
	/// When flush is called by IndexWriter, or, we flush
	/// internally when autoCommit=false, we forcefully idle all
	/// threads and flush only once they are all idle.  This
	/// means you can call flush with a given thread even while
	/// other threads are actively adding/deleting documents.
	/// 
	/// 
	/// Exceptions:
	/// 
	/// Because this class directly updates in-memory posting
	/// lists, and flushes stored fields and term vectors
	/// directly to files in the directory, there are certain
	/// limited times when an exception can corrupt this state.
	/// For example, a disk full while flushing stored fields
	/// leaves this file in a corrupt state.  Or, an OOM
	/// exception while appending to the in-memory posting lists
	/// can corrupt that posting list.  We call such exceptions
	/// "aborting exceptions".  In these cases we must call
	/// abort() to discard all docs added since the last flush.
	/// 
	/// All other exceptions ("non-aborting exceptions") can
	/// still partially update the index structures.  These
	/// updates are consistent, but, they represent only a part
	/// of the document seen up until the exception was hit.
	/// When this happens, we immediately mark the document as
	/// deleted so that the document is always atomically ("all
	/// or none") added to the index.
	/// </summary>
	
	public sealed class DocumentsWriter
	{
		internal class AnonymousClassIndexingChain:IndexingChain
		{
			
			internal override DocConsumer GetChain(DocumentsWriter documentsWriter)
			{
				/*
				This is the current indexing chain:
				
				DocConsumer / DocConsumerPerThread
				--> code: DocFieldProcessor / DocFieldProcessorPerThread
				--> DocFieldConsumer / DocFieldConsumerPerThread / DocFieldConsumerPerField
				--> code: DocFieldConsumers / DocFieldConsumersPerThread / DocFieldConsumersPerField
				--> code: DocInverter / DocInverterPerThread / DocInverterPerField
				--> InvertedDocConsumer / InvertedDocConsumerPerThread / InvertedDocConsumerPerField
				--> code: TermsHash / TermsHashPerThread / TermsHashPerField
				--> TermsHashConsumer / TermsHashConsumerPerThread / TermsHashConsumerPerField
				--> code: FreqProxTermsWriter / FreqProxTermsWriterPerThread / FreqProxTermsWriterPerField
				--> code: TermVectorsTermsWriter / TermVectorsTermsWriterPerThread / TermVectorsTermsWriterPerField
				--> InvertedDocEndConsumer / InvertedDocConsumerPerThread / InvertedDocConsumerPerField
				--> code: NormsWriter / NormsWriterPerThread / NormsWriterPerField
				--> code: StoredFieldsWriter / StoredFieldsWriterPerThread / StoredFieldsWriterPerField
				*/
				
				// Build up indexing chain:
				
				TermsHashConsumer termVectorsWriter = new TermVectorsTermsWriter(documentsWriter);
				TermsHashConsumer freqProxWriter = new FreqProxTermsWriter();
				
				InvertedDocConsumer termsHash = new TermsHash(documentsWriter, true, freqProxWriter, new TermsHash(documentsWriter, false, termVectorsWriter, null));
				NormsWriter normsWriter = new NormsWriter();
				DocInverter docInverter = new DocInverter(termsHash, normsWriter);
				return new DocFieldProcessor(documentsWriter, docInverter);
			}
		}
		private void  InitBlock()
		{
			maxFieldLength = IndexWriter.DEFAULT_MAX_FIELD_LENGTH;
			maxBufferedDeleteTerms = IndexWriter.DEFAULT_MAX_BUFFERED_DELETE_TERMS;
			ramBufferSize = (long) (IndexWriter.DEFAULT_RAM_BUFFER_SIZE_MB * 1024 * 1024);
			waitQueuePauseBytes = (long) (ramBufferSize * 0.1);
			waitQueueResumeBytes = (long) (ramBufferSize * 0.05);
			freeTrigger = (long) (IndexWriter.DEFAULT_RAM_BUFFER_SIZE_MB * 1024 * 1024 * 1.05);
			freeLevel = (long) (IndexWriter.DEFAULT_RAM_BUFFER_SIZE_MB * 1024 * 1024 * 0.95);
			maxBufferedDocs = IndexWriter.DEFAULT_MAX_BUFFERED_DOCS;
			skipDocWriter = new SkipDocWriter();
            byteBlockAllocator = new ByteBlockAllocator(this, DocumentsWriter.BYTE_BLOCK_SIZE);
            perDocAllocator = new ByteBlockAllocator(this,DocumentsWriter.PER_DOC_BLOCK_SIZE);
			waitQueue = new WaitQueue(this);
		}
		
		internal IndexWriter writer;
		internal Directory directory;
		
		internal System.String segment; // Current segment we are working on
		private System.String docStoreSegment; // Current doc-store segment we are writing
		private int docStoreOffset; // Current starting doc-store offset of current segment
		
		private int nextDocID; // Next docID to be added
		private int numDocsInRAM; // # docs buffered in RAM
		internal int numDocsInStore; // # docs written to doc stores
		
		// Max # ThreadState instances; if there are more threads
		// than this they share ThreadStates
		private const int MAX_THREAD_STATE = 5;
		private DocumentsWriterThreadState[] threadStates = new DocumentsWriterThreadState[0];
		private System.Collections.Hashtable threadBindings = new System.Collections.Hashtable();
		
		private int pauseThreads; // Non-zero when we need all threads to
		// pause (eg to flush)
		internal bool flushPending; // True when a thread has decided to flush
		internal bool bufferIsFull; // True when it's time to write segment
		private bool aborting; // True if an abort is pending
		
		private DocFieldProcessor docFieldProcessor;
		
		internal System.IO.StreamWriter infoStream;
		internal int maxFieldLength;
		internal Similarity similarity;
		
		internal System.Collections.IList newFiles;
		
		internal class DocState
		{
			internal DocumentsWriter docWriter;
			internal Analyzer analyzer;
			internal int maxFieldLength;
			internal System.IO.StreamWriter infoStream;
			internal Similarity similarity;
			internal int docID;
			internal Document doc;
			internal System.String maxTermPrefix;
			
			// deprecated
            [Obsolete]
			internal bool allowMinus1Position;
			
			// Only called by asserts
			public bool TestPoint(System.String name)
			{
				return docWriter.writer.TestPoint(name);
			}

            public void Clear()
            {
                // don't hold onto doc nor analyzer, in case it is
                // largish:
                doc = null;
                analyzer = null;
            }
		}
		
		/// <summary>Consumer returns this on each doc.  This holds any
		/// state that must be flushed synchronized "in docID
		/// order".  We gather these and flush them in order. 
		/// </summary>
		internal abstract class DocWriter
		{
			internal DocWriter next;
			internal int docID;
			public abstract void  Finish();
			public abstract void  Abort();
			public abstract long SizeInBytes();
			
			internal void  SetNext(DocWriter next)
			{
				this.next = next;
			}
		}
		
        /**
        * Create and return a new DocWriterBuffer.
        */
        internal PerDocBuffer NewPerDocBuffer()
        {
            return new PerDocBuffer(this);
        }

        /**
        * RAMFile buffer for DocWriters.
        */
        internal class PerDocBuffer : Mono.Lucene.Net.Store.RAMFile
        {
            DocumentsWriter enclosingInstance;
            public PerDocBuffer(DocumentsWriter enclosingInstance)
            {
                this.enclosingInstance = enclosingInstance;
            }
            /**
            * Allocate bytes used from shared pool.
            */
            public override byte[] NewBuffer(int size)
            {
                System.Diagnostics.Debug.Assert(size == PER_DOC_BLOCK_SIZE);
                return enclosingInstance.perDocAllocator.GetByteBlock(false);
            }

            /**
            * Recycle the bytes used.
            */
            internal void Recycle()
            {
                lock (this)
                {
                    if (buffers.Count > 0)
                    {
                        SetLength(0);

                        // Recycle the blocks
                        enclosingInstance.perDocAllocator.RecycleByteBlocks(buffers);
                        buffers.Clear();
                        sizeInBytes = 0;

                        System.Diagnostics.Debug.Assert(NumBuffers() == 0);
                    }
                }
            }
        }

		/// <summary> The IndexingChain must define the {@link #GetChain(DocumentsWriter)} method
		/// which returns the DocConsumer that the DocumentsWriter calls to process the
		/// documents. 
		/// </summary>
		internal abstract class IndexingChain
		{
			internal abstract DocConsumer GetChain(DocumentsWriter documentsWriter);
		}
		
		internal static readonly IndexingChain DefaultIndexingChain;
		
		internal DocConsumer consumer;
		
		// Deletes done after the last flush; these are discarded
		// on abort
		private BufferedDeletes deletesInRAM = new BufferedDeletes(false);
		
		// Deletes done before the last flush; these are still
		// kept on abort
		private BufferedDeletes deletesFlushed = new BufferedDeletes(true);
		
		// The max number of delete terms that can be buffered before
		// they must be flushed to disk.
		private int maxBufferedDeleteTerms;
		
		// How much RAM we can use before flushing.  This is 0 if
		// we are flushing by doc count instead.
		private long ramBufferSize;
		private long waitQueuePauseBytes;
		private long waitQueueResumeBytes;
		
		// If we've allocated 5% over our RAM budget, we then
		// free down to 95%
		private long freeTrigger;
		private long freeLevel;
		
		// Flush @ this number of docs.  If ramBufferSize is
		// non-zero we will flush by RAM usage instead.
		private int maxBufferedDocs;
		
		private int flushedDocCount; // How many docs already flushed to index
		
		internal void  UpdateFlushedDocCount(int n)
		{
			lock (this)
			{
				flushedDocCount += n;
			}
		}
		internal int GetFlushedDocCount()
		{
			lock (this)
			{
				return flushedDocCount;
			}
		}
		internal void  SetFlushedDocCount(int n)
		{
			lock (this)
			{
				flushedDocCount = n;
			}
		}
		
		private bool closed;
		
		internal DocumentsWriter(Directory directory, IndexWriter writer, IndexingChain indexingChain)
		{
			InitBlock();
			this.directory = directory;
			this.writer = writer;
			this.similarity = writer.GetSimilarity();
			flushedDocCount = writer.MaxDoc();
			
			consumer = indexingChain.GetChain(this);
			if (consumer is DocFieldProcessor)
			{
				docFieldProcessor = (DocFieldProcessor) consumer;
			}
		}
		
		/// <summary>Returns true if any of the fields in the current
		/// buffered docs have omitTermFreqAndPositions==false 
		/// </summary>
		internal bool HasProx()
		{
			return (docFieldProcessor != null)?docFieldProcessor.fieldInfos.HasProx():true;
		}
		
		/// <summary>If non-null, various details of indexing are printed
		/// here. 
		/// </summary>
		internal void  SetInfoStream(System.IO.StreamWriter infoStream)
		{
			lock (this)
			{
				this.infoStream = infoStream;
				for (int i = 0; i < threadStates.Length; i++)
					threadStates[i].docState.infoStream = infoStream;
			}
		}
		
		internal void  SetMaxFieldLength(int maxFieldLength)
		{
			lock (this)
			{
				this.maxFieldLength = maxFieldLength;
				for (int i = 0; i < threadStates.Length; i++)
					threadStates[i].docState.maxFieldLength = maxFieldLength;
			}
		}
		
		internal void  SetSimilarity(Similarity similarity)
		{
			lock (this)
			{
				this.similarity = similarity;
				for (int i = 0; i < threadStates.Length; i++)
					threadStates[i].docState.similarity = similarity;
			}
		}
		
		internal void  SetAllowMinus1Position()
		{
			lock (this)
			{
				for (int i = 0; i < threadStates.Length; i++)
					threadStates[i].docState.allowMinus1Position = true;
			}
		}
		
		/// <summary>Set how much RAM we can use before flushing. </summary>
		internal void  SetRAMBufferSizeMB(double mb)
		{
			lock (this)
			{
				if (mb == IndexWriter.DISABLE_AUTO_FLUSH)
				{
					ramBufferSize = IndexWriter.DISABLE_AUTO_FLUSH;
					waitQueuePauseBytes = 4 * 1024 * 1024;
					waitQueueResumeBytes = 2 * 1024 * 1024;
				}
				else
				{
					ramBufferSize = (long) (mb * 1024 * 1024);
					waitQueuePauseBytes = (long) (ramBufferSize * 0.1);
					waitQueueResumeBytes = (long) (ramBufferSize * 0.05);
					freeTrigger = (long) (1.05 * ramBufferSize);
					freeLevel = (long) (0.95 * ramBufferSize);
				}
			}
		}
		
		internal double GetRAMBufferSizeMB()
		{
			lock (this)
			{
				if (ramBufferSize == IndexWriter.DISABLE_AUTO_FLUSH)
				{
					return ramBufferSize;
				}
				else
				{
					return ramBufferSize / 1024.0 / 1024.0;
				}
			}
		}
		
		/// <summary>Set max buffered docs, which means we will flush by
		/// doc count instead of by RAM usage. 
		/// </summary>
		internal void  SetMaxBufferedDocs(int count)
		{
			maxBufferedDocs = count;
		}
		
		internal int GetMaxBufferedDocs()
		{
			return maxBufferedDocs;
		}
		
		/// <summary>Get current segment name we are writing. </summary>
		internal System.String GetSegment()
		{
			return segment;
		}
		
		/// <summary>Returns how many docs are currently buffered in RAM. </summary>
		internal int GetNumDocsInRAM()
		{
			return numDocsInRAM;
		}
		
		/// <summary>Returns the current doc store segment we are writing
		/// to.  This will be the same as segment when autoCommit
		/// * is true. 
		/// </summary>
		internal System.String GetDocStoreSegment()
		{
			lock (this)
			{
				return docStoreSegment;
			}
		}
		
		/// <summary>Returns the doc offset into the shared doc store for
		/// the current buffered docs. 
		/// </summary>
		internal int GetDocStoreOffset()
		{
			return docStoreOffset;
		}
		
		/// <summary>Closes the current open doc stores an returns the doc
		/// store segment name.  This returns null if there are *
		/// no buffered documents. 
		/// </summary>
		internal System.String CloseDocStore()
		{
			lock (this)
			{
				
				System.Diagnostics.Debug.Assert(AllThreadsIdle());
				
				if (infoStream != null)
					Message("closeDocStore: " + openFiles.Count + " files to flush to segment " + docStoreSegment + " numDocs=" + numDocsInStore);
				
				bool success = false;
				
				try
				{
					InitFlushState(true);
					closedFiles.Clear();
					
					consumer.CloseDocStore(flushState);
					System.Diagnostics.Debug.Assert(0 == openFiles.Count);
					
					System.String s = docStoreSegment;
					docStoreSegment = null;
					docStoreOffset = 0;
					numDocsInStore = 0;
					success = true;
					return s;
				}
				finally
				{
					if (!success)
					{
						Abort();
					}
				}
			}
		}
		
		private System.Collections.Generic.ICollection<string> abortedFiles; // List of files that were written before last abort()
		
		private SegmentWriteState flushState;

        internal System.Collections.Generic.ICollection<string> AbortedFiles()
		{
			return abortedFiles;
		}
		
		internal void  Message(System.String message)
		{
			if (infoStream != null)
				writer.Message("DW: " + message);
		}

        internal System.Collections.Generic.IList<string> openFiles = new System.Collections.Generic.List<string>();
        internal System.Collections.Generic.IList<string> closedFiles = new System.Collections.Generic.List<string>();
		
		/* Returns Collection of files in use by this instance,
		* including any flushed segments. */
		internal System.Collections.Generic.IList<string> OpenFiles()
		{
			lock (this)
			{
                string[] tmp = new string[openFiles.Count];
                openFiles.CopyTo(tmp, 0);
				return tmp;
			}
		}
		
		internal System.Collections.Generic.IList<string> ClosedFiles()
		{
            lock (this)
            {
                string[] tmp = new string[closedFiles.Count];
                closedFiles.CopyTo(tmp, 0);
                return tmp;
            }
		}
		
		internal void  AddOpenFile(System.String name)
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(!openFiles.Contains(name));
				openFiles.Add(name);
			}
		}
		
		internal void  RemoveOpenFile(System.String name)
		{
			lock (this)
			{
				System.Diagnostics.Debug.Assert(openFiles.Contains(name));
				openFiles.Remove(name);
				closedFiles.Add(name);
			}
		}
		
		internal void  SetAborting()
		{
			lock (this)
			{
				aborting = true;
			}
		}
		
		/// <summary>Called if we hit an exception at a bad time (when
		/// updating the index files) and must discard all
		/// currently buffered docs.  This resets our state,
		/// discarding any docs added since last flush. 
		/// </summary>
		internal void  Abort()
		{
			lock (this)
			{
				
				try
				{
					if (infoStream != null)
						Message("docWriter: now abort");
					
					// Forcefully remove waiting ThreadStates from line
					waitQueue.Abort();
					
					// Wait for all other threads to finish with
					// DocumentsWriter:
					PauseAllThreads();
					
					try
					{
						
						System.Diagnostics.Debug.Assert(0 == waitQueue.numWaiting);
						
						waitQueue.waitingBytes = 0;
						
						try
						{
							abortedFiles = OpenFiles();
						}
						catch (System.Exception t)
						{
							abortedFiles = null;
						}
						
						deletesInRAM.Clear();
                        deletesFlushed.Clear();
						openFiles.Clear();
						
						for (int i = 0; i < threadStates.Length; i++)
							try
							{
								threadStates[i].consumer.Abort();
							}
							catch (System.Exception t)
							{
							}
						
						try
						{
							consumer.Abort();
						}
						catch (System.Exception t)
						{
						}
						
						docStoreSegment = null;
						numDocsInStore = 0;
						docStoreOffset = 0;
						
						// Reset all postings data
						DoAfterFlush();
					}
					finally
					{
						ResumeAllThreads();
					}
				}
				finally
				{
					aborting = false;
					System.Threading.Monitor.PulseAll(this);
                    if (infoStream != null)
                    {
                        Message("docWriter: done abort; abortedFiles=" + abortedFiles);
                    }
				}
			}
		}
		
		/// <summary>Reset after a flush </summary>
		private void  DoAfterFlush()
		{
			// All ThreadStates should be idle when we are called
			System.Diagnostics.Debug.Assert(AllThreadsIdle());
			threadBindings.Clear();
			waitQueue.Reset();
			segment = null;
			numDocsInRAM = 0;
			nextDocID = 0;
			bufferIsFull = false;
			flushPending = false;
			for (int i = 0; i < threadStates.Length; i++)
				threadStates[i].DoAfterFlush();
			numBytesUsed = 0;
		}
		
		// Returns true if an abort is in progress
		internal bool PauseAllThreads()
		{
			lock (this)
			{
				pauseThreads++;
				while (!AllThreadsIdle())
				{
					try
					{
						System.Threading.Monitor.Wait(this);
					}
					catch (System.Threading.ThreadInterruptedException ie)
					{
						// In 3.0 we will change this to throw
						// InterruptedException instead
						SupportClass.ThreadClass.Current().Interrupt();
						throw new System.SystemException(ie.Message, ie);
					}
				}
				
				return aborting;
			}
		}
		
		internal void  ResumeAllThreads()
		{
			lock (this)
			{
				pauseThreads--;
				System.Diagnostics.Debug.Assert(pauseThreads >= 0);
				if (0 == pauseThreads)
					System.Threading.Monitor.PulseAll(this);
			}
		}
		
		private bool AllThreadsIdle()
		{
			lock (this)
			{
				for (int i = 0; i < threadStates.Length; i++)
					if (!threadStates[i].isIdle)
						return false;
				return true;
			}
		}
		
		internal bool AnyChanges()
		{
			lock (this)
			{
				return numDocsInRAM != 0 || deletesInRAM.numTerms != 0 || deletesInRAM.docIDs.Count != 0 || deletesInRAM.queries.Count != 0;
			}
		}
		
		private void  InitFlushState(bool onlyDocStore)
		{
			lock (this)
			{
				InitSegmentName(onlyDocStore);
				flushState = new SegmentWriteState(this, directory, segment, docStoreSegment, numDocsInRAM, numDocsInStore, writer.GetTermIndexInterval());
			}
		}
		
		/// <summary>Flush all pending docs to a new segment </summary>
		internal int Flush(bool closeDocStore)
		{
			lock (this)
			{
				
				System.Diagnostics.Debug.Assert(AllThreadsIdle());
				
				System.Diagnostics.Debug.Assert(numDocsInRAM > 0);
				
				System.Diagnostics.Debug.Assert(nextDocID == numDocsInRAM);
				System.Diagnostics.Debug.Assert(waitQueue.numWaiting == 0);
				System.Diagnostics.Debug.Assert(waitQueue.waitingBytes == 0);
				
				InitFlushState(false);
				
				docStoreOffset = numDocsInStore;
				
				if (infoStream != null)
					Message("flush postings as segment " + flushState.segmentName + " numDocs=" + numDocsInRAM);
				
				bool success = false;
				
				try
				{
					
					if (closeDocStore)
					{
						System.Diagnostics.Debug.Assert(flushState.docStoreSegmentName != null);
						System.Diagnostics.Debug.Assert(flushState.docStoreSegmentName.Equals(flushState.segmentName));
						CloseDocStore();
						flushState.numDocsInStore = 0;
					}
					
					System.Collections.Hashtable threads = new System.Collections.Hashtable();
					for (int i = 0; i < threadStates.Length; i++)
						threads[threadStates[i].consumer] = threadStates[i].consumer;
					consumer.Flush(threads, flushState);
					
					if (infoStream != null)
					{
                        SegmentInfo si = new SegmentInfo(flushState.segmentName, flushState.numDocs, directory);
                        long newSegmentSize = si.SizeInBytes();
                        System.String message = System.String.Format(nf, "  oldRAMSize={0:d} newFlushedSize={1:d} docs/MB={2:f} new/old={3:%}",
                            new System.Object[] { numBytesUsed, newSegmentSize, (numDocsInRAM / (newSegmentSize / 1024.0 / 1024.0)), (100.0 * newSegmentSize / numBytesUsed) });
						Message(message);
					}
					
					flushedDocCount += flushState.numDocs;
					
					DoAfterFlush();
					
					success = true;
				}
				finally
				{
					if (!success)
					{
						Abort();
					}
				}
				
				System.Diagnostics.Debug.Assert(waitQueue.waitingBytes == 0);
				
				return flushState.numDocs;
			}
		}

        internal System.Collections.ICollection GetFlushedFiles()
        {
            return flushState.flushedFiles;
        }
		
		/// <summary>Build compound file for the segment we just flushed </summary>
		internal void  CreateCompoundFile(System.String segment)
		{
			
			CompoundFileWriter cfsWriter = new CompoundFileWriter(directory, segment + "." + IndexFileNames.COMPOUND_FILE_EXTENSION);
			System.Collections.IEnumerator it = flushState.flushedFiles.GetEnumerator();
			while (it.MoveNext())
			{
				cfsWriter.AddFile((System.String) ((System.Collections.DictionaryEntry) it.Current).Key);
			}
			
			// Perform the merge
			cfsWriter.Close();
		}
		
		/// <summary>Set flushPending if it is not already set and returns
		/// whether it was set. This is used by IndexWriter to
		/// trigger a single flush even when multiple threads are
		/// trying to do so. 
		/// </summary>
		internal bool SetFlushPending()
		{
			lock (this)
			{
				if (flushPending)
					return false;
				else
				{
					flushPending = true;
					return true;
				}
			}
		}
		
		internal void  ClearFlushPending()
		{
			lock (this)
			{
				flushPending = false;
			}
		}
		
		internal void  PushDeletes()
		{
			lock (this)
			{
				deletesFlushed.Update(deletesInRAM);
			}
		}
		
		internal void  Close()
		{
			lock (this)
			{
				closed = true;
				System.Threading.Monitor.PulseAll(this);
			}
		}
		
		internal void  InitSegmentName(bool onlyDocStore)
		{
			lock (this)
			{
				if (segment == null && (!onlyDocStore || docStoreSegment == null))
				{
					segment = writer.NewSegmentName();
					System.Diagnostics.Debug.Assert(numDocsInRAM == 0);
				}
				if (docStoreSegment == null)
				{
					docStoreSegment = segment;
					System.Diagnostics.Debug.Assert(numDocsInStore == 0);
				}
			}
		}
		
		/// <summary>Returns a free (idle) ThreadState that may be used for
		/// indexing this one document.  This call also pauses if a
		/// flush is pending.  If delTerm is non-null then we
		/// buffer this deleted term after the thread state has
		/// been acquired. 
		/// </summary>
		internal DocumentsWriterThreadState GetThreadState(Document doc, Term delTerm)
		{
			lock (this)
			{
				
				// First, find a thread state.  If this thread already
				// has affinity to a specific ThreadState, use that one
				// again.
				DocumentsWriterThreadState state = (DocumentsWriterThreadState) threadBindings[SupportClass.ThreadClass.Current()];
				if (state == null)
				{
					
					// First time this thread has called us since last
					// flush.  Find the least loaded thread state:
					DocumentsWriterThreadState minThreadState = null;
					for (int i = 0; i < threadStates.Length; i++)
					{
						DocumentsWriterThreadState ts = threadStates[i];
						if (minThreadState == null || ts.numThreads < minThreadState.numThreads)
							minThreadState = ts;
					}
					if (minThreadState != null && (minThreadState.numThreads == 0 || threadStates.Length >= MAX_THREAD_STATE))
					{
						state = minThreadState;
						state.numThreads++;
					}
					else
					{
						// Just create a new "private" thread state
						DocumentsWriterThreadState[] newArray = new DocumentsWriterThreadState[1 + threadStates.Length];
						if (threadStates.Length > 0)
							Array.Copy(threadStates, 0, newArray, 0, threadStates.Length);
						state = newArray[threadStates.Length] = new DocumentsWriterThreadState(this);
						threadStates = newArray;
					}
					threadBindings[SupportClass.ThreadClass.Current()] = state;
				}
				
				// Next, wait until my thread state is idle (in case
				// it's shared with other threads) and for threads to
				// not be paused nor a flush pending:
				WaitReady(state);
				
				// Allocate segment name if this is the first doc since
				// last flush:
				InitSegmentName(false);
				
				state.isIdle = false;
				
				bool success = false;
				try
				{
					state.docState.docID = nextDocID;
					
					System.Diagnostics.Debug.Assert(writer.TestPoint("DocumentsWriter.ThreadState.init start"));
					
					if (delTerm != null)
					{
						AddDeleteTerm(delTerm, state.docState.docID);
						state.doFlushAfter = TimeToFlushDeletes();
					}
					
					System.Diagnostics.Debug.Assert(writer.TestPoint("DocumentsWriter.ThreadState.init after delTerm"));
					
					nextDocID++;
					numDocsInRAM++;
					
					// We must at this point commit to flushing to ensure we
					// always get N docs when we flush by doc count, even if
					// > 1 thread is adding documents:
					if (!flushPending && maxBufferedDocs != IndexWriter.DISABLE_AUTO_FLUSH && numDocsInRAM >= maxBufferedDocs)
					{
						flushPending = true;
						state.doFlushAfter = true;
					}
					
					success = true;
				}
				finally
				{
					if (!success)
					{
						// Forcefully idle this ThreadState:
						state.isIdle = true;
						System.Threading.Monitor.PulseAll(this);
						if (state.doFlushAfter)
						{
							state.doFlushAfter = false;
							flushPending = false;
						}
					}
				}
				
				return state;
			}
		}
		
		/// <summary>Returns true if the caller (IndexWriter) should now
		/// flush. 
		/// </summary>
		internal bool AddDocument(Document doc, Analyzer analyzer)
		{
			return UpdateDocument(doc, analyzer, null);
		}
		
		internal bool UpdateDocument(Term t, Document doc, Analyzer analyzer)
		{
			return UpdateDocument(doc, analyzer, t);
		}
		
		internal bool UpdateDocument(Document doc, Analyzer analyzer, Term delTerm)
		{
			
			// This call is synchronized but fast
			DocumentsWriterThreadState state = GetThreadState(doc, delTerm);
			
			DocState docState = state.docState;
			docState.doc = doc;
			docState.analyzer = analyzer;

            bool doReturnFalse = false; // {{Aroush-2.9}} to handle return from finally clause

			bool success = false;
			try
			{
				// This call is not synchronized and does all the
				// work
				DocWriter perDoc;
                try
                {
                    perDoc = state.consumer.ProcessDocument();
                }
                finally
                {
                    docState.Clear();
                }
				// This call is synchronized but fast
				FinishDocument(state, perDoc);
				success = true;
			}
			finally
			{
				if (!success)
				{
					lock (this)
					{
						
						if (aborting)
						{
							state.isIdle = true;
							System.Threading.Monitor.PulseAll(this);
							Abort();
						}
						else
						{
							skipDocWriter.docID = docState.docID;
							bool success2 = false;
							try
							{
								waitQueue.Add(skipDocWriter);
								success2 = true;
							}
							finally
							{
								if (!success2)
								{
									state.isIdle = true;
									System.Threading.Monitor.PulseAll(this);
									Abort();
									// return false; // {{Aroush-2.9}} this 'return false' is move to outside finally
                                    doReturnFalse = true;
								}
							}

                            if (!doReturnFalse)   // {{Aroush-2.9}} added because of the above 'return false' removal
                            {
								state.isIdle = true;
								System.Threading.Monitor.PulseAll(this);
							
								// If this thread state had decided to flush, we
								// must clear it so another thread can flush
								if (state.doFlushAfter)
								{
									state.doFlushAfter = false;
									flushPending = false;
									System.Threading.Monitor.PulseAll(this);
								}
								
								// Immediately mark this document as deleted
								// since likely it was partially added.  This
								// keeps indexing as "all or none" (atomic) when
								// adding a document:
								AddDeleteDocID(state.docState.docID);
                            }
						}
					}
				}
			}

            if (doReturnFalse)  // {{Aroush-2.9}} see comment abouve
            {
                return false;
            }

			return state.doFlushAfter || TimeToFlushDeletes();
		}
		
		// for testing
		internal int GetNumBufferedDeleteTerms()
		{
			lock (this)
			{
				return deletesInRAM.numTerms; 
			}
		}
		
		// for testing
		internal System.Collections.IDictionary GetBufferedDeleteTerms()
		{
			lock (this)
			{
				return deletesInRAM.terms;
			}
		}
		
		/// <summary>Called whenever a merge has completed and the merged segments had deletions </summary>
		internal void  RemapDeletes(SegmentInfos infos, int[][] docMaps, int[] delCounts, MergePolicy.OneMerge merge, int mergeDocCount)
		{
			lock (this)
			{
				if (docMaps == null)
				// The merged segments had no deletes so docIDs did not change and we have nothing to do
					return ;
				MergeDocIDRemapper mapper = new MergeDocIDRemapper(infos, docMaps, delCounts, merge, mergeDocCount);
				deletesInRAM.Remap(mapper, infos, docMaps, delCounts, merge, mergeDocCount);
				deletesFlushed.Remap(mapper, infos, docMaps, delCounts, merge, mergeDocCount);
				flushedDocCount -= mapper.docShift;
			}
		}
		
		private void  WaitReady(DocumentsWriterThreadState state)
		{
			lock (this)
			{
				
				while (!closed && ((state != null && !state.isIdle) || pauseThreads != 0 || flushPending || aborting))
				{
					try
					{
						System.Threading.Monitor.Wait(this);
					}
					catch (System.Threading.ThreadInterruptedException ie)
					{
						// In 3.0 we will change this to throw
						// InterruptedException instead
						SupportClass.ThreadClass.Current().Interrupt();
						throw new System.SystemException(ie.Message, ie);
					}
				}
				
				if (closed)
					throw new AlreadyClosedException("this IndexWriter is closed");
			}
		}
		
		internal bool BufferDeleteTerms(Term[] terms)
		{
			lock (this)
			{
				WaitReady(null);
				for (int i = 0; i < terms.Length; i++)
					AddDeleteTerm(terms[i], numDocsInRAM);
				return TimeToFlushDeletes();
			}
		}
		
		internal bool BufferDeleteTerm(Term term)
		{
			lock (this)
			{
				WaitReady(null);
				AddDeleteTerm(term, numDocsInRAM);
				return TimeToFlushDeletes();
			}
		}
		
		internal bool BufferDeleteQueries(Query[] queries)
		{
			lock (this)
			{
				WaitReady(null);
				for (int i = 0; i < queries.Length; i++)
					AddDeleteQuery(queries[i], numDocsInRAM);
				return TimeToFlushDeletes();
			}
		}
		
		internal bool BufferDeleteQuery(Query query)
		{
			lock (this)
			{
				WaitReady(null);
				AddDeleteQuery(query, numDocsInRAM);
				return TimeToFlushDeletes();
			}
		}
		
		internal bool DeletesFull()
		{
			lock (this)
			{
				return (ramBufferSize != IndexWriter.DISABLE_AUTO_FLUSH && (deletesInRAM.bytesUsed + deletesFlushed.bytesUsed + numBytesUsed) >= ramBufferSize) || (maxBufferedDeleteTerms != IndexWriter.DISABLE_AUTO_FLUSH && ((deletesInRAM.Size() + deletesFlushed.Size()) >= maxBufferedDeleteTerms));
			}
		}
		
		internal bool DoApplyDeletes()
		{
			lock (this)
			{
				// Very similar to deletesFull(), except we don't count
				// numBytesAlloc, because we are checking whether
				// deletes (alone) are consuming too many resources now
				// and thus should be applied.  We apply deletes if RAM
				// usage is > 1/2 of our allowed RAM buffer, to prevent
				// too-frequent flushing of a long tail of tiny segments
				// when merges (which always apply deletes) are
				// infrequent.
				return (ramBufferSize != IndexWriter.DISABLE_AUTO_FLUSH && (deletesInRAM.bytesUsed + deletesFlushed.bytesUsed) >= ramBufferSize / 2) || (maxBufferedDeleteTerms != IndexWriter.DISABLE_AUTO_FLUSH && ((deletesInRAM.Size() + deletesFlushed.Size()) >= maxBufferedDeleteTerms));
			}
		}
		
		private bool TimeToFlushDeletes()
		{
			lock (this)
			{
				return (bufferIsFull || DeletesFull()) && SetFlushPending();
			}
		}
		
		internal void  SetMaxBufferedDeleteTerms(int maxBufferedDeleteTerms)
		{
			this.maxBufferedDeleteTerms = maxBufferedDeleteTerms;
		}
		
		internal int GetMaxBufferedDeleteTerms()
		{
			return maxBufferedDeleteTerms;
		}
		
		internal bool HasDeletes()
		{
			lock (this)
			{
				return deletesFlushed.Any();
			}
		}
		
		internal bool ApplyDeletes(SegmentInfos infos)
		{
			lock (this)
			{
				
				if (!HasDeletes())
					return false;
				
				if (infoStream != null)
					Message("apply " + deletesFlushed.numTerms + " buffered deleted terms and " + deletesFlushed.docIDs.Count + " deleted docIDs and " + deletesFlushed.queries.Count + " deleted queries on " + (+ infos.Count) + " segments.");
				
				int infosEnd = infos.Count;
				
				int docStart = 0;
				bool any = false;
				for (int i = 0; i < infosEnd; i++)
				{
					
					// Make sure we never attempt to apply deletes to
					// segment in external dir
					System.Diagnostics.Debug.Assert(infos.Info(i).dir == directory);
					
					SegmentReader reader = writer.readerPool.Get(infos.Info(i), false);
					try
					{
						any |= ApplyDeletes(reader, docStart);
						docStart += reader.MaxDoc();
					}
					finally
					{
						writer.readerPool.Release(reader);
					}
				}
				
				deletesFlushed.Clear();
				
				return any;
			}
		}

        // used only by assert
        private Term lastDeleteTerm;

        // used only by assert
        private bool CheckDeleteTerm(Term term) 
        {
            if (term != null) {
                System.Diagnostics.Debug.Assert(lastDeleteTerm == null || term.CompareTo(lastDeleteTerm) > 0, "lastTerm=" + lastDeleteTerm + " vs term=" + term);
            }
            lastDeleteTerm = term;
            return true;
        }
		
		// Apply buffered delete terms, queries and docIDs to the
		// provided reader
		private bool ApplyDeletes(IndexReader reader, int docIDStart)
		{
			lock (this)
			{
				
				int docEnd = docIDStart + reader.MaxDoc();
				bool any = false;
				
                System.Diagnostics.Debug.Assert(CheckDeleteTerm(null));

				// Delete by term
                //System.Collections.IEnumerator iter = new System.Collections.Hashtable(deletesFlushed.terms).GetEnumerator();
				System.Collections.IEnumerator iter = deletesFlushed.terms.GetEnumerator();
				TermDocs docs = reader.TermDocs();
				try
				{
					while (iter.MoveNext())
					{
						System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) iter.Current;
						Term term = (Term) entry.Key;
						// LUCENE-2086: we should be iterating a TreeMap,
                        // here, so terms better be in order:
                        System.Diagnostics.Debug.Assert(CheckDeleteTerm(term));
						docs.Seek(term);
						int limit = ((BufferedDeletes.Num) entry.Value).GetNum();
						while (docs.Next())
						{
							int docID = docs.Doc();
							if (docIDStart + docID >= limit)
								break;
							reader.DeleteDocument(docID);
							any = true;
						}
					}
				}
				finally
				{
					docs.Close();
				}
				
				// Delete by docID
				iter = deletesFlushed.docIDs.GetEnumerator();
				while (iter.MoveNext())
				{
					int docID = ((System.Int32) iter.Current);
					if (docID >= docIDStart && docID < docEnd)
					{
						reader.DeleteDocument(docID - docIDStart);
						any = true;
					}
				}
				
				// Delete by query
				IndexSearcher searcher = new IndexSearcher(reader);
				iter = new System.Collections.Hashtable(deletesFlushed.queries).GetEnumerator();
				while (iter.MoveNext())
				{
					System.Collections.DictionaryEntry entry = (System.Collections.DictionaryEntry) iter.Current;
					Query query = (Query) entry.Key;
					int limit = ((System.Int32) entry.Value);
					Weight weight = query.Weight(searcher);
					Scorer scorer = weight.Scorer(reader, true, false);
					if (scorer != null)
					{
						while (true)
						{
							int doc = scorer.NextDoc();
							if (((long) docIDStart) + doc >= limit)
								break;
							reader.DeleteDocument(doc);
							any = true;
						}
					}
				}
				searcher.Close();
				return any;
			}
		}
		
		// Buffer a term in bufferedDeleteTerms, which records the
		// current number of documents buffered in ram so that the
		// delete term will be applied to those documents as well
		// as the disk segments.
		private void  AddDeleteTerm(Term term, int docCount)
		{
			lock (this)
			{
				BufferedDeletes.Num num = (BufferedDeletes.Num) deletesInRAM.terms[term];
				int docIDUpto = flushedDocCount + docCount;
				if (num == null)
					deletesInRAM.terms[term] = new BufferedDeletes.Num(docIDUpto);
				else
					num.SetNum(docIDUpto);
				deletesInRAM.numTerms++;
				
				deletesInRAM.AddBytesUsed(BYTES_PER_DEL_TERM + term.text.Length * CHAR_NUM_BYTE);
			}
		}
		
		// Buffer a specific docID for deletion.  Currently only
		// used when we hit a exception when adding a document
		private void  AddDeleteDocID(int docID)
		{
			lock (this)
			{
				deletesInRAM.docIDs.Add((System.Int32) (flushedDocCount + docID));
				deletesInRAM.AddBytesUsed(BYTES_PER_DEL_DOCID);
			}
		}
		
		private void  AddDeleteQuery(Query query, int docID)
		{
			lock (this)
			{
				deletesInRAM.queries[query] = (System.Int32) (flushedDocCount + docID);
				deletesInRAM.AddBytesUsed(BYTES_PER_DEL_QUERY);
			}
		}
		
		internal bool DoBalanceRAM()
		{
			lock (this)
			{
				return ramBufferSize != IndexWriter.DISABLE_AUTO_FLUSH && !bufferIsFull && (numBytesUsed + deletesInRAM.bytesUsed + deletesFlushed.bytesUsed >= ramBufferSize || numBytesAlloc >= freeTrigger);
			}
		}
		
		/// <summary>Does the synchronized work to finish/flush the
		/// inverted document. 
		/// </summary>
		private void  FinishDocument(DocumentsWriterThreadState perThread, DocWriter docWriter)
		{
			
			if (DoBalanceRAM())
			// Must call this w/o holding synchronized(this) else
			// we'll hit deadlock:
				BalanceRAM();
			
			lock (this)
			{
				
				System.Diagnostics.Debug.Assert(docWriter == null || docWriter.docID == perThread.docState.docID);
				
				if (aborting)
				{
					
					// We are currently aborting, and another thread is
					// waiting for me to become idle.  We just forcefully
					// idle this threadState; it will be fully reset by
					// abort()
					if (docWriter != null)
						try
						{
							docWriter.Abort();
						}
						catch (System.Exception t)
						{
						}
					
					perThread.isIdle = true;
					System.Threading.Monitor.PulseAll(this);
					return ;
				}
				
				bool doPause;
				
				if (docWriter != null)
					doPause = waitQueue.Add(docWriter);
				else
				{
					skipDocWriter.docID = perThread.docState.docID;
					doPause = waitQueue.Add(skipDocWriter);
				}
				
				if (doPause)
					WaitForWaitQueue();
				
				if (bufferIsFull && !flushPending)
				{
					flushPending = true;
					perThread.doFlushAfter = true;
				}
				
				perThread.isIdle = true;
				System.Threading.Monitor.PulseAll(this);
			}
		}
		
		internal void  WaitForWaitQueue()
		{
			lock (this)
			{
				do 
				{
					try
					{
						System.Threading.Monitor.Wait(this);
					}
					catch (System.Threading.ThreadInterruptedException ie)
					{
						// In 3.0 we will change this to throw
						// InterruptedException instead
						SupportClass.ThreadClass.Current().Interrupt();
						throw new System.SystemException(ie.Message, ie);
					}
				}
				while (!waitQueue.DoResume());
			}
		}
		
		internal class SkipDocWriter:DocWriter
		{
			public override void  Finish()
			{
			}
			public override void  Abort()
			{
			}
			public override long SizeInBytes()
			{
				return 0;
			}
		}
		internal SkipDocWriter skipDocWriter;
		
		internal long GetRAMUsed()
		{
			return numBytesUsed + deletesInRAM.bytesUsed + deletesFlushed.bytesUsed;
		}
		
		internal long numBytesAlloc;
		internal long numBytesUsed;
		
		internal System.Globalization.NumberFormatInfo nf = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
		
		// Coarse estimates used to measure RAM usage of buffered deletes
		internal const int OBJECT_HEADER_BYTES = 8;
		internal static readonly int POINTER_NUM_BYTE;
		internal const int INT_NUM_BYTE = 4;
		internal const int CHAR_NUM_BYTE = 2;
		
		/* Rough logic: HashMap has an array[Entry] w/ varying
		load factor (say 2 * POINTER).  Entry is object w/ Term
		key, BufferedDeletes.Num val, int hash, Entry next
		(OBJ_HEADER + 3*POINTER + INT).  Term is object w/
		String field and String text (OBJ_HEADER + 2*POINTER).
		We don't count Term's field since it's interned.
		Term's text is String (OBJ_HEADER + 4*INT + POINTER +
		OBJ_HEADER + string.length*CHAR).  BufferedDeletes.num is
		OBJ_HEADER + INT. */
		
		internal static readonly int BYTES_PER_DEL_TERM = 8 * POINTER_NUM_BYTE + 5 * OBJECT_HEADER_BYTES + 6 * INT_NUM_BYTE;
		
		/* Rough logic: del docIDs are List<Integer>.  Say list
		allocates ~2X size (2*POINTER).  Integer is OBJ_HEADER
		+ int */
		internal static readonly int BYTES_PER_DEL_DOCID = 2 * POINTER_NUM_BYTE + OBJECT_HEADER_BYTES + INT_NUM_BYTE;
		
		/* Rough logic: HashMap has an array[Entry] w/ varying
		load factor (say 2 * POINTER).  Entry is object w/
		Query key, Integer val, int hash, Entry next
		(OBJ_HEADER + 3*POINTER + INT).  Query we often
		undercount (say 24 bytes).  Integer is OBJ_HEADER + INT. */
		internal static readonly int BYTES_PER_DEL_QUERY = 5 * POINTER_NUM_BYTE + 2 * OBJECT_HEADER_BYTES + 2 * INT_NUM_BYTE + 24;
		
		/* Initial chunks size of the shared byte[] blocks used to
		store postings data */
		internal const int BYTE_BLOCK_SHIFT = 15;
		internal static readonly int BYTE_BLOCK_SIZE = 1 << BYTE_BLOCK_SHIFT;
		internal static readonly int BYTE_BLOCK_MASK = BYTE_BLOCK_SIZE - 1;
		internal static readonly int BYTE_BLOCK_NOT_MASK = ~ BYTE_BLOCK_MASK;
		
		internal class ByteBlockAllocator:ByteBlockPool.Allocator
		{
            public ByteBlockAllocator(DocumentsWriter enclosingInstance, int blockSize)
			{
                this.blockSize = blockSize;
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(DocumentsWriter enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private DocumentsWriter enclosingInstance;
			public DocumentsWriter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}

            int blockSize;
			internal System.Collections.ArrayList freeByteBlocks = new System.Collections.ArrayList();
            
			/* Allocate another byte[] from the shared pool */
			public /*internal*/ override byte[] GetByteBlock(bool trackAllocations)
			{
				lock (Enclosing_Instance)
				{
					int size = freeByteBlocks.Count;
					byte[] b;
					if (0 == size)
					{
						// Always record a block allocated, even if
						// trackAllocations is false.  This is necessary
						// because this block will be shared between
						// things that don't track allocations (term
						// vectors) and things that do (freq/prox
						// postings).
                        Enclosing_Instance.numBytesAlloc += blockSize;
						b = new byte[blockSize];
					}
					else
					{
						System.Object tempObject;
						tempObject = freeByteBlocks[size - 1];
						freeByteBlocks.RemoveAt(size - 1);
						b = (byte[]) tempObject;
					}
					if (trackAllocations)
						Enclosing_Instance.numBytesUsed += blockSize;
					System.Diagnostics.Debug.Assert(Enclosing_Instance.numBytesUsed <= Enclosing_Instance.numBytesAlloc);
					return b;
				}
			}
			
			/* Return byte[]'s to the pool */
			public /*internal*/ override void  RecycleByteBlocks(byte[][] blocks, int start, int end)
			{
				lock (Enclosing_Instance)
				{
                    for (int i = start; i < end; i++)
                    {
                        freeByteBlocks.Add(blocks[i]);
                        blocks[i] = null;
                    }
                    if (enclosingInstance.infoStream != null && blockSize != 1024)
                    {
                        enclosingInstance.Message("DW.recycleByteBlocks blockSize=" + blockSize + " count=" + (end - start) + " total now " + freeByteBlocks.Count);
                    }
				}
			}

            public /*internal*/ override void RecycleByteBlocks(System.Collections.ArrayList blocks)
            {
                lock (Enclosing_Instance)
                {
                    int size = blocks.Count;
                    for(int i=0;i<size;i++)
                        freeByteBlocks.Add(blocks[i]);
                }
            }
		}
		
		/* Initial chunks size of the shared int[] blocks used to
		store postings data */
		internal const int INT_BLOCK_SHIFT = 13;
		internal static readonly int INT_BLOCK_SIZE = 1 << INT_BLOCK_SHIFT;
		internal static readonly int INT_BLOCK_MASK = INT_BLOCK_SIZE - 1;
		
		private System.Collections.ArrayList freeIntBlocks = new System.Collections.ArrayList();
		
		/* Allocate another int[] from the shared pool */
		internal int[] GetIntBlock(bool trackAllocations)
		{
			lock (this)
			{
				int size = freeIntBlocks.Count;
				int[] b;
				if (0 == size)
				{
					// Always record a block allocated, even if
					// trackAllocations is false.  This is necessary
					// because this block will be shared between
					// things that don't track allocations (term
					// vectors) and things that do (freq/prox
					// postings).
					numBytesAlloc += INT_BLOCK_SIZE * INT_NUM_BYTE;
					b = new int[INT_BLOCK_SIZE];
				}
				else
				{
					System.Object tempObject;
					tempObject = freeIntBlocks[size - 1];
					freeIntBlocks.RemoveAt(size - 1);
					b = (int[]) tempObject;
				}
				if (trackAllocations)
					numBytesUsed += INT_BLOCK_SIZE * INT_NUM_BYTE;
				System.Diagnostics.Debug.Assert(numBytesUsed <= numBytesAlloc);
				return b;
			}
		}
		
		internal void  BytesAllocated(long numBytes)
		{
			lock (this)
			{
				numBytesAlloc += numBytes;
			}
		}
		
		internal void  BytesUsed(long numBytes)
		{
			lock (this)
			{
				numBytesUsed += numBytes;
				System.Diagnostics.Debug.Assert(numBytesUsed <= numBytesAlloc);
			}
		}
		
		/* Return int[]s to the pool */
		internal void  RecycleIntBlocks(int[][] blocks, int start, int end)
		{
			lock (this)
			{
                for (int i = start; i < end; i++)
                {
                    freeIntBlocks.Add(blocks[i]);
                    blocks[i] = null;
                }
                if (infoStream != null)
                {
                    Message("DW.recycleIntBlocks count=" + (end - start) + " total now " + freeIntBlocks.Count);
                }
			}
		}
		
		internal ByteBlockAllocator byteBlockAllocator;

        internal static int PER_DOC_BLOCK_SIZE = 1024;

        ByteBlockAllocator perDocAllocator;
		
		/* Initial chunk size of the shared char[] blocks used to
		store term text */
		internal const int CHAR_BLOCK_SHIFT = 14;
		internal static readonly int CHAR_BLOCK_SIZE = 1 << CHAR_BLOCK_SHIFT;
		internal static readonly int CHAR_BLOCK_MASK = CHAR_BLOCK_SIZE - 1;
		
		internal static readonly int MAX_TERM_LENGTH = CHAR_BLOCK_SIZE - 1;
		
		private System.Collections.ArrayList freeCharBlocks = new System.Collections.ArrayList();
		
		/* Allocate another char[] from the shared pool */
		internal char[] GetCharBlock()
		{
			lock (this)
			{
				int size = freeCharBlocks.Count;
				char[] c;
				if (0 == size)
				{
					numBytesAlloc += CHAR_BLOCK_SIZE * CHAR_NUM_BYTE;
					c = new char[CHAR_BLOCK_SIZE];
				}
				else
				{
					System.Object tempObject;
					tempObject = freeCharBlocks[size - 1];
					freeCharBlocks.RemoveAt(size - 1);
					c = (char[]) tempObject;
				}
				// We always track allocations of char blocks, for now,
				// because nothing that skips allocation tracking
				// (currently only term vectors) uses its own char
				// blocks.
				numBytesUsed += CHAR_BLOCK_SIZE * CHAR_NUM_BYTE;
				System.Diagnostics.Debug.Assert(numBytesUsed <= numBytesAlloc);
				return c;
			}
		}
		
		/* Return char[]s to the pool */
		internal void  RecycleCharBlocks(char[][] blocks, int numBlocks)
		{
			lock (this)
			{
                for (int i = 0; i < numBlocks; i++)
                {
                    freeCharBlocks.Add(blocks[i]);
                    blocks[i] = null;
                }
                if (infoStream != null)
                {
                    Message("DW.recycleCharBlocks count=" + numBlocks + " total now " + freeCharBlocks.Count);
                }
			}
		}
		
		internal System.String ToMB(long v)
		{
			return System.String.Format(nf, "{0:f}", new System.Object[] { (v / 1024F / 1024F) });
		}


        /* We have four pools of RAM: Postings, byte blocks
        * (holds freq/prox posting data), char blocks (holds
        * characters in the term) and per-doc buffers (stored fields/term vectors).  
        * Different docs require varying amount of storage from 
        * these four classes.
        * 
        * For example, docs with many unique single-occurrence
        * short terms will use up the Postings RAM and hardly any
        * of the other two.  Whereas docs with very large terms
        * will use alot of char blocks RAM and relatively less of
        * the other two.  This method just frees allocations from
        * the pools once we are over-budget, which balances the
        * pools to match the current docs. */
		internal void  BalanceRAM()
		{
			
			// We flush when we've used our target usage
			long flushTrigger = ramBufferSize;
			
			long deletesRAMUsed = deletesInRAM.bytesUsed + deletesFlushed.bytesUsed;
			
			if (numBytesAlloc + deletesRAMUsed > freeTrigger)
			{
				
				if (infoStream != null)
					Message(
                        "  RAM: now balance allocations: usedMB=" + ToMB(numBytesUsed) + 
                        " vs trigger=" + ToMB(flushTrigger) + 
                        " allocMB=" + ToMB(numBytesAlloc) + 
                        " deletesMB=" + ToMB(deletesRAMUsed) + 
                        " vs trigger=" + ToMB(freeTrigger) + 
                        " byteBlockFree=" + ToMB(byteBlockAllocator.freeByteBlocks.Count * BYTE_BLOCK_SIZE) +
                        " perDocFree=" + ToMB(perDocAllocator.freeByteBlocks.Count * PER_DOC_BLOCK_SIZE) +
                        " charBlockFree=" + ToMB(freeCharBlocks.Count * CHAR_BLOCK_SIZE * CHAR_NUM_BYTE));
				
				long startBytesAlloc = numBytesAlloc + deletesRAMUsed;
				
				int iter = 0;
				
				// We free equally from each pool in 32 KB
				// chunks until we are below our threshold
				// (freeLevel)
				
				bool any = true;
				
				while (numBytesAlloc + deletesRAMUsed > freeLevel)
				{
					
					lock (this)
					{
                        if (0 == perDocAllocator.freeByteBlocks.Count
                              && 0 == byteBlockAllocator.freeByteBlocks.Count
                              && 0 == freeCharBlocks.Count
                              && 0 == freeIntBlocks.Count
                              && !any)
						{
							// Nothing else to free -- must flush now.
							bufferIsFull = numBytesUsed + deletesRAMUsed > flushTrigger;
							if (infoStream != null)
							{
                                if (bufferIsFull)
									Message("    nothing to free; now set bufferIsFull");
								else
									Message("    nothing to free");
							}
							System.Diagnostics.Debug.Assert(numBytesUsed <= numBytesAlloc);
							break;
						}
						
						if ((0 == iter % 5) && byteBlockAllocator.freeByteBlocks.Count > 0)
						{
							byteBlockAllocator.freeByteBlocks.RemoveAt(byteBlockAllocator.freeByteBlocks.Count - 1);
							numBytesAlloc -= BYTE_BLOCK_SIZE;
						}
						
						if ((1 == iter % 5) && freeCharBlocks.Count > 0)
						{
							freeCharBlocks.RemoveAt(freeCharBlocks.Count - 1);
							numBytesAlloc -= CHAR_BLOCK_SIZE * CHAR_NUM_BYTE;
						}
						
						if ((2 == iter % 5) && freeIntBlocks.Count > 0)
						{
							freeIntBlocks.RemoveAt(freeIntBlocks.Count - 1);
							numBytesAlloc -= INT_BLOCK_SIZE * INT_NUM_BYTE;
						}

                        if ((3 == iter % 5) && perDocAllocator.freeByteBlocks.Count > 0)
                        {
                            // Remove upwards of 32 blocks (each block is 1K)
                            for (int i = 0; i < 32; ++i)
                            {
                                perDocAllocator.freeByteBlocks.RemoveAt(perDocAllocator.freeByteBlocks.Count - 1);
                                numBytesAlloc -= PER_DOC_BLOCK_SIZE;
                                if (perDocAllocator.freeByteBlocks.Count == 0)
                                {
                                    break;
                                }
                            }
                        }
					}
					
					if ((4 == iter % 5) && any)
					// Ask consumer to free any recycled state
						any = consumer.FreeRAM();
					
					iter++;
				}
				
				if (infoStream != null)
					Message(System.String.Format(nf, "    after free: freedMB={0:f} usedMB={1:f} allocMB={2:f}",
						new System.Object[] { ((startBytesAlloc - numBytesAlloc) / 1024.0 / 1024.0), (numBytesUsed / 1024.0 / 1024.0), (numBytesAlloc / 1024.0 / 1024.0) }));
            }
			else
			{
				// If we have not crossed the 100% mark, but have
				// crossed the 95% mark of RAM we are actually
				// using, go ahead and flush.  This prevents
				// over-allocating and then freeing, with every
				// flush.
				lock (this)
				{
					
					if (numBytesUsed + deletesRAMUsed > flushTrigger)
					{
						if (infoStream != null)
							Message(System.String.Format(nf, "  RAM: now flush @ usedMB={0:f} allocMB={1:f} triggerMB={2:f}",
								new object[] { (numBytesUsed / 1024.0 / 1024.0), (numBytesAlloc / 1024.0 / 1024.0), (flushTrigger / 1024.0 / 1024.0) }));
						
						bufferIsFull = true;
					}
				}
			}
		}
		
		internal WaitQueue waitQueue;
		
		internal class WaitQueue
		{
			private void  InitBlock(DocumentsWriter enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private DocumentsWriter enclosingInstance;
			public DocumentsWriter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal DocWriter[] waiting;
			internal int nextWriteDocID;
			internal int nextWriteLoc;
			internal int numWaiting;
			internal long waitingBytes;
			
			public WaitQueue(DocumentsWriter enclosingInstance)
			{
				InitBlock(enclosingInstance);
				waiting = new DocWriter[10];
			}
			
			internal void  Reset()
			{
				lock (this)
				{
					// NOTE: nextWriteLoc doesn't need to be reset
					System.Diagnostics.Debug.Assert(numWaiting == 0);
					System.Diagnostics.Debug.Assert(waitingBytes == 0);
					nextWriteDocID = 0;
				}
			}
			
			internal bool DoResume()
			{
				lock (this)
				{
					return waitingBytes <= Enclosing_Instance.waitQueueResumeBytes;
				}
			}
			
			internal bool DoPause()
			{
				lock (this)
				{
					return waitingBytes > Enclosing_Instance.waitQueuePauseBytes;
				}
			}
			
			internal void  Abort()
			{
				lock (this)
				{
					int count = 0;
					for (int i = 0; i < waiting.Length; i++)
					{
						DocWriter doc = waiting[i];
						if (doc != null)
						{
							doc.Abort();
							waiting[i] = null;
							count++;
						}
					}
					waitingBytes = 0;
					System.Diagnostics.Debug.Assert(count == numWaiting);
					numWaiting = 0;
				}
			}
			
			private void  WriteDocument(DocWriter doc)
			{
                System.Diagnostics.Debug.Assert(doc == Enclosing_Instance.skipDocWriter || nextWriteDocID == doc.docID);
				bool success = false;
				try
				{
					doc.Finish();
					nextWriteDocID++;
					Enclosing_Instance.numDocsInStore++;
					nextWriteLoc++;
					System.Diagnostics.Debug.Assert(nextWriteLoc <= waiting.Length);
					if (nextWriteLoc == waiting.Length)
						nextWriteLoc = 0;
					success = true;
				}
				finally
				{
					if (!success)
						Enclosing_Instance.SetAborting();
				}
			}
			
			public bool Add(DocWriter doc)
			{
				lock (this)
				{
					
					System.Diagnostics.Debug.Assert(doc.docID >= nextWriteDocID);
					
					if (doc.docID == nextWriteDocID)
					{
						WriteDocument(doc);
						while (true)
						{
							doc = waiting[nextWriteLoc];
							if (doc != null)
							{
								numWaiting--;
								waiting[nextWriteLoc] = null;
								waitingBytes -= doc.SizeInBytes();
								WriteDocument(doc);
							}
							else
								break;
						}
					}
					else
					{
						
						// I finished before documents that were added
						// before me.  This can easily happen when I am a
						// small doc and the docs before me were large, or,
						// just due to luck in the thread scheduling.  Just
						// add myself to the queue and when that large doc
						// finishes, it will flush me:
						int gap = doc.docID - nextWriteDocID;
						if (gap >= waiting.Length)
						{
							// Grow queue
							DocWriter[] newArray = new DocWriter[ArrayUtil.GetNextSize(gap)];
							System.Diagnostics.Debug.Assert(nextWriteLoc >= 0);
							Array.Copy(waiting, nextWriteLoc, newArray, 0, waiting.Length - nextWriteLoc);
							Array.Copy(waiting, 0, newArray, waiting.Length - nextWriteLoc, nextWriteLoc);
							nextWriteLoc = 0;
							waiting = newArray;
							gap = doc.docID - nextWriteDocID;
						}
						
						int loc = nextWriteLoc + gap;
						if (loc >= waiting.Length)
							loc -= waiting.Length;
						
						// We should only wrap one time
						System.Diagnostics.Debug.Assert(loc < waiting.Length);
						
						// Nobody should be in my spot!
						System.Diagnostics.Debug.Assert(waiting [loc] == null);
						waiting[loc] = doc;
						numWaiting++;
						waitingBytes += doc.SizeInBytes();
					}
					
					return DoPause();
				}
			}
		}
		static DocumentsWriter()
		{
			DefaultIndexingChain = new AnonymousClassIndexingChain();
			POINTER_NUM_BYTE = Constants.JRE_IS_64BIT?8:4;
		}

        public static int BYTE_BLOCK_SIZE_ForNUnit
        {
            get { return BYTE_BLOCK_SIZE; }
        }

        public static int CHAR_BLOCK_SIZE_ForNUnit
        {
            get { return CHAR_BLOCK_SIZE; }
        }
	}
}
