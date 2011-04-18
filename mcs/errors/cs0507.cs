// CS0507: `DerivedClass.Show()': cannot change access modifiers when overriding `protected' inherited member `BaseClass.Show()'
// Line: 9

class BaseClass {
        protected virtual void Show () {}
}

class DerivedClass: BaseClass {
        public override void Show () {}
}

