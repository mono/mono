using System;

namespace InternalAccess
{
	public abstract class Base
	{
		internal Base () { }
		internal string Prop { get { return "A"; } }
	}

	public class DerivedInternalExample : Base
	{
		public DerivedInternalExample () { }
		internal new string Prop { get { return "D"; } }
	}

	public class DerivedProtectedExample : Base
	{
		public DerivedProtectedExample () { }
		protected new string Prop { get { return "E"; } }
	}

	class MainClass
	{
		public static int Main ()
		{
			DerivedInternalExample die = new DerivedInternalExample ();
			if (die.Prop != "D")
				return 1;

			DerivedProtectedExample dpe = new DerivedProtectedExample ();
			if (dpe.Prop != "A")
				return 2;

			return 0;
		}
	}
}
