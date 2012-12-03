// Compiler options: -linkresource:test-513.cs -linkresource:test-512.cs -linkresource:./test-511.cs,test

using System;
using System.IO;
using System.Reflection;

public class Test
{
	public static int Main ()
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		string[] resourceNames = a.GetManifestResourceNames ();
		if (resourceNames.Length != 3)
			return 1;
		if (resourceNames[0] != "test-513.cs")
			return 1;
		if (resourceNames[1] != "test-512.cs")
			return 1;
		if (resourceNames[2] != "test")
			return 1;
		FileStream f = a.GetFile ("test-513.cs");
		if (f == null)
			return 1;
		f = a.GetFile ("test-512.cs");
		if (f == null)
			return 1;
		f = a.GetFile ("test-511.cs");
		if (f == null)
			return 1;
		f = a.GetFile ("test");
		if (f != null)
			return 1;
		Stream s = a.GetManifestResourceStream ("test-513.cs");
		if (s == null)
			return 1;
		s = a.GetManifestResourceStream ("test-512.cs");
		if (s == null)
			return 1;
		s = a.GetManifestResourceStream ("test");
		if (s == null)
			return 1;
		s = a.GetManifestResourceStream ("test-511.cs");
		if (s != null)
			return 1;
		
		return 0;
	}
}
