//
// System.Collections.CaseInsensitiveComparer
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//



using System;
using System.Collections;
using System.Globalization;



namespace System.Collections {

	[Serializable]
	public class CaseInsensitiveComparer : IComparer {

		private static CaseInsensitiveComparer singleton;


		// Class constructor

		static CaseInsensitiveComparer ()
		{
			singleton=new CaseInsensitiveComparer ();
		}


		// Public instance constructor

		public CaseInsensitiveComparer ()
		{
		}

		[MonoTODO]
		public CaseInsensitiveComparer (CultureInfo culture)
		{
			throw new NotImplementedException ();
		}


		//
		// Public static properties
		//

		public static CaseInsensitiveComparer Default {
			get {
				return singleton;
			}
		}


		//
		// Instance methods
		//

		//
		// IComparer
		//

		public int Compare (object a, object b)
		{
			string str1 = a as string;
			string str2 = b as string;


			int res = 0;

			if (str1 != null && str2 != null) {
				res = String.Compare (str1, str2, true);
			}

			return res;
		}



	} // CaseInsensitiveComparer
}

