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
using OutputStream = Monodoc.Lucene.Net.Store.OutputStream;
namespace Monodoc.Lucene.Net.Index
{
	
	sealed class FieldsWriter
	{
		private FieldInfos fieldInfos;
		private OutputStream fieldsStream;
		private OutputStream indexStream;
		
		internal FieldsWriter(Directory d, System.String segment, FieldInfos fn)
		{
			fieldInfos = fn;
			fieldsStream = d.CreateFile(segment + ".fdt");
			indexStream = d.CreateFile(segment + ".fdx");
		}
		
		internal void  Close()
		{
			fieldsStream.Close();
			indexStream.Close();
		}
		
		internal void  AddDocument(Document doc)
		{
			indexStream.WriteLong(fieldsStream.GetFilePointer());
			
			int storedCount = 0;
            foreach (Field field  in doc.Fields())
            {
				if (field.IsStored())
					storedCount++;
			}
			fieldsStream.WriteVInt(storedCount);
			
            foreach (Field field in doc.Fields())
            {
				if (field.IsStored())
				{
					fieldsStream.WriteVInt(fieldInfos.FieldNumber(field.Name()));
					
					byte bits = 0;
					if (field.IsTokenized())
						bits |= 1;
					fieldsStream.WriteByte(bits);
					
					fieldsStream.WriteString(field.StringValue());
				}
			}
		}
	}
}
