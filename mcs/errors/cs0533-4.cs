// CS0533: `B.MyEvent' hides inherited abstract member `A.MyEvent'
// Line: 11

using System;

abstract class A {
	public abstract event EventHandler MyEvent;
}

class B : A {
	public event EventHandler MyEvent;
}
