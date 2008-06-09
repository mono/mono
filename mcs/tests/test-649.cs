using System;

class MainClass
{
	public struct Decimal2
	{
		public Decimal2 (double d)
		{
			value = new Decimal (d);
		}

		public Decimal2 (Decimal d)
		{
			value = d;
		}

		public static explicit operator Decimal2 (Decimal d)
		{
			return new Decimal2 (d);
		}

		public static explicit operator Decimal2 (double d)
		{
			return new Decimal2 (d); 
		}

		public static implicit operator Decimal (Decimal2 d)
		{
			return d.value;
		}

		private Decimal value;
	}

	public static void Main (string [] args)
	{
		Console.WriteLine ("double   = {0}", 1.1367 * 11.9767 - 0.6);
		Console.WriteLine ("Decimal2 = {0}", ((Decimal2) 1.1367 * (Decimal2) 11.9767 - (Decimal2) 0.6));
		Console.WriteLine ("Decimal2 = {0}", new Decimal2 (1.1367) * (Decimal2) 11.9767 - (Decimal2) 0.6);
		Console.WriteLine ("Decimal2 = {0}", (Decimal2) 11.9767 * new Decimal2 (1.1367) - (Decimal2) 0.6);
		Console.WriteLine ("Decimal2 = {0}", (new Decimal2 (1.1367) * (Decimal2) 11.9767 - (Decimal2) 0.6));
		Console.WriteLine ("Decimal2 = {0}", ((Decimal2) 1.1367 * (Decimal2) 11.9767 - (Decimal) 0.6));
		Console.WriteLine ("Decimal2 = {0}", (1.14 * 11.9767 - 0.6));
		Console.WriteLine ("Decimal2 = {0}", ((Decimal2) 1.1367 * (Decimal) 11.9767 - (Decimal2) 0.6));
		Console.WriteLine ("Decimal2 = {0}", ((Decimal2) 1.1367 * (Decimal2) 11.9767 - (Decimal2) 0.6));
	}
}
