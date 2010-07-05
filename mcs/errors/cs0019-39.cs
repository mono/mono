// CS0019: Operator `!=' cannot be applied to operands of type `method group' and `string'
// Line: 20

namespace InternalAccess
{
	public abstract class Base
	{
		internal string Prop () { return "a"; }
	}

	public class DerivedProtectedExample : Base
	{
		protected new string Prop { get { return "E"; } }
	}

	class MainClass
	{
		public static int Main ()
		{
			DerivedProtectedExample dpe = new DerivedProtectedExample ();
			if (dpe.Prop != "A")
				return 2;

			return 0;
		}
	}
}
