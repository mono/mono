// cs0115-4.cs: `DerivedClass.get_Value()': no suitable method found to override
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
