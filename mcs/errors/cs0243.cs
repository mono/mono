// CS0243: Conditional not valid on `DerivedClass.Show()' because it is an override method
// Line: 10

class BaseClass {
        protected virtual void Show () {}
}

class DerivedClass: BaseClass {
        [System.Diagnostics.Conditional("DEBUG")] protected override void Show () {}
            
        static void Main () {}
}

