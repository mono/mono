

using System;

class X {
        static void SetValue<T> (object o, T x)
        {
        }

        public static void Main ()
        {
                object o = null;
                double [] d = null;

                SetValue (o, d);
        }
}
