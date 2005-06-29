// cs0553.cs: User-defined conversion `DerivedClass.implicit operator BaseClass(DerivedClass)' cannot convert to or from base class
// Line: 8

class BaseClass {
}

class DerivedClass: BaseClass {
        public static implicit operator BaseClass(DerivedClass value) {
                return new BaseClass();
        }
}


