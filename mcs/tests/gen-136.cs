using System;

namespace Martin {
	public class A
	{
		public readonly long Data;

		public A (long data)
		{
			this.Data = data;
		}

		public static explicit operator B (A a)
		{
			return new B ((int) a.Data);
		}
	}

	public class B
	{
		public readonly int Data;

		public B (int data)
		{
			this.Data = data;
		}

		public static implicit operator A (B b)
		{
			return new A (b.Data);
		}
	}

	class X
	{
		static void Main ()
		{
			B? b = new B (5);
			A? a = b;
			B? c = (B?) a;
			B? d = (Martin.B?) a;
		}
	}
}
