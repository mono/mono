//
// System.Collections.CaseInsensitiveHashCodeProvider.cs
//
// Authors:
//   Sergey Chaban (serge@wildwestsoftware.com)
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

namespace System.Collections
{
	[Serializable]
	[MonoTODO ("Fix serialization compatibility with MS.NET")]
	public class CaseInsensitiveHashCodeProvider : IHashCodeProvider
	{
		static readonly CaseInsensitiveHashCodeProvider singleton = new CaseInsensitiveHashCodeProvider ();
		static readonly CaseInsensitiveHashCodeProvider singletonInvariant = new CaseInsensitiveHashCodeProvider (true);

		CultureInfo culture;

		// Public instance constructor
		public CaseInsensitiveHashCodeProvider ()
		{
			culture = CultureInfo.CurrentCulture;
		}

		private CaseInsensitiveHashCodeProvider (bool invariant)
		{
			// leave culture == null
		}

		public CaseInsensitiveHashCodeProvider (CultureInfo culture)
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

		public static CaseInsensitiveHashCodeProvider Default {
			get {
				return singleton;
			}
		}

#if NET_1_1
		public
#else
		internal
#endif
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

			if (culture != null && culture.LCID != CultureInfo.InvariantCulture.LCID) {
				str = str.ToLower (culture);
				for (int i = 0; i < str.Length; i++) {
					c = str [i];
					h = h * 31 + c;
				}
			}
			else {
				for (int i = 0; i < str.Length; i++) {
					c = Char.ToLowerInvariant (str [i]);
					h = h * 31 + c;
				}
			}
			return h;
		}
	}
}
