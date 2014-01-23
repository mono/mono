// Compiler options: -r:FULL=System.dll

extern alias FULL;
using System;
using NameValueCollection =
FULL::System.Collections.Specialized.NameValueCollection;

public class test
{
	public static void Main () { }
}
