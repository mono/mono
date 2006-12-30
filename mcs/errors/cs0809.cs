// CS0809: Obsolete member `B.Property' overrides non-obsolete member `A.Property'
// Line: 17
// Compiler options: -warnaserror -warn:4

using System;

class A
{
	public virtual int Property {
		set { }
	}
}

class B : A
{
	[Obsolete ("TEST")]
	public override int Property {
		set { }
	}
}