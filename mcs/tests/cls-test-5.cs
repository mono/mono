using System;

[assembly:CLSCompliant(true)]

class X1 {
        public bool AA;
        internal bool aa;
}

class X2: X1 {
        public bool aA;
}

public class X3 {
        internal void bb(bool arg) {}
        internal bool bB;
        public void BB() {}
}

class X4 {
        public void method(int arg) {}
        public void method(bool arg) {}
        public bool method() { return false; }
}


public class BaseClass {
        //protected internal bool mEthod() { return false; }
}

public class CLSClass: BaseClass {
        public CLSClass() {}
        public CLSClass(int arg) {}
            
        //public int this[int index] { set {} }
        //protected int this[bool index] { set {} }
       
        public bool setItem;
        static public implicit operator CLSClass(bool value) {
               return new CLSClass(2);
        }

        static public implicit operator CLSClass(int value) {
               return new CLSClass(2);
        }
        
        [CLSCompliant(false)]
        public void Method() {}
            
        internal int Method(bool arg) { return 1; }
        internal void methoD() {}
            
        public static void Main() {}
}

public class oBject: Object {
}

namespace A {
    public class C1 {
    }
}

namespace B {
    public class c1 {
    }
}

public class c1 {
}