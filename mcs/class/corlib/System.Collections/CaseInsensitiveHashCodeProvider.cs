//
// System.Collections.CaseInsensitiveHashCodeProvider.cs
//
// Authors:
//   Sergey Chaban (serge@wildwestsoftware.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
	[Serializable]
	[ComVisible(true)]
	[Obsolete ("Please use StringComparer instead.")]
#if INSIDE_CORLIB
	public
#else
	internal
#endif
	class CaseInsensitiveHashCodeProvider : IHashCodeProvider
	{
		static readonly CaseInsensitiveHashCodeProvider singletonInvariant = new CaseInsensitiveHashCodeProvider (
			CultureInfo.InvariantCulture);
		static CaseInsensitiveHashCodeProvider singleton;
		static readonly object sync = new object ();

		TextInfo m_text; // must match MS name for serialization

		// Public instance constructor
		public CaseInsensitiveHashCodeProvider ()
		{
			CultureInfo culture = CultureInfo.CurrentCulture;
			if (!AreEqual (culture, CultureInfo.InvariantCulture))
				m_text = CultureInfo.CurrentCulture.TextInfo;
		}

		public CaseInsensitiveHashCodeProvider (CultureInfo culture)
		{
			if (culture == null)
 				throw new ArgumentNullException ("culture");
			if (!AreEqual (culture, CultureInfo.InvariantCulture))
				m_text = culture.TextInfo;
		}

		//
		// Public static properties
		//

		public static CaseInsensitiveHashCodeProvider Default {
			get {
				// MS actually constructs a new instance on each call, for
				// performance reasons we're only constructing a new instance
				// if the CurrentCulture changes
				lock (sync) {
					if (singleton == null) {
						singleton = new CaseInsensitiveHashCodeProvider ();
					} else if (singleton.m_text == null) {
						if (!AreEqual (CultureInfo.CurrentCulture, CultureInfo.InvariantCulture))
							singleton = new CaseInsensitiveHashCodeProvider ();
					} else if (!AreEqual (singleton.m_text, CultureInfo.CurrentCulture)) {
						singleton = new CaseInsensitiveHashCodeProvider ();
					}
					return singleton;
				}
			}
		}

		static bool AreEqual (CultureInfo a, CultureInfo b)
		{
#if !NET_2_1
			return a.LCID == b.LCID;
#else
			return a.Name == b.Name;
#endif
		}

		static bool AreEqual (TextInfo info, CultureInfo culture)
		{
#if !NET_2_1
			return info.LCID == culture.LCID;
#else
			return info.CultureName == culture.Name;
#endif
		}

		public
		static CaseInsensitiveHashCodeProvider DefaultInvariant {
			get {
				return singletonInvariant;
			}
		}

		//
		// IHashCodeProvider
		//

		public int GetHashCode (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			string str = obj as string;

			if (str == null)
				return obj.GetHashCode ();

			int h = 0;
			char c;

			if ((m_text != null) && !AreEqual (m_text, CultureInfo.InvariantCulture)) {
				str = m_text.ToLower (str);
				for (int i = 0; i < str.Length; i++) {
					c = str [i];
					h = h * 31 + c;
				}
			} else {
				for (int i = 0; i < str.Length; i++) {
					c = Char.ToLower (str [i], CultureInfo.InvariantCulture);
					h = h * 31 + c;
				}
			}
			return h;
		}
	}
}
