// cs0560.cs: Accessor 'ErrorClass.Value.get' : cannot override 'BaseClass.Value.get' because it is hidden by 'DerivedClass.get_Value()'
// Line: 22

class BaseClass {
        protected virtual int Value { 
                get {
                        return 0;
                }
                set { }
        }
}

abstract class DerivedClass: BaseClass {
        protected new int get_Value () {
                return 1;
        }
}


class ErrorClass: DerivedClass {
        protected override int Value { 
                get {
                        return 0;
                }
                set { }
        }

		static void Main () {}
}

