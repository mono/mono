// No array static/dynamic initilizers should be produced in this test

using System;
using System.Reflection;

class T
{
	const byte c = 0;
	const string s = null;

	long [,,] a1 = new long [,,] {{{10,0}, {0,0}}, {{0,0}, {0,c}}};	
	byte [] a2 = new byte [] { 2 - 2, 0, c };
	decimal [] a3 = new decimal [] { 2m - 2m, 0m, c };
	string[,] a4 = new string[,] { {s, null}, { s, s }};
	T[] a5 = new T[] { null, default (T) };

	public static int Main ()
	{
		ConstructorInfo mi = typeof(T).GetConstructors ()[0];
		MethodBody mb = mi.GetMethodBody();
		
		if (mb.GetILAsByteArray ().Length > 90) {
			Console.WriteLine("Optimization failed");
			return 3;
		}
			
		return 0;
	}
}