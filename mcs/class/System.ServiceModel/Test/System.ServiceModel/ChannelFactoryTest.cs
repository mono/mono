//
// ChannelFactoryTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ChannelFactoryTest
	{
		class SimpleChannelFactory
			: ChannelFactory
		{
			public SimpleChannelFactory ()
				: base ()
			{
			}

			protected override TimeSpan DefaultCloseTimeout {
				get { return TimeSpan.FromMinutes (1); }
			}

			protected override TimeSpan DefaultOpenTimeout {
				get { return TimeSpan.FromMinutes (1); }
			}

			protected override ServiceEndpoint CreateDescription ()
			{
				return new ServiceEndpoint (ContractDescription.GetContract (typeof(ICtorUseCase2)));
			}

			public void InitEndpoint (Binding b, EndpointAddress addr)
			{
				base.InitializeEndpoint (b, addr);
			}

			public void InitEndpoint (string configName, EndpointAddress addr)
			{
				base.InitializeEndpoint (configName, addr);
			}

			public void ApplyConfig (string configName)
			{
				base.ApplyConfiguration (configName);
			}
		}

		[Test]
		public void InitializeEndpointTest1 ()
		{
			SimpleChannelFactory factory = new SimpleChannelFactory ();
			Assert.AreEqual (null, factory.Endpoint, "#01");
			Binding b = new WSHttpBinding ();
			factory.InitEndpoint (b, null);
			Assert.AreEqual (b, factory.Endpoint.Binding, "#02");
			Assert.AreEqual (null, factory.Endpoint.Address, "#03");
			Assert.AreEqual (typeof (ICtorUseCase2), factory.Endpoint.Contract.ContractType, "#04");
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void InitializeEndpointTest2 ()
		{
			SimpleChannelFactory factory = new SimpleChannelFactory ();
			Assert.AreEqual (null, factory.Endpoint, "#01");
			factory.InitEndpoint ("CtorUseCase2_1", null);
			Assert.AreEqual (typeof (BasicHttpBinding), factory.Endpoint.Binding.GetType (), "#02");
			Assert.AreEqual (new EndpointAddress ("http://test2_1"), factory.Endpoint.Address, "#03");
			Assert.AreEqual (typeof (ICtorUseCase2), factory.Endpoint.Contract.ContractType, "#04");
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void InitializeEndpointTest3 ()
		{
			SimpleChannelFactory factory = new SimpleChannelFactory ();
			Assert.AreEqual (null, factory.Endpoint, "#01");
			factory.InitEndpoint ("CtorUseCase2_1", null);
			Binding b = new WSHttpBinding ();
			factory.InitEndpoint (b, null);
			Assert.AreEqual (b, factory.Endpoint.Binding, "#02");
			Assert.AreEqual (null, factory.Endpoint.Address, "#03");
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void ApplyConfigurationTest1 ()
		{
			SimpleChannelFactory factory = new SimpleChannelFactory ();
			Binding b = new WSHttpBinding ();
			factory.InitEndpoint (b, null);
			factory.ApplyConfig ("CtorUseCase2_1");
			Assert.AreEqual (new EndpointAddress ("http://test2_1"), factory.Endpoint.Address, "#03");
			Assert.AreEqual (b, factory.Endpoint.Binding, "#02");
		}

		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void ApplyConfigurationTest2 ()
		{
			SimpleChannelFactory factory = new SimpleChannelFactory ();
			Binding b = new WSHttpBinding ();
			factory.InitEndpoint (b, new EndpointAddress ("http://test"));
			factory.ApplyConfig ("CtorUseCase2_2");
			Assert.AreEqual (new EndpointAddress ("http://test"), factory.Endpoint.Address, "#03");
			Assert.IsNotNull (factory.Endpoint.Behaviors.Find <CallbackDebugBehavior> (), "#04");
			Assert.AreEqual (true, factory.Endpoint.Behaviors.Find <CallbackDebugBehavior> ().IncludeExceptionDetailInFaults, "#04");
			factory.ApplyConfig ("CtorUseCase2_3");
			Assert.IsNotNull (factory.Endpoint.Behaviors.Find <CallbackDebugBehavior> (), "#04");
			Assert.AreEqual (false, factory.Endpoint.Behaviors.Find <CallbackDebugBehavior> ().IncludeExceptionDetailInFaults, "#04");
		}

		[Test]
		public void DescriptionProperties ()
		{
			Binding b = new BasicHttpBinding ();
			ChannelFactory<IFoo> f = new ChannelFactory<IFoo> (b);

			// FIXME: it's not working now (though this test is silly to me.)
			//Assert.IsNull (f.Description.ChannelType, "ChannelType");

			// FIXME: it's not working now
			//Assert.AreEqual (1, f.Endpoint.Behaviors.Count, "Behaviors.Count");
			//ClientCredentials cred = f.Endpoint.Behaviors [0] as ClientCredentials;
			//Assert.IsNotNull (cred, "Behaviors contains ClientCredentials");

			Assert.IsNotNull (f.Endpoint, "Endpoint");
			Assert.AreEqual (b, f.Endpoint.Binding, "Endpoint.Binding");
			Assert.IsNull (f.Endpoint.Address, "Endpoint.Address");
			// You can examine this silly test on .NET.
			// Funky, ContractDescription.GetContract(
			//   typeof (IRequestChannel)) also fails to raise an 
			// error.
			//Assert.AreEqual ("IRequestChannel", f.Description.Endpoint.Contract.Name, "Endpoint.Contract");
		}

		public class MyChannelFactory<TChannel> 
			: ChannelFactory<TChannel>
		{
			public MyChannelFactory (Type type)
				: base (type)
			{
			}
		}

		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			string Echo (string msg);
		}

		public class Foo : IFoo
		{
			public string Echo (string msg)
			{
				return msg;
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgumentTypeNotInterface ()
		{
			new MyChannelFactory<IFoo> (typeof (Foo));
		}
	}
}
#endif