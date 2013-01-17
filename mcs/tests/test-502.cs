class Base
{
    public int Property { get { return 42; } }
    public static void Main () {}
}

// TC #1
class Derived : Base
{
    public int get_Property() { return 42; }
}

// TC #2
class BaseClass {
        protected virtual int Value { 
                get {
                        return 0;
                }
                set { }
        }
}

abstract class DerivedClass: BaseClass {
        protected int get_Value () {
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
}