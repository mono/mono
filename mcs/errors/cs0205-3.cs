// cs0205-3.cs: Cannot call an abstract base member `A.Foobar'
// Line: 15
// Compiler options: -r:CS0205-3-lib.dll

using System;

public class B: A1
{
        protected override int Foobar  {
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

