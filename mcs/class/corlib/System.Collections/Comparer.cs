//
// System.Collections.Comparer
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//


using System;
using System.Collections;


namespace System.Collections {

	[Serializable]
	public sealed class Comparer : IComparer {

		public static readonly Comparer Default;


		// Class constructor

		static Comparer ()
		{
			Default = new Comparer ();
		}



		// Public instance constructor

		private Comparer ()
		{
		}



		// IComparer

		public int Compare (object a, object b)
		{
			if (a == b)
				return 0;
			else if (a == null)
				return -1;
			else if (b == null)
				return 1;
			else if (a is IComparable)
				return (a as IComparable).CompareTo (b);
			else if (b is IComparable)
				return -(b as IComparable).CompareTo (a);

			throw new ArgumentException ("Neither a nor b IComparable");
		}
	}
}
