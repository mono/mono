using System;

public delegate void Foo (object source);

interface IFoo {
        event Foo OnFoo;
}

class ErrorCS0071 : IFoo {
        public event Foo IFoo.OnFoo () { }
        public static void Main () {
        }
}






