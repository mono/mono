//
// System.Collections.CaseInsensitiveComparer
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//



using System;
using System.Collections;



namespace System.Collections {

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

		public override string ToString ()
		{
			return "mono::System.Collections.CaseInsensitiveComparer";
		}


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

