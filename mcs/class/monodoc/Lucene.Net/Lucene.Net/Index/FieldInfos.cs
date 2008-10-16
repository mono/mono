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
using Field = Monodoc.Lucene.Net.Documents.Field;
using Directory = Monodoc.Lucene.Net.Store.Directory;
using InputStream = Monodoc.Lucene.Net.Store.InputStream;
using OutputStream = Monodoc.Lucene.Net.Store.OutputStream;
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary>Access to the Field Info file that describes document fields and whether or
	/// not they are indexed. Each segment has a separate Field Info file. Objects
	/// of this class are thread-safe for multiple readers, but only one thread can
	/// be adding documents at a time, with no other reader or writer threads
	/// accessing this object.
	/// </summary>
	sealed public class FieldInfos
	{
		private System.Collections.ArrayList byNumber = new System.Collections.ArrayList();
		private System.Collections.Hashtable byName = new System.Collections.Hashtable();
		
		public /*internal*/ FieldInfos()
		{
			Add("", false);
		}
		
		/// <summary> Construct a FieldInfos object using the directory and the name of the file
		/// InputStream
		/// </summary>
		/// <param name="d">The directory to open the InputStream from
		/// </param>
		/// <param name="name">The name of the file to open the InputStream from in the Directory
		/// </param>
		/// <throws>  IOException </throws>
		/// <summary> 
		/// </summary>
		/// <seealso cref="#read">
		/// </seealso>
		public /*internal*/ FieldInfos(Directory d, System.String name)
		{
			InputStream input = d.OpenFile(name);
			try
			{
				Read(input);
			}
			finally
			{
				input.Close();
			}
		}
		
		/// <summary>Adds Field info for a Document. </summary>
		public void  Add(Document doc)
		{
            foreach (Field field in doc.Fields())
            {
                Add(field.Name(), field.IsIndexed(), field.IsTermVectorStored());
            }
		}
		
		/// <param name="names">The names of the fields
		/// </param>
		/// <param name="storeTermVectors">Whether the fields store term vectors or not
		/// </param>
		public void  AddIndexed(System.Collections.ICollection names, bool storeTermVectors)
		{
			System.Collections.IEnumerator i = names.GetEnumerator();
			int j = 0;
			while (i.MoveNext())
			{
                System.Collections.DictionaryEntry t = (System.Collections.DictionaryEntry) i.Current;
				Add((System.String) t.Key, true, storeTermVectors);
			}
		}
		
		/// <summary> Assumes the Field is not storing term vectors </summary>
		/// <param name="names">The names of the fields
		/// </param>
		/// <param name="isIndexed">Whether the fields are indexed or not
		/// 
		/// </param>
		/// <seealso cref="boolean)">
		/// </seealso>
		public void  Add(System.Collections.ICollection names, bool isIndexed)
		{
			System.Collections.IEnumerator i = names.GetEnumerator();
			int j = 0;
			while (i.MoveNext())
			{
                System.Collections.DictionaryEntry t = (System.Collections.DictionaryEntry) i.Current;
                Add((System.String) t.Key, isIndexed);
			}
		}
		
		/// <summary> Calls three parameter add with false for the storeTermVector parameter </summary>
		/// <param name="name">The name of the Field
		/// </param>
		/// <param name="isIndexed">true if the Field is indexed
		/// </param>
		/// <seealso cref="boolean, boolean)">
		/// </seealso>
		public void  Add(System.String name, bool isIndexed)
		{
			Add(name, isIndexed, false);
		}
		
		
		/// <summary>If the Field is not yet known, adds it. If it is known, checks to make
		/// sure that the isIndexed flag is the same as was given previously for this
		/// Field. If not - marks it as being indexed.  Same goes for storeTermVector
		/// 
		/// </summary>
		/// <param name="name">The name of the Field
		/// </param>
		/// <param name="isIndexed">true if the Field is indexed
		/// </param>
		/// <param name="storeTermVector">true if the term vector should be stored
		/// </param>
		public void  Add(System.String name, bool isIndexed, bool storeTermVector)
		{
			FieldInfo fi = FieldInfo(name);
			if (fi == null)
			{
				AddInternal(name, isIndexed, storeTermVector);
			}
			else
			{
				if (fi.isIndexed != isIndexed)
				{
					fi.isIndexed = true; // once indexed, always index
				}
				if (fi.storeTermVector != storeTermVector)
				{
					fi.storeTermVector = true; // once vector, always vector
				}
			}
		}
		
		private void  AddInternal(System.String name, bool isIndexed, bool storeTermVector)
		{
			FieldInfo fi = new FieldInfo(name, isIndexed, byNumber.Count, storeTermVector);
			byNumber.Add(fi);
			byName[name] = fi;
		}
		
		public int FieldNumber(System.String fieldName)
		{
			FieldInfo fi = FieldInfo(fieldName);
			if (fi != null)
				return fi.number;
			else
				return - 1;
		}
		
		public FieldInfo FieldInfo(System.String fieldName)
		{
			return (FieldInfo) byName[fieldName];
		}
		
		public System.String FieldName(int fieldNumber)
		{
			return FieldInfo(fieldNumber).name;
		}
		
		public FieldInfo FieldInfo(int fieldNumber)
		{
			return (FieldInfo) byNumber[fieldNumber];
		}
		
		public int Size()
		{
			return byNumber.Count;
		}
		
		public bool HasVectors()
		{
			bool hasVectors = false;
			for (int i = 0; i < Size(); i++)
			{
				if (FieldInfo(i).storeTermVector)
					hasVectors = true;
			}
			return hasVectors;
		}
		
		public void  Write(Directory d, System.String name)
		{
			OutputStream output = d.CreateFile(name);
			try
			{
				Write(output);
			}
			finally
			{
				output.Close();
			}
		}
		
		public void  Write(OutputStream output)
		{
			output.WriteVInt(Size());
			for (int i = 0; i < Size(); i++)
			{
				FieldInfo fi = FieldInfo(i);
				byte bits = (byte) (0x0);
				if (fi.isIndexed)
					bits |= (byte) (0x1);
				if (fi.storeTermVector)
					bits |= (byte) (0x2);
				output.WriteString(fi.name);
				//Was REMOVE
				//output.writeByte((byte)(fi.isIndexed ? 1 : 0));
				output.WriteByte(bits);
			}
		}
		
		private void  Read(InputStream input)
		{
			int size = input.ReadVInt(); //read in the size
			for (int i = 0; i < size; i++)
			{
				System.String name = String.Intern(input.ReadString());
				byte bits = input.ReadByte();
				bool isIndexed = (bits & 0x1) != 0;
				bool storeTermVector = (bits & 0x2) != 0;
				AddInternal(name, isIndexed, storeTermVector);
			}
		}
	}
}