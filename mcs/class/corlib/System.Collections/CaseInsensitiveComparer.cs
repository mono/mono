//
// System.Collections.CaseInsensitiveComparer.cs
//
// Authors:
//   Sergey Chaban (serge@wildwestsoftware.com)
//   Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System.Globalization;

namespace System.Collections
{
	[Serializable]
	public class CaseInsensitiveComparer : IComparer
	{
		private static CaseInsensitiveComparer defaultComparer = new CaseInsensitiveComparer ();
		private static CaseInsensitiveComparer defaultInvariantComparer = new CaseInsensitiveComparer (true);

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

#if NET_1_1
		public
#else
		internal
#endif
		static CaseInsensitiveComparer DefaultInvariant {
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
