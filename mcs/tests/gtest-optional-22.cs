using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

interface I
{
	void Explicit ();
}

class CallerMemberTest : I
{
	object field = TraceStatic ("field");
	
	CallerMemberTest ()
		: this (TraceStatic (".ctor"))
	{
	}
	
	CallerMemberTest (object arg)
	{
	}
	
	static IEnumerable<int> Enumerator ()
	{
		TraceStatic ("Enumerator");
		yield return 1;
	}
	
	void I.Explicit ()
	{
		Trace ("Explicit");
	}
	
	public void GenericMethod<T> ()
	{
		Trace ("GenericMethod");
	}
	
	public int this [string arg] {
		set {
			Trace ("Item");
		}
	}
	
	public bool Property {
		get {
			Trace ("Property");
			return false;
		}
	}
	
	public static implicit operator CallerMemberTest (int i)
	{
		TraceStatic ("op_Implicit");
		return new CallerMemberTest ();
	}

	public void Trace(string expected, [CallerMemberName] string member = ";;")
	{
		Console.WriteLine (member);
		if (expected != member)
			throw new ApplicationException (member);
	}
	
	public static object TraceStatic(string expected, [CallerMemberName] object member = null)
	{
		Console.WriteLine (member);
		
		if (expected != member as string)
			throw new ApplicationException (string.Format ("`{0}' !=  `{1}'", expected, member as string));
		
		return member;
	}
	
	public static void Main ()
	{
		var c = new CallerMemberTest ();
		c.Trace ("Main");
		Action a = () => {
			c.Trace ("Main");
		};
		a ();
		
		a = () => TraceStatic ("Main");
		a ();
		
		foreach (var e in Enumerator ()) {
		}
		
		var atype = new {
			OO = TraceStatic ("Main")
		};
		
		var l = (from x in "ab" select TraceStatic ("Main")).ToList ();
		
		c.GenericMethod<long> ();
		c ["aa"] = 4;
		var p = c.Property;
		
		I i = c;
		i.Explicit ();
		
		CallerMemberTest op = 1;
	}
}