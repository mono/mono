//
// System.Drawing.Text.FontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//			Sanjay Gupta (gsanjay@novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
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
using System;
using System.Collections;
using System.Collections.Specialized;
using awt = java.awt;

namespace System.Drawing.Text
{
	/// <summary>
	/// Summary description for FontCollection.
	/// </summary>
	public abstract class FontCollection : IDisposable
	{
		protected ArrayList _fonts;

		protected FontCollection()
		{
			_fonts = new ArrayList();
		}

		public FontFamily[] Families {
			get {
				Hashtable h = CollectionsUtil.CreateCaseInsensitiveHashtable(_fonts.Count);
				for (int i = 0; i < _fonts.Count; i++) {
					string family = ((awt.Font)_fonts[i]).getFamily();
					if (!h.ContainsKey(family))
						h[family] = new FontFamily(family);
				}

				ICollection values = h.Values;
				FontFamily[] families = new FontFamily[values.Count];
				values.CopyTo(families, 0);
				return families;
			}
		}

		internal virtual string GetFamilyName(string name) {
			for (int i = 0; i < _fonts.Count; i++) {
				string family = ((awt.Font)_fonts[i]).getFamily();
				if (string.Compare(family, name, true) == 0)
					return family;
			}

			return null;
		}
		#region IDisposable Members

		public void Dispose() {
			// TODO:  Add FontCollection.Dispose implementation
		}

		#endregion
	}
}
