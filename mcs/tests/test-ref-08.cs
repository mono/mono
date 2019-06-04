using System;

namespace ClassLibrary1
{
	public class C
	{

		class B
		{
			int v;
			public ref int this[int index]
			{
				get
				{
					return ref v;
				}
			}
		}


		class Gen<T> where T : struct
		{
			T v;
			public ref T this[int index]
			{
				get
				{
					return ref v;
				}
			}
		}

		struct Val
		{
		}

		class BB
		{
			Val v;
			public ref Val this[int index]
			{
				get
				{
					return ref v;
				}
			}
		}

		void MM ()
		{
			var bbb = new BB();
			Val v1 = bbb[0];
			bbb[1] = v1;

			ref Val v2 = ref bbb[2];
			bbb[2] = v2;
		}

		static int[] a = new int[1];
		public static void Main()
		{
			var bb = new B();
			int b = 1;
			bb[0] = b;
			a[0] = Add2(ref b, 2);

			var bbb = new BB();
			bbb[0] = new Val();

			var v = new Val();
			bbb[1] = v;

			var v2 = bbb[2];

			bbb[3] = v2;


			bbb[3] = bbb[2];



			var ggg = new Gen<Val>();
			ggg[0] = new Val();

			var g = new Val();
			ggg[1] = v;

			var g2 = ggg[2];

			ggg[3] = v2;


			ggg[3] = ggg[2];
		}

		public static ref int Add2(ref int a, int b)
		{
			return ref a;
		}
	}
}