//
// MonoTests.Remoting.GenericTest.cs
//
// Authors:
//     Robert Jordan  <robertj@gmx.net>
//


using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using NUnit.Framework;

using MonoTests.Helpers;

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
			return i + 500;
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
			return i + 500;
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
		// Under MS.NET, INested.Test<V>(V v) isn't supported over the
		// xappdom channel anymore (as of .NET 3.5). The stacktrace
		// looks like if INested.Test(int) is invoked in place of
		// INested.Test<int>(int).
		[Category("NotDotNet")]
		[Test]
		public void TestCrossAppDomainChannel ()
		{
			RunTests (RegisterAndConnect <Server<object>> ());
		}

		[Test]
		public void TestTcpChannel ()
		{
			var port = NetworkHelpers.FindFreePort ();
			IDictionary props = new Hashtable ();
			props ["name"] = Guid.NewGuid ().ToString("N");
			props ["port"] = port;
			props ["bindTo"] = "127.0.0.1";
			TcpChannel chan = new TcpChannel (props, null, null);
			ChannelServices.RegisterChannel (chan);
			
			try {
				Register <Server<object>> ("gentcptest.rem");
				RunTests (Connect <Server<object>> ($"tcp://localhost:{port}/gentcptest.rem"));
			} finally {
				ChannelServices.UnregisterChannel (chan);
			}
		}

		static T RegisterAndConnect <T> () where T: MarshalByRefObject
		{
			AppDomain d = BaseCallTest.CreateDomain ("GenericTests");
			return (T) d.CreateInstanceAndUnwrap (
				typeof (T).Assembly.FullName,
				typeof (T).FullName);
		}

		static void Register <T> (string uri) where T: MarshalByRefObject
		{
			object obj = Activator.CreateInstance (typeof(T));
			RemotingServices.Marshal ((MarshalByRefObject)obj, uri);
		}

		static T Connect <T> (string uri) where T: MarshalByRefObject
		{
			return (T) RemotingServices.Connect (typeof (T), uri);
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

			Assert.AreEqual (42 + 500, cao.Test (42),
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

			Assert.AreEqual (42 + 500, cao.Test (42),
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

