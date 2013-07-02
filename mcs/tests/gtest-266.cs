class Test<T>
{
        int priv;
        private sealed class Inner<U>
        {
                Test<U> test;
                void Foo ()
                {
                        test.priv = 0;
                }
        }
}

class Test { public static void Main () { } }
