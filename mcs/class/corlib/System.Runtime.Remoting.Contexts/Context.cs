//
// System.Runtime.Remoting.Contexts.Context..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Contexts {

	public class Context {
		static Context default_context;
		static ArrayList domain_contexts = new ArrayList();

		// Default server context sink chain
		static IMessageSink default_context_sink;

		// The sink chain that has to be used by all calls entering the context
		IMessageSink server_context_sink_chain = null;

		ArrayList context_properties;
		int context_id;
		bool frozen;
		static int global_count;
		
		static Context ()
		{
			// Creates the default context sink chain
			default_context_sink = new ServerContextTerminatorSink();

			default_context = new Context ();
			default_context.frozen = true;
		}
		
		public Context ()
		{
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
				server_context_sink_chain = default_context_sink;
			}
			return server_context_sink_chain;
		}


		[MonoTODO("Create object sinks from contributor properties")]
		internal IMessageSink CreateServerObjectSinkChain (MarshalByRefObject obj)
		{
			IMessageSink objectSink = new StackBuilderSink(obj);
			objectSink = new ServerObjectTerminatorSink(objectSink);
			return new Lifetime.LeaseSink(objectSink);
		}
	}
}
