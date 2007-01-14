// CS0672: Member `B.MyEvent' overrides obsolete member `A.MyEvent'. Add the Obsolete attribute to `B.MyEvent'
// Line: 13
// Compiler options: -warnaserror

using System;

class A {
	[Obsolete]
	public virtual event EventHandler MyEvent;
}

class B : A {
	public override event EventHandler MyEvent;
}
