// CS0115: `B.MyMissingSuperclassEvent' is marked as an override but no suitable event found to override
// Line: 10

using System;

class A {
}

class B : A {
	public override event EventHandler MyMissingSuperclassEvent;
}