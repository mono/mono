// cs0553.cs: 'BaseClass(DerivedClass)' : user defined conversion to/from base class
// Line: 8

class BaseClass {
}

class DerivedClass: BaseClass {
        public static implicit operator BaseClass(DerivedClass value) {
                return new BaseClass();
        }
}

