//
// System.Collections.CaseInsensitiveComparer
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//   Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//



using System;
using System.Threading;
using System.Collections;
using System.Globalization;



namespace System.Collections {

	[Serializable]
	public class CaseInsensitiveComparer : IComparer {

		private static CaseInsensitiveComparer singleton;
		private CultureInfo cinfo;
		// Class constructor

		static CaseInsensitiveComparer ()
		{
		    singleton=new CaseInsensitiveComparer ();
		}


		// Public instance constructor

		public CaseInsensitiveComparer ()
		{
		    cinfo = Thread.CurrentThread.CurrentCulture;
		}

		public CaseInsensitiveComparer (CultureInfo culture)
		{
		    if (culture==null)
			throw new ArgumentNullException("culture");
		    cinfo = culture;
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
  		    string sa = a as string;
		    string sb = b as string;

		    if ((sa != null) && (sb != null))
			return String.Compare (sa,sb,true,cinfo);
		    
		    return Comparer.Default.Compare (a,b);
		}



	} // CaseInsensitiveComparer
}

