//
// System.Collections.Comparer
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//


using System;
using System.Collections;
using System.Globalization;
using System.Threading;


namespace System.Collections {

	[Serializable]
	public sealed class Comparer : IComparer {

		public static readonly Comparer Default;

#if NET_1_1
		public static readonly Comparer DefaultInvariant;
#endif

		CultureInfo _culture;

		// Class constructor

		static Comparer ()
		{
			Default = new Comparer ();
#if NET_1_1
			DefaultInvariant = new Comparer (CultureInfo.InvariantCulture);
#endif
		}
		

		// Public instance constructor

		private Comparer ()
		{
		}

#if NET_1_1
		private Comparer (CultureInfo culture)
		{
			_culture = culture;
		}
#endif


		// IComparer

		public int Compare (object a, object b)
		{
			if (a == b)
				return 0;
			else if (a == null)
				return -1;
			else if (b == null)
				return 1;
			else if (a is string && b is string) {
				if (_culture != null)
					return _culture.CompareInfo.Compare ((string)a, (string)b);
				else
					return Thread.CurrentThread.CurrentCulture.CompareInfo.Compare ((string)a, (string)b);
			}
			else if (a is IComparable)
				return (a as IComparable).CompareTo (b);
			else if (b is IComparable)
				return -(b as IComparable).CompareTo (a);

			throw new ArgumentException ("Neither a nor b IComparable");
		}
	}
}
