// CS0506: `DerivedClass.Test': cannot override inherited member `BaseClass.Test' because it is not marked virtual, abstract or override
// Line: 11

using System;

class BaseClass {
        protected event EventHandler Test;
}

class DerivedClass: BaseClass {
        protected override event EventHandler Test;
}

