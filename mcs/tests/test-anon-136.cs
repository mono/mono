using System;

delegate void Handler<T> (object sender);

interface IBar<T> {
	event Handler<T> Handler;
}

class Foo<T> {

	IBar<T> proxy, real;

	event Handler<T> handler;

	Handler<T> proxyHandler;

	public event Handler<T> Handler {
		add {
			if (handler == null) {
				if (proxyHandler == null)
					proxyHandler = (object s) => handler (proxy);
			}
			handler += value;
		}
		remove {
			handler -= value;
		}
	}
}

class Program {

	public static int Main ()
	{
		var x = new Foo<int> ();
		x.Handler += null;
		return 0;
	}
}
