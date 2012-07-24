//
// System.Collections.SecureHashCodeProvider.cs
//
// Authors:
//   Sergey Chaban (serge@wildwestsoftware.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright 2012 Xamarin, Inc (http://xamarin.com)
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
using System.Globalization;

namespace System.Web.Util
{
	class SecureHashCodeProvider : IHashCodeProvider
	{
		static readonly SecureHashCodeProvider singletonInvariant = new SecureHashCodeProvider (CultureInfo.InvariantCulture);
		static SecureHashCodeProvider singleton;
		static readonly object sync = new object ();
		static readonly int seed;
		
		TextInfo m_text; // must match MS name for serialization

		public static SecureHashCodeProvider Default {
			get {
				lock (sync) {
					if (singleton == null) {
						singleton = new SecureHashCodeProvider ();
					} else if (singleton.m_text == null) {
						if (!AreEqual (CultureInfo.CurrentCulture, CultureInfo.InvariantCulture))
							singleton = new SecureHashCodeProvider ();
					} else if (!AreEqual (singleton.m_text, CultureInfo.CurrentCulture)) {
						singleton = new SecureHashCodeProvider ();
					}
					return singleton;
				}
			}
		}
		
		public static SecureHashCodeProvider DefaultInvariant {
			get { return singletonInvariant; }
		}
		
		static SecureHashCodeProvider ()
		{
			// It should be enough to fend off the attack described in
			// https://bugzilla.novell.com/show_bug.cgi?id=739119
			// In order to predict value of the seed, the attacker would have to know the exact time when
			// the server process started and since it's a remote attack, this is next to impossible.
			// Using milliseconds instead of ticks here would make it easier for the attackers since there
			// would only be as many as 1000 possible values
			seed = (int)DateTime.UtcNow.Ticks;
		}
		
		// Public instance constructor
		public SecureHashCodeProvider ()
		{
			CultureInfo culture = CultureInfo.CurrentCulture;
			if (!AreEqual (culture, CultureInfo.InvariantCulture))
				m_text = CultureInfo.CurrentCulture.TextInfo;
		}

		public SecureHashCodeProvider (CultureInfo culture)
		{
			if (culture == null)
 				throw new ArgumentNullException ("culture");
			if (!AreEqual (culture, CultureInfo.InvariantCulture))
				m_text = culture.TextInfo;
		}

		static bool AreEqual (CultureInfo a, CultureInfo b)
		{
			return a.LCID == b.LCID;
		}

		static bool AreEqual (TextInfo info, CultureInfo culture)
		{
			return info.LCID == culture.LCID;
		}
		
		public int GetHashCode (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			string str = obj as string;

			if (str == null)
				return obj.GetHashCode ();

			int h = seed;
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
