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

[TestFixture]
public class CharEnumeratorTest
{
	public CharEnumeratorTest () {}

	string _s;

	[SetUp]
	protected void SetUp ()
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

	[Test]
	public void TestBasic ()
	{
		CharEnumerator ce = _s.GetEnumerator ();

		ce.MoveNext ();

		Assert.AreEqual (_s, GetFromEnumerator (ce), "A1");
	}

	[Test]
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

		Assert.AreEqual ("Sophie", GetFromEnumerator(ce2), "A1");
	}

	[Test]
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
		Assert.IsTrue (exception, "A1");

		Assert.AreEqual(true, ce.MoveNext (), "A2");

		Assert.AreEqual (_s, GetFromEnumerator (ce), "A3");

		try {
			c = ce.Current;
		}
		catch (InvalidOperationException) {
			exception = true;
		}
		Assert.IsTrue (exception, "A4");

		Assert.AreEqual(false, ce.MoveNext() , "A5");
		Assert.AreEqual(false, ce.MoveNext() , "A6");

		ce.Reset ();

		try {
			c = ce.Current;
		}
		catch (InvalidOperationException) {
			exception = true;
		}
		Assert.IsTrue (exception, "A7");

		Assert.AreEqual (true, ce.MoveNext (), "A8");

		Assert.AreEqual (_s, GetFromEnumerator (ce), "A9");

		Assert.AreEqual (false, ce.MoveNext (), "A10");
		Assert.AreEqual (false, ce.MoveNext (), "A11");
	}

}

}
