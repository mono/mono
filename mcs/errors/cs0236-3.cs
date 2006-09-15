// CS0236: A field initializer cannot reference the nonstatic field, method, or property `Test.o1'
// Line: 8
delegate void Foo ();

class Test
{
        object o1;

        Foo h = delegate () {
                o1 = null;
        };
}

