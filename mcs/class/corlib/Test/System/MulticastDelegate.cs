// MulticastDelegate.cs - NUnit Test Cases for MulticastDelegates (C# delegates)
//
// Daniel Stodden (stodden@in.tum.de)
//
// (C) Daniel Stodden
// 

using NUnit.Framework;
using System;

namespace MonoTests.System
{

[TestFixture]
public class MulticastDelegateTest {
	
	public MulticastDelegateTest() {}

	private delegate char MyDelegate( ref string s );

	private char MethodA( ref string s ) 
	{
		s += "a";
		return 'a';
	}

	private char MethodB( ref string s )
	{
		s += "b";
		return 'b';
	}

	private char MethodC( ref string s )
	{
		s += "c";
		return 'c';
	}

	private char MethodD( ref string s )
	{
		s += "d";
		return 'd';
	}

	[Test]
	public void TestEquals()
	{
		MyDelegate dela = new MyDelegate( MethodA );
		MyDelegate delb = new MyDelegate( MethodB );
		MyDelegate delc = new MyDelegate( MethodC );

		Assert.AreEqual(false, dela == delb , "#A01");
		
		MyDelegate del1, del2;

		del1 = dela + delb;
		del2 = delb + delc;
		Assert.AreEqual(false, del1 == del2 , "#A02");
		
		del1 += delc;
		del2 = dela + del2;
		Assert.AreEqual(true, del1 == del2 , "#A03");
		
		object o = new object ();
		
		Assert.AreEqual (false, dela.Equals (o), "#A04");
		
	}

	[Test]
	public void TestCombineRemove()
	{
		MyDelegate dela = new MyDelegate( MethodA );
		MyDelegate delb = new MyDelegate( MethodB );
		MyDelegate delc = new MyDelegate( MethodC );
		MyDelegate deld = new MyDelegate( MethodD );

		string val;
		char res;

		// test combine
		MyDelegate del1, del2;
		del1 = dela + delb + delb + delc + delb + delb + deld;
		val = "";
		res = del1( ref val );
		Assert.AreEqual("abbcbbd", val , "#A01");
		Assert.AreEqual('d', res , "#A02");

		// test remove
		del2 = del1 - ( delb + delb );
		val = "";
		res = del2( ref val );
		Assert.AreEqual("abbcd", val , "#A03");
		Assert.AreEqual('d', res , "#A04");

		// we did not affect del1, did we?
		val = "";
		res = del1( ref val );
		Assert.AreEqual("abbcbbd", val , "#A05");
	}

	[Test] //Bug #12536
	public void TestCombineBothDirections ()
	{
		MyDelegate dela = new MyDelegate( MethodA );
		MyDelegate delb = new MyDelegate( MethodB );
		MyDelegate delc = new MyDelegate( MethodC );
		MyDelegate deld = new MyDelegate( MethodD );

		string val;
		char res;

		MyDelegate a = dela + delb;
		val = "";
		res = a (ref val);
		Assert.AreEqual ("ab", val, "#1");
		Assert.AreEqual ('b', res, "#2");

		MyDelegate b = delc + deld;
		val = "";
		res = b (ref val);
		Assert.AreEqual ("cd", val, "#3");
		Assert.AreEqual ('d', res, "#4");

		MyDelegate c = a + b;
		val = "";
		res = c (ref val);
		Assert.AreEqual ("abcd", val, "#5");
		Assert.AreEqual ('d', res, "#6");
	}
}
}
