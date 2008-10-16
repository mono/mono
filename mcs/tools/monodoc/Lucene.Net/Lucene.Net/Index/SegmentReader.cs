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
using Directory = Monodoc.Lucene.Net.Store.Directory;
using InputStream = Monodoc.Lucene.Net.Store.InputStream;
using OutputStream = Monodoc.Lucene.Net.Store.OutputStream;
using BitVector = Monodoc.Lucene.Net.Util.BitVector;
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary> FIXME: Describe class <code>SegmentReader</code> here.
	/// 
	/// </summary>
	/// <version>  $Id: SegmentReader.java,v 1.23 2004/07/10 06:19:01 otis Exp $
	/// </version>
	sealed public class SegmentReader : Monodoc.Lucene.Net.Index.IndexReader
	{
		private System.String segment;
		
		internal FieldInfos fieldInfos;
		private FieldsReader fieldsReader;
		
		internal TermInfosReader tis;
		internal TermVectorsReader termVectorsReader;
		
		internal BitVector deletedDocs = null;
		private bool deletedDocsDirty = false;
		private bool normsDirty = false;
		private bool undeleteAll = false;
		
		internal InputStream freqStream;
		internal InputStream proxStream;
		
		// Compound File Reader when based on a compound file segment
		internal CompoundFileReader cfsReader;
		
		private class Norm
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
			public Norm(SegmentReader enclosingInstance, InputStream in_Renamed, int number)
			{
				InitBlock(enclosingInstance);
				this.in_Renamed = in_Renamed;
				this.number = number;
			}
			
			public InputStream in_Renamed;  // private -> public
			public byte[] bytes;           // private -> public
			public bool dirty;              // private -> public
			public int number;              // private -> public
			
			public void  ReWrite()          // private -> public
			{
				// NOTE: norms are re-written in regular directory, not cfs
				OutputStream out_Renamed = Enclosing_Instance.Directory().CreateFile(Enclosing_Instance.segment + ".tmp");
				try
				{
					out_Renamed.WriteBytes(bytes, Enclosing_Instance.MaxDoc());
				}
				finally
				{
					out_Renamed.Close();
				}
				System.String fileName = Enclosing_Instance.segment + ".f" + number;
				Enclosing_Instance.Directory().RenameFile(Enclosing_Instance.segment + ".tmp", fileName);
				this.dirty = false;
			}
		}
		
		private System.Collections.Hashtable norms = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		public /*internal*/ SegmentReader(SegmentInfos sis, SegmentInfo si, bool closeDir) : base(si.dir, sis, closeDir)
		{
			Initialize(si);
		}
		
		public /*internal*/ SegmentReader(SegmentInfo si) : base(si.dir)
		{
			Initialize(si);
		}
		
		private void  Initialize(SegmentInfo si)
		{
			segment = si.name;
			
			// Use compound file directory for some files, if it exists
			Directory cfsDir = Directory();
			if (Directory().FileExists(segment + ".cfs"))
			{
				cfsReader = new CompoundFileReader(Directory(), segment + ".cfs");
				cfsDir = cfsReader;
			}
			
			// No compound file exists - use the multi-file format
			fieldInfos = new FieldInfos(cfsDir, segment + ".fnm");
			fieldsReader = new FieldsReader(cfsDir, segment, fieldInfos);
			
			tis = new TermInfosReader(cfsDir, segment, fieldInfos);
			
			// NOTE: the bitvector is stored using the regular directory, not cfs
			if (HasDeletions(si))
				deletedDocs = new BitVector(Directory(), segment + ".del");
			
			// make sure that all index files have been read or are kept open
			// so that if an index update removes them we'll still have them
			freqStream = cfsDir.OpenFile(segment + ".frq");
			proxStream = cfsDir.OpenFile(segment + ".prx");
			OpenNorms(cfsDir);
			
			if (fieldInfos.HasVectors())
			{
				// open term vector files only as needed
				termVectorsReader = new TermVectorsReader(cfsDir, segment, fieldInfos);
			}
		}
		
		protected internal override void  DoCommit()
		{
			if (deletedDocsDirty)
			{
				// re-write deleted 
				deletedDocs.Write(Directory(), segment + ".tmp");
				Directory().RenameFile(segment + ".tmp", segment + ".del");
			}
			if (undeleteAll && Directory().FileExists(segment + ".del"))
			{
				Directory().DeleteFile(segment + ".del");
			}
			if (normsDirty)
			{
				// re-write norms 
				System.Collections.IEnumerator values = norms.Values.GetEnumerator();
				while (values.MoveNext())
				{
					Norm norm = (Norm) values.Current;
					if (norm.dirty)
					{
						norm.ReWrite();
					}
				}
			}
			deletedDocsDirty = false;
			normsDirty = false;
			undeleteAll = false;
		}
		
		protected internal override void  DoClose()
		{
			fieldsReader.Close();
			tis.Close();
			
			if (freqStream != null)
				freqStream.Close();
			if (proxStream != null)
				proxStream.Close();
			
			CloseNorms();
			if (termVectorsReader != null)
				termVectorsReader.Close();
			
			if (cfsReader != null)
				cfsReader.Close();
		}
		
		internal static bool HasDeletions(SegmentInfo si)
		{
			return si.dir.FileExists(si.name + ".del");
		}
		
		public override bool HasDeletions()
		{
			return deletedDocs != null;
		}
		
		
		internal static bool UsesCompoundFile(SegmentInfo si)
		{
			return si.dir.FileExists(si.name + ".cfs");
		}
		
		internal static bool HasSeparateNorms(SegmentInfo si)
		{
			System.String[] result = si.dir.List();
			System.String pattern = si.name + ".f";
			int patternLength = pattern.Length;
			for (int i = 0; i < 0; i++)
			{
				if (result[i].StartsWith(pattern) && System.Char.IsDigit(result[i][patternLength]))
					return true;
			}
			return false;
		}
		
		protected internal override void  DoDelete(int docNum)
		{
			if (deletedDocs == null)
				deletedDocs = new BitVector(MaxDoc());
			deletedDocsDirty = true;
			undeleteAll = false;
			deletedDocs.Set(docNum);
		}
		
		protected internal override void  DoUndeleteAll()
		{
			deletedDocs = null;
			deletedDocsDirty = false;
			undeleteAll = true;
		}
		
		internal System.Collections.ArrayList Files()
		{
			System.Collections.ArrayList files = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(16));
			System.String[] ext = new System.String[]{"cfs", "fnm", "fdx", "fdt", "tii", "tis", "frq", "prx", "del", "tvx", "tvd", "tvf", "tvp"};
			
			for (int i = 0; i < ext.Length; i++)
			{
				System.String name = segment + "." + ext[i];
				if (Directory().FileExists(name))
					files.Add(name);
			}
			
			for (int i = 0; i < fieldInfos.Size(); i++)
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				if (fi.isIndexed)
					files.Add(segment + ".f" + i);
			}
			return files;
		}
		
		public override TermEnum Terms()
		{
			return tis.Terms();
		}
		
		public override TermEnum Terms(Term t)
		{
			return tis.Terms(t);
		}
		
		public override Document Document(int n)
		{
			lock (this)
			{
				if (IsDeleted(n))
					throw new System.ArgumentException("attempt to access a deleted document");
				return fieldsReader.Doc(n);
			}
		}
		
		public override bool IsDeleted(int n)
		{
			lock (this)
			{
				return (deletedDocs != null && deletedDocs.Get(n));
			}
		}
		
		public override TermDocs TermDocs()
		{
			return new SegmentTermDocs(this);
		}
		
		public override TermPositions TermPositions()
		{
			return new SegmentTermPositions(this);
		}
		
		public override int DocFreq(Term t)
		{
			TermInfo ti = tis.Get(t);
			if (ti != null)
				return ti.docFreq;
			else
				return 0;
		}
		
		public override int NumDocs()
		{
			int n = MaxDoc();
			if (deletedDocs != null)
				n -= deletedDocs.Count();
			return n;
		}
		
		public override int MaxDoc()
		{
			return fieldsReader.Size();
		}
		
		/// <seealso cref="Monodoc.Lucene.Net.Index.IndexReader#GetFieldNames()">
		/// </seealso>
		public override System.Collections.ICollection GetFieldNames()
		{
			// maintain a unique set of Field names
			System.Collections.Hashtable fieldSet = new System.Collections.Hashtable();
			for (int i = 0; i < fieldInfos.Size(); i++)
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				fieldSet.Add(fi.name, fi.name);
			}
			return fieldSet;
		}
		
		/// <seealso cref="Monodoc.Lucene.Net.Index.IndexReader#GetFieldNames(boolean)">
		/// </seealso>
		public override System.Collections.ICollection GetFieldNames(bool indexed)
		{
			// maintain a unique set of Field names
			System.Collections.Hashtable fieldSet = new System.Collections.Hashtable();
			for (int i = 0; i < fieldInfos.Size(); i++)
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				if (fi.isIndexed == indexed)
					fieldSet.Add(fi.name, fi.name);
			}
			return fieldSet;
		}
		
		/// <summary> </summary>
		/// <param name="storedTermVector">if true, returns only Indexed fields that have term vector info, 
		/// else only indexed fields without term vector info 
		/// </param>
		/// <returns> Collection of Strings indicating the names of the fields
		/// </returns>
		public override System.Collections.ICollection GetIndexedFieldNames(bool storedTermVector)
		{
			// maintain a unique set of Field names
			System.Collections.Hashtable fieldSet = new System.Collections.Hashtable();
			for (int ii = 0; ii < fieldInfos.Size(); ii++)
			{
				FieldInfo fi = fieldInfos.FieldInfo(ii);
				if (fi.isIndexed == true && fi.storeTermVector == storedTermVector)
				{
					fieldSet.Add(fi.name, fi.name);
				}
			}
			return fieldSet;
		}
		
		public override byte[] Norms(System.String field)
		{
			lock (this)
			{
				Norm norm = (Norm) norms[field];
				if (norm == null)
				// not an indexed Field
					return null;
				if (norm.bytes == null)
				{
					// value not yet read
					byte[] bytes = new byte[MaxDoc()];
					Norms(field, bytes, 0);
					norm.bytes = bytes; // cache it
				}
				return norm.bytes;
			}
		}
		
		protected internal override void  DoSetNorm(int doc, System.String field, byte value_Renamed)
		{
			Norm norm = (Norm) norms[field];
			if (norm == null)
			// not an indexed Field
				return ;
			norm.dirty = true; // mark it dirty
			normsDirty = true;
			
			Norms(field)[doc] = value_Renamed; // set the value
		}
		
		/// <summary>Read norms into a pre-allocated array. </summary>
		public override void  Norms(System.String field, byte[] bytes, int offset)
		{
			lock (this)
			{
				
				Norm norm = (Norm) norms[field];
				if (norm == null)
					return ; // use zeros in array
				
				if (norm.bytes != null)
				{
					// can copy from cache
					Array.Copy(norm.bytes, 0, bytes, offset, MaxDoc());
					return ;
				}
				
				InputStream normStream = (InputStream) norm.in_Renamed.Clone();
				try
				{
					// read from disk
					normStream.Seek(0);
					normStream.ReadBytes(bytes, offset, MaxDoc());
				}
				finally
				{
					normStream.Close();
				}
			}
		}
		
		private void  OpenNorms(Directory cfsDir)
		{
			for (int i = 0; i < fieldInfos.Size(); i++)
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				if (fi.isIndexed)
				{
					System.String fileName = segment + ".f" + fi.number;
					// look first for re-written file, then in compound format
					Directory d = Directory().FileExists(fileName)?Directory():cfsDir;
					norms[fi.name] = new Norm(this, d.OpenFile(fileName), fi.number);
				}
			}
		}
		
		private void  CloseNorms()
		{
			lock (norms.SyncRoot)
			{
				System.Collections.IEnumerator enumerator = norms.Values.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Norm norm = (Norm) enumerator.Current;
					norm.in_Renamed.Close();
				}
			}
		}
		
		/// <summary>Return a term frequency vector for the specified document and Field. The
		/// vector returned contains term numbers and frequencies for all terms in
		/// the specified Field of this document, if the Field had storeTermVector
		/// flag set.  If the flag was not set, the method returns null.
		/// </summary>
		public override TermFreqVector GetTermFreqVector(int docNumber, System.String field)
		{
			// Check if this Field is invalid or has no stored term vector
			FieldInfo fi = fieldInfos.FieldInfo(field);
			if (fi == null || !fi.storeTermVector)
				return null;
			
			return termVectorsReader.Get(docNumber, field);
		}
		
		
		/// <summary>Return an array of term frequency vectors for the specified document.
		/// The array contains a vector for each vectorized Field in the document.
		/// Each vector vector contains term numbers and frequencies for all terms
		/// in a given vectorized Field.
		/// If no such fields existed, the method returns null.
		/// </summary>
		public override TermFreqVector[] GetTermFreqVectors(int docNumber)
		{
			if (termVectorsReader == null)
				return null;
			
			return termVectorsReader.Get(docNumber);
		}
	}
}