using System;

class T{
        void SomeMethod (Converter <Int32, Int32> converter) {}
        void SomeCaller () {
                SomeMethod (delegate (Int32 a) { return a; });
        }

	public static void Main ()
	{ }
}
