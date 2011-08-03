// CS0541: `Interface.ICloneable.Clone()': explicit interface declaration can only be declared in a class or struct
// Line: 7

using System;

interface Interface: ICloneable {
        void ICloneable.Clone ();
}

class Test {
	static void Main () {}
}

