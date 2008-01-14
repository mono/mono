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
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Document : Node, IDocument
	{
		private nsIDOMHTMLDocument document;

		public Document (WebBrowser control, nsIDOMHTMLDocument document)
			: base (control, document)
		{
			if (control.platform != control.enginePlatform)
				this.document = nsDOMHTMLDocument.GetProxy (control, document);
			else
				this.document = document;
		}

		#region IDisposable Members
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.resources.Clear ();
					this.document = null;
				}
			}
			base.Dispose (disposing);
		}
		#endregion

		#region Internal
		internal nsIDOMDocument ComObject
		{
			get { return document; }
		}
		#endregion

		#region IDocument Members

		public IElement Body
		{
			get
			{
				if (!resources.Contains ("Body")) {
					nsIDOMHTMLElement element;
					this.document.getBody (out element);
					nsIDOMHTMLBodyElement b = element as nsIDOMHTMLBodyElement;
					resources.Add ("Body", new HTMLElement (control, b));
				}
				return resources["Body"] as IElement;
			}
		}

		public IElement DocumentElement
		{
			get
			{
				if (!resources.Contains ("DocumentElement")) {
					nsIDOMElement element;
					this.document.getDocumentElement (out element);
					resources.Add ("DocumentElement", new Element (control, element));
				}
				return resources["DocumentElement"] as IElement;
			}
		}

		public string Text
		{
			set
			{
			}
		}

		public string Title
		{
			get
			{
				this.document.getTitle (storage);
				return Base.StringGet (storage);
			}
			set
			{
				Base.StringSet (storage, value);
				this.document.setTitle (storage);
			}
		}

		public string Url
		{
			get
			{
				((nsIDOMHTMLDocument) this.document).getURL (storage);
				return Base.StringGet (storage);
			}
		}

		public IElement GetElementById (string id)
		{
			if (!resources.Contains ("GetElementById" + id)) {
				nsIDOMElement nsElement;
				Base.StringSet (storage, id);
				this.document.getElementById (storage, out nsElement);
				resources.Add ("GetElementById" + id, new HTMLElement (control, nsElement as nsIDOMHTMLElement));
			}
			return resources["GetElementById" + id] as IElement;
		}

		public IElementCollection GetElementsByTagName (string name)
		{
			if (!resources.Contains ("GetElementsByTagName" + name)) {
				nsIDOMNodeList nodes;
				this.document.getElementsByTagName (storage, out nodes);
				resources.Add ("GetElementsByTagName" + name, nodes);
			}
			return resources["GetElementsByTagName" + name] as IElementCollection;
		}


		#endregion
	}
}
