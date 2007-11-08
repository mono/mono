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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class DOMHTMLDocument: DOMObject, IDOMHTMLDocument
	{
		private nsIDOMHTMLDocument document;
		
		public DOMHTMLDocument (nsIDOMHTMLDocument document)
		{
			this.document = document;
		}

		#region IDOMDocument Members

		public IDOMHTMLElement Body {
			get	{
				nsIDOMHTMLElement body;
				this.document.getBody (out body);
				return new DOMHTMLElement (body);
			}
		}

		public string Title {
			get {
				this.document.getTitle (storage);
				return Base.StringGet (storage);
			}
			set {
				Base.StringSet (storage, value);
				this.document.setTitle (storage);
			}
		}

		public string Url {
			get
			{
				this.document.getURL (storage);
				return Base.StringGet (storage);
			}
		}

		public IDOMHTMLElement getElementById (string id)
		{
			nsIDOMElement nsElement;
			Base.StringSet (storage, id);
			this.document.getElementById (storage, out nsElement);
			return new DOMHTMLElement ((nsIDOMHTMLElement)nsElement);
		}

		#endregion
	}
}
