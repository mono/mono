//
// System.Collections.CaseInsensitiveComparer.cs
//
// Authors:
//   Sergey Chaban (serge@wildwestsoftware.com)
//   Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Collections
{
	[ComVisible(true)]
	[Serializable]
#if INSIDE_CORLIB
	public
#else
	internal
#endif
	class CaseInsensitiveComparer : IComparer
	{
		readonly static CaseInsensitiveComparer defaultComparer = new CaseInsensitiveComparer ();
		readonly static CaseInsensitiveComparer defaultInvariantComparer = new CaseInsensitiveComparer (true);

		private CultureInfo culture;

		// Public instance constructor
		public CaseInsensitiveComparer ()
		{
			//LAMESPEC: This seems to be encoded while the object is created while Comparer does this at runtime.
			culture = CultureInfo.CurrentCulture;
		}

		private CaseInsensitiveComparer (bool invariant)
		{
			// leave culture == null
		}

		public CaseInsensitiveComparer (CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			if (culture.LCID != CultureInfo.InvariantCulture.LCID)
				this.culture = culture;
			// else leave culture == null
		}

		//
		// Public static properties
		//
		public static CaseInsensitiveComparer Default {
			get {
				return defaultComparer;
			}
		}

		public static CaseInsensitiveComparer DefaultInvariant {
			get {
				return defaultInvariantComparer;
			}
		}

		//
		// IComparer
		//
		public int Compare (object a, object b)
		{
			string sa = a as string;
			string sb = b as string;

			if ((sa != null) && (sb != null)) {
				if (culture != null)
					return culture.CompareInfo.Compare (sa, sb, CompareOptions.IgnoreCase);
				else
					// FIXME: We should call directly into an invariant compare once available in string
					return CultureInfo.InvariantCulture.CompareInfo.Compare (sa, sb, CompareOptions.IgnoreCase);
			}
			else
				return Comparer.Default.Compare (a, b);
		}
	}
}
