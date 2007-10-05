// CS0553: User-defined conversion `DerivedClass.implicit operator BaseClass(DerivedClass)' cannot convert to or from a base class
// Line: 8

class BaseClass {
}

class DerivedClass: BaseClass {
        public static implicit operator BaseClass(DerivedClass value) {
                return new BaseClass();
        }
}


