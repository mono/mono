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
			int res=0;

			if (a is IComparable) {
				res = (a as IComparable).CompareTo (b);
			} else if (b is IComparable) {
				res = (b as IComparable).CompareTo (a);
			} else {
				throw new ArgumentException ("Neither a nor b IComparable");
			}

			return res;
		}

		public override string ToString ()
		{
			return "mono::System.Collections.Comparer";
		}

	}
}
