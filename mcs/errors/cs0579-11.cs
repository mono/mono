// CS0579: The attribute `System.ObsoleteAttribute' cannot be applied multiple times
// Line: 14


using System;

partial class C
{
	[Obsolete ("A")]
	partial void PartialMethod ()
	{
	}
	
	[Obsolete ("A")]
	partial void PartialMethod ();	
}
