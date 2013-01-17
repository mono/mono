// CS0656: The compiler required member `System.Threading.Interlocked.CompareExchange(ref T, T, T)' could not be found or is inaccessible
// Line: 20
// Compiler options: -nostdlib CS0656-corlib.cs

namespace System {
    public partial class Delegate {
	public static Delegate Combine(Delegate a, Delegate b) { return null; }
	public static void Remove(Delegate a, Delegate b) { return; }
    }
}

namespace System.Threading {
    class Interlocked {}
}

delegate void D();

class Test
{
    event D ev;
}
