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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class DOMImplementation : DOMObject, IDOMImplementation
	{
		private nsIDOMDOMImplementation unmanagedDomImpl;
		protected int hashcode;
		
		public DOMImplementation(WebBrowser control, nsIDOMDOMImplementation domImpl) : base (control)
		{
			if (control.platform != control.enginePlatform)
				unmanagedDomImpl = nsDOMDOMImplementation.GetProxy (control, domImpl);
			else
				unmanagedDomImpl = domImpl;
			hashcode = unmanagedDomImpl.GetHashCode ();				
		}
		
		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.unmanagedDomImpl = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion

		
		#region IDOMImplementation Members
		
		public bool HasFeature (string feature, string version) 
		{
			Base.StringSet (storage, feature);
			UniString ver = new UniString (version);
			bool ret;
			unmanagedDomImpl.hasFeature (storage, ver.Handle, out ret);
			return ret;
		}
		
		public IDocumentType CreateDocumentType (string qualifiedName, 
		                                         string publicId, 
		                                         string systemId)
		{
			nsIDOMDocumentType doctype;
			Base.StringSet (storage, qualifiedName);
			UniString pubId = new UniString (publicId);
			UniString sysId = new UniString (systemId);
			unmanagedDomImpl.createDocumentType (storage, pubId.Handle, sysId.Handle, out doctype);
			return new DocumentType (this.control, doctype);
		}
		
		public IDocument CreateDocument(string namespaceURI, 
		                                string qualifiedName, 
		                                IDocumentType doctype)
		{
			nsIDOMDocument doc;
			Base.StringSet (storage, namespaceURI);
			UniString qual = new UniString (qualifiedName);
			unmanagedDomImpl.createDocument (storage, qual.Handle, ((DocumentType)doctype).ComObject, out doc);
			control.documents.Add (doc.GetHashCode (), new Document (this.control, doc));
			return control.documents[doc.GetHashCode ()] as IDocument;
			
		}
		#endregion
				
	}
}
