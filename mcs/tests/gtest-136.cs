using System;

namespace Martin {
	public struct A
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

	public struct B
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
		public static void Main ()
		{
			B? b = new B (5);
			A? a = b;
			B? c = (B?) a;
			B? d = (Martin.B?) a;
		}
	}
}
