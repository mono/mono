// CS0579: The attribute `System.CLSCompliantAttribute' cannot be applied multiple times
// Line : 10

using System;

[assembly:CLSCompliant(true)]

namespace DuplicateAttributes {
	[CLSCompliant(true)]
	[CLSCompliant(true)]
	public class ClassA {}
}
