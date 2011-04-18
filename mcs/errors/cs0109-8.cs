// CS0109: The member `DerivedClass.get_Value()' does not hide an inherited member. The new keyword is not required
// Line: 14
// Compiler options: -warnaserror -warn:4

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

