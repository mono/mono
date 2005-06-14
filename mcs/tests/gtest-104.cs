class MainClass
{
        class Gen<T>
        {
		public void Test ()
		{ }
        }

        class Der : Gen<int>
        {
        }

        static void Main ()
        {
		object o = new Der ();
                Gen<int> b = (Gen<int>) o;
		b.Test ();
        }
}

