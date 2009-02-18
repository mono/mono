using System;

namespace MonoBug
{
	public static class Stuff
	{
		public static GenericStuff<T1, T2> CreateThing<T1, T2> (T1 firstValue, T2 secondValue)
		{
			return new GenericStuff<T1, T2> (firstValue, secondValue);
		}
	}

	public class GenericStuff<T1, T2>
	{
		public readonly T1 FirstValue;
		public readonly T2 SecondValue;

		public GenericStuff (T1 firstValue, T2 secondValue)
		{
			FirstValue = firstValue;
			SecondValue = secondValue;
		}
	}

	public static class Program
	{
		public static void Main ()
		{
			var thing = Stuff.CreateThing (default (string), "abc");
			Console.WriteLine (thing.FirstValue);
			Console.WriteLine (thing.SecondValue);
		}
	}
}
