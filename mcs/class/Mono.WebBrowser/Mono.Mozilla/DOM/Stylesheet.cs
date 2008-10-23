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
	internal class Stylesheet : DOMObject, IStylesheet
	{
		private nsIDOMStyleSheet unmanagedStyle;
		protected int hashcode;
		
		public Stylesheet(WebBrowser control, nsIDOMStyleSheet stylesheet) : base (control)
		{
			if (control.platform != control.enginePlatform)
				unmanagedStyle = nsDOMStyleSheet.GetProxy (control, stylesheet);
			else
				unmanagedStyle = stylesheet;
			hashcode = unmanagedStyle.GetHashCode ();			
		}
		
		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.unmanagedStyle = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion
		
#region Properties
		public string Type {
			get {
				unmanagedStyle.getType (storage);
				return Base.StringGet (storage);
			}
		}
		public string Href {
			get { 
				unmanagedStyle.getHref (storage);
				return Base.StringGet (storage);
			}
		}
		
		
		public bool Disabled { 
			get {
				bool ret;
				unmanagedStyle.getDisabled (out ret);
				return ret;
			}
			
			set {
				unmanagedStyle.setDisabled (value);
			}
		}
		
		public INode OwnerNode { 
			get {
				nsIDOMNode owner;
				unmanagedStyle.getOwnerNode (out owner);
/*				
				nsIClassInfo classinfo = owner as nsIClassInfo;
				uint count;
				IntPtr list;
				bool isStyleElement = false;
				classinfo.getInterfaces (out count, out list);
				for (int i = 0; i < count; i++) {
					Guid guid = (Guid) System.Runtime.InteropServices.Marshal.PtrToStructure (list, typeof(Guid));
					if (typeof (Mono.Mozilla.nsIDOMHTMLStyleElement).GUID.Equals (guid)) {
						isStyleElement = true;
						break;
					}
				}
				if (isStyleElement) 
*/
				
//				if ((owner as nsIDOMHTMLElement) != null)
//					return new HTMLElement (this.control, owner as Mono.Mozilla.nsIDOMHTMLElement);
				return GetTypedNode (owner);
			}
		}
		
		public IStylesheet ParentStyleSheet { 
			get {
				nsIDOMStyleSheet parent;
				unmanagedStyle.getParentStyleSheet (out parent);
				return new Stylesheet (this.control, parent);
			}
		}

		public string Title { 
			get {
				unmanagedStyle.getTitle (storage);
				return Base.StringGet (storage);
			}
		}
		public IMediaList Media { 
			get {
				return null;
			}
		}
#endregion
		
		
#region Methods		
		public override int GetHashCode () {
			return this.hashcode;
		}
#endregion
	}
}
