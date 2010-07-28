// CS0205: Cannot call an abstract base member `A.Foobar()'
// Line: 21

using System;

public abstract class A
{
        public abstract void Foobar ();
}

public class B: A
{
        public override void Foobar ()
        {
                base.Foobar ();
        }

        static void Main ()
        {
                B b = new B ();
                b.Foobar ();
        }
}
