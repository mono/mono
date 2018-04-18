// CS0165: Use of unassigned local variable `s'
// Line: 12

class X
{
    static string Foo (object arg)
    {
        if (arg is string s) {

        }

        return s;
    }
}