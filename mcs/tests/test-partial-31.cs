using System;

namespace TestPartialOverride.BaseNamespace
{
	public abstract class Base
	{
		protected virtual void OverrideMe ()
		{
			Console.Out.WriteLine ("OverrideMe");
		}
	}
}

namespace TestPartialOverride.Outer.Nested.Namespace
{
	internal partial class Inherits
	{
		protected override void OverrideMe ()
		{
			Console.Out.WriteLine ("Overridden");
		}
	}
}

namespace TestPartialOverride.Outer
{
	namespace Nested.Namespace
	{
		internal partial class Inherits : TestPartialOverride.BaseNamespace.Base
		{
			public void DoesSomethignElse ()
			{
				OverrideMe ();
			}
		}
	}

	public class C
	{
		public static void Main ()
		{
			new TestPartialOverride.Outer.Nested.Namespace.Inherits ().DoesSomethignElse ();
		}
	}
}
