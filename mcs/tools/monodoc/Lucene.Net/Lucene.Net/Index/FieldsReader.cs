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
namespace Monodoc.Lucene.Net.Index
{
	
	/// <summary> Class responsible for access to stored document fields.
	/// 
	/// It uses &lt;segment&gt;.fdt and &lt;segment&gt;.fdx; files.
	/// 
	/// </summary>
	/// <version>  $Id: FieldsReader.java,v 1.7 2004/03/29 22:48:02 cutting Exp $
	/// </version>
	sealed public class FieldsReader
	{
		private FieldInfos fieldInfos;
		private InputStream fieldsStream;
		private InputStream indexStream;
		private int size;
		
		public /*internal*/ FieldsReader(Directory d, System.String segment, FieldInfos fn)
		{
			fieldInfos = fn;
			
			fieldsStream = d.OpenFile(segment + ".fdt");
			indexStream = d.OpenFile(segment + ".fdx");
			
			size = (int) (indexStream.Length() / 8);
		}
		
		public /*internal*/ void  Close()
		{
			fieldsStream.Close();
			indexStream.Close();
		}
		
		public /*internal*/ int Size()
		{
			return size;
		}
		
		public /*internal*/ Document Doc(int n)
		{
			indexStream.Seek(n * 8L);
			long position = indexStream.ReadLong();
			fieldsStream.Seek(position);
			
			Document doc = new Document();
			int numFields = fieldsStream.ReadVInt();
			for (int i = 0; i < numFields; i++)
			{
				int fieldNumber = fieldsStream.ReadVInt();
				FieldInfo fi = fieldInfos.FieldInfo(fieldNumber);
				
				byte bits = fieldsStream.ReadByte();
				
				doc.Add(new Field(fi.name, fieldsStream.ReadString(), true, fi.isIndexed, (bits & 1) != 0, fi.storeTermVector)); // vector
			}
			
			return doc;
		}
	}
}