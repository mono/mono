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
using DateField = Lucene.Net.Documents.DateField;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
namespace Lucene.Net.Demo
{
	
	/// <summary>A utility for making Lucene Documents from a File. </summary>
	
	public class FileDocument
	{
		/// <summary>Makes a document for a File.
		/// <p>
		/// The document has three fields:
		/// <ul>
		/// <li><code>path</code>--containing the pathname of the file, as a stored,
		/// tokenized Field;
		/// <li><code>modified</code>--containing the last modified date of the file as
		/// a keyword Field as encoded by <a
		/// href="lucene.document.DateField.html">DateField</a>; and
		/// <li><code>contents</code>--containing the full contents of the file, as a
		/// Reader Field;
		/// </summary>
		public static Document Document(System.IO.FileInfo f)
		{
			
			// make a new, empty document
			Document doc = new Document();
			
			// Add the path of the file as a Field named "path".  Use a Text Field, so
			// that the index stores the path, and so that the path is searchable
			doc.Add(Field.Text("path", f.FullName));
			
			// Add the last modified date of the file a Field named "modified".  Use a
			// Keyword Field, so that it's searchable, but so that no attempt is made
			// to tokenize the Field into words.
			doc.Add(Field.Keyword("modified", DateField.TimeToString(((f.LastWriteTime.Ticks - 621355968000000000) / 10000))));
			
			// Add the contents of the file a Field named "contents".  Use a Text
			// Field, specifying a Reader, so that the text of the file is tokenized.
			// ?? why doesn't FileReader work here ??
			System.IO.FileStream is_Renamed = new System.IO.FileStream(f.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			System.IO.StreamReader reader = new System.IO.StreamReader(new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).CurrentEncoding);
			doc.Add(Field.Text("contents", reader));
			
			// return the document
			return doc;
		}
		
		private FileDocument()
		{
		}
	}
}