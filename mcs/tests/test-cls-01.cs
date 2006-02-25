using System;

[assembly: CLSCompliant (true)]

public class CLSClass
{
        public byte XX {
            get { return 5; }
        }

        public static void Main() {}
}

[CLSCompliant (false)]
public partial class C1
{
}

public partial class C1
{
	public void method (uint u)
	{
	}
}

