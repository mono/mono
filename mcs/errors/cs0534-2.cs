// CS0534: `DerivedClass' does not implement inherited abstract member `BaseClass.Value.set'
// Line: 8

abstract class BaseClass {
        protected abstract int Value { get; set; }
}

class DerivedClass: BaseClass {
        protected override int Value { get {} }
}
