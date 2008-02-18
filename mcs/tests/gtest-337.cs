

using System;

class X {
        static void SetValue<T> (object o, T x)
        {
        }

        static void Main ()
        {
                object o = null;
                double [] d = null;

                SetValue (o, d);
        }
}
