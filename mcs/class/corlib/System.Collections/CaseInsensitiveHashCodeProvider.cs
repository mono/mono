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

		private static CaseInsensitiveHashCodeProvider singleton;
		private static CaseInsensitiveHashCodeProvider singletonInvariant;
		
		CultureInfo culture;


		// Class constructor

		static CaseInsensitiveHashCodeProvider ()
		{
			singleton = new CaseInsensitiveHashCodeProvider ();
			singletonInvariant = new CaseInsensitiveHashCodeProvider (CultureInfo.InvariantCulture);
		}



		// Public instance constructor

		public CaseInsensitiveHashCodeProvider ()
		{
		}

		public CaseInsensitiveHashCodeProvider (CultureInfo culture)
		{
			this.culture = culture;
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
		public static CaseInsensitiveHashCodeProvider DefaultInvariant {
			get {
				return singletonInvariant;
			}
		}
#endif


		//
		// Instance methods
		//

		//
		// IHashCodeProvider
		//

		public int GetHashCode (object obj)
		{
			if (obj == null) {
				throw new ArgumentNullException ("obj is null");
			}

			string str = obj as string;

			if (str == null)
				return obj.GetHashCode ();

			int h = 0;
			char c;

			int length = str.Length;
			for (int i = 0;i<length;i++) {
				c = Char.ToLower (str [i], culture);
				h = h * 31 + c;
			}

			return h;
		}

	} // CaseInsensitiveHashCodeProvider
}

