using System;

class Tzap
{
	protected class Baz : Tzap.Bar
	{

		public void Gazonk ()
		{
			this.Foo ();
		}

		public static void Main ()
		{
		}
	}

	protected abstract class Bar
	{
		protected virtual void Foo ()
		{
		}
	}
}