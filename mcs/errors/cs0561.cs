// cs0561.cs: 'DerivedClass.get_Value()' : cannot override 'BaseClass.Value.get' because it is a special compiler-generated method
// Line: 13

class BaseClass {
        protected virtual int Value { 
                get {
                        return 0;
                }
        }
}

class DerivedClass: BaseClass {
        protected override int get_Value () {
                return 1;
        }

		static void Main () {}
}
