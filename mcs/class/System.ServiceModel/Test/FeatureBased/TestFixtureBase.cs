#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;
using System.Reflection;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Net;
using MonoTests.stand_alone.WebHarness;
using System.ServiceModel.Dispatcher;
using System.Collections.ObjectModel;

using MonoTests.Helpers;

namespace MonoTests.Features
{
	public class Configuration
	{
		static Configuration() {
			var port = NetworkHelpers.FindFreePort ();
			onlyServers = Boolean.Parse (ConfigurationManager.AppSettings ["onlyServers"]  ?? "false");
			onlyClients = Boolean.Parse (ConfigurationManager.AppSettings ["onlyClients"]  ?? "false");
			endpointBase = ConfigurationManager.AppSettings ["endpointBase"] ?? $"http://localhost:{port}/";
			if (!endpointBase.EndsWith ("/"))
				endpointBase = endpointBase + '/';
			logMessages = Boolean.Parse (ConfigurationManager.AppSettings ["logMessages"] ?? "false");
		}
		public static bool onlyServers;
		public static bool onlyClients;
		public static string endpointBase;
		public static bool logMessages;
		public static bool IsLocal { get { return !onlyServers && !onlyClients; } }
	}

	class LoggerMessageInspector : IDispatchMessageInspector
	{
		#region IDispatchMessageInspector Members

		public object AfterReceiveRequest (ref Message request, IClientChannel channel, InstanceContext instanceContext) {
			Console.WriteLine ("****Begin message received by host:");
			Console.WriteLine (request);
			Console.WriteLine ("****End message received by host:");
			return new object ();
		}

		public void BeforeSendReply (ref Message reply, object correlationState) {
			Console.WriteLine ("****Begin message reply from host:");
			Console.WriteLine (reply);
			Console.WriteLine ("****End message reply from host:");
		}

		#endregion
	}
	class LoggerBehavior : IServiceBehavior
	{

		#region IServiceBehavior Members

		public void AddBindingParameters (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) {

		}

		public void ApplyDispatchBehavior (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
			ChannelDispatcher dispatcher = serviceHostBase.ChannelDispatchers [0] as ChannelDispatcher;
			dispatcher.Endpoints [0].DispatchRuntime.MessageInspectors.Add (new LoggerMessageInspector ());
		}

		public void Validate (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {

		}

		#endregion
	}

	public abstract class TestFixtureBase<TClient, TServer, IServer> where TClient : new() where TServer: new()
	{
		ServiceHost _hostBase;
		ChannelFactory<IServer> factory;

		protected TestFixtureBase () { }		

		[TearDown]
		public void TearDown ()
		{
			if (_hostBase != null)
				_hostBase.Close ();
			if (factory != null)
				factory.Close ();
		}

		[SetUp]
		public virtual void Run (){
			bool runServer = true;
			bool runClient = true;
			if (Configuration.onlyClients)
				runServer = false;
			if (Configuration.onlyServers)
				runClient = false;
			Run (runServer, runClient);			
		}

		public void CheckWsdlImpl () {
			string goldWsdl;
			try {
				Assembly _assembly = Assembly.GetExecutingAssembly ();
				StreamReader _stream = new StreamReader (_assembly.GetManifestResourceStream ("MonoTests.System.ServiceModel.Test.FeatureBased.Features.Contracts." + typeof (TServer).Name + ".xml"));
				goldWsdl = _stream.ReadToEnd ();
			}
			catch {
				Console.WriteLine ("Couldn't test WSDL of server " + typeof (TServer).Name + " because gold wsdl is not embedded in test !");
				return;
			}
			string currentWsdl = "";

			HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create (getMexEndpoint () + "?wsdl");
			// Obtain a 'Stream' object associated with the response object.
			WebResponse response = myReq.GetResponse ();
			Stream ReceiveStream = response.GetResponseStream ();

			Encoding encode = global::System.Text.Encoding.GetEncoding ("utf-8");

			// Pipe the stream to a higher level stream reader with the required encoding format. 
			StreamReader readStream = new StreamReader (ReceiveStream, encode);
			Console.WriteLine ("\nResponse stream received");
			int maxLen = 10 * 1024;
			Char [] read = new Char [maxLen];

			// Read 256 charcters at a time.    
			int count = readStream.Read (read, 0, maxLen);
			while (count > 0) {
				currentWsdl = currentWsdl + new String (read, 0, count);
				count = readStream.Read (read, 0, 256);
			}
			readStream.Close ();
			response.Close ();

			XmlComparer comparer = new XmlComparer (XmlComparer.Flags.IgnoreAttribOrder, true);
			Assert.IsTrue (comparer.AreEqual (goldWsdl, currentWsdl), "Service WSDL does not match gold WSDL");

		}

		protected void Run (bool runServer, bool runClient) {

			if (runServer) {
				_hostBase = InitializeServiceHost ();
				_hostBase.Open ();
			}

		}

        string getEndpoint()
        {
			return Configuration.endpointBase + typeof(TServer).Name;
        }

		public string getMexEndpoint () 
		{
			return getEndpoint () + "_wsdl"; // should be getEndpoint() but currently implementation is buggy
		}

		TClient _client;
		protected virtual TClient InitializeClient () {
			//return new TClient(new BasicHttpBinding(), new EndpointAddress( getEndpoint) );
			Type [] paramsTypes = new Type [] { typeof (Binding), typeof (EndpointAddress) };
			object [] parameters = new object [] { new BasicHttpBinding (), new EndpointAddress (getEndpoint())};

			ConstructorInfo info = typeof (TClient).GetConstructor (paramsTypes);
			return (TClient) info.Invoke (parameters);
		}

		public TClient ClientProxy {
			get {
				if (_client == null)
					_client = InitializeClient ();			
				return _client;
			}			
		}

		public IServer Client {
			get {
				factory = new ChannelFactory<IServer> (new BasicHttpBinding (), new EndpointAddress (getEndpoint ()));
				return factory.CreateChannel ();
			}
		}

		protected virtual ServiceHost InitializeServiceHost () {
            ServiceHost host = new ServiceHost(typeof(TServer));
            host.AddServiceEndpoint(typeof(IServer), new BasicHttpBinding(), getEndpoint());
			ServiceMetadataBehavior smb = new ServiceMetadataBehavior ();
			smb.HttpGetEnabled = true;
			smb.HttpGetUrl = new Uri (getMexEndpoint ());
			host.Description.Behaviors.Add (smb);
			host.Description.Behaviors.Add (new ServiceThrottlingBehavior () { MaxConcurrentCalls = 1, MaxConcurrentSessions = 1 });
			if (Configuration.logMessages)
				host.Description.Behaviors.Add (new LoggerBehavior ());
            return host;
		}


		protected ServiceHost Host {
			get {
				return _hostBase;
			}
		}

		[TearDown]
		protected virtual void Close () {
			if (!Configuration.onlyClients && !Configuration.onlyServers &&  Host.State == CommunicationState.Opened)
				Host.Close ();
		}
	}
}
#endif