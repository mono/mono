//
// System.Drawing.InstalledFontCollection.cs
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
// Author: Konstantin Triger (kostat@mainsoft.com)
//

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
	/// Summary description for InstalledFontCollection.
	/// </summary>
	public sealed class InstalledFontCollection : FontCollection
	{
		Hashtable _names;
		public InstalledFontCollection()
		{
			_fonts.AddRange(
				java.awt.GraphicsEnvironment.getLocalGraphicsEnvironment().getAllFonts());

			Hashtable h = CollectionsUtil.CreateCaseInsensitiveHashtable(_fonts.Count);
			for (int i = 0; i < _fonts.Count; i++) {
				string family = ((awt.Font)_fonts[i]).getFamily();
				if (!h.ContainsKey(family))
					h[family] = family;
			}

			_names = h;
		}

		internal override string GetFamilyName(string name) {
			return (string)_names[name];
		}

	}
}
