using System;

namespace A
{
	interface IA<T>
	{
		int Foo (T value);
	}

	internal partial class C : IA<C.NA>
	{
		private abstract class NA
		{
		}

		int IA<NA>.Foo (NA value)
		{
			return 0;
		}

		static void Main ()
		{
		}
	}
}

namespace A
{
	internal partial class C : IA<C.NB>
	{
		private class NB
		{
		}

		int IA<NB>.Foo (NB value)
		{
			return 0;
		}
	}
}