//
// System.Web.UI.Triplet.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	public class Triplet
	{
		public object First;
		public object Second;
		public object Third;

		public Triplet ()
		{
		}

		public Triplet (object x, object y)
		{
			First = x;
			Second = y;
		}

		public Triplet (object x, object y, object z)
		{
			First = x;
			Second = y;
			Third = z;
		}
	}
}
