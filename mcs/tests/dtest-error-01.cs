// Compiler options: -unsafe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

class Helper
{
	public unsafe static int* Foo (int i)
	{
		return null;
	}
}

class Tester
{
#pragma warning disable 169
	void NonInvocable ()
	{
		AssertError (
			() => {
				dynamic d = 1;
				d ();
			}, "Cannot invoke a non-delegate type");
	}
	
	void Using_1 ()
	{
		AssertError (
			() => {
				using (dynamic d = 1) { }
			}, "Cannot implicitly convert type 'int' to 'System.IDisposable'");
	}
	
	void Unsafe_1 ()
	{
		dynamic d = 1;
		AssertError (
			() => Helper.Foo (d),
			"Dynamic calls cannot be used in conjunction with pointers");
	}
	
	void NullableConversion ()
	{
		dynamic d = 1;
		AssertError (
			() => {
				dynamic b = false;
				byte? b2 = null;
				b &= b2;
			}, "Operator '&=' cannot be applied to operands of type 'bool' and 'byte?'");
	}
	
#pragma warning restore 169
	
	static void AssertError (Action a, string msg)
	{
		try {
			a ();
		} catch (RuntimeBinderException e) {
			if (e.Message != msg)
				throw new ApplicationException ("Expected error message: " + e.Message);
			
			return;
		}
		
		throw new ApplicationException ("Expected error");
	}

	static bool RunTest (MethodInfo test)
	{
		Console.Write ("Running test {0, -25}", test.Name);
		try {
			test.Invoke (new Tester (), null);
			Console.WriteLine ("OK");
			return true;
		} catch (Exception e) {
			Console.WriteLine ("FAILED");
			Console.WriteLine (e.InnerException.Message);
			return false;
		}
	}

	public static int Main ()
	{
		var tests = from test in typeof (Tester).GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
					where test.GetParameters ().Length == 0
					orderby test.Name
					select RunTest (test);

		int failures = tests.Count (a => !a);
		Console.WriteLine (failures + " tests failed");
		return failures;
	}
}
