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
	public CharEnumeratorTest () {}

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

		AssertEquals("A2", true, ce.MoveNext ());

		AssertEquals ("A3", _s, GetFromEnumerator (ce));

		try {
			c = ce.Current;
		}
		catch (InvalidOperationException) {
			exception = true;
		}
		Assert ("A4", exception);

		AssertEquals("A5", false, ce.MoveNext() );
		AssertEquals("A6", false, ce.MoveNext() );

		ce.Reset ();

		try {
			c = ce.Current;
		}
		catch (InvalidOperationException) {
			exception = true;
		}
		Assert ("A7", exception);

		AssertEquals ("A8", true, ce.MoveNext ());

		AssertEquals ("A9", _s, GetFromEnumerator (ce));

		AssertEquals ("A10", false, ce.MoveNext ());
		AssertEquals ("A11", false, ce.MoveNext ());
	}

}

}
