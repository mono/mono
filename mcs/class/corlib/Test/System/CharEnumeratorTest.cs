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

	string _s;

	protected override void SetUp ()
	{
		_s = "Emma en Sophie";
	}

	private string GetFromEnumerator (CharEnumerator ce)
	{
		string res = "";
		bool cont = true;

		while (cont) {
			res += ce.Current;
			cont = ce.MoveNext ();
		}

		return res;
	}

	public void TestBasic ()
	{
		CharEnumerator ce = _s.GetEnumerator ();

		ce.MoveNext ();

		AssertEquals ("A1", _s, GetFromEnumerator (ce));
	}

	public void TestClone ()
	{
		CharEnumerator ce1, ce2=null;
		bool cont;

		ce1 = _s.GetEnumerator ();
		cont = ce1.MoveNext ();
		while (cont) {
			if (ce1.Current == 'S') {
				ce2 = (CharEnumerator) (ce1.Clone ());
			}
			cont = ce1.MoveNext ();
		}

		AssertEquals ("A1", "Sophie", GetFromEnumerator(ce2));
	}

	public void TestReadOutOfBounds ()
	{
		char c;
		bool exception;
		CharEnumerator ce = _s.GetEnumerator ();
	
		try {
			c = ce.Current;
			exception = false;
		}
		catch (InvalidOperationException) {
			exception = true;
		}
		Assert ("A1", exception);

		ce.MoveNext ();

		AssertEquals ("A2", _s, GetFromEnumerator (ce));

		try {
			c = ce.Current;
		}
		catch (InvalidOperationException) {
			exception = true;
		}
		Assert ("A3", exception);

		ce.Reset ();

		try {
			c = ce.Current;
		}
		catch (InvalidOperationException) {
			exception = true;
		}
		Assert ("A4", exception);

		ce.MoveNext ();

		AssertEquals ("A5", _s, GetFromEnumerator (ce));
	}

}

}
