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

class Test { static void Main () { } }
