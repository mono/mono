// Compiler options: -r:MyAssembly01=test-externalias-00-lib.dll -r:MyAssembly02=test-externalias-01-lib.dll

extern alias MyAssembly01;
extern alias MyAssembly02;
using System;

public class Test
{
	public static void Main ()
	{
		MyAssembly01::GlobalClass.JustForFirst ();
		MyAssembly02::GlobalClass.JustForSecond ();
		
		MyAssembly01::Namespace1.MyClass1.JustForFirst ();
		MyAssembly02::Namespace1.MyClass1.JustForSecond ();
	}
}

