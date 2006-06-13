public class Test
{
        void Bar ()
        {
                G<int> g = G<int>.Instance;
        }

        // When it goes outside, there is no error.
        public class G<T>
        {
                public static G<T> Instance;
        }

	static void Main ()
	{ }
}
