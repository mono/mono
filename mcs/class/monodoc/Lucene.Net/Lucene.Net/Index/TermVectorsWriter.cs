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
using Directory = Monodoc.Lucene.Net.Store.Directory;
using OutputStream = Monodoc.Lucene.Net.Store.OutputStream;
using StringHelper = Monodoc.Lucene.Net.Util.StringHelper;
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary> Writer works by opening a document and then opening the fields within the document and then
	/// writing out the vectors for each Field.
	/// 
	/// Rough usage:
	/// 
	/// <CODE>
	/// for each document
	/// {
	/// writer.openDocument();
	/// for each Field on the document
	/// {
	/// writer.openField(Field);
	/// for all of the terms
	/// {
	/// writer.addTerm(...)
	/// }
	/// writer.closeField
	/// }
	/// writer.closeDocument()    
	/// }
	/// </CODE>
	/// </summary>
	sealed public class TermVectorsWriter
	{
		public const int FORMAT_VERSION = 1;
		//The size in bytes that the FORMAT_VERSION will take up at the beginning of each file 
		public const int FORMAT_SIZE = 4;
		
		//TODO: Figure out how to write with or w/o position information and read back in
		public const System.String TVX_EXTENSION = ".tvx";
		public const System.String TVD_EXTENSION = ".tvd";
		public const System.String TVF_EXTENSION = ".tvf";
		private OutputStream tvx = null, tvd = null, tvf = null;
		private System.Collections.ArrayList fields = null;
		private System.Collections.ArrayList terms = null;
		private FieldInfos fieldInfos;
		
		private TVField currentField = null;
		private long currentDocPointer = - 1;
		
		/// <summary>Create term vectors writer for the specified segment in specified
		/// directory.  A new TermVectorsWriter should be created for each
		/// segment. The parameter <code>maxFields</code> indicates how many total
		/// fields are found in this document. Not all of these fields may require
		/// termvectors to be stored, so the number of calls to
		/// <code>openField</code> is less or equal to this number.
		/// </summary>
		public TermVectorsWriter(Directory directory, System.String segment, FieldInfos fieldInfos)
		{
			// Open files for TermVector storage
			tvx = directory.CreateFile(segment + TVX_EXTENSION);
			tvx.WriteInt(FORMAT_VERSION);
			tvd = directory.CreateFile(segment + TVD_EXTENSION);
			tvd.WriteInt(FORMAT_VERSION);
			tvf = directory.CreateFile(segment + TVF_EXTENSION);
			tvf.WriteInt(FORMAT_VERSION);
			
			this.fieldInfos = fieldInfos;
			fields = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(fieldInfos.Size()));
			terms = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
		}
		
		
		public void  OpenDocument()
		{
			CloseDocument();
			
			currentDocPointer = tvd.GetFilePointer();
		}
		
		
		public void  CloseDocument()
		{
			if (IsDocumentOpen())
			{
				CloseField();
				WriteDoc();
				fields.Clear();
				currentDocPointer = - 1;
			}
		}
		
		
		public bool IsDocumentOpen()
		{
			return currentDocPointer != - 1;
		}
		
		
		/// <summary>Start processing a Field. This can be followed by a number of calls to
		/// addTerm, and a final call to closeField to indicate the end of
		/// processing of this Field. If a Field was previously open, it is
		/// closed automatically.
		/// </summary>
		public void  OpenField(System.String field)
		{
			if (!IsDocumentOpen())
				throw new System.SystemException("Cannot open Field when no document is open.");
			
			CloseField();
			currentField = new TVField(fieldInfos.FieldNumber(field));
		}
		
		/// <summary>Finished processing current Field. This should be followed by a call to
		/// openField before future calls to addTerm.
		/// </summary>
		public void  CloseField()
		{
			if (IsFieldOpen())
			{
				/* DEBUG */
				//System.out.println("closeField()");
				/* DEBUG */
				
				// save Field and terms
				WriteField();
				fields.Add(currentField);
				terms.Clear();
				currentField = null;
			}
		}
		
		/// <summary>Return true if a Field is currently open. </summary>
		public bool IsFieldOpen()
		{
			return currentField != null;
		}
		
		/// <summary>Add term to the Field's term vector. Field must already be open
		/// of NullPointerException is thrown. Terms should be added in
		/// increasing order of terms, one call per unique termNum. ProxPointer
		/// is a pointer into the TermPosition file (prx). Freq is the number of
		/// times this term appears in this Field, in this document.
		/// </summary>
		public void  AddTerm(System.String termText, int freq)
		{
			if (!IsDocumentOpen())
				throw new System.SystemException("Cannot add terms when document is not open");
			if (!IsFieldOpen())
				throw new System.SystemException("Cannot add terms when Field is not open");
			
			AddTermInternal(termText, freq);
		}
		
		private void  AddTermInternal(System.String termText, int freq)
		{
			currentField.length += freq;
			TVTerm term = new TVTerm();
			term.termText = termText;
			term.freq = freq;
			terms.Add(term);
		}
		
		
		/// <summary>Add specified vectors to the document.</summary>
		public void  AddVectors(TermFreqVector[] vectors)
		{
			if (!IsDocumentOpen())
				throw new System.SystemException("Cannot add term vectors when document is not open");
			if (IsFieldOpen())
				throw new System.SystemException("Cannot add term vectors when Field is open");
			
			for (int i = 0; i < vectors.Length; i++)
			{
				AddTermFreqVector(vectors[i]);
			}
		}
		
		
		/// <summary>Add specified vector to the document. Document must be open but no Field
		/// should be open or exception is thrown. The same document can have <code>addTerm</code>
		/// and <code>addVectors</code> calls mixed, however a given Field must either be
		/// populated with <code>addTerm</code> or with <code>addVector</code>.     *
		/// </summary>
		public void  AddTermFreqVector(TermFreqVector vector)
		{
			if (!IsDocumentOpen())
				throw new System.SystemException("Cannot add term vector when document is not open");
			if (IsFieldOpen())
				throw new System.SystemException("Cannot add term vector when Field is open");
			AddTermFreqVectorInternal(vector);
		}
		
		private void  AddTermFreqVectorInternal(TermFreqVector vector)
		{
			OpenField(vector.GetField());
			for (int i = 0; i < vector.Size(); i++)
			{
				AddTermInternal(vector.GetTerms()[i], vector.GetTermFrequencies()[i]);
			}
			CloseField();
		}
		
		
		
		
		/// <summary>Close all streams. </summary>
		public /*internal*/ void  Close()
		{
			try
			{
				CloseDocument();
			}
			finally
			{
				// make an effort to close all streams we can but remember and re-throw
				// the first exception encountered in this process
				System.IO.IOException keep = null;
				if (tvx != null)
					try
					{
						tvx.Close();
					}
					catch (System.IO.IOException e)
					{
						if (keep == null)
							keep = e;
					}
				if (tvd != null)
					try
					{
						tvd.Close();
					}
					catch (System.IO.IOException e)
					{
						if (keep == null)
							keep = e;
					}
				if (tvf != null)
					try
					{
						tvf.Close();
					}
					catch (System.IO.IOException e)
					{
						if (keep == null)
							keep = e;
					}
				if (keep != null)
				{
					throw new System.IO.IOException(keep.StackTrace);
				}
			}
		}
		
		
		
		private void  WriteField()
		{
			// remember where this Field is written
			currentField.tvfPointer = tvf.GetFilePointer();
			//System.out.println("Field Pointer: " + currentField.tvfPointer);
			int size;
			
			tvf.WriteVInt(size = terms.Count);
			tvf.WriteVInt(currentField.length - size);
			System.String lastTermText = "";
			// write term ids and positions
			for (int i = 0; i < size; i++)
			{
				TVTerm term = (TVTerm) terms[i];
				//tvf.writeString(term.termText);
				int start = StringHelper.StringDifference(lastTermText, term.termText);
				int length = term.termText.Length - start;
				tvf.WriteVInt(start); // write shared prefix length
				tvf.WriteVInt(length); // write delta length
				tvf.WriteChars(term.termText, start, length); // write delta chars
				tvf.WriteVInt(term.freq);
				lastTermText = term.termText;
			}
		}
		
		
		
		
		private void  WriteDoc()
		{
			if (IsFieldOpen())
				throw new System.SystemException("Field is still open while writing document");
			//System.out.println("Writing doc pointer: " + currentDocPointer);
			// write document index record
			tvx.WriteLong(currentDocPointer);
			
			// write document data record
			int size;
			
			// write the number of fields
			tvd.WriteVInt(size = fields.Count);
			
			// write Field numbers
			int lastFieldNumber = 0;
			for (int i = 0; i < size; i++)
			{
				TVField field = (TVField) fields[i];
				tvd.WriteVInt(field.number - lastFieldNumber);
				
				lastFieldNumber = field.number;
			}
			
			// write Field pointers
			long lastFieldPointer = 0;
			for (int i = 0; i < size; i++)
			{
				TVField field = (TVField) fields[i];
				tvd.WriteVLong(field.tvfPointer - lastFieldPointer);
				
				lastFieldPointer = field.tvfPointer;
			}
			//System.out.println("After writing doc pointer: " + tvx.getFilePointer());
		}
		
		
		private class TVField
		{
			internal int number;
			internal long tvfPointer = 0;
			internal int length = 0; // number of distinct term positions
			
			internal TVField(int number)
			{
				this.number = number;
			}
		}
		
		private class TVTerm
		{
			internal System.String termText;
			internal int freq = 0;
			//int positions[] = null;
		}
	}
}