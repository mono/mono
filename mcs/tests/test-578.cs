namespace Test1
{
	public delegate int TestDelegate1 ();

	public interface TestItem
	{
		event TestDelegate1 OnUpdate;
	}

	public class TestItem1 : TestItem
	{
		private TestDelegate1 delegates1;

		public TestItem1()
		{
		}

		public int Test()
		{
			return delegates1 ();
		}

		public virtual event TestDelegate1 OnUpdate
		{
			add
			{
				System.Console.WriteLine("OnUpdate add 1");

				delegates1 += value;
			}
			remove
			{
				System.Console.WriteLine("OnUpdate remove 2");
				delegates1 -= value;
			}
		}

	}

	public class TestItem2 : TestItem1
	{
		public TestItem2()
		{
		}

		public override event TestDelegate1 OnUpdate
		{
			add
			{
				System.Console.WriteLine("OnUpdate add 2");
				base.OnUpdate += value;
			}
			remove
			{
				System.Console.WriteLine("OnUpdate remove 2");
				base.OnUpdate -= value;
			}
		}
	}

	class CC
	{
		public static int Main()
		{
			TestItem1 ti = new TestItem2();
			ti.OnUpdate += delegate() { return 5;  };
			if (ti.Test() != 5)
				return 1;
			
			return 0;
		}
	}
}
