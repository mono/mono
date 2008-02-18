// CS0746: Invalid anonymous type member declarator. Anonymous type members must be a member assignment, simple name or member access expression
// Line: 16


using System;

public class Test
{
	static void Main ()
	{
		var c = new { new Test () };
	}
}
