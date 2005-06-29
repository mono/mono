// cs0554.cs: User-defined conversion `BaseClass.implicit operator BaseClass(DerivedClass)' cannot convert to or from derived class
// Line: 5

class BaseClass {
        public static implicit operator BaseClass(DerivedClass value) {
                return new BaseClass();
        }
}

class DerivedClass: BaseClass {
}

