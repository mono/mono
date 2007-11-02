// CS0122: `Test.SomeAttribute.SomeAttribute()' is inaccessible due to its protection level
// Line: 10

using System;

namespace Test
{
	public class SomeAttribute : Attribute
	{
		SomeAttribute() {}
	}

	[SomeAttribute]
	public class SomeClass
	{
	} 
}
