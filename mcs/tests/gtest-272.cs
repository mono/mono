using System;

public delegate void Handler<T> (T t);

public static class X
{
        public static void Foo<T> (Handler<T> handler) {
                AsyncCallback d = delegate (IAsyncResult ar) {
                        Response<T> (handler);
                };
        }

        static void Response<T> (Handler<T> handler)
	{ }

	static void Test<T> (T t)
	{ }

	public static void Main ()
	{
		Foo<long> (Test);
	}
} 
