using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class CrashyCrashy {
    [DllImport ("libtest")]
    public static extern void libtest_kill_foreign_thread_crash ();
     
    public static void Main () {
	Console.WriteLine ("in managed");
	Go ();
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    public static void Go () {
	libtest_kill_foreign_thread_crash ();
    }
  
}
