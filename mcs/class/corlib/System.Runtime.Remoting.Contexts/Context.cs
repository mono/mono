//
// System.Runtime.Remoting.Contexts.Context..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lluis Sanchez Gual (lluis@ideary.com)
//   Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Collections;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Contexts {

	public class Context 
	{
		public int domain_id;
		int context_id;
		int process_id;

		// Default server context sink chain
		static IMessageSink default_server_context_sink;

		// Default client context sink chain
		static IMessageSink default_client_context_sink;

		// The sink chain that has to be used by all calls entering the context
		IMessageSink server_context_sink_chain = null;

		// The sink chain that has to be used by all calls exiting the context
		IMessageSink client_context_sink_chain = null;

		ArrayList context_properties;
		bool frozen;
		static int global_count;
		
		public Context ()
		{
			domain_id = Thread.GetDomainID();
			context_id = 1 + global_count++;
		}

		public static Context DefaultContext {
			get {
				return AppDomain.InternalGetDefaultContext ();
			}
		}

		public virtual int ContextID {
			get {
				return context_id;
			}
		}

		internal bool IsDefaultContext
		{
			get { return context_id == 0; }
		}

		public virtual IContextProperty GetProperty (string name)
		{
			if (context_properties == null)
				return null;

			foreach (IContextProperty p in context_properties)
				if (p.Name == name)
					return p;
			
			return null;
		}

		public virtual void SetProperty (IContextProperty prop)
		{
			if (prop == null)
				throw new ArgumentNullException ("IContextProperty");
			if (this == DefaultContext)
				throw new InvalidOperationException ("Can not add properties to " +
								     "default context");
			if (frozen)
				throw new InvalidOperationException ("Context is Frozen");
			
			if (context_properties == null)
				context_properties = new ArrayList ();

			context_properties.Add (prop);
		}

		public virtual void Freeze ()
		{
			if (context_properties != null)
			{
				foreach (IContextProperty prop in context_properties)
					prop.Freeze (this);
			}
		}

		public override string ToString()
		{
			return "ContextID: " + context_id;
		}

		internal IMessageSink GetServerContextSinkChain()
		{
			if (server_context_sink_chain == null)
			{
				if (default_server_context_sink == null)
					default_server_context_sink = new ServerContextTerminatorSink();

				server_context_sink_chain = default_server_context_sink;

				if (context_properties != null) {
					foreach (IContextProperty prop in context_properties) {
						IContributeServerContextSink contributor = prop as IContributeServerContextSink;
						if (contributor != null)
							server_context_sink_chain = contributor.GetServerContextSink (server_context_sink_chain);
					}
				}
			}
			return server_context_sink_chain;
		}

		internal IMessageSink GetClientContextSinkChain()
		{
			if (client_context_sink_chain == null)
			{
				if (default_client_context_sink == null)
					default_client_context_sink = new ClientContextTerminatorSink();

				client_context_sink_chain = default_client_context_sink;

				if (context_properties != null) {
					foreach (IContextProperty prop in context_properties) {
						IContributeClientContextSink contributor = prop as IContributeClientContextSink;
						if (contributor != null)
							client_context_sink_chain = contributor.GetClientContextSink (client_context_sink_chain);
					}
				}
			}
			return client_context_sink_chain;
		}

		internal IMessageSink CreateServerObjectSinkChain (MarshalByRefObject obj)
		{
			IMessageSink objectSink = new StackBuilderSink(obj);
			objectSink = new ServerObjectTerminatorSink(objectSink);
			objectSink = new Lifetime.LeaseSink(objectSink);

			if (context_properties != null)
			{
				foreach (IContextProperty prop in context_properties)
				{
					IContributeObjectSink contributor = prop as IContributeObjectSink;
					if (contributor != null)
						objectSink = contributor.GetObjectSink (obj, objectSink);
				}
			}
			return objectSink;
		}

		internal IMessageSink CreateEnvoySink (MarshalByRefObject serverObject)
		{
			IMessageSink sink = EnvoyTerminatorSink.Instance;
			if (context_properties != null)
			{
				foreach (IContextProperty prop in context_properties)
				{
					IContributeEnvoySink contributor = prop as IContributeEnvoySink;
					if (contributor != null)
						sink = contributor.GetEnvoySink (serverObject, sink);
				}
			}
			return sink;
		}

		[MonoTODO("Notify dynamic sinks")]
		internal static Context SwitchToContext (Context newContext)
		{
			return AppDomain.InternalSetContext (newContext);
		}

		internal static Context CreateNewContext (IConstructionCallMessage msg)
		{
			// Create the new context

			Context newContext = new Context();

			foreach (IContextProperty prop in msg.ContextProperties)
			{
				if (newContext.GetProperty (prop.Name) == null)
					newContext.SetProperty (prop);
			}
			newContext.Freeze();


			// Ask each context property whether the new context is OK

			foreach (IContextProperty prop in msg.ContextProperties)
				if (!prop.IsNewContextOK (newContext)) 
					throw new RemotingException("A context property did not approve the candidate context for activating the object");

			return newContext;
		}
	}
}
