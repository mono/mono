public class outer {
	public class inner {
	}
}

public class gen <T> {
	public static void foo ()
	{
	}
}

namespace ns {
	public class gen_m <T> {
		public static void foo ()
		{
		}
		public static void foo <A> (A _a)
		{
		}
		public static void foo <A, B> (A _a)
		{
		}
	}
}

public interface If1 <T> {
}

public interface If2 <T> {
}

public interface If3 {
}
	
public struct gen_struct <T> {
	public static void foo ()
	{
	}
}
