using System;

interface IA
{
}

class C : IA
{
	void Foo<T> () where T : class, IA
	{
		Func<T, T> m = l => {
			T i = default (T);
			if (l == i) {
				Func<T> m2 = () => i;
				m2 ();
			}
			
			return i;
		};
		
		m (null);
	}
	
	public static int Main ()
	{
		var c = new C ();
		c.Foo<C> ();
		return 0;
	}
}