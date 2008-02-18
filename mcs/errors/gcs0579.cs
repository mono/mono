// CS0579: The attribute `System.ObsoleteAttribute' cannot be applied multiple times
// Line: 12


using System;

partial class C
{
	[Obsolete ("A")]
	partial void PartialMethod ();
	[Obsolete ("A")]
	partial void PartialMethod ()
	{
	}
}
