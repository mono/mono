// CS0165: Use of unassigned local variable `foo'
// Line: 17

class X
{
        static void Main ()
        {
                int foo;

                int i = 0;
                if (i == 1)
                        goto e;

                goto f;

        b:
                i += foo;

        c:
                goto b;

        e:
                foo = 5;

        f:
                goto c;
        }
}
