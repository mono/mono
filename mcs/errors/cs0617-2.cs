// cs0617.cs : 'foo3' is not a valid named attribute argument. Named attribute arguments must be fields which are not readonly, static or const, or read-write properties which are not static.
// Line: 11

class BazAttribute : System.Attribute 
{
	public object foo3 { set {} }
}

class Test 
{
	[Baz (foo3 = 3)] void f3() {}
}
