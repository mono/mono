//
// Fix for bug: 71819, we were producing the wrong
// opcodes when loading parameters in the proxy produced
// by the compiler in class B to implement IB.
//
namespace FLMID.Bugs.BoolOne
{
	public interface IB
	{
		void Add(bool v1, bool v2, uint v3, bool v4);
	}
	
	public class A
	{
		public static bool ok;

		public void Add(bool v1, bool v2, uint v3, bool v4)
		{
			ok = v4;
		}
	}

	public class B : A, IB
	{
	}

	public class Test
	{
		public static int Main(string[] args)
		{
			IB aux = new B();
			
			aux.Add(false, false, 0, true);	
			return A.ok ? 0 : 1;
		}
	}
}
