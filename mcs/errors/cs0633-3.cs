// cs0633-3.cs: The argument to the `System.Diagnostics.ConditionalAttribute' attribute must be a valid identifier
// Line: 8

using System;
using System.Diagnostics;

class TestClass {
	[Conditional ("UNDEFINED CONDITION")]
	static void ConditionalMethod ()
	{
	}
}


