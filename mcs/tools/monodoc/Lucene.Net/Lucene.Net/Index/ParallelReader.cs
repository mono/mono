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
using FieldSelectorResult = Mono.Lucene.Net.Documents.FieldSelectorResult;
using Fieldable = Mono.Lucene.Net.Documents.Fieldable;

namespace Mono.Lucene.Net.Index
{
	
	
	/// <summary>An IndexReader which reads multiple, parallel indexes.  Each index added
	/// must have the same number of documents, but typically each contains
	/// different fields.  Each document contains the union of the fields of all
	/// documents with the same document number.  When searching, matches for a
	/// query term are from the first index added that has the field.
	/// 
	/// <p/>This is useful, e.g., with collections that have large fields which
	/// change rarely and small fields that change more frequently.  The smaller
	/// fields may be re-indexed in a new index and both indexes may be searched
	/// together.
	/// 
	/// <p/><strong>Warning:</strong> It is up to you to make sure all indexes
	/// are created and modified the same way. For example, if you add
	/// documents to one index, you need to add the same documents in the
	/// same order to the other indexes. <em>Failure to do so will result in
	/// undefined behavior</em>.
	/// </summary>
	public class ParallelReader:IndexReader, System.ICloneable
	{
		private System.Collections.ArrayList readers = new System.Collections.ArrayList();
		private System.Collections.IList decrefOnClose = new System.Collections.ArrayList(); // remember which subreaders to decRef on close
		internal bool incRefReaders = false;
		private System.Collections.SortedList fieldToReader = new System.Collections.SortedList();
		private System.Collections.IDictionary readerToFields = new System.Collections.Hashtable();
		private System.Collections.IList storedFieldReaders = new System.Collections.ArrayList();
		
		private int maxDoc;
		private int numDocs;
		private bool hasDeletions;
		
		/// <summary>Construct a ParallelReader. 
		/// <p/>Note that all subreaders are closed if this ParallelReader is closed.<p/>
		/// </summary>
		public ParallelReader():this(true)
		{
		}
		
		/// <summary>Construct a ParallelReader. </summary>
		/// <param name="closeSubReaders">indicates whether the subreaders should be closed
		/// when this ParallelReader is closed
		/// </param>
		public ParallelReader(bool closeSubReaders):base()
		{
			this.incRefReaders = !closeSubReaders;
		}
		
		/// <summary>Add an IndexReader.</summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  Add(IndexReader reader)
		{
			EnsureOpen();
			Add(reader, false);
		}
		
		/// <summary>Add an IndexReader whose stored fields will not be returned.  This can
		/// accellerate search when stored fields are only needed from a subset of
		/// the IndexReaders.
		/// 
		/// </summary>
		/// <throws>  IllegalArgumentException if not all indexes contain the same number </throws>
		/// <summary>     of documents
		/// </summary>
		/// <throws>  IllegalArgumentException if not all indexes have the same value </throws>
		/// <summary>     of {@link IndexReader#MaxDoc()}
		/// </summary>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public virtual void  Add(IndexReader reader, bool ignoreStoredFields)
		{
			
			EnsureOpen();
			if (readers.Count == 0)
			{
				this.maxDoc = reader.MaxDoc();
				this.numDocs = reader.NumDocs();
				this.hasDeletions = reader.HasDeletions();
			}
			
			if (reader.MaxDoc() != maxDoc)
			// check compatibility
				throw new System.ArgumentException("All readers must have same maxDoc: " + maxDoc + "!=" + reader.MaxDoc());
			if (reader.NumDocs() != numDocs)
				throw new System.ArgumentException("All readers must have same numDocs: " + numDocs + "!=" + reader.NumDocs());
			
			System.Collections.Generic.ICollection<string> fields = reader.GetFieldNames(IndexReader.FieldOption.ALL);
			readerToFields[reader] = fields;
			System.Collections.IEnumerator i = fields.GetEnumerator();
			while (i.MoveNext())
			{
				// update fieldToReader map
				System.String field = (System.String) i.Current;
				if (fieldToReader[field] == null)
					fieldToReader[field] = reader;
			}
			
			if (!ignoreStoredFields)
				storedFieldReaders.Add(reader); // add to storedFieldReaders
			readers.Add(reader);
			
			if (incRefReaders)
			{
				reader.IncRef();
			}
			decrefOnClose.Add(incRefReaders);
		}
		
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
		
		/// <summary> Tries to reopen the subreaders.
		/// <br/>
		/// If one or more subreaders could be re-opened (i. e. subReader.reopen() 
		/// returned a new instance != subReader), then a new ParallelReader instance 
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
		
		protected internal virtual IndexReader DoReopen(bool doClone)
		{
			EnsureOpen();
			
			bool reopened = false;
			System.Collections.IList newReaders = new System.Collections.ArrayList();
			
			bool success = false;
			
			try
			{
				for (int i = 0; i < readers.Count; i++)
				{
					IndexReader oldReader = (IndexReader) readers[i];
					IndexReader newReader = null;
					if (doClone)
					{
						newReader = (IndexReader) oldReader.Clone();
					}
					else
					{
						newReader = oldReader.Reopen();
					}
					newReaders.Add(newReader);
					// if at least one of the subreaders was updated we remember that
					// and return a new ParallelReader
					if (newReader != oldReader)
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
					for (int i = 0; i < newReaders.Count; i++)
					{
						IndexReader r = (IndexReader) newReaders[i];
						if (r != readers[i])
						{
							try
							{
								r.Close();
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
				System.Collections.IList newDecrefOnClose = new System.Collections.ArrayList();
				ParallelReader pr = new ParallelReader();
				for (int i = 0; i < readers.Count; i++)
				{
					IndexReader oldReader = (IndexReader) readers[i];
					IndexReader newReader = (IndexReader) newReaders[i];
					if (newReader == oldReader)
					{
						newDecrefOnClose.Add(true);
						newReader.IncRef();
					}
					else
					{
						// this is a new subreader instance, so on close() we don't
						// decRef but close it 
						newDecrefOnClose.Add(false);
					}
					pr.Add(newReader, !storedFieldReaders.Contains(oldReader));
				}
				pr.decrefOnClose = newDecrefOnClose;
				pr.incRefReaders = incRefReaders;
				return pr;
			}
			else
			{
				// No subreader was refreshed
				return this;
			}
		}
		
		
		public override int NumDocs()
		{
			// Don't call ensureOpen() here (it could affect performance)
			return numDocs;
		}
		
		public override int MaxDoc()
		{
			// Don't call ensureOpen() here (it could affect performance)
			return maxDoc;
		}
		
		public override bool HasDeletions()
		{
			// Don't call ensureOpen() here (it could affect performance)
			return hasDeletions;
		}
		
		// check first reader
		public override bool IsDeleted(int n)
		{
			// Don't call ensureOpen() here (it could affect performance)
			if (readers.Count > 0)
				return ((IndexReader) readers[0]).IsDeleted(n);
			return false;
		}
		
		// delete in all readers
		protected internal override void  DoDelete(int n)
		{
			for (int i = 0; i < readers.Count; i++)
			{
				((IndexReader) readers[i]).DeleteDocument(n);
			}
			hasDeletions = true;
		}
		
		// undeleteAll in all readers
		protected internal override void  DoUndeleteAll()
		{
			for (int i = 0; i < readers.Count; i++)
			{
				((IndexReader) readers[i]).UndeleteAll();
			}
			hasDeletions = false;
		}
		
		// append fields from storedFieldReaders
		public override Document Document(int n, FieldSelector fieldSelector)
		{
			EnsureOpen();
			Document result = new Document();
			for (int i = 0; i < storedFieldReaders.Count; i++)
			{
				IndexReader reader = (IndexReader) storedFieldReaders[i];
				
				bool include = (fieldSelector == null);
				if (!include)
				{
					System.Collections.IEnumerator it = ((System.Collections.ICollection) readerToFields[reader]).GetEnumerator();
					while (it.MoveNext())
					{
						if (fieldSelector.Accept((System.String) it.Current) != FieldSelectorResult.NO_LOAD)
						{
							include = true;
							break;
						}
					}
				}
				if (include)
				{
					System.Collections.IEnumerator fieldIterator = reader.Document(n, fieldSelector).GetFields().GetEnumerator();
					while (fieldIterator.MoveNext())
					{
						result.Add((Fieldable) fieldIterator.Current);
					}
				}
			}
			return result;
		}
		
		// get all vectors
		public override TermFreqVector[] GetTermFreqVectors(int n)
		{
			EnsureOpen();
			System.Collections.ArrayList results = new System.Collections.ArrayList();
            System.Collections.IEnumerator i = new System.Collections.Hashtable(fieldToReader).GetEnumerator();
			while (i.MoveNext())
			{
				System.Collections.DictionaryEntry e = (System.Collections.DictionaryEntry) i.Current;
				System.String field = (System.String) e.Key;
				IndexReader reader = (IndexReader) e.Value;
				TermFreqVector vector = reader.GetTermFreqVector(n, field);
				if (vector != null)
					results.Add(vector);
			}
			return (TermFreqVector[]) results.ToArray(typeof(TermFreqVector));
		}
		
		public override TermFreqVector GetTermFreqVector(int n, System.String field)
		{
			EnsureOpen();
			IndexReader reader = ((IndexReader) fieldToReader[field]);
			return reader == null?null:reader.GetTermFreqVector(n, field);
		}
		
		
		public override void  GetTermFreqVector(int docNumber, System.String field, TermVectorMapper mapper)
		{
			EnsureOpen();
			IndexReader reader = ((IndexReader) fieldToReader[field]);
			if (reader != null)
			{
				reader.GetTermFreqVector(docNumber, field, mapper);
			}
		}
		
		public override void  GetTermFreqVector(int docNumber, TermVectorMapper mapper)
		{
			EnsureOpen();

            System.Collections.IEnumerator i = new System.Collections.Hashtable(fieldToReader).GetEnumerator();
			while (i.MoveNext())
			{
				System.Collections.DictionaryEntry e = (System.Collections.DictionaryEntry) i.Current;
				System.String field = (System.String) e.Key;
				IndexReader reader = (IndexReader) e.Value;
				reader.GetTermFreqVector(docNumber, field, mapper);
			}
		}
		
		public override bool HasNorms(System.String field)
		{
			EnsureOpen();
			IndexReader reader = ((IndexReader) fieldToReader[field]);
			return reader == null?false:reader.HasNorms(field);
		}
		
		public override byte[] Norms(System.String field)
		{
			EnsureOpen();
			IndexReader reader = ((IndexReader) fieldToReader[field]);
			return reader == null?null:reader.Norms(field);
		}
		
		public override void  Norms(System.String field, byte[] result, int offset)
		{
			EnsureOpen();
			IndexReader reader = ((IndexReader) fieldToReader[field]);
			if (reader != null)
				reader.Norms(field, result, offset);
		}
		
		protected internal override void  DoSetNorm(int n, System.String field, byte value_Renamed)
		{
			IndexReader reader = ((IndexReader) fieldToReader[field]);
			if (reader != null)
				reader.DoSetNorm(n, field, value_Renamed);
		}
		
		public override TermEnum Terms()
		{
			EnsureOpen();
			return new ParallelTermEnum(this);
		}
		
		public override TermEnum Terms(Term term)
		{
			EnsureOpen();
			return new ParallelTermEnum(this, term);
		}
		
		public override int DocFreq(Term term)
		{
			EnsureOpen();
			IndexReader reader = ((IndexReader) fieldToReader[term.Field()]);
			return reader == null?0:reader.DocFreq(term);
		}
		
		public override TermDocs TermDocs(Term term)
		{
			EnsureOpen();
			return new ParallelTermDocs(this, term);
		}
		
		public override TermDocs TermDocs()
		{
			EnsureOpen();
			return new ParallelTermDocs(this);
		}
		
		public override TermPositions TermPositions(Term term)
		{
			EnsureOpen();
			return new ParallelTermPositions(this, term);
		}
		
		public override TermPositions TermPositions()
		{
			EnsureOpen();
			return new ParallelTermPositions(this);
		}
		
		/// <summary> Checks recursively if all subreaders are up to date. </summary>
		public override bool IsCurrent()
		{
			for (int i = 0; i < readers.Count; i++)
			{
				if (!((IndexReader) readers[i]).IsCurrent())
				{
					return false;
				}
			}
			
			// all subreaders are up to date
			return true;
		}
		
		/// <summary> Checks recursively if all subindexes are optimized </summary>
		public override bool IsOptimized()
		{
			for (int i = 0; i < readers.Count; i++)
			{
				if (!((IndexReader) readers[i]).IsOptimized())
				{
					return false;
				}
			}
			
			// all subindexes are optimized
			return true;
		}
		
		
		/// <summary>Not implemented.</summary>
		/// <throws>  UnsupportedOperationException </throws>
		public override long GetVersion()
		{
			throw new System.NotSupportedException("ParallelReader does not support this method.");
		}
		
		// for testing
		public /*internal*/ virtual IndexReader[] GetSubReaders()
		{
			return (IndexReader[]) readers.ToArray(typeof(IndexReader));
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
			for (int i = 0; i < readers.Count; i++)
				((IndexReader) readers[i]).Commit(commitUserData);
		}
		
		protected internal override void  DoClose()
		{
			lock (this)
			{
				for (int i = 0; i < readers.Count; i++)
				{
					if (((System.Boolean) decrefOnClose[i]))
					{
						((IndexReader) readers[i]).DecRef();
					}
					else
					{
						((IndexReader) readers[i]).Close();
					}
				}
			}

            Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.Purge(this);
		}

        public override System.Collections.Generic.ICollection<string> GetFieldNames(IndexReader.FieldOption fieldNames)
		{
			EnsureOpen();
            System.Collections.Generic.List<string> fieldSet = new System.Collections.Generic.List<string>();
			for (int i = 0; i < readers.Count; i++)
			{
				IndexReader reader = ((IndexReader) readers[i]);
				System.Collections.Generic.ICollection<string> names = reader.GetFieldNames(fieldNames);
                fieldSet.AddRange(names);
			}
			return fieldSet;
		}
		
		private class ParallelTermEnum:TermEnum
		{
			private void  InitBlock(ParallelReader enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ParallelReader enclosingInstance;
			public ParallelReader Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private System.String field;
			private System.Collections.IEnumerator fieldIterator;
			private TermEnum termEnum;
			
			public ParallelTermEnum(ParallelReader enclosingInstance)
			{
				InitBlock(enclosingInstance);
				try
				{
					field = ((System.String) Enclosing_Instance.fieldToReader.GetKey(0));
				}
				catch (ArgumentOutOfRangeException e)
				{
					// No fields, so keep field == null, termEnum == null
					return;
				}
				if (field != null)
					termEnum = ((IndexReader) Enclosing_Instance.fieldToReader[field]).Terms();
			}
			
			public ParallelTermEnum(ParallelReader enclosingInstance, Term term)
			{
				InitBlock(enclosingInstance);
				field = term.Field();
				IndexReader reader = ((IndexReader) Enclosing_Instance.fieldToReader[field]);
				if (reader != null)
					termEnum = reader.Terms(term);
			}
			
			public override bool Next()
			{
				if (termEnum == null)
					return false;
				
				// another term in this field?
				if (termEnum.Next() && (System.Object) termEnum.Term().Field() == (System.Object) field)
					return true; // yes, keep going
				
				termEnum.Close(); // close old termEnum
				
				// find the next field with terms, if any
				if (fieldIterator == null)
				{
                    System.Collections.Comparer comparer = System.Collections.Comparer.Default;
                    System.Collections.SortedList newList = new System.Collections.SortedList();
                    if (Enclosing_Instance.fieldToReader != null)
                    {
                        if (Enclosing_Instance.fieldToReader.Count > 0)
                        {
                            int index = 0;
                            while (comparer.Compare(Enclosing_Instance.fieldToReader.GetKey(index), field) < 0)
                                index++;
                            for (; index < Enclosing_Instance.fieldToReader.Count; index++)
                            {
                                newList.Add(Enclosing_Instance.fieldToReader.GetKey(index), Enclosing_Instance.fieldToReader[Enclosing_Instance.fieldToReader.GetKey(index)]);
                            }
                        }
                    }

                    fieldIterator = newList.Keys.GetEnumerator();
                    fieldIterator.MoveNext();
					System.Object generatedAux = fieldIterator.Current; // Skip field to get next one
				}
				while (fieldIterator.MoveNext())
				{
					field = ((System.String) fieldIterator.Current);
					termEnum = ((IndexReader) Enclosing_Instance.fieldToReader[field]).Terms(new Term(field));
					Term term = termEnum.Term();
					if (term != null && (System.Object) term.Field() == (System.Object) field)
						return true;
					else
						termEnum.Close();
				}
				
				return false; // no more fields
			}
			
			public override Term Term()
			{
				if (termEnum == null)
					return null;
				
				return termEnum.Term();
			}
			
			public override int DocFreq()
			{
				if (termEnum == null)
					return 0;
				
				return termEnum.DocFreq();
			}
			
			public override void  Close()
			{
				if (termEnum != null)
					termEnum.Close();
			}
		}
		
		// wrap a TermDocs in order to support seek(Term)
		private class ParallelTermDocs : TermDocs
		{
			private void  InitBlock(ParallelReader enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ParallelReader enclosingInstance;
			public ParallelReader Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			protected internal TermDocs termDocs;
			
			public ParallelTermDocs(ParallelReader enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			public ParallelTermDocs(ParallelReader enclosingInstance, Term term)
			{
				InitBlock(enclosingInstance);
				if (term == null)
					termDocs = (Enclosing_Instance.readers.Count == 0)?null:((IndexReader) Enclosing_Instance.readers[0]).TermDocs(null);
				else
					Seek(term);
			}
			
			public virtual int Doc()
			{
				return termDocs.Doc();
			}
			public virtual int Freq()
			{
				return termDocs.Freq();
			}
			
			public virtual void  Seek(Term term)
			{
				IndexReader reader = ((IndexReader) Enclosing_Instance.fieldToReader[term.Field()]);
				termDocs = reader != null?reader.TermDocs(term):null;
			}
			
			public virtual void  Seek(TermEnum termEnum)
			{
				Seek(termEnum.Term());
			}
			
			public virtual bool Next()
			{
				if (termDocs == null)
					return false;
				
				return termDocs.Next();
			}
			
			public virtual int Read(int[] docs, int[] freqs)
			{
				if (termDocs == null)
					return 0;
				
				return termDocs.Read(docs, freqs);
			}
			
			public virtual bool SkipTo(int target)
			{
				if (termDocs == null)
					return false;
				
				return termDocs.SkipTo(target);
			}
			
			public virtual void  Close()
			{
				if (termDocs != null)
					termDocs.Close();
			}
		}
		
		private class ParallelTermPositions:ParallelTermDocs, TermPositions
		{
			private void  InitBlock(ParallelReader enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ParallelReader enclosingInstance;
			public new ParallelReader Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			public ParallelTermPositions(ParallelReader enclosingInstance):base(enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			public ParallelTermPositions(ParallelReader enclosingInstance, Term term):base(enclosingInstance)
			{
				InitBlock(enclosingInstance);
				Seek(term);
			}
			
			public override void  Seek(Term term)
			{
				IndexReader reader = ((IndexReader) Enclosing_Instance.fieldToReader[term.Field()]);
				termDocs = reader != null?reader.TermPositions(term):null;
			}
			
			public virtual int NextPosition()
			{
				// It is an error to call this if there is no next position, e.g. if termDocs==null
				return ((TermPositions) termDocs).NextPosition();
			}
			
			public virtual int GetPayloadLength()
			{
				return ((TermPositions) termDocs).GetPayloadLength();
			}
			
			public virtual byte[] GetPayload(byte[] data, int offset)
			{
				return ((TermPositions) termDocs).GetPayload(data, offset);
			}
			
			
			// TODO: Remove warning after API has been finalized
			public virtual bool IsPayloadAvailable()
			{
				return ((TermPositions) termDocs).IsPayloadAvailable();
			}
		}
	}
}
