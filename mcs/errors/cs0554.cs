// cs0554.cs: 'BaseClass.implicit operator BaseClass(DerivedClass)' : user defined conversion to/from derived class
// Line: 5

class BaseClass {
        public static implicit operator BaseClass(DerivedClass value) {
                return new BaseClass();
        }
}

class DerivedClass: BaseClass {
}

