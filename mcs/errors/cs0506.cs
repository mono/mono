// cs0506.cs: `DerivedClass.Show()': cannot override inherited member `BaseClass.Show()' because it is not marked virtual, abstract or override
// Line: 9

class BaseClass {
        protected void Show () {}
}

class DerivedClass: BaseClass {
        protected override void Show () {}
}

