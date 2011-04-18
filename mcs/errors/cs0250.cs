// CS0250: Do not directly call your base class Finalize method. It is called automatically from your destructor
// Line: 9

class BaseClass {
}

class DerivedClass: BaseClass {
        ~DerivedClass () {
                base.Finalize ();
        }
}

