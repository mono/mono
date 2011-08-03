// CS0238: `DerivedClass.Show()' cannot be sealed because it is not an override
// Line: 10

class BaseClass {
        void Show() {}
}

class DerivedClass: BaseClass {
        sealed void Show() {}
            
        static void Main() {}
}

