// CS0534: `B' does not implement inherited abstract member `A.MyEvent.add'
// Line: 11

using System;

abstract class A {
	public abstract event EventHandler MyEvent;
}

class B : A {
	public event EventHandler MyEvent;
}
