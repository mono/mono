// cs0508.cs: 'DerivedClass.Show()' : cannot change return type when overriding inherited member 'BaseClass.Show()'
// Line: 9

class BaseClass {
        protected virtual void Show () {}
}

class DerivedClass: BaseClass {
        protected override bool Show () {}
}

