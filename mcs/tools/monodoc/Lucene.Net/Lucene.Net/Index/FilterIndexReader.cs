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
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary>A <code>FilterIndexReader</code> contains another Monodoc.Lucene.Net.Index.IndexReader, which it
	/// uses as its basic source of data, possibly transforming the data along the
	/// way or providing additional functionality. The class
	/// <code>FilterIndexReader</code> itself simply implements all abstract methods
	/// of <code>Monodoc.Lucene.Net.Index.IndexReader</code> with versions that pass all requests to the
	/// contained index reader. Subclasses of <code>FilterIndexReader</code> may
	/// further override some of these methods and may also provide additional
	/// methods and fields.
	/// </summary>
	public class FilterIndexReader : Monodoc.Lucene.Net.Index.IndexReader
	{
		
		/// <summary>Base class for filtering {@link TermDocs} implementations. </summary>
		public class FilterTermDocs : TermDocs
		{
			protected internal TermDocs in_Renamed;
			
			public FilterTermDocs(TermDocs in_Renamed)
			{
				this.in_Renamed = in_Renamed;
			}
			
			public virtual void  Seek(Term term)
			{
				in_Renamed.Seek(term);
			}
			public virtual void  Seek(TermEnum termEnum)
			{
				in_Renamed.Seek(termEnum);
			}
			public virtual int Doc()
			{
				return in_Renamed.Doc();
			}
			public virtual int Freq()
			{
				return in_Renamed.Freq();
			}
			public virtual bool Next()
			{
				return in_Renamed.Next();
			}
			public virtual int Read(int[] docs, int[] freqs)
			{
				return in_Renamed.Read(docs, freqs);
			}
			public virtual bool SkipTo(int i)
			{
				return in_Renamed.SkipTo(i);
			}
			public virtual void  Close()
			{
				in_Renamed.Close();
			}
		}
		
		/// <summary>Base class for filtering {@link TermPositions} implementations. </summary>
		public class FilterTermPositions:FilterTermDocs, TermPositions
		{
			
			public FilterTermPositions(TermPositions in_Renamed):base(in_Renamed)
			{
			}
			
			public virtual int NextPosition()
			{
				return ((TermPositions) this.in_Renamed).NextPosition();
			}
		}
		
		/// <summary>Base class for filtering {@link TermEnum} implementations. </summary>
		public class FilterTermEnum:TermEnum
		{
			protected internal TermEnum in_Renamed;
			
			public FilterTermEnum(TermEnum in_Renamed)
			{
				this.in_Renamed = in_Renamed;
			}
			
			public override bool Next()
			{
				return in_Renamed.Next();
			}
			public override Term Term()
			{
				return in_Renamed.Term();
			}
			public override int DocFreq()
			{
				return in_Renamed.DocFreq();
			}
			public override void  Close()
			{
				in_Renamed.Close();
			}
		}
		
		protected internal Monodoc.Lucene.Net.Index.IndexReader in_Renamed;
		
		/// <summary> <p>Construct a FilterIndexReader based on the specified base reader.
		/// Directory locking for delete, undeleteAll, and setNorm operations is
		/// left to the base reader.</p>
		/// <p>Note that base reader is closed if this FilterIndexReader is closed.</p>
		/// </summary>
		/// <param name="in">specified base reader.
		/// </param>
		public FilterIndexReader(Monodoc.Lucene.Net.Index.IndexReader in_Renamed):base(in_Renamed.Directory())
		{
			this.in_Renamed = in_Renamed;
		}
		
		public override TermFreqVector[] GetTermFreqVectors(int docNumber)
		{
			return in_Renamed.GetTermFreqVectors(docNumber);
		}
		
		public override TermFreqVector GetTermFreqVector(int docNumber, System.String field)
		{
			return in_Renamed.GetTermFreqVector(docNumber, field);
		}
		
		public override int NumDocs()
		{
			return in_Renamed.NumDocs();
		}
		public override int MaxDoc()
		{
			return in_Renamed.MaxDoc();
		}
		
		public override Document Document(int n)
		{
			return in_Renamed.Document(n);
		}
		
		public override bool IsDeleted(int n)
		{
			return in_Renamed.IsDeleted(n);
		}
		public override bool HasDeletions()
		{
			return in_Renamed.HasDeletions();
		}
		protected internal override void  DoUndeleteAll()
		{
			in_Renamed.UndeleteAll();
		}
		
		public override byte[] Norms(System.String f)
		{
			return in_Renamed.Norms(f);
		}
		public override void  Norms(System.String f, byte[] bytes, int offset)
		{
			in_Renamed.Norms(f, bytes, offset);
		}
		protected internal override void  DoSetNorm(int d, System.String f, byte b)
		{
			in_Renamed.SetNorm(d, f, b);
		}
		
		public override TermEnum Terms()
		{
			return in_Renamed.Terms();
		}
		public override TermEnum Terms(Term t)
		{
			return in_Renamed.Terms(t);
		}
		
		public override int DocFreq(Term t)
		{
			return in_Renamed.DocFreq(t);
		}
		
		public override TermDocs TermDocs()
		{
			return in_Renamed.TermDocs();
		}
		
		public override TermPositions TermPositions()
		{
			return in_Renamed.TermPositions();
		}
		
		protected internal override void  DoDelete(int n)
		{
			in_Renamed.Delete(n);
		}
		protected internal override void  DoCommit()
		{
			in_Renamed.Commit();
		}
		protected internal override void  DoClose()
		{
			in_Renamed.Close();
		}
		
		public override System.Collections.ICollection GetFieldNames()
		{
			return in_Renamed.GetFieldNames();
		}
		
		public override System.Collections.ICollection GetFieldNames(bool indexed)
		{
			return in_Renamed.GetFieldNames(indexed);
		}
		
		/// <summary> </summary>
		/// <param name="storedTermVector">if true, returns only Indexed fields that have term vector info, 
		/// else only indexed fields without term vector info 
		/// </param>
		/// <returns> Collection of Strings indicating the names of the fields
		/// </returns>
		public override System.Collections.ICollection GetIndexedFieldNames(bool storedTermVector)
		{
			return in_Renamed.GetIndexedFieldNames(storedTermVector);
		}
	}
}
