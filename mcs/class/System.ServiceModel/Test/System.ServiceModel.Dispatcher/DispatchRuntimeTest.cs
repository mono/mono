//
// DispatchRuntimeTest.cs
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
using System.Collections.ObjectModel;
using SMMessage = System.ServiceModel.Channels.Message;
using System.Threading;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class DispatchRuntimeTest
	{
		[Test]
		public void TestConstructor ()
		{
			// This test is rather to just detect implementation 
			// differences than digging in-depth meanings, so feel
			// free to change if MS implementation does not make 
			// sense.
			DispatchRuntime r = new EndpointDispatcher (
				new EndpointAddress ("http://localhost:8080"), "IFoo", "urn:foo").DispatchRuntime;
			Assert.AreEqual (AuditLogLocation.Default,
					 r.SecurityAuditLogLocation, "#1");

			Assert.IsTrue (r.AutomaticInputSessionShutdown, "#2");
			Assert.IsNotNull (r.CallbackClientRuntime, "#3");
			Assert.IsNull (r.ExternalAuthorizationPolicies, "#4");
			Assert.IsFalse (r.IgnoreTransactionMessageProperty, "#5");
			Assert.IsFalse (r.ImpersonateCallerForAllOperations, "#6");
			Assert.AreEqual (0, r.InputSessionShutdownHandlers.Count, "#7");
			Assert.AreEqual (0, r.InstanceContextInitializers.Count, "#8");
			//Assert.AreEqual (0, r.InstanceContextLifetimes.Count, "#9");
			Assert.IsNull (r.InstanceProvider, "#10");
			Assert.IsNull (r.InstanceContextProvider, "#10-2");
			Assert.AreEqual (AuditLevel.None,
					 r.MessageAuthenticationAuditLevel, "#11");
			Assert.AreEqual (0, r.MessageInspectors.Count, "#12");
			Assert.AreEqual (0, r.Operations.Count, "#13");
			Assert.IsNull (r.OperationSelector, "#14");
			// This is silly, but anyways there will be similar 
			// functionality that just represents unix "Groups".
			Assert.AreEqual (PrincipalPermissionMode.UseWindowsGroups,
					 r.PrincipalPermissionMode, "#15");
			Assert.IsFalse (r.ReleaseServiceInstanceOnTransactionComplete, "#16");
			Assert.IsNull (r.RoleProvider, "#17");
			Assert.AreEqual (AuditLevel.None, r.ServiceAuthorizationAuditLevel, "#18");
			Assert.IsNull (r.ServiceAuthorizationManager, "#19");
			Assert.IsTrue (r.SuppressAuditFailure, "#20");
			Assert.IsNull (r.SynchronizationContext, "#21");
			Assert.IsFalse (r.TransactionAutoCompleteOnSessionClose, "#22");
			Assert.IsNull (r.Type, "#23");
			Assert.IsNotNull (r.UnhandledDispatchOperation, "#24");
			DispatchOperation udo = r.UnhandledDispatchOperation;
			Assert.AreEqual ("*", udo.Name, "#24-2");
			Assert.AreEqual ("*", udo.Action, "#24-3");
			Assert.AreEqual ("*", udo.ReplyAction, "#24-4");
			Assert.IsFalse (udo.IsOneWay, "#24-5");
		}

		[Test]
		public void TestInstanceBehavior1 ()
		{
			
			Result res = new Result ();
			MessageInspectBehavior b = new MessageInspectBehavior ();
			b.instanceCtxInitializer = new MyInstanceContextInitializer (res);
			b.instanceCtxProvider = new MyInstanceContextProvider (null, res);
			//b.instanceProvider = new MyInstanceProvider (null, res);
			b.msgInspect = new MyMessageInspector (res);
			string expected = "GetExistingInstanceContext , InitializeInstanceContext , OperationContext , InstanceContext = Opening , Initialize , OperationContext , InstanceContext = Opening , AfterReceiveRequest , OperationContext , InstanceContext = Opened , BeforeSendReply , OperationContext , InstanceContext = Opened , IsIdle , OperationContext , InstanceContext = Opened , NotifyIdle , OperationContext , InstanceContext = Opened , ";
			TestInstanceBehavior (b, expected, res, 1);
			Assert.IsTrue (res.Done, "done");
		}

		[Test]
		public void TestInstanceBehavior2 ()
		{
			Result res = new Result ();
			MessageInspectBehavior b = new MessageInspectBehavior ();
			b.instanceCtxInitializer = new MyInstanceContextInitializer (res);
			b.instanceCtxProvider = new MyInstanceContextProvider (null, res);
			b.instanceProvider = new MyInstanceProvider (new AllActions (res), res);
			b.msgInspect = new MyMessageInspector (res);
			string expected = "GetExistingInstanceContext , InitializeInstanceContext , OperationContext , InstanceContext = Opening , Initialize , OperationContext , InstanceContext = Opening , AfterReceiveRequest , OperationContext , InstanceContext = Opened , GetInstance1 , OperationContext , InstanceContext = Opened , BeforeSendReply , OperationContext , InstanceContext = Opened , ReleaseInstance , OperationContext , InstanceContext = Opened , IsIdle , OperationContext , InstanceContext = Opened , NotifyIdle , OperationContext , InstanceContext = Opened , ";
			TestInstanceBehavior (b, expected, res, 1);
			Assert.IsTrue (res.Done, "done");
		}

		[Test]
		public void TestInstanceBehavior3 ()
		{
			Result res = new Result ();
			MessageInspectBehavior b = new MessageInspectBehavior ();
			b.instanceCtxInitializer = new MyInstanceContextInitializer (res);

			InstanceContext c = new InstanceContext (new AllActions (res));
			
			b.instanceCtxProvider = new MyInstanceContextProvider (c, res);
			b.instanceProvider = new MyInstanceProvider (new AllActions (res), res);
			b.msgInspect = new MyMessageInspector (res);
			string expected = "GetExistingInstanceContext , InitializeInstanceContext , OperationContext , InstanceContext = Opening , Initialize , OperationContext , InstanceContext = Opening , AfterReceiveRequest , OperationContext , InstanceContext = Opened , BeforeSendReply , OperationContext , InstanceContext = Opened , ";
			TestInstanceBehavior (b, expected, res, 1);					Assert.IsTrue (res.Done, "done");
		}

		[Test]
		public void TestInstanceBehavior4 ()
		{
			Result res = new Result ();
			MessageInspectBehavior b = new MessageInspectBehavior ();
			b.instanceCtxInitializer = new MyInstanceContextInitializer (res);	
		
			b.instanceCtxProvider = new MyInstanceContextProvider (null, res);
			b.instanceProvider = new MyInstanceProvider (new AllActions (res), res);
			b.msgInspect = new MyMessageInspector (res);
			string expected = "GetExistingInstanceContext , InitializeInstanceContext , OperationContext , InstanceContext = Opening , Initialize , OperationContext , InstanceContext = Opening , AfterReceiveRequest , OperationContext , InstanceContext = Opened , GetInstance1 , OperationContext , InstanceContext = Opened , BeforeSendReply , OperationContext , InstanceContext = Opened , ReleaseInstance , OperationContext , InstanceContext = Opened , IsIdle , OperationContext , InstanceContext = Opened , NotifyIdle , OperationContext , InstanceContext = Opened , GetExistingInstanceContext , InitializeInstanceContext , OperationContext , InstanceContext = Opening , Initialize , OperationContext , InstanceContext = Opening , AfterReceiveRequest , OperationContext , InstanceContext = Opened , GetInstance1 , OperationContext , InstanceContext = Opened , BeforeSendReply , OperationContext , InstanceContext = Opened , ReleaseInstance , OperationContext , InstanceContext = Opened , IsIdle , OperationContext , InstanceContext = Opened , NotifyIdle , OperationContext , InstanceContext = Opened , ";
			TestInstanceBehavior (b, expected, res, 2);					Assert.IsTrue (res.Done, "done");
		}

		void TestInstanceBehavior (MessageInspectBehavior b, string expected, Result actual, int invocations)
		{
			ServiceHost h = new ServiceHost (typeof (AllActions), new Uri ("http://localhost:8080"));
			try {
				h.AddServiceEndpoint (typeof (IAllActions).FullName, new BasicHttpBinding (), "AllActions");
				h.Description.Behaviors.Add (b);
				ServiceDebugBehavior db = h.Description.Behaviors.Find<ServiceDebugBehavior> ();
				db.IncludeExceptionDetailInFaults = true;
				h.Open ();
				AllActionsProxy p = new AllActionsProxy (new BasicHttpBinding () { SendTimeout = TimeSpan.FromSeconds (5), ReceiveTimeout = TimeSpan.FromSeconds (5) }, new EndpointAddress ("http://localhost:8080/AllActions"));

				for (int i = 0; i < invocations; ++i)
					p.Get (10);
				p.Close ();

				//let ther server finish his work
				Thread.Sleep (100);
				Assert.AreEqual (expected, actual.string_res);
				actual.Done = true;
			}
			finally {				
				h.Close ();
			}
		}
	}


#region helpers

	#region message inspectors

	public class MessageInspectBehavior : IServiceBehavior
	{
		public MyMessageInspector msgInspect;
		public MyInstanceContextProvider instanceCtxProvider;
		public MyInstanceProvider instanceProvider;
		public MyInstanceContextInitializer instanceCtxInitializer;

		public void AddBindingParameters (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) {
			
		}

		public void ApplyDispatchBehavior (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
			ChannelDispatcher d = serviceHostBase.ChannelDispatchers [0] as ChannelDispatcher;
			d.Endpoints [0].DispatchRuntime.MessageInspectors.Add (msgInspect);
			d.Endpoints [0].DispatchRuntime.InstanceContextProvider = instanceCtxProvider;
			d.Endpoints [0].DispatchRuntime.InstanceProvider = instanceProvider;
			d.Endpoints [0].DispatchRuntime.InstanceContextInitializers.Add (instanceCtxInitializer);
		}

		public void Validate (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
			
		}
	}

	public class MyMessageInspector : IDispatchMessageInspector
	{
		Result res;

		public MyMessageInspector (Result result) {
			res = result;
		}
		#region IDispatchMessageInspector Members

		public object AfterReceiveRequest (ref Message request, IClientChannel channel, InstanceContext instanceContext) {
			res.string_res += "AfterReceiveRequest , ";
			res.AddCurrentOperationContextInfo ();
			return null;
		}

		public void BeforeSendReply (ref Message reply, object correlationState) {
			res.string_res += "BeforeSendReply , ";
			res.AddCurrentOperationContextInfo ();
		}

		#endregion
	}

	#endregion

	#region InstanceProvider

	public class MyInstanceProvider : IInstanceProvider
	{		
		object instance;
		Result res;

		public MyInstanceProvider (object obj, Result result) {
			instance = obj;
			res = result;
		}

		#region IInstanceProvider Members

		public object GetInstance (InstanceContext instanceContext, Message message) {
			res.string_res += "GetInstance1 , ";
			res.AddCurrentOperationContextInfo ();
			return instance;
		}

		public object GetInstance (InstanceContext instanceContext) {
			res.string_res += "GetInstance2 , ";
			res.AddCurrentOperationContextInfo ();
			return instance;
		}

		public void ReleaseInstance (InstanceContext instanceContext, object instance) {
			res.string_res += "ReleaseInstance , ";
			res.AddCurrentOperationContextInfo ();
		}

		#endregion
	}

	#endregion

	#region InstanceContextProvider

	public class MyInstanceContextProvider : IInstanceContextProvider
	{

		InstanceContext existing;
		Result res;

		public MyInstanceContextProvider (InstanceContext exist, Result result) {
			existing = exist;
			res = result;
		}

		#region IInstanceContextProvider Members

		public InstanceContext GetExistingInstanceContext (Message message, IContextChannel channel) {
			res.string_res += "GetExistingInstanceContext , ";
			res.AddCurrentOperationContextInfo ();
			return existing;
		}

		public void InitializeInstanceContext (InstanceContext instanceContext, Message message, IContextChannel channel) {
			res.string_res += "InitializeInstanceContext , ";
			res.AddCurrentOperationContextInfo ();
		}

		public bool IsIdle (InstanceContext instanceContext) {
			res.string_res += "IsIdle , ";
			res.AddCurrentOperationContextInfo ();
			return false;
		}

		public void NotifyIdle (InstanceContextIdleCallback callback, InstanceContext instanceContext) {
			res.string_res += "NotifyIdle , ";
			res.AddCurrentOperationContextInfo ();			
		}

		#endregion
	}

	#endregion

	#region InstanceContextInitializer

	public class MyInstanceContextInitializer : IInstanceContextInitializer
	{
		Result res;

		public MyInstanceContextInitializer (Result result) {
			res = result;
		}

		public void Initialize (InstanceContext instanceContext, Message message) {
			res.string_res += "Initialize , ";
			res.AddCurrentOperationContextInfo ();
		}
	}

	#endregion

	#region Helpers

	public class Result
	{
		public bool Done;
		public string string_res = "";

		public void AddCurrentOperationContextInfo()
		{
			if (OperationContext.Current != null) {
				string_res += "OperationContext , ";
				if (OperationContext.Current.InstanceContext != null) {
					string_res += ("InstanceContext = " + OperationContext.Current.InstanceContext.State + " , ");
					//if (OperationContext.Current.InstanceContext != null)
					//    string_res += ("Instance = " + OperationContext.Current.InstanceContext.GetServiceInstance () + " , ");
				}
			}
		}
	}

	class AllActions : IAllActions, IDisposable
	{
		Result res;

		public AllActions () { }

		public AllActions (Result result) {
			res = result;
		}

		[OperationBehavior (ReleaseInstanceMode = ReleaseInstanceMode.BeforeAndAfterCall)]
		public int Get(int i)
		{
			return i;
		}

		public void Dispose () {
			if (res != null)
				res.string_res += "Disposed , ";
		}
	}

	[ServiceContract (Namespace = "http://MonoTests.System.ServiceModel.Dispatcher")]
	public interface IAllActions
	{		
		[OperationContract]		
		int Get(int i);
	}

	#endregion

	#region ClientProxy
	
	public class AllActionsProxy : ClientBase<IAllActions>, IAllActions
	{
		public AllActionsProxy (Binding binding, EndpointAddress remoteAddress) :
			base (binding, remoteAddress)
		{
		}

		public int Get (int i) {
			return base.Channel.Get (i);
		}
	}	

	#endregion
#endregion

}
