// CS0114: `B.MyEvent' hides inherited member `A.MyEvent'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword
// Line: 12
// Compiler options: -warnaserror -warn:2

using System;

abstract class A {
	public abstract event EventHandler MyEvent;
}

class B : A {
	public event EventHandler MyEvent;
}
