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
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Attribute : Node, IAttribute
	{
		private nsIDOMAttr attribute;

		public Attribute (WebBrowser control, nsIDOMAttr domAttribute)
			: base (control, domAttribute as nsIDOMNode)
		{
			if (control.platform != control.enginePlatform)
				this.attribute = nsDOMAttr.GetProxy (control, domAttribute);
			else
				this.attribute = domAttribute;
		}

		#region IDisposable Members
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.attribute = null;
				}
			}
			base.Dispose (disposing);
		}
		#endregion

		#region IAttribute Members
		public string Name {
			get {
				this.attribute.getName (storage);
				return Base.StringGet (storage);
			}
		}

		public new string Value {
			get {
				this.attribute.getValue (storage);
				return Base.StringGet (storage);
			}
			set {
				Base.StringSet (storage, value);
				this.attribute.setValue (storage);
			}
		}
		#endregion
		
		public override int GetHashCode () {
			return this.hashcode;
		}
	}
}
