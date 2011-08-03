// CS0546: `DerivedClass.Value.set': cannot override because `BaseClass.Value' does not have an overridable set accessor
// Line: 9

abstract class BaseClass {
        protected abstract int Value { get; }
}

class DerivedClass: BaseClass {
        protected override int Value { get {} set {} }
}

