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

		private static CaseInsensitiveComparer default_comparer, default_invariant_comparer;
		private CultureInfo cinfo;

		// Public instance constructor

		public CaseInsensitiveComparer ()
		{
		    cinfo = Thread.CurrentThread.CurrentCulture;
		}

		public CaseInsensitiveComparer (CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException("culture");
			cinfo = culture;
		}


		//
		// Public static properties
		//

		/* Don't do this in the class constructor, because
		 * CultureInfo needs to be able to use
		 * CaseInsensitiveComparer (Invariant), and the
		 * default CIC needs to construct a CultureInfo.
		 */
		public static CaseInsensitiveComparer Default {
			get {
				if(default_comparer==null) {
					lock (typeof (CaseInsensitiveComparer)) {
						if(default_comparer==null) {
							default_comparer=new CaseInsensitiveComparer ();
						}
					}
				}
				
				return(default_comparer);
			}
		}

		public static CaseInsensitiveComparer DefaultInvariant {
			get {
				if(default_invariant_comparer==null) {
					lock (typeof (CaseInsensitiveComparer)) {
						if(default_invariant_comparer==null) {
							default_invariant_comparer=new CaseInsensitiveComparer (CultureInfo.InvariantCulture);
						}
					}
				}
				
				return(default_invariant_comparer);
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

