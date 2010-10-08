//
// ContractDescriptionTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Net.Security;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class ContractDescriptionTest
	{
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetNonContract ()
		{
			ContractDescription cd = ContractDescription.GetContract (
				typeof (object));
		}

		[Test]
		public void GetContract ()
		{
			InternalTestGetContract (
				ContractDescription.GetContract (typeof (IFoo)));
		}

		[Test]
		public void GetContractParamRenamed ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (IFooMsgParams));

			Assert.AreEqual (1, cd.Operations.Count, "Operation count");

			// Operation #1
			OperationDescription od = cd.Operations [0];

			ServiceAssert.AssertOperationDescription (
				"MyFoo", null, null, 
				typeof (IFooMsgParams).GetMethod ("Foo"),
				true, false, false,
				od, "MyFoo");

			// Operation #1 -> Message #1
			MessageDescription md = od.Messages [0];

			ServiceAssert.AssertMessageAndBodyDescription (
				"http://tempuri.org/IFooMsgParams/MyFoo",
				MessageDirection.Input,
				null, "MyFoo", "http://tempuri.org/", false,
				md, "MyFoo");

			ServiceAssert.AssertMessagePartDescription (
				"MyParam", "http://tempuri.org/", 0, false,
				ProtectionLevel.None, typeof (string), md.Body.Parts [0], "MyFoo.msg");

			md = od.Messages [1];

			ServiceAssert.AssertMessageAndBodyDescription (
				"http://tempuri.org/IFooMsgParams/MyFooResponse",
				MessageDirection.Output,
				null, "MyFooResponse",
				"http://tempuri.org/", true,
				md, "MyFoo");

			ServiceAssert.AssertMessagePartDescription (
				"MyResult", "http://tempuri.org/", 0, false,
				ProtectionLevel.None, typeof (string), md.Body.ReturnValue, "MyResult ReturnValue");
		}

		[Test]
		public void GetContractConfigName ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (ICtorUseCase2));
			Assert.AreEqual("CtorUseCase2", cd.ConfigurationName);
			Assert.AreEqual("ICtorUseCase2", cd.Name);
			cd = ContractDescription.GetContract (typeof (ICtorUseCase1));
			Assert.AreEqual("MonoTests.System.ServiceModel.ICtorUseCase1", cd.ConfigurationName);
			Assert.AreEqual("ICtorUseCase1", cd.Name);
		}

		[Test]
		public void GetContract2 ()
		{
			InternalTestGetContract (
				ContractDescription.GetContract (typeof (Foo)));
		}

		public void InternalTestGetContract (ContractDescription cd)
		{
			ServiceAssert.AssertContractDescription (
				"IFoo", "http://tempuri.org/", SessionMode.Allowed, typeof (IFoo), null,
				cd, "contract");

			Assert.AreEqual (2, cd.Operations.Count, "Operation count");

			// Operation #1
			OperationDescription od = cd.Operations [0];

			ServiceAssert.AssertOperationDescription (
				"HeyDude", null, null, 
				typeof (IFoo).GetMethod ("HeyDude"),
				true, false, false,
				od, "HeyDude");

			// Operation #1 -> Message #1
			MessageDescription md = od.Messages [0];

			ServiceAssert.AssertMessageAndBodyDescription (
				"http://tempuri.org/IFoo/HeyDude",
				MessageDirection.Input,
				null, "HeyDude", "http://tempuri.org/", false,
				md, "HeyDude");

			ServiceAssert.AssertMessagePartDescription (
				"msg", "http://tempuri.org/", 0, false,
				ProtectionLevel.None, typeof (string), md.Body.Parts [0], "HeyDude.msg");
			ServiceAssert.AssertMessagePartDescription (
				"msg2", "http://tempuri.org/", 1, false,
				ProtectionLevel.None, typeof (string), md.Body.Parts [1], "HeyDude.msg");

			// Operation #1 -> Message #2
			md = od.Messages [1];

			ServiceAssert.AssertMessageAndBodyDescription (
				"http://tempuri.org/IFoo/HeyDudeResponse",
				MessageDirection.Output,
				null, "HeyDudeResponse",
				"http://tempuri.org/", true,
				md, "HeyDude");

			ServiceAssert.AssertMessagePartDescription (
				"HeyDudeResult", "http://tempuri.org/", 0, false,
				ProtectionLevel.None, typeof (string), md.Body.ReturnValue, "HeyDudeResponse ReturnValue");

			// Operation #2
			od = cd.Operations [1];

			ServiceAssert.AssertOperationDescription (
				"HeyHey", null, null,
				typeof (IFoo).GetMethod ("HeyHey"),
				true, false, false,
				od, "HeyHey");

			// Operation #2 -> Message #1
			md = od.Messages [0];

			ServiceAssert.AssertMessageAndBodyDescription (
				"http://tempuri.org/IFoo/HeyHey",
				MessageDirection.Input,
				null, "HeyHey", "http://tempuri.org/", false,
				md, "HeyHey");

			ServiceAssert.AssertMessagePartDescription (
				"ref1", "http://tempuri.org/", 0, false,
				ProtectionLevel.None, typeof (string), md.Body.Parts [0], "HeyHey.ref1");

			// Operation #2 -> Message #2
			md = od.Messages [1];

			ServiceAssert.AssertMessageAndBodyDescription (
				"http://tempuri.org/IFoo/HeyHeyResponse",
				MessageDirection.Output,
				null, "HeyHeyResponse",
				"http://tempuri.org/", true,
				md, "HeyHey");

			ServiceAssert.AssertMessagePartDescription (
				"HeyHeyResult", "http://tempuri.org/", 0, false,
				ProtectionLevel.None, typeof (void), md.Body.ReturnValue, "HeyHeyResponse ReturnValue");

			ServiceAssert.AssertMessagePartDescription (
				"out1", "http://tempuri.org/", 0, false,
				ProtectionLevel.None, typeof (string), md.Body.Parts [0], "HeyHey.out1");
			ServiceAssert.AssertMessagePartDescription (
				"ref1", "http://tempuri.org/", 1, false,
				ProtectionLevel.None, typeof (string), md.Body.Parts [1], "HeyHey.ref1");
		}

		[Test]
		public void GetContractInherit ()
		{
			ContractDescription.GetContract (typeof (Foo));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetMultipleServiceContract ()
		{
			ContractDescription.GetContract (typeof (FooBar));
		}

		[Test]
		// [ExpectedException (typeof (InvalidOperationException))]
		public void GetContractNoOperation ()
		{
			ContractDescription.GetContract (typeof (INoOperation));
		}

		[Test]
		[Category ("NotWorking")]
		public void GetContractMessageParameter ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (IMessageParameter));

			ServiceAssert.AssertContractDescription (
				"IMessageParameter", "http://tempuri.org/", 
				SessionMode.Allowed, typeof (IMessageParameter), null,
				cd, "contract");

			OperationDescription od = cd.Operations [0];

			ServiceAssert.AssertOperationDescription (
				"ReturnMessage", null, null, 
				typeof (IMessageParameter).GetMethod ("ReturnMessage"),
				true, false, false,
				od, "operation");

			MessageDescription md = od.Messages [0];

			ServiceAssert.AssertMessageAndBodyDescription (
				"http://tempuri.org/IMessageParameter/ReturnMessage",
				MessageDirection.Input,
				// Body.WrapperName is null
				null, null, null, false,
				md, "ReturnMessage");

			ServiceAssert.AssertMessagePartDescription (
				"arg", "http://tempuri.org/", 0, false,
				ProtectionLevel.None, typeof (Message), md.Body.Parts [0], "ReturnMessage input");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetContractInvalidAsync ()
		{
			ContractDescription.GetContract (typeof (IInvalidAsync));
		}

		[Test]
		// IMetadataExchange contains async patterns.
		public void GetContractIMetadataExchange ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (IMetadataExchange));
			OperationDescription od = cd.Operations [0];
			Assert.AreEqual (2, od.Messages.Count, "premise: message count");
			foreach (MessageDescription md in od.Messages) {
				if (md.Direction == MessageDirection.Input) {
					Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2004/09/transfer/Get", md.Action, "#1-1");
					Assert.AreEqual (1, md.Body.Parts.Count, "#1-2");
					Assert.IsNull (md.Body.ReturnValue, "#1-3");
					Assert.AreEqual (typeof (Message), md.Body.Parts [0].Type, "#1-4");
				} else {
					Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse", md.Action, "#2-1");
					Assert.AreEqual (0, md.Body.Parts.Count, "#2-2");
					Assert.IsNotNull (md.Body.ReturnValue, "#2-3");
					Assert.AreEqual (typeof (Message), md.Body.ReturnValue.Type, "#2-4");
				}
			}
		}

		[Test]
		// enable it if we want to become a compatibility kid. It has
		// no ServiceContract, thus it should not be accepted. But
		// there is an abuse of ChannelFactory<IRequestChannel> in
		// MSDN documentations and probably examples.
		[Category ("NotWorking")]
		public void GetContractIRequestChannel ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (IRequestChannel));
			Assert.AreEqual (typeof (IRequestChannel), cd.ContractType, "#_1");
			Assert.AreEqual ("IRequestChannel", cd.Name, "#_2");
			Assert.AreEqual ("http://schemas.microsoft.com/2005/07/ServiceModel", cd.Namespace, "#_3");
			Assert.AreEqual (false, cd.HasProtectionLevel, "#_4");
			Assert.AreEqual (SessionMode.NotAllowed, cd.SessionMode, "#_5");
			Assert.AreEqual (0, cd.Behaviors.Count, "#_6");
			Assert.AreEqual (1, cd.Operations.Count, "#_7");
			OperationDescription od = cd.Operations [0];
			Assert.IsNull (od.SyncMethod, "#_8");
			Assert.IsNull (od.BeginMethod, "#_9");
			Assert.IsNull (od.EndMethod, "#_10");
			Assert.AreEqual (false, od.IsOneWay, "#_11");
			Assert.AreEqual (false, od.HasProtectionLevel, "#_12");
			Assert.AreEqual ("Request", od.Name, "#_13");
			Assert.AreEqual (true, od.IsInitiating, "#_14");
			Assert.AreEqual (0, od.Behaviors.Count, "#_15");
			Assert.AreEqual (2, od.Messages.Count, "#_16");
			foreach (MessageDescription md in od.Messages) {
				if (md.Direction == MessageDirection.Output) {
					Assert.AreEqual ("*", md.Action, "#_17");
					Assert.AreEqual (false, md.HasProtectionLevel, "#_18");
					Assert.AreEqual (0, md.Headers.Count, "#_19");
					Assert.AreEqual (0, md.Properties.Count, "#_20");
					Assert.IsNull (md.MessageType, "#_21");
					MessageBodyDescription mb = md.Body;
					Assert.AreEqual (null, mb.WrapperName, "#_22");
					Assert.AreEqual (null, mb.WrapperNamespace, "#_23");
					Assert.IsNull (mb.ReturnValue, "#_24");
					Assert.AreEqual (0, mb.Parts.Count, "#_25");
				} else {
					Assert.AreEqual ("*", md.Action, "#_17_");
					Assert.AreEqual (false, md.HasProtectionLevel, "#_18_");
					Assert.AreEqual (0, md.Headers.Count, "#_19_");
					Assert.AreEqual (0, md.Properties.Count, "#_20_");
					Assert.IsNull (md.MessageType, "#_21_");
					MessageBodyDescription mb = md.Body;
					Assert.AreEqual (null, mb.WrapperName, "#_22_");
					Assert.AreEqual (null, mb.WrapperNamespace, "#_23_");
					Assert.IsNull (mb.ReturnValue, "#_24_");
					Assert.AreEqual (0, mb.Parts.Count, "#_25_");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WrongAsyncEndContract ()
		{
			ContractDescription.GetContract (typeof (IWrongAsyncEndContract));
		}

		[Test]
		public void AsyncContract1 ()
		{
			ContractDescription cd =
				ContractDescription.GetContract (typeof (IAsyncContract1));
			Assert.AreEqual (1, cd.Operations.Count);
			OperationDescription od = cd.Operations [0];
			Assert.AreEqual ("Sum", od.Name, "#1");
			Assert.IsNotNull (od.BeginMethod, "#2");
			Assert.IsNotNull (od.EndMethod, "#3");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DuplicateOperationNames ()
		{
			ContractDescription.GetContract (typeof (IDuplicateOperationNames));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AsyncMethodNameDoesNotStartWithBegin ()
		{
			ContractDescription.GetContract (typeof (IAsyncMethodNameDoesNotStartWithBegin));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AsyncNameDoesNotStartWithBeginButExplicitName ()
		{
			// it is still invalid ...
			ContractDescription.GetContract (typeof (IAsyncNameDoesNotStartWithBeginButExplicitName));
		}

		[Test]
		public void MessageBodyMemberIsNotInferred ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (MessageBodyMemberIsNotInferredService));
			OperationDescription od = cd.Operations [0];
			MessageDescription md = od.Messages [0];
			Assert.AreEqual (0, md.Body.Parts.Count);
		}

		[Test]
		public void TestContractFromObject () {
			ContractDescription cd = ContractDescription.GetContract (typeof (Foo));
			ServiceAssert.AssertContractDescription (typeof (IFoo).Name, "http://tempuri.org/", SessionMode.Allowed, typeof (IFoo), null, cd, "#1");
			Assert.AreEqual (cd.Operations.Count, 2);
			OperationBehaviorAttribute op = cd.Operations.Find ("HeyHey").Behaviors.Find<OperationBehaviorAttribute> ();
			Assert.IsNotNull (op);
			Assert.AreEqual (
				op.ReleaseInstanceMode,
				ReleaseInstanceMode.None, "#2");

			cd = ContractDescription.GetContract (typeof (IFoo), typeof (Foo));
			ServiceAssert.AssertContractDescription (typeof (IFoo).Name, "http://tempuri.org/", SessionMode.Allowed, typeof (IFoo), null, cd, "#3");
			Assert.AreEqual (cd.Operations.Count, 2, "#4");
			Assert.AreEqual (
				cd.Operations.Find ("HeyHey").Behaviors.Find<OperationBehaviorAttribute> ().ReleaseInstanceMode,
				ReleaseInstanceMode.AfterCall, "#5");
		}

		[Test]
		public void GetDerivedContract ()
		{
			var cd = ContractDescription.GetContract (typeof (IFoo3));
			Assert.AreEqual (typeof (IFoo3), cd.ContractType, "#1");
			Assert.AreEqual (3, cd.Operations.Count, "#2");
			cd = ContractDescription.GetContract (typeof (Foo3));
			Assert.AreEqual (typeof (IFoo3), cd.ContractType, "#3");
			Assert.AreEqual (3, cd.Operations.Count, "#4");
		}
		
		[Test]
		public static void MultipleContractsInTypeHierarchy ()
		{
			ContractDescription.GetContract (typeof (DuplicateCheckClassWrapper.ServiceInterface));

			var host = new ServiceHost (typeof (DuplicateCheckClassWrapper.DummyService)); // fine in MS, fails in Mono with "A contract cannot have two operations that have the identical names and different set of parameters"
		}

		// It is for testing attribute search in interfaces.
		public class Foo : IFoo
		{
			public string HeyDude (string msg, string msg2)
			{
				return null;
			}

			[OperationBehavior (ReleaseInstanceMode = ReleaseInstanceMode.AfterCall)]
			public void HeyHey (out string out1, ref string ref1)
			{
				out1 = null;
			}
		}

		// It inherits both IFoo and IBar, thus cannot be a contract.
		public class FooBar : IFoo, IBar
		{
			public string HeyDude (string msg, string msg2)
			{
				return null;
			}
			
			public void HeyHey (out string out1, ref string ref1)
			{
				out1 = null;
			}

			public void OpenBar () {}
		}

		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			string HeyDude (string msg, string msg2);

			[OperationContract]
			void HeyHey (out string out1, ref string ref1);
		}

		[ServiceContract]
		public interface IFoo2
		{
			// FIXME: it does not pass yet
			[OperationContract]
			OregoMessage Nanoda (OregoMessage msg);

			// FIXME: it does not pass yet
			[OperationContract]
			OregoMessage Nanoda2 (OregoMessage msg1, OregoMessage msg2);

			// FIXME: it does not pass yet
			[OperationContract]
			Mona NewMona (Mona source);
		}

		[ServiceContract]
		public interface IFoo3 : IFoo
		{
			[OperationContract]
			string HeyMan (string msg, string msg2);
		}

		public class Foo3 : Foo, IFoo3
		{
			public string HeyMan (string msg, string msg2)
			{
				return msg + msg2;
			}
		}

		[ServiceContract]
		public interface IBar
		{
			[OperationContract]
			void OpenBar ();
		}

		[MessageContract]
		public class OregoMessage
		{
			[MessageBodyMember]
			public string Neutral;
			[MessageBodyMember]
			public Assembly Huh;
			[MessageBodyMember] // it should be ignored ...
			public string Setter { set { } }
			public string NonMember;
		}

		public class Mona
		{
			public string OmaeMona;
			public string OreMona;
		}

		[ServiceContract]
		public interface INoOperation
		{
		}

		[ServiceContract]
		public interface IMessageParameter
		{
			[OperationContract]
			Message ReturnMessage (Message arg);
		}

		[ServiceContract]
		public interface IInvalidAsync
		{
			[OperationContract]
			Message ReturnMessage (Message arg);

			[OperationContract (AsyncPattern = true)]
			IAsyncResult BeginReturnMessage (Message arg, AsyncCallback callback, object state);

			// and no EndReturnMessage().
		}

		[ServiceContract]
		public interface IWrongAsyncEndContract
		{
			[OperationContract]
			int Sum (int a, int b);

			[OperationContract (AsyncPattern = true)]
			IAsyncResult BeginSum (int a, int b, AsyncCallback cb, object state);

			// this OperationContractAttribute is not allowed.
			[OperationContract (AsyncPattern = true)]
			int EndSum (IAsyncResult result);
		}

		[ServiceContract]
		public interface IAsyncContract1
		{
			[OperationContract]
			int Sum (int a, int b);

			[OperationContract (AsyncPattern = true)]
			IAsyncResult BeginSum (int a, int b, AsyncCallback cb, object state);

			int EndSum (IAsyncResult result);
		}

		[ServiceContract]
		public interface IAsyncMethodNameDoesNotStartWithBegin
		{
			[OperationContract]
			int Sum (int a, int b);

			[OperationContract (AsyncPattern = true)]
			IAsyncResult StartSum (int a, int b, AsyncCallback cb, object state);

			int EndSum (IAsyncResult result);
		}

		[ServiceContract]
		public interface IAsyncNameDoesNotStartWithBeginButExplicitName
		{
			[OperationContract]
			int Sum (int a, int b);

			[OperationContract (Name = "Sum", AsyncPattern = true)]
			IAsyncResult StartSum (int a, int b, AsyncCallback cb, object state);

			int EndSum (IAsyncResult result);
		}

		[ServiceContract]
		public interface IDuplicateOperationNames
		{
			[OperationContract]
			string Echo (string s);

			[OperationContract]
			string Echo (string s1, string s2);
		}

		[ServiceContract]
		public interface IFooMsgParams
		{
			[OperationContract (Name = "MyFoo")]
			[return: MessageParameter (Name = "MyResult")]
			string Foo ([MessageParameter (Name = "MyParam")] string param);
		}

		[ServiceContract]
		public class MessageBodyMemberIsNotInferredService
		{
			[OperationContract]
			public void Echo (MessageBodyMemberIsNotInferredContract msg)
			{
			}
		}

		[MessageContract]
		public class MessageBodyMemberIsNotInferredContract
		{
			string foo = "foo";
			public string Foo {
				get { return foo; }
				set { foo = value; }
			}
		}

		public class DuplicateCheckClassWrapper
		{

			[ServiceContract]
			internal interface ServiceInterface : Foo
			{
			}

			[ServiceContract]
			internal interface Foo : Bar
			{
				[OperationContract] void Foo();
			}

			[ServiceContract]
			internal interface Bar
			{
				[OperationContract] void FooBar();
			}

			internal class DummyService : ServiceInterface
			{
				public void FooBar() { }

				public void Foo() { }
			}
		}
	}
}
