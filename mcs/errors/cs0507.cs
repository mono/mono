// cs0506.cs: 'DerivedClass.Show()' : cannot change access modifiers when overriding inherited member 'BaseClass.Show()'
// Line: 9

class BaseClass {
        protected virtual void Show () {}
}

class DerivedClass: BaseClass {
        public override void Show () {}
}

