// CS0507: `DerivedClass.Test': cannot change access modifiers when overriding `protected' inherited member `BaseClass.Test'
// Line: 11

using System;

class BaseClass {
        protected virtual event EventHandler Test;
}

class DerivedClass: BaseClass {
        public override sealed event EventHandler Test;
}

