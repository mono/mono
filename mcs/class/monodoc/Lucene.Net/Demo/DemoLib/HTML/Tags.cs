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
namespace Lucene.Net.Demo.Html
{
	
	
    public sealed class Tags
    {
		
        /// <summary> contains all tags for which whitespaces have to be inserted for proper tokenization</summary>
        public static readonly System.Collections.Hashtable WS_ELEMS = new System.Collections.Hashtable();
        //public static readonly SupportClass.SetSupport WS_ELEMS = (SupportClass.SetSupport) new SupportClass.HashSetSupport(SupportClass.HashSetSupport.Synchronized(new SupportClass.HashSetSupport(new SupportClass.HashSetSupport())));
        static Tags()
        {
            {
                WS_ELEMS.Add("<hr", "<hr");
                WS_ELEMS.Add("<hr/", "<hr/"); // note that "<hr />" does not need to be listed explicitly
                WS_ELEMS.Add("<br", "<br");
                WS_ELEMS.Add("<br/", "<br/");
                WS_ELEMS.Add("<p", "<p");
                WS_ELEMS.Add("</p", "</p");
                WS_ELEMS.Add("<div", "<div");
                WS_ELEMS.Add("</div", "</div");
                WS_ELEMS.Add("<td", "<td");
                WS_ELEMS.Add("</td", "</td");
                WS_ELEMS.Add("<li", "<li");
                WS_ELEMS.Add("</li", "</li");
                WS_ELEMS.Add("<q", "<q");
                WS_ELEMS.Add("</q", "</q");
                WS_ELEMS.Add("<blockquote", "<blockquote");
                WS_ELEMS.Add("</blockquote", "</blockquote");
                WS_ELEMS.Add("<dt", "<dt");
                WS_ELEMS.Add("</dt", "</dt");
                WS_ELEMS.Add("<h1", "<h1");
                WS_ELEMS.Add("</h1", "</h1");
                WS_ELEMS.Add("<h2", "<h2");
                WS_ELEMS.Add("</h2", "</h2");
                WS_ELEMS.Add("<h3", "<h3");
                WS_ELEMS.Add("</h3", "</h3");
                WS_ELEMS.Add("<h4", "<h4");
                WS_ELEMS.Add("</h4", "</h4");
                WS_ELEMS.Add("<h5", "<h5");
                WS_ELEMS.Add("</h5", "</h5");
                WS_ELEMS.Add("<h6", "<h6");
                WS_ELEMS.Add("</h6", "</h6");
            }
        }
    }
}