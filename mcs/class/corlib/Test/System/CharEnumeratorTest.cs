//
// CharEnumeratorTest.cs - NUnit Test Cases for the System.CharEnumerator class
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2002 Duco Fijma
//

using NUnit.Framework;
using System;

namespace MonoTests.System
{

public class CharEnumeratorTest : TestCase
{
	public CharEnumeratorTest () : base ("MonoTests.System.CharEnumeratorTest testcase") {}
        public CharEnumeratorTest (string name) : base (name) {}

	public static ITest Suite {
		get {
			return new TestSuite (typeof (CharEnumeratorTest));
		}
	}

	public void TestBasic ()
	{
		string s = "Emma en Sophie";
		string s2 = "";
		CharEnumerator ce = s.GetEnumerator ();
		bool cont;

		cont = ce.MoveNext ();
		while (cont) {
			s2 += ce.Current;
			cont = ce.MoveNext ();
		}
	
		AssertEquals ("A1", s, s2);
	}

}

}
