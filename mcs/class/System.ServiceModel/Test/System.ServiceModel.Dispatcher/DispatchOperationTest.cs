//
// DispatchOperationTest.cs
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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class DispatchOperationTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestConstructorNullName ()
		{
			new DispatchOperation (CreateRuntime (), null, null);
		}

		[Test]
		public void TestConstructorNullAction ()
		{
			DispatchRuntime r = CreateRuntime ();
			// null Action is allowed.
			new DispatchOperation (r, String.Empty, null);
			// null ReplyAction as well.
			new DispatchOperation (r, String.Empty, null, null);
		}

		[Test]
		public void TestConstructor ()
		{
			DispatchOperation o = new DispatchOperation (
				CreateRuntime (), String.Empty, null, null);
			Assert.IsTrue (o.DeserializeRequest, "#1");
			Assert.IsTrue (o.SerializeReply, "#2");
			Assert.IsNull (o.Formatter, "#3");
			Assert.AreEqual (0, o.FaultContractInfos.Count, "#4");
			Assert.IsNull (o.Invoker, "#5");
			Assert.IsFalse (o.IsOneWay, "#6");
			Assert.IsFalse (o.IsTerminating, "#7");
			Assert.IsFalse (o.ReleaseInstanceBeforeCall, "#8");
			Assert.IsFalse (o.ReleaseInstanceAfterCall, "#9");
			Assert.IsFalse (o.TransactionAutoComplete, "#10");
			Assert.IsFalse (o.TransactionRequired, "#11");
			Assert.AreEqual (0, o.ParameterInspectors.Count, "#12");
		}

		DispatchRuntime CreateRuntime ()
		{
			return new EndpointDispatcher (
				new EndpointAddress ("http://localhost:8080"), "IFoo", "urn:foo").DispatchRuntime;
		}

		[Test]
		public void FaultContractInfos ()
		{
			var host = new ServiceHost (typeof (TestFaultContract));
			host.Description.Behaviors.Find<ServiceDebugBehavior> ().IncludeExceptionDetailInFaults = false;
			host.AddServiceEndpoint (typeof (ITestFaultContract), new BasicHttpBinding (), new Uri ("http://localhost:37564"));
			host.Open ();
			try {
				var cf = new ChannelFactory<ITestFaultContract> (new BasicHttpBinding (), new EndpointAddress ("http://localhost:37564"));
				var cli = cf.CreateChannel ();
				try {
					cli.Run ("default");
					Assert.Fail ("#1");
				} catch (FaultException<PrivateAffairError> ex) {
					var p  = ex.Detail;
					Assert.AreEqual (5, p.ErrorCode, "#2");
					Assert.AreEqual ("foobarerror", p.Text, "#3");
				}

				try {
					cli.Run ("deriveddata");
					Assert.Fail ("#4");
				} catch (Exception ex) {
					// The type must be explicitly listed in the [FaultContract],
					// it is not allowed to use a subclass of the exception data type.
					Assert.AreEqual (typeof (FaultException), ex.GetType (), "#5");
				}

				try {
					cli.Run ("derivedexception");
					Assert.Fail ("#6");
				} catch (Exception ex) {
					// However, it is allowed to derive from FaultException<T>, provided
					// that T is explicitly listed in [FaultContract].  Bug #7177.
					Assert.AreEqual (typeof (FaultException<PrivateAffairError>), ex.GetType (), "#7");
				}
			} finally {
				host.Close ();
			}
		}

		[ServiceContract]
		public interface ITestFaultContract
		{
			[OperationContract]
			[FaultContract (typeof (PrivateAffairError), Action = "urn:myfault")]
			string Run (string input);
		}

		class TestFaultContract : ITestFaultContract
		{
			public string Run (string input)
			{
				Assert.AreEqual (1, ContractDescription.GetContract (typeof (TestFaultContract)).Operations [0].Faults.Count, "s#0");

				var dr = OperationContext.Current.EndpointDispatcher.DispatchRuntime;
				Assert.AreEqual (1, dr.Operations.Count);
				var dop = dr.Operations [0];
				Assert.AreEqual ("Run", dop.Name, "s#1");
				Assert.AreEqual (1, dop.FaultContractInfos.Count, "s#2");
				var fci = dop.FaultContractInfos [0];
				Assert.AreEqual (typeof (PrivateAffairError), fci.Detail, "s#3");
				if (input.Equals ("default"))
					throw new FaultException<PrivateAffairError> (new PrivateAffairError () { ErrorCode = 5, Text = "foobarerror" });
				else if (input.Equals ("deriveddata"))
					throw new FaultException<DerivedPrivateAffairError> (new DerivedPrivateAffairError () { ErrorCode = 5, Text = "foobarerror" });
				else if (input.Equals ("derivedexception"))
					throw new DerivedFaultException (new PrivateAffairError () { ErrorCode = 5, Text = "foobarerror" });
				else
					throw new FaultException ("Invalid operation");
			}
		}

		[DataContract]
		class PrivateAffairError
		{
			[DataMember]
			public int ErrorCode { get; set; }
			[DataMember]
			public string Text { get; set; }
		}

		[DataContract]
		class DerivedPrivateAffairError : PrivateAffairError
		{
		}

		class DerivedFaultException : FaultException<PrivateAffairError>
		{
			public DerivedFaultException (PrivateAffairError error)
				: base (error)
			{ }
		}
	}
}