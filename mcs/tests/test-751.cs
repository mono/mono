using SomeOtherNS;
using LocalNS;
using OneMoreNS;

namespace SomeOtherNS.Compiler
{
}

namespace OneMoreNS.Compiler
{
}

namespace LocalNS
{
	public class Compiler
	{
	}
}

namespace System.Local
{
	class M
	{
		public static void Main ()
		{
			Compiler c = new LocalNS.Compiler ();
		}
	}
}