// cs0205: can not call abstract base method
//
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
