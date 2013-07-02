//
// Important test: Type parameters and boxing (26.7.3).
//
// This tests the constrained_ prefix opcode.
//
using System;

public interface ICounter
{
	void Increment ();
}

namespace ValueTypeCounters
{
	public struct SimpleCounter : ICounter
	{
		public int Value;

		public void Increment ()
		{
			Value += 2;
		}
	}

	public struct PrintingCounter : ICounter
	{
		public int Value;

		public override string ToString ()
		{
			return Value.ToString ();
		}

		public void Increment ()
		{
			Value += 2;
		}
	}

	public struct ExplicitCounter : ICounter
	{
		public int Value;

		public override string ToString ()
		{
			return Value.ToString ();
		}

		void ICounter.Increment ()
		{
			Value++;
		}
	}

	public struct InterfaceCounter : ICounter
	{
		public int Value;

		public override string ToString ()
		{
			return Value.ToString ();
		}

		void ICounter.Increment ()
		{
			Value++;
		}

		public void Increment ()
		{
			Value += 2;
		}
	}
}

namespace ReferenceTypeCounters
{
	public class SimpleCounter : ICounter
	{
		public int Value;

		public void Increment ()
		{
			Value += 2;
		}
	}

	public class PrintingCounter : ICounter
	{
		public int Value;

		public override string ToString ()
		{
			return Value.ToString ();
		}

		public void Increment ()
		{
			Value += 2;
		}
	}

	public class ExplicitCounter : ICounter
	{
		public int Value;

		public override string ToString ()
		{
			return Value.ToString ();
		}

		void ICounter.Increment ()
		{
			Value++;
		}
	}

	public class InterfaceCounter : ICounter
	{
		public int Value;

		public override string ToString ()
		{
			return Value.ToString ();
		}

		void ICounter.Increment ()
		{
			Value++;
		}

		public void Increment ()
		{
			Value += 2;
		}
	}
}

namespace Test
{
	using V = ValueTypeCounters;
	using R = ReferenceTypeCounters;

	public class Test<T>
		where T : ICounter
	{
		public static void Foo (T x)
		{
			Console.WriteLine (x.ToString ());
			x.Increment ();
			Console.WriteLine (x.ToString ());
		}
	}

	public class X
	{
		public static void Main ()
		{
			Test<V.SimpleCounter>.Foo (new V.SimpleCounter ());
			Test<V.PrintingCounter>.Foo (new V.PrintingCounter ());
			Test<V.ExplicitCounter>.Foo (new V.ExplicitCounter ());
			Test<V.InterfaceCounter>.Foo (new V.InterfaceCounter ());
			Test<R.SimpleCounter>.Foo (new R.SimpleCounter ());
			Test<R.PrintingCounter>.Foo (new R.PrintingCounter ());
			Test<R.ExplicitCounter>.Foo (new R.ExplicitCounter ());
			Test<R.InterfaceCounter>.Foo (new R.InterfaceCounter ());
		}
	}
}
