// cs0579.cs : Duplicate 'CLSCompliant' attribute
// Line : 10

using System;

[assembly:CLSCompliant(true)]

namespace DuplicateAttributes {
	[CLSCompliant(true)]
	[type:CLSCompliant(true)]
	public class ClassA {}
}
