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
using System.Collections;
using System.Collections.Generic;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class StylesheetList : DOMObject, IStylesheetList
	{
		private nsIDOMStyleSheetList unmanagedStyles;
		private List<IStylesheet> styles;
		
		public StylesheetList(WebBrowser control, nsIDOMStyleSheetList stylesheetList) : base (control)
		{
			if (control.platform != control.enginePlatform)
				unmanagedStyles = nsDOMStyleSheetList.GetProxy (control, stylesheetList);
			else
				unmanagedStyles = stylesheetList;
			styles = new List<IStylesheet>();
		}
		
		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					Clear ();
				}
			}
			base.Dispose(disposing);
		}		
		#endregion
		
		#region Helpers
		protected void Clear () 
		{
			styles.Clear ();
		}

		internal void Load ()
		{
			Clear ();			
			uint count;
			unmanagedStyles.getLength (out count);
			for (int i = 0; i < count;i++) {
				nsIDOMStyleSheet style;
				unmanagedStyles.item ((uint)i, out style);
				styles.Add (new Stylesheet (control, style));
			}
		}
		#endregion
				
		IEnumerator IEnumerable.GetEnumerator()
		{
			if (styles.Count == 0)
				Load ();
			return styles.GetEnumerator(); 
		}

		public IStylesheet this [int index] {
			get {
				return styles[index];
			}
			set {
				styles[index] = value;
			}
		}
	
		public int Count {
			get {
				if (styles.Count == 0)
					Load ();
				return styles.Count;
			}
		}
	}
}
