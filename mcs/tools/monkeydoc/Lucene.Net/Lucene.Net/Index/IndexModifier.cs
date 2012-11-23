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
using Directory = Mono.Lucene.Net.Store.Directory;
using FSDirectory = Mono.Lucene.Net.Store.FSDirectory;
using LockObtainFailedException = Mono.Lucene.Net.Store.LockObtainFailedException;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> <p/>[Note that as of <b>2.1</b>, all but one of the
	/// methods in this class are available via {@link
	/// IndexWriter}.  The one method that is not available is
	/// {@link #DeleteDocument(int)}.]<p/>
	/// 
	/// A class to modify an index, i.e. to delete and add documents. This
	/// class hides {@link IndexReader} and {@link IndexWriter} so that you
	/// do not need to care about implementation details such as that adding
	/// documents is done via IndexWriter and deletion is done via IndexReader.
	/// 
	/// <p/>Note that you cannot create more than one <code>IndexModifier</code> object
	/// on the same directory at the same time.
	/// 
	/// <p/>Example usage:
	/// 
	/// <!-- ======================================================== -->
	/// <!-- = Java Sourcecode to HTML automatically converted code = -->
	/// <!-- =   Java2Html Converter V4.1 2004 by Markus Gebhard  markus@jave.de   = -->
	/// <!-- =     Further information: http://www.java2html.de     = -->
	/// <div align="left" class="java">
	/// <table border="0" cellpadding="3" cellspacing="0" bgcolor="#ffffff">
	/// <tr>
	/// <!-- start source code -->
	/// <td nowrap="nowrap" valign="top" align="left">
	/// <code>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">Analyzer&#160;analyzer&#160;=&#160;</font><font color="#7f0055"><b>new&#160;</b></font><font color="#000000">StandardAnalyzer</font><font color="#000000">()</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#3f7f5f">//&#160;create&#160;an&#160;index&#160;in&#160;/tmp/index,&#160;overwriting&#160;an&#160;existing&#160;one:</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">IndexModifier&#160;indexModifier&#160;=&#160;</font><font color="#7f0055"><b>new&#160;</b></font><font color="#000000">IndexModifier</font><font color="#000000">(</font><font color="#2a00ff">&#34;/tmp/index&#34;</font><font color="#000000">,&#160;analyzer,&#160;</font><font color="#7f0055"><b>true</b></font><font color="#000000">)</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">Document&#160;doc&#160;=&#160;</font><font color="#7f0055"><b>new&#160;</b></font><font color="#000000">Document</font><font color="#000000">()</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">doc.add</font><font color="#000000">(</font><font color="#7f0055"><b>new&#160;</b></font><font color="#000000">Field</font><font color="#000000">(</font><font color="#2a00ff">&#34;id&#34;</font><font color="#000000">,&#160;</font><font color="#2a00ff">&#34;1&#34;</font><font color="#000000">,&#160;Field.Store.YES,&#160;Field.Index.NOT_ANALYZED</font><font color="#000000">))</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">doc.add</font><font color="#000000">(</font><font color="#7f0055"><b>new&#160;</b></font><font color="#000000">Field</font><font color="#000000">(</font><font color="#2a00ff">&#34;body&#34;</font><font color="#000000">,&#160;</font><font color="#2a00ff">&#34;a&#160;simple&#160;test&#34;</font><font color="#000000">,&#160;Field.Store.YES,&#160;Field.Index.ANALYZED</font><font color="#000000">))</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">indexModifier.addDocument</font><font color="#000000">(</font><font color="#000000">doc</font><font color="#000000">)</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#7f0055"><b>int&#160;</b></font><font color="#000000">deleted&#160;=&#160;indexModifier.delete</font><font color="#000000">(</font><font color="#7f0055"><b>new&#160;</b></font><font color="#000000">Term</font><font color="#000000">(</font><font color="#2a00ff">&#34;id&#34;</font><font color="#000000">,&#160;</font><font color="#2a00ff">&#34;1&#34;</font><font color="#000000">))</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">System.out.println</font><font color="#000000">(</font><font color="#2a00ff">&#34;Deleted&#160;&#34;&#160;</font><font color="#000000">+&#160;deleted&#160;+&#160;</font><font color="#2a00ff">&#34;&#160;document&#34;</font><font color="#000000">)</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">indexModifier.flush</font><font color="#000000">()</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">System.out.println</font><font color="#000000">(</font><font color="#000000">indexModifier.docCount</font><font color="#000000">()&#160;</font><font color="#000000">+&#160;</font><font color="#2a00ff">&#34;&#160;docs&#160;in&#160;index&#34;</font><font color="#000000">)</font><font color="#000000">;</font><br/>
	/// <font color="#ffffff">&#160;&#160;&#160;&#160;</font><font color="#000000">indexModifier.close</font><font color="#000000">()</font><font color="#000000">;</font></code>
	/// </td>
	/// <!-- end source code -->
	/// </tr>
	/// </table>
	/// </div>
	/// <!-- =       END of automatically generated HTML code       = -->
	/// <!-- ======================================================== -->
	/// 
	/// <p/>Not all methods of IndexReader and IndexWriter are offered by this
	/// class. If you need access to additional methods, either use those classes
	/// directly or implement your own class that extends <code>IndexModifier</code>.
	/// 
	/// <p/>Although an instance of this class can be used from more than one
	/// thread, you will not get the best performance. You might want to use
	/// IndexReader and IndexWriter directly for that (but you will need to
	/// care about synchronization yourself then).
	/// 
	/// <p/>While you can freely mix calls to add() and delete() using this class,
	/// you should batch you calls for best performance. For example, if you
	/// want to update 20 documents, you should first delete all those documents,
	/// then add all the new documents.
	/// 
	/// </summary>
	/// <deprecated> Please use {@link IndexWriter} instead.
	/// </deprecated>
    [Obsolete("Please use IndexWriter instead.")]
	public class IndexModifier
	{
		private void  InitBlock()
		{
			maxBufferedDocs = IndexWriter.DEFAULT_MAX_BUFFERED_DOCS;
			maxFieldLength = IndexWriter.DEFAULT_MAX_FIELD_LENGTH;
			mergeFactor = IndexWriter.DEFAULT_MERGE_FACTOR;
		}
		
		protected internal IndexWriter indexWriter = null;
		protected internal IndexReader indexReader = null;
		
		protected internal Directory directory = null;
		protected internal Analyzer analyzer = null;
		protected internal bool open = false, closeDir = false;
		
		// Lucene defaults:
		protected internal System.IO.StreamWriter infoStream = null;
		protected internal bool useCompoundFile = true;
		protected internal int maxBufferedDocs;
		protected internal int maxFieldLength;
		protected internal int mergeFactor;
		
		/// <summary> Open an index with write access.
		/// 
		/// </summary>
		/// <param name="directory">the index directory
		/// </param>
		/// <param name="analyzer">the analyzer to use for adding new documents
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite the existing one;
		/// <code>false</code> to append to the existing index
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public IndexModifier(Directory directory, Analyzer analyzer, bool create)
		{
			InitBlock();
			Init(directory, analyzer, create);
		}
		
		/// <summary> Open an index with write access.
		/// 
		/// </summary>
		/// <param name="dirName">the index directory
		/// </param>
		/// <param name="analyzer">the analyzer to use for adding new documents
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite the existing one;
		/// <code>false</code> to append to the existing index
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public IndexModifier(System.String dirName, Analyzer analyzer, bool create)
		{
			InitBlock();
			Directory dir = FSDirectory.GetDirectory(dirName);
			this.closeDir = true;
			Init(dir, analyzer, create);
		}
		
		/// <summary> Open an index with write access.
		/// 
		/// </summary>
		/// <param name="file">the index directory
		/// </param>
		/// <param name="analyzer">the analyzer to use for adding new documents
		/// </param>
		/// <param name="create"><code>true</code> to create the index or overwrite the existing one;
		/// <code>false</code> to append to the existing index
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public IndexModifier(System.IO.FileInfo file, Analyzer analyzer, bool create)
		{
			InitBlock();
			Directory dir = FSDirectory.GetDirectory(file);
			this.closeDir = true;
			Init(dir, analyzer, create);
		}
		
		/// <summary> Initialize an IndexWriter.</summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		protected internal virtual void  Init(Directory directory, Analyzer analyzer, bool create)
		{
			this.directory = directory;
			lock (this.directory)
			{
				this.analyzer = analyzer;
				indexWriter = new IndexWriter(directory, analyzer, create, IndexWriter.MaxFieldLength.LIMITED);
				open = true;
			}
		}
		
		/// <summary> Throw an IllegalStateException if the index is closed.</summary>
		/// <throws>  IllegalStateException </throws>
		protected internal virtual void  AssureOpen()
		{
			if (!open)
			{
				throw new System.SystemException("Index is closed");
			}
		}
		
		/// <summary> Close the IndexReader and open an IndexWriter.</summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		protected internal virtual void  CreateIndexWriter()
		{
			if (indexWriter == null)
			{
				if (indexReader != null)
				{
					indexReader.Close();
					indexReader = null;
				}
				indexWriter = new IndexWriter(directory, analyzer, false, new IndexWriter.MaxFieldLength(maxFieldLength));
				// IndexModifier cannot use ConcurrentMergeScheduler
				// because it synchronizes on the directory which can
				// cause deadlock
				indexWriter.SetMergeScheduler(new SerialMergeScheduler());
				indexWriter.SetInfoStream(infoStream);
				indexWriter.SetUseCompoundFile(useCompoundFile);
				if (maxBufferedDocs != IndexWriter.DISABLE_AUTO_FLUSH)
					indexWriter.SetMaxBufferedDocs(maxBufferedDocs);
				indexWriter.SetMergeFactor(mergeFactor);
			}
		}
		
		/// <summary> Close the IndexWriter and open an IndexReader.</summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		protected internal virtual void  CreateIndexReader()
		{
			if (indexReader == null)
			{
				if (indexWriter != null)
				{
					indexWriter.Close();
					indexWriter = null;
				}
				indexReader = IndexReader.Open(directory);
			}
		}
		
		/// <summary> Make sure all changes are written to disk.</summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  Flush()
		{
			lock (directory)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					indexWriter.Close();
					indexWriter = null;
					CreateIndexWriter();
				}
				else
				{
					indexReader.Close();
					indexReader = null;
					CreateIndexReader();
				}
			}
		}
		
		/// <summary> Adds a document to this index, using the provided analyzer instead of the
		/// one specific in the constructor.  If the document contains more than
		/// {@link #SetMaxFieldLength(int)} terms for a given field, the remainder are
		/// discarded.
		/// </summary>
		/// <seealso cref="IndexWriter.AddDocument(Document, Analyzer)">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  AddDocument(Document doc, Analyzer docAnalyzer)
		{
			lock (directory)
			{
				AssureOpen();
				CreateIndexWriter();
				if (docAnalyzer != null)
					indexWriter.AddDocument(doc, docAnalyzer);
				else
					indexWriter.AddDocument(doc);
			}
		}
		
		/// <summary> Adds a document to this index.  If the document contains more than
		/// {@link #SetMaxFieldLength(int)} terms for a given field, the remainder are
		/// discarded.
		/// </summary>
		/// <seealso cref="IndexWriter.AddDocument(Document)">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  AddDocument(Document doc)
		{
			AddDocument(doc, null);
		}
		
		/// <summary> Deletes all documents containing <code>term</code>.
		/// This is useful if one uses a document field to hold a unique ID string for
		/// the document.  Then to delete such a document, one merely constructs a
		/// term with the appropriate field and the unique ID string as its text and
		/// passes it to this method.  Returns the number of documents deleted.
		/// </summary>
		/// <returns> the number of documents deleted
		/// </returns>
		/// <seealso cref="IndexReader.DeleteDocuments(Term)">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
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
			lock (directory)
			{
				AssureOpen();
				CreateIndexReader();
				return indexReader.DeleteDocuments(term);
			}
		}
		
		/// <summary> Deletes the document numbered <code>docNum</code>.</summary>
		/// <seealso cref="IndexReader.DeleteDocument(int)">
		/// </seealso>
		/// <throws>  StaleReaderException if the index has changed </throws>
		/// <summary>  since this reader was opened
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IllegalStateException if the index is closed </throws>
		public virtual void  DeleteDocument(int docNum)
		{
			lock (directory)
			{
				AssureOpen();
				CreateIndexReader();
				indexReader.DeleteDocument(docNum);
			}
		}
		
		
		/// <summary> Returns the number of documents currently in this
		/// index.  If the writer is currently open, this returns
		/// {@link IndexWriter#DocCount()}, else {@link
		/// IndexReader#NumDocs()}.  But, note that {@link
		/// IndexWriter#DocCount()} does not take deletions into
		/// account, unlike {@link IndexReader#numDocs}.
		/// </summary>
		/// <throws>  IllegalStateException if the index is closed </throws>
		public virtual int DocCount()
		{
			lock (directory)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					return indexWriter.DocCount();
				}
				else
				{
					return indexReader.NumDocs();
				}
			}
		}
		
		/// <summary> Merges all segments together into a single segment, optimizing an index
		/// for search.
		/// </summary>
		/// <seealso cref="IndexWriter.Optimize()">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  Optimize()
		{
			lock (directory)
			{
				AssureOpen();
				CreateIndexWriter();
				indexWriter.Optimize();
			}
		}
		
		/// <summary> If non-null, information about merges and a message when
		/// {@link #GetMaxFieldLength()} is reached will be printed to this.
		/// <p/>Example: <tt>index.setInfoStream(System.err);</tt>
		/// </summary>
		/// <seealso cref="IndexWriter.SetInfoStream(PrintStream)">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
		public virtual void  SetInfoStream(System.IO.StreamWriter infoStream)
		{
			lock (directory)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					indexWriter.SetInfoStream(infoStream);
				}
				this.infoStream = infoStream;
			}
		}
		
		/// <seealso cref="IndexModifier.SetInfoStream(PrintStream)">
		/// </seealso>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual System.IO.StreamWriter GetInfoStream()
		{
			lock (directory)
			{
				AssureOpen();
				CreateIndexWriter();
				return indexWriter.GetInfoStream();
			}
		}
		
		/// <summary> Setting to turn on usage of a compound file. When on, multiple files
		/// for each segment are merged into a single file once the segment creation
		/// is finished. This is done regardless of what directory is in use.
		/// </summary>
		/// <seealso cref="IndexWriter.SetUseCompoundFile(boolean)">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
		public virtual void  SetUseCompoundFile(bool useCompoundFile)
		{
			lock (directory)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					indexWriter.SetUseCompoundFile(useCompoundFile);
				}
				this.useCompoundFile = useCompoundFile;
			}
		}
		
		/// <seealso cref="IndexModifier.SetUseCompoundFile(boolean)">
		/// </seealso>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual bool GetUseCompoundFile()
		{
			lock (directory)
			{
				AssureOpen();
				CreateIndexWriter();
				return indexWriter.GetUseCompoundFile();
			}
		}
		
		/// <summary> The maximum number of terms that will be indexed for a single field in a
		/// document.  This limits the amount of memory required for indexing, so that
		/// collections with very large files will not crash the indexing process by
		/// running out of memory.<p/>
		/// Note that this effectively truncates large documents, excluding from the
		/// index terms that occur further in the document.  If you know your source
		/// documents are large, be sure to set this value high enough to accommodate
		/// the expected size.  If you set it to Integer.MAX_VALUE, then the only limit
		/// is your memory, but you should anticipate an OutOfMemoryError.<p/>
		/// By default, no more than 10,000 terms will be indexed for a field.
		/// </summary>
		/// <seealso cref="IndexWriter.SetMaxFieldLength(int)">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
		public virtual void  SetMaxFieldLength(int maxFieldLength)
		{
			lock (directory)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					indexWriter.SetMaxFieldLength(maxFieldLength);
				}
				this.maxFieldLength = maxFieldLength;
			}
		}
		
		/// <seealso cref="IndexModifier.SetMaxFieldLength(int)">
		/// </seealso>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual int GetMaxFieldLength()
		{
			lock (directory)
			{
				AssureOpen();
				CreateIndexWriter();
				return indexWriter.GetMaxFieldLength();
			}
		}
		
		/// <summary> Determines the minimal number of documents required before the buffered
		/// in-memory documents are merging and a new Segment is created.
		/// Since Documents are merged in a {@link Mono.Lucene.Net.Store.RAMDirectory},
		/// large value gives faster indexing.  At the same time, mergeFactor limits
		/// the number of files open in a FSDirectory.
		/// 
		/// <p/>The default value is 10.
		/// 
		/// </summary>
		/// <seealso cref="IndexWriter.SetMaxBufferedDocs(int)">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
		/// <throws>  IllegalArgumentException if maxBufferedDocs is smaller than 2 </throws>
		public virtual void  SetMaxBufferedDocs(int maxBufferedDocs)
		{
			lock (directory)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					indexWriter.SetMaxBufferedDocs(maxBufferedDocs);
				}
				this.maxBufferedDocs = maxBufferedDocs;
			}
		}
		
		/// <seealso cref="IndexModifier.SetMaxBufferedDocs(int)">
		/// </seealso>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual int GetMaxBufferedDocs()
		{
			lock (directory)
			{
				AssureOpen();
				CreateIndexWriter();
				return indexWriter.GetMaxBufferedDocs();
			}
		}
		
		/// <summary> Determines how often segment indices are merged by addDocument().  With
		/// smaller values, less RAM is used while indexing, and searches on
		/// unoptimized indices are faster, but indexing speed is slower.  With larger
		/// values, more RAM is used during indexing, and while searches on unoptimized
		/// indices are slower, indexing is faster.  Thus larger values (&gt; 10) are best
		/// for batch index creation, and smaller values (&lt; 10) for indices that are
		/// interactively maintained.
		/// <p/>This must never be less than 2.  The default value is 10.
		/// 
		/// </summary>
		/// <seealso cref="IndexWriter.SetMergeFactor(int)">
		/// </seealso>
		/// <throws>  IllegalStateException if the index is closed </throws>
		public virtual void  SetMergeFactor(int mergeFactor)
		{
			lock (directory)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					indexWriter.SetMergeFactor(mergeFactor);
				}
				this.mergeFactor = mergeFactor;
			}
		}
		
		/// <seealso cref="IndexModifier.SetMergeFactor(int)">
		/// </seealso>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  LockObtainFailedException if another writer </throws>
		/// <summary>  has this index open (<code>write.lock</code> could not
		/// be obtained)
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual int GetMergeFactor()
		{
			lock (directory)
			{
				AssureOpen();
				CreateIndexWriter();
				return indexWriter.GetMergeFactor();
			}
		}
		
		/// <summary> Close this index, writing all pending changes to disk.
		/// 
		/// </summary>
		/// <throws>  IllegalStateException if the index has been closed before already </throws>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  Close()
		{
			lock (directory)
			{
				if (!open)
					throw new System.SystemException("Index is closed already");
				if (indexWriter != null)
				{
					indexWriter.Close();
					indexWriter = null;
				}
				else if (indexReader != null)
				{
					indexReader.Close();
					indexReader = null;
				}
				open = false;
				if (closeDir)
				{
					directory.Close();
				}
				closeDir = false;
			}
		}
		
		public override System.String ToString()
		{
			return "Index@" + directory;
		}
		
		/*
		// used as an example in the javadoc:
		public static void main(String[] args) throws IOException {
		Analyzer analyzer = new StandardAnalyzer();
		// create an index in /tmp/index, overwriting an existing one:
		IndexModifier indexModifier = new IndexModifier("/tmp/index", analyzer, true);
		Document doc = new Document();
		doc.add(new Fieldable("id", "1", Fieldable.Store.YES, Fieldable.Index.NOT_ANALYZED));
		doc.add(new Fieldable("body", "a simple test", Fieldable.Store.YES, Fieldable.Index.ANALYZED));
		indexModifier.addDocument(doc);
		int deleted = indexModifier.delete(new Term("id", "1"));
		System.out.println("Deleted " + deleted + " document");
		indexModifier.flush();
		System.out.println(indexModifier.docCount() + " docs in index");
		indexModifier.close();
		}*/
	}
}
