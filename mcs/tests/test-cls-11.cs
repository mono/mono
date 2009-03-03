// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (true)]
public abstract class CLSClass {
        [CLSCompliant (true)]
        public abstract void Test (IComparable arg);
}

public abstract class CLSCLass_2 {
        public abstract void Test ();
}

public abstract class CLSClass_3 {
        internal abstract void Test ();
}

[CLSCompliant(true)]
public interface ICallable
{
	object Call(params object[] args);
	object Target
	{
		get;
	}
}

public class MainClass {
        public static void Main () {
        }
}
