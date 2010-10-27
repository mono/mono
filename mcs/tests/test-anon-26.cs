namespace TestGotoLabels
{
	class GotoLabelsTest
	{
		public delegate void MyDelegate ();

		public static int Main ()
		{
			TestMethod2 (delegate () {
				goto outLabel;
			outLabel:
				return;
			});

			return 0;
		}

		public static void TestMethod2 (MyDelegate md)
		{
			md.Invoke ();
		}
	}
} 
