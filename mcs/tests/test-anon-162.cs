using System;
using System.Collections.Generic;

delegate void D (object param);

class T
{
	void Assert (object a) { }
	void Execute (Action a) { }

	private D GetD<T> (object input)
	{
		return delegate (object param) {
			IList<object> col = null;

			try {
				object v = null;

				Execute (() => {
						v = col[0];
						Assert (input);
					});
			} finally {
			}
		};
	}

	public static void Main ()
	{
		new T ().GetD<long> (null) (9);
	}
}
