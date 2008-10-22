// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using Mono.WebBrowser;

namespace Mono.WebBrowser.DOM
{
	public interface IDocument : INode
	{
		IElement 			Active { get; }
		string 				ActiveLinkColor { get; set;}
		IElementCollection 	Anchors { get; }
		IElementCollection 	Applets { get; }
		string 				Background { get; set; }
		string 				BackColor { get; set; }
		IElement 			Body { get; }
		string 				Charset { get; set; }
		string 				Cookie { get; set; }
		IElement 			DocumentElement { get; }
		IDocumentType		DocType { get; }		
		string 				Domain { get; }
		string 				ForeColor { get; set; }
		IElementCollection 	Forms { get; }
		IElementCollection 	Images { get; }
		IDOMImplementation	Implementation { get; }
		string 				LinkColor { get; set; }
		IElementCollection 	Links { get; }
		IStylesheetList 	Stylesheets {get;}
		string 				Title { get; set;}
		string 				Url { get; }
		string 				VisitedLinkColor { get; set; }
		IWindow 			Window { get; }

		IAttribute 			CreateAttribute (string name);
		IElement 			CreateElement (string tagName);
		IElement 			GetElementById (string id);
		IElement 			GetElement (int x, int y);
		IElementCollection 	GetElementsByTagName (string id);
		void 				Write (string text);
		
		string InvokeScript (string script);

		int GetHashCode ();
		
		event EventHandler LoadStopped;
	}
}
