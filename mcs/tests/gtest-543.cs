using System.Collections.Generic;

public class Blah<T>
{
	public class WrapperWrapper<N>
	{
		public readonly Wrapper<N> Wrapper;

		public WrapperWrapper ()
			: this (Wrapper<N>.Empty)
		{
		}

		protected WrapperWrapper (Wrapper<N> val)
		{
			Wrapper = val;
		}

		public WrapperWrapper<N> NewWrapperWrapper (Wrapper<N> val)
		{
			return new WrapperWrapper<N> (val);
		}
	}
}

public class Wrapper<U>
{
	public static Wrapper<U> Empty = new Wrapper<U> (default (U));
	
	private Wrapper (U u)
	{
	}
}

public class C
{
	public static int Main ()
	{
		var r = new Blah<ulong>.WrapperWrapper<byte>().NewWrapperWrapper (Wrapper<byte>.Empty);
		if (r == null)
			return 1;
		
		return 0;
	}
}
