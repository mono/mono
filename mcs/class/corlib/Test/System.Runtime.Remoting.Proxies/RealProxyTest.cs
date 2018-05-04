//
// MonoTests.System.Runtime.Remoting.Proxies.RealProxyTest.cs
//
//

using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting.Proxies {
	[TestFixture]
	public class RealProxyTest {

		public class ExampleInterfaceProxy : RealProxy {
			public bool Called;

			public ExampleInterfaceProxy () : base (typeof(IComparable))
			{
				Called = false;
			}

			public override IMessage Invoke (IMessage msg)
			{
				Called = true;
				return new ReturnMessage (typeof(IComparable), null, 0, null, null);
			}
		}

		[Test]
		public void InterfaceProxyGetTypeOkay ()
		{
			// Regression test for #17325
			// Check that GetType () for a proxy of an interface
			// returns the interface.
			var prox = new ExampleInterfaceProxy ();
			var tprox = prox.GetTransparentProxy ();

			Assert.IsNotNull (tprox, "#1");

			var tproxType = tprox.GetType ();

			Assert.IsFalse (prox.Called, "#2"); // this is true on .NET Framework, but false on Mono.

			Assert.IsNotNull (tproxType, "#3");
			Assert.IsTrue (tproxType.IsAssignableFrom (typeof(IComparable)), "#4");
		}

		[Test]
		public void InterfaceProxyGetTypeViaReflectionOkay ()
		{
			// Regression test for #17325
			// Check that GetType () for a proxy of an interface
			// returns the interface.
			//
			// This versions calls GetType using reflection, which
			// avoids the fast path in the JIT.
			var prox = new ExampleInterfaceProxy ();
			var tprox = prox.GetTransparentProxy ();

			Assert.IsNotNull (tprox, "#1");


			var m = typeof(object).GetMethod ("GetType");

			var tproxType = m.Invoke (tprox, null);

			Assert.IsTrue (prox.Called, "#2");

			Assert.IsNotNull (tproxType, "#3");
			Assert.IsTrue (tproxType is Type, "#4");
			Assert.IsTrue ((tproxType as Type).IsAssignableFrom (typeof(IComparable)), "#5");
		}

	}
}
