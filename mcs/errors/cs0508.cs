// cs0508.cs: `DerivedClass.Show()': return type must be `void' to match overridden member `BaseClass.Show()'
// Line: 9

class BaseClass {
        protected virtual void Show () {}
}

class DerivedClass: BaseClass {
        protected override bool Show () {}
}

