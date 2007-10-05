// CS0554: User-defined conversion `BaseClass.implicit operator BaseClass(DerivedClass)' cannot convert to or from a derived class
// Line: 5

class BaseClass {
        public static implicit operator BaseClass(DerivedClass value) {
                return new BaseClass();
        }
}

class DerivedClass: BaseClass {
}

