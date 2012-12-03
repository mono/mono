using System;

namespace FirstOuter
{
	namespace FirstInner
	{
		public class First
		{
			public string MyIdentity { 
				get {
					return this.GetType().FullName;
				}		
			}
		}
	}
	
	public class Second : FirstInner.First {}
	
	namespace SecondInner
	{
		public class Third : FirstOuter.FirstInner.First {}
	}
	
	namespace FirstInner // purposefully again
	{
		public class Fourth : First {} // must understand First in the nom qualified form
	}
}

public class Fifth : FirstOuter.FirstInner.First {}

class Application
{
	public static int Main(string[] args)
	{
		FirstOuter.FirstInner.First V1 = new FirstOuter.FirstInner.First();
		FirstOuter.Second V2 = new FirstOuter.Second();
		FirstOuter.SecondInner.Third V3 = new FirstOuter.SecondInner.Third();
		FirstOuter.FirstInner.Fourth V4 = new FirstOuter.FirstInner.Fourth();
		Fifth V5 = new Fifth();
	
		Console.WriteLine("V1 is " + V1.MyIdentity);
		Console.WriteLine("V2 is " + V2.MyIdentity);
		Console.WriteLine("V3 is " + V3.MyIdentity);
		Console.WriteLine("V4 is " + V4.MyIdentity);
		Console.WriteLine("V5 is " + V5.MyIdentity);
		
		return 0;
	}
}

