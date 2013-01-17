using System;

public delegate void Handler <T> (T t);

public class T {
        public void Foo <T> (Handler <T> handler) {
                AsyncCallback d = delegate (IAsyncResult ar) {
                        Response <T> (handler);
                };
        }

        void Response <T> (Handler <T> handler) {}

	public static void Main ()
	{ }
} 

