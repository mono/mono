using System;

// Without namespace, this error does not happen.
namespace MyTest
{
        public class Test
        {
                public interface Inner
                {
                        void Foo ();
                }
        }

        public class Test2 : MarshalByRefObject, Test.Inner
        {
                // This is OK: public void Foo ()
                void Test.Inner.Foo ()
                {
                }

		public static void Main ()
		{ }
        }
}
