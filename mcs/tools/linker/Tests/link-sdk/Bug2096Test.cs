using System;
#if XAMCORE_2_0
using Foundation;
using ObjCRuntime;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif
#endif	
using NUnit.Framework;

namespace LinkSdk.Aot {
	
	[TestFixture]
	// we want the test to be availble if we use the linker
	[Preserve (AllMembers = true)]
	public partial class AotBugsTest {
		
		public struct VT {
			public Action a;
		}
		
		public class D {
		}
		
		public class A {
			public void OuterMethod<TArg1> (TArg1 value)
			{
				this.InnerMethod<TArg1, long> (value, 0);
			}
			
			private void InnerMethod<TArg1, TArg2> (TArg1 v1, TArg2 v2)
			{
				Console.WriteLine ("{0} {1}", v1, v2);
			}
		}
		
		[Test]
		public void Bug2096_Aot ()
		{
			var a = new A ();

			a.OuterMethod<int> (1);
			a.OuterMethod<DateTime> (DateTime.Now);
			// works with 5.0.2 (5.1)
			
			var v = new VT ();
			a.OuterMethod<VT> (v);
			// works with 5.0.2 (5.1)
			
			var x = new D ();
			a.OuterMethod<D> (x);
			// fails with 5.0.2 (5.1) when running on devices with
			// Attempting to JIT compile method 'A:InnerMethod<D, long> (D,long)' while running with --aot-only.
#if MONOTOUCH
			if (Runtime.Arch == Arch.SIMULATOR)
				Assert.Inconclusive ("only fails on devices");
#endif
		}
	}
}