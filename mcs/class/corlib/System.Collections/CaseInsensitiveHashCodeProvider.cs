//
// System.Collections.CaseInsensitiveHashCodeProvider
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//



using System;
using System.Collections;
using System.Globalization;

namespace System.Collections {

	[Serializable]
	public class CaseInsensitiveHashCodeProvider : IHashCodeProvider {

		static readonly CaseInsensitiveHashCodeProvider singleton = new CaseInsensitiveHashCodeProvider ();
		static readonly CaseInsensitiveHashCodeProvider singletonInvariant = new CaseInsensitiveHashCodeProvider (true);
		
		CultureInfo culture;

		// Public instance constructor

		public CaseInsensitiveHashCodeProvider ()
		{
			culture = CultureInfo.CurrentCulture;
		}

		public CaseInsensitiveHashCodeProvider (bool invariant)
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
		// Instance methods
		//

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

			int length = str.Length;
			if (culture != null) {
				for (int i = 0;i<length;i++) {
					c = Char.ToLower (str [i], culture);
					h = h * 31 + c;
				}
			}
			else {
				for (int i = 0;i<length;i++) {
					c = Char.ToLowerInvariant (str [i]);
					h = h * 31 + c;
				}
			}

			return h;
		}

	} // CaseInsensitiveHashCodeProvider
}

