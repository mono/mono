using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
class SomeCustomAttribute : Attribute {
	public SomeCustomAttribute ()
	{
	}
}

class MainClass {

	[SomeCustomAttribute]
	public int a;

	[SomeCustomAttribute]
	public int x, y;

	public static int Main ()
	{
		Type t = typeof (MainClass);
		FieldInfo[] fia = t.GetFields();

		foreach (FieldInfo fi in fia) {
			object[] ca = fi.GetCustomAttributes(typeof (SomeCustomAttribute), false);
			System.Console.WriteLine ("Field: {0} [{1}]", fi.Name, ca.Length);
			if (ca.Length != 1)
				return 1;
		}
		
		Console.WriteLine ("OK");
		
		return 0;
	}
}