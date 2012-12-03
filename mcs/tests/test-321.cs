using System;
 
struct X : IDisposable {
        public void Dispose ()
        {
        }
 
        public static void Main ()
        {
                X x = new X ();
                using (x)
                        ;
        }
}