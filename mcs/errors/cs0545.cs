// CS0545: `DerivedClass.Value.get': cannot override because `BaseClass.Value' does not have an overridable get accessor
// Line: 9

abstract class BaseClass {
        protected abstract int Value { set; }
}

class DerivedClass: BaseClass {
        protected override int Value { get {} set {} }
}

