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
#if NET_2_0
using System.Collections.Generic;
#endif
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class StylesheetList : DOMObject, IStylesheetList
	{
		private nsIDOMStyleSheetList unmanagedStyles;
#if NET_2_0
		private List<IStylesheet> styles;
#else
		private IStylesheet[] styles;
		protected int styleCount;
#endif
		
		public StylesheetList(WebBrowser control, nsIDOMStyleSheetList stylesheetList) : base (control)
		{
			if (control.platform != control.enginePlatform)
				unmanagedStyles = nsDOMStyleSheetList.GetProxy (control, stylesheetList);
			else
				unmanagedStyles = stylesheetList;
#if NET_2_0
			styles = new List<IStylesheet>();
#endif
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
#if NET_2_0
			styles.Clear ();
#else
			if (styles != null) {
				for (int i = 0; i < styleCount; i++) {
					styles[i] = null;
				}
				styleCount = 0;
				styles = null;
			}
#endif
		}

		internal void Load ()
		{
			Clear ();			
			uint count;
			unmanagedStyles.getLength (out count);
#if ONLY_1_1
			Stylesheet[] tmpstyles = new Stylesheet[count];
#endif
			for (int i = 0; i < count;i++) {
				nsIDOMStyleSheet style;
				unmanagedStyles.item ((uint)i, out style);
#if NET_2_0
				styles.Add (new Stylesheet (control, style));
#else
				tmpstyles[styleCount++] = new Stylesheet (control, style);
#endif
			}
#if ONLY_1_1
			
			styles = new Stylesheet[styleCount];
			Array.Copy (tmpstyles, styles, styleCount);
#endif
		}
		#endregion
				
#if NET_2_0
		IEnumerator IEnumerable.GetEnumerator()
		{
			if (styles.Count == 0)
				Load ();
			return styles.GetEnumerator(); 
		}
#else
		
#region IEnumerable members
		public IEnumerator GetEnumerator () 
		{
			return new StyleEnumerator (this);
		}
#endregion

#endif
		
#if NET_2_0
		public IStylesheet this [int index] {
			get {
				return styles[index];
			}
			set {
				styles[index] = value;
			}
		}
#else
		public IStylesheet this [int index] {
			get {
				if (index < 0 || index >= styleCount)
					throw new ArgumentOutOfRangeException ("index");
				return styles [index];								
			}
			set {
				if (index < 0 || index >= styleCount)
					throw new ArgumentOutOfRangeException ("index");
				styles [index] = value as IStylesheet;
			}
		}		
#endif
		
	
		public int Count {
			get {
#if NET_2_0
				if (styles.Count == 0)
					Load ();
				return styles.Count;
#else
				if (unmanagedStyles != null && styles == null)
					Load ();
				return styleCount; 
#endif
			}
		}
		
#if ONLY_1_1
		internal class StyleEnumerator : IEnumerator {

			private StylesheetList collection;
			private int index = -1;

			public StyleEnumerator (StylesheetList collection)
			{
				this.collection = collection;
			}

			public object Current {
				get {
					if (index == -1)
						return null;
					return collection [index];
				}
			}

			public bool MoveNext ()
			{
				if (index + 1 >= collection.Count)
					return false;
				index++;
				return true;
			}

			public void Reset ()
			{
				index = -1;
			}
		}		
#endif
		
		
	
	}
}
