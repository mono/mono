// CS1715: `DerivedClass.Prop': type must be `System.EventHandler' to match overridden member `BaseClass.Prop'
// Line: 9

using System;

class BaseClass {
        protected virtual event EventHandler Prop;
}

delegate void TestD ();

class DerivedClass: BaseClass {
        protected override event TestD Prop;
}



