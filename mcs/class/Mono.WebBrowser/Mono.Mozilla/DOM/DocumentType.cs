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
	internal class DocumentType : Node, IDocumentType
	{
		internal nsIDOMDocumentType doctype;

		public DocumentType (WebBrowser control, nsIDOMDocumentType doctype)
			: base (control, doctype as nsIDOMNode)
		{
			if (control.platform != control.enginePlatform)
				this.doctype = nsDOMDocumentType.GetProxy (control, doctype);
			else
				this.doctype = doctype;
		}
		

		#region IDisposable Members
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.resources.Clear ();
					this.doctype = null;
				}
			}
			base.Dispose (disposing);
		}
		#endregion

		internal nsIDOMDocumentType ComObject
		{
			get { return doctype; }
		}
		
		#region IDocumentType
		public string Name {
			get {
				doctype.getName (storage);
				return Base.StringGet (storage);
			}
		}
		public INamedNodeMap Entities {
			get {
				nsIDOMNamedNodeMap nodeMap;
				doctype.getEntities (out nodeMap);
				return new NamedNodeMap (this.control, nodeMap);
			}
		}
		public INamedNodeMap Notations {
			get {
				nsIDOMNamedNodeMap nodeMap;
				doctype.getNotations (out nodeMap);
				return new NamedNodeMap (this.control, nodeMap);
			}
		}
		public string PublicId {
			get {
				doctype.getPublicId (storage);
				return Base.StringGet (storage);					
			}
		}
		public string SystemId {
			get {
				doctype.getSystemId (storage);
				return Base.StringGet (storage);					
			}
		}
		public string InternalSubset  {
			get {
				doctype.getInternalSubset (storage);
				return Base.StringGet (storage);					
			}
		}
		#endregion
		
		public override int GetHashCode () {
			return this.hashcode;
		}		

	}
}
