// cs0023: The `+' operator cannot be applied to operand of type `X'
// Line : 6

class X {
        static void Foo (object o)
        {
        }
        
        static void Main () {
                Foo (+(X)null);
        }
}



