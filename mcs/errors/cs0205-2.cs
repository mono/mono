// cs0205-2.cs: Cannot call an abstract base member `A.Foobar'
// Line: 15

using System;

public abstract class A
{
        public abstract int Foobar { get; }
}

public class B: A
{
        public override int Foobar  {
		get {
                	return base.Foobar;
		}
        }

        static void Main ()
        {
                B b = new B ();
                if (b.Foobar == 1)
			;
        }
}

