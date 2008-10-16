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
using HTMLParser = Lucene.Net.Demo.Html.HTMLParser;
using Lucene.Net.Documents;
namespace Lucene.Net.Demo
{
	
    /// <summary>A utility for making Lucene Documents for HTML documents. </summary>
	
    public class HTMLDocument
    {
        internal static char dirSep = System.IO.Path.DirectorySeparatorChar.ToString()[0];
		
        public static System.String UID(System.IO.FileInfo f)
        {
            // Append path and date into a string in such a way that lexicographic
            // sorting gives the same results as a walk of the file hierarchy.  Thus
            // null (\u0000) is used both to separate directory components and to
            // separate the path from the date.
            return f.FullName.Replace(dirSep, '\u0000') + "\u0000" + DateField.TimeToString(((f.LastWriteTime.Ticks - 621355968000000000) / 10000));
        }
		
        public static System.String UID2URL(System.String uid)
        {
            System.String url = uid.Replace('\u0000', '/'); // replace nulls with slashes
            return url.Substring(0, (url.LastIndexOf((System.Char) '/')) - (0)); // remove date from end
        }
		
        public static Document Document(System.IO.FileInfo f)
        {
            // make a new, empty document
            Document doc = new Document();
			
            // Add the url as a field named "url".  Use an UnIndexed field, so
            // that the url is just stored with the document, but is not searchable.
            doc.Add(Field.UnIndexed("url", f.FullName.Replace(dirSep, '/')));
			
            // Add the last modified date of the file a field named "modified".  Use a
            // Keyword field, so that it's searchable, but so that no attempt is made
            // to tokenize the field into words.
            doc.Add(Field.Keyword("modified", DateField.TimeToString(((f.LastWriteTime.Ticks - 621355968000000000) / 10000))));
			
            // Add the uid as a field, so that index can be incrementally maintained.
            // This field is not stored with document, it is indexed, but it is not
            // tokenized prior to indexing.
            doc.Add(new Field("uid", UID(f), false, true, false));
			
            HTMLParser parser = new HTMLParser(f);
			
            // Add the tag-stripped contents as a Reader-valued Text field so it will
            // get tokenized and indexed.
            doc.Add(Field.Text("contents", parser.GetReader()));
			
            // Add the summary as an UnIndexed field, so that it is stored and returned
            // with hit documents for display.
            doc.Add(Field.UnIndexed("summary", parser.GetSummary()));
			
            // Add the title as a separate Text field, so that it can be searched
            // separately.
            doc.Add(Field.Text("title", parser.GetTitle()));
			
            // return the document
            return doc;
        }
		
        private HTMLDocument()
        {
        }
    }
}