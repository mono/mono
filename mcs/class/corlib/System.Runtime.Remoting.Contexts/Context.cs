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
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Contexts {

	public class Context 
	{
		public int domain_id;
		int context_id;
		int process_id;

		static Context default_context;
		static ArrayList domain_contexts = new ArrayList();

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
		
		static Context ()
		{
			// Creates the default context sink chain

			default_server_context_sink = new ServerContextTerminatorSink();
			default_client_context_sink = new ClientContextTerminatorSink();

			default_context = new Context ();
			default_context.frozen = true;
		}
		
		public Context ()
		{
			domain_id = Thread.GetDomainID();
			context_id = global_count++;
		}

		public static Context DefaultContext {
			get {
				return default_context;
			}
		}

		public virtual int ContextID {
			get {
				return context_id;
			}
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
			if (this == default_context)
				throw new InvalidOperationException ("Can not add properties to " +
								     "default context");
			if (frozen)
				throw new InvalidOperationException ("Context is Frozen");
			
			if (context_properties == null)
				context_properties = new ArrayList ();

			context_properties.Add (prop);
		}

		[MonoTODO("Create sinks from contributor properties")]
		internal IMessageSink GetServerContextSinkChain()
		{
			if (server_context_sink_chain == null)
			{
				server_context_sink_chain = default_server_context_sink;
			}
			return server_context_sink_chain;
		}

		[MonoTODO("Create sinks from contributor properties")]
		internal IMessageSink GetClientContextSinkChain()
		{
			if (client_context_sink_chain == null)
			{
				client_context_sink_chain = default_client_context_sink;
			}
			return client_context_sink_chain;
		}

		[MonoTODO("Create object sinks from contributor properties")]
		internal IMessageSink CreateServerObjectSinkChain (MarshalByRefObject obj)
		{
			IMessageSink objectSink = new StackBuilderSink(obj);
			objectSink = new ServerObjectTerminatorSink(objectSink);
			return new Lifetime.LeaseSink(objectSink);
		}

		[MonoTODO("Get sink from properties")]
		internal IMessageSink CreateEnvoySink (MarshalByRefObject serverObject)
		{
			return EnvoyTerminatorSink.Instance;
		}

		[MonoTODO("Notify dynamic sinks")]
		internal static Context SwitchToContext (Context newContext)
		{
			return AppDomain.InternalSetContext (newContext);
		}

		[MonoTODO("Check type properties")]
		internal static Context GetContextForType (Type type)
		{
			// This method returns a context for a new instace of the provided type.
			// It can be the current context or a new one.

			return default_context;
		}
	}
}
