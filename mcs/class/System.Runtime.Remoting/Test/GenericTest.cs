//
// MonoTests.Remoting.GenericTest.cs
//
// Authors:
//     Robert Jordan  <robertj@gmx.net>
//

#if NET_2_0

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	public interface INested
	{
		int Test ();
		int Test (int i);
		int Test (int a, int b);
		V Test <V> (V v);
		V Test <V, T> (V v, T t);
	}

	public interface ITest
	{
		V TestIface<V> (V v);
		int TestDirectIfaceImpl (int i);
		INested GetNested ();
		INested GetNestedMbr ();
	}

	public class ServerBase<T> : MarshalByRefObject, ITest
	{
		public virtual V TestVirt<V> (V v)
		{
			return default (V);
		}

		public V TestIface<V> (V v)
		{
			return v;
		}

		int ITest.TestDirectIfaceImpl (int i)
		{
			return i;
		}

		public int TestDirectIfaceImpl (int i)
		{
			return -1;
		}

		public INested GetNested ()
		{
			return new Nested ();
		}

		public INested GetNested (string s)
		{
			return new Nested ();
		}

		public INested GetNestedMbr ()
		{
			return new NestedMbr ();
		}
	}

	public class Server<T> : ServerBase<T>
	{
		public override V TestVirt<V> (V v)
		{
			return v;
		}
	}

	[Serializable]
	public class Nested : INested
	{
		public int Test ()
		{
			return 47;
		}

		int INested.Test ()
		{
			return 42;
		}
		
		public int Test (int i)
		{
			return i;
		}

		int INested.Test (int a, int b)
		{
			return a + b;
		}

		public V Test <V> (V v)
		{
			return v;
		}

		V INested.Test <V, T> (V v, T t)
		{
			return default (V);
		}
	}

	public class NestedMbr : MarshalByRefObject, INested
	{
		public int Test ()
		{
			return 47;
		}
		
		int INested.Test ()
		{
			return 42;
		}

		public int Test (int i)
		{
			return i;
		}

		int INested.Test (int a, int b)
		{
			return a + b;
		}

		public V Test <V> (V v)
		{
			return v;
		}

		V INested.Test <V, T> (V v, T t)
		{
			return default (V);
		}
	}


	[TestFixture]
	public class GenericTest
	{
		[Test]
		public void TestCrossAppDomainChannel ()
		{
			RunTests (GetRemObject <Server<object>> ());
		}

		[Test]
		[Ignore ("disabled as it got not working by NUnit upgrade to 2.4.8 (applies to .NET too)")]
		public void TestTcpChannel ()
		{
			RunTests (GetRemObjectTcp <Server<object>> ());
		}

		static T GetRemObject <T> () where T: MarshalByRefObject
		{
			AppDomain d = BaseCallTest.CreateDomain ("Foo");
			return (T) d.CreateInstanceAndUnwrap (
				typeof (T).Assembly.FullName,
				typeof (T).FullName);
		}

		static T GetRemObjectTcp <T> () where T: MarshalByRefObject
		{
			new TcpChannel (18191);
			object obj = Activator.CreateInstance (typeof(T));
			RemotingServices.Marshal ((MarshalByRefObject)obj, "test.rem");
			return (T) RemotingServices.Connect (typeof (T), "tcp://localhost:18191/test.rem");
		}

		static void RunTests (ServerBase<object> rem)
		{
			Assert.AreEqual (42, rem.TestIface<int>(42),
					 "#1 calling TestIface on object instance");

			Assert.AreEqual (42, rem.TestVirt<int>(42),
					 "#2 calling TestVirt");

			ITest i = rem;
			Assert.AreEqual (42, i.TestIface<int>(42),
					 "#3 calling TestIface on interface");

			Assert.AreEqual (42, i.TestDirectIfaceImpl (42),
					 "#4 calling TestDirectIfaceImp");

			INested cao = rem.GetNested ();
			Assert.AreEqual (42, cao.Test (),
					 "#5a calling INested.Test ()");

			Assert.AreEqual (42, cao.Test (42),
					 "#5 calling INested.Test (int)");

			Assert.AreEqual (42, cao.Test (21, 21),
					 "#6 calling INested.Test (int, int)");

			Assert.AreEqual (42, cao.Test<int> (42),
					 "#7 calling INested.Test<V>");

			Assert.AreEqual (0, cao.Test<int, string> (42, "bar"),
					 "#8 calling INested.Test<V, T>");

			cao = rem.GetNestedMbr ();
			Assert.AreEqual (42, cao.Test (),
					 "#9a calling INested.Test ()");

			Assert.AreEqual (42, cao.Test (42),
					 "#9 calling INested.Test (int)");

			Assert.AreEqual (42, cao.Test (21, 21),
					 "#10 calling INested.Test (int, int)");

			Assert.AreEqual (42, cao.Test<int> (42),
					 "#11 calling INested.Test<V>");

			Assert.AreEqual (0, cao.Test<int, string> (42, "bar"),
					 "#12 calling INested.Test<V, T>");
		}
	}
}

#endif
