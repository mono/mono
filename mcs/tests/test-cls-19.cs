// Compiler options: -warnaserror

using System;

[assembly: CLSCompliant (true)]

public partial class A
{
	public partial class PartialClass
	{
		public void Method1 (int arg)
		{
		}

		[CLSCompliant (false)]
		public void Method2 (uint arg)
		{
		}
	}
	
	public static void Main ()
	{
	}
}

partial class A
{
	/*public*/ partial class PartialClass
	{
		[CLSCompliant (false)]
		public void Method3 (uint arg)
		{
		}
	}
}
