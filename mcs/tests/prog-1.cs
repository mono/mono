// Compiler options: -r:dll-1.dll

namespace QtSamples
{
	using Qt;

	public class QtClass: QtSupport
	{
		public QtClass()
		{
			mousePressEvent += new MousePressEvent( pressEvent );
		}
		
		public void pressEvent() { }
	}


	public class Testing
	{
		public static int Main()
		{
			QtClass q = new QtClass();

			return 0;
		}
	}
}



