// CS0505: `DerivedClass.value()': cannot override because `BaseClass.value' is not a method
// Line: 9

class BaseClass {
        protected int value;
}

class DerivedClass: BaseClass {
        protected override int value() {}
}

