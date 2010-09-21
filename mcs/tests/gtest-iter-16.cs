using System.Collections.Generic;

namespace Test
{
	public abstract class Base
	{
		public virtual IEnumerable<Base> GetStuff (int a)
		{
			yield return this;
		}
	}

	public abstract class Derived : Base
	{
		public override IEnumerable<Base> GetStuff (int a)
		{
			foreach (var x in base.GetStuff (a))
				yield return x;
		}
	}

	public class SpecialDerived : Derived
	{
		public override IEnumerable<Base> GetStuff (int a)
		{
			foreach (var x in base.GetStuff (a))
				yield return x;
		}

		public static void Main ()
		{
			Base b = new SpecialDerived ();
			foreach (var a in b.GetStuff (5)) {
			}
		}
	}
}
