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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Contexts {

	[System.Runtime.InteropServices.ComVisible (true)]
	public class Context 
	{
#pragma warning disable 169, 414
		#region Sync with domain-internals.h
		int domain_id;
		int context_id;
		UIntPtr static_data; /* GC-tracked */
		#endregion
#pragma warning restore 169, 414

		// Default server context sink chain
		static IMessageSink default_server_context_sink;

		// The sink chain that has to be used by all calls entering the context
		IMessageSink server_context_sink_chain = null;

		// The sink chain that has to be used by all calls exiting the context
		IMessageSink client_context_sink_chain = null;

		object[] datastore;
		ArrayList context_properties;
		bool frozen;
		
		static int global_count;

		/* Wrap this in a nested class so its not constructed during shutdown */
		class NamedSlots {
			public static Hashtable namedSlots = new Hashtable ();
		}

		static DynamicPropertyCollection global_dynamic_properties;
		DynamicPropertyCollection context_dynamic_properties;
		ContextCallbackObject callback_object = null;
		
		public Context ()
		{
			domain_id = Thread.GetDomainID();
			context_id = 1 + global_count++;
		}

		~Context ()
		{
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

		public virtual IContextProperty[] ContextProperties
		{
			get 
			{
				if (context_properties == null) return new IContextProperty[0];
				else return (IContextProperty[]) context_properties.ToArray (typeof(IContextProperty[]));
			}
		}
		
		internal bool IsDefaultContext
		{
			get { return context_id == 0; }
		}

		internal bool NeedsContextSink
		{
			get {
				return context_id != 0 || 
					(global_dynamic_properties != null && global_dynamic_properties.HasProperties) || 
					(context_dynamic_properties != null && context_dynamic_properties.HasProperties);
			}
		}

		public static bool RegisterDynamicProperty(IDynamicProperty prop, ContextBoundObject obj, Context ctx)
		{
			DynamicPropertyCollection col = GetDynamicPropertyCollection (obj, ctx);
			return col.RegisterDynamicProperty (prop);
		}

		public static bool UnregisterDynamicProperty(string name, ContextBoundObject obj, Context ctx)
		{
			DynamicPropertyCollection col = GetDynamicPropertyCollection (obj, ctx);
			return col.UnregisterDynamicProperty (name);
		}
		
		static DynamicPropertyCollection GetDynamicPropertyCollection(ContextBoundObject obj, Context ctx)
		{
			if (ctx == null && obj != null)
			{
				if (RemotingServices.IsTransparentProxy(obj))
				{
					RealProxy rp = RemotingServices.GetRealProxy (obj);
					return rp.ObjectIdentity.ClientDynamicProperties;
				}
				else
					return obj.ObjectIdentity.ServerDynamicProperties;
			}
			else if (ctx != null && obj == null)
			{
				if (ctx.context_dynamic_properties == null) ctx.context_dynamic_properties = new DynamicPropertyCollection ();
				return ctx.context_dynamic_properties;
			}
			else if (ctx == null && obj == null)
			{
				if (global_dynamic_properties == null) global_dynamic_properties = new DynamicPropertyCollection ();
				return global_dynamic_properties;
			}
			else
				throw new ArgumentException ("Either obj or ctx must be null");
		}
		
		internal static void NotifyGlobalDynamicSinks  (bool start, IMessage req_msg, bool client_site, bool async)
		{
			if (global_dynamic_properties != null && global_dynamic_properties.HasProperties) 
				global_dynamic_properties.NotifyMessage (start, req_msg, client_site, async);
		}

		internal static bool HasGlobalDynamicSinks
		{
			get { return (global_dynamic_properties != null && global_dynamic_properties.HasProperties); }
		}

		internal void NotifyDynamicSinks  (bool start, IMessage req_msg, bool client_site, bool async)
		{
			if (context_dynamic_properties != null && context_dynamic_properties.HasProperties) 
				context_dynamic_properties.NotifyMessage (start, req_msg, client_site, async);
		}

		internal bool HasDynamicSinks
		{
			get { return (context_dynamic_properties != null && context_dynamic_properties.HasProperties); }
		}

		internal bool HasExitSinks
		{
			get
			{
				// Needs to go through the client context sink if there are custom
				// client context or dynamic sinks.

				return ( !(GetClientContextSinkChain() is ClientContextTerminatorSink) || HasDynamicSinks || HasGlobalDynamicSinks);
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
					// Enumerate in reverse order
					for (int n = context_properties.Count-1; n>=0; n--) {
						IContributeServerContextSink contributor = context_properties[n] as IContributeServerContextSink;
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
				client_context_sink_chain = new ClientContextTerminatorSink (this);

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

		internal IMessageSink CreateServerObjectSinkChain (MarshalByRefObject obj, bool forceInternalExecute)
		{
			IMessageSink objectSink = new StackBuilderSink (obj, forceInternalExecute);
			objectSink = new ServerObjectTerminatorSink (objectSink);
			objectSink = new Lifetime.LeaseSink (objectSink);

			if (context_properties != null)
			{
				// Contribute object sinks in reverse order
				for (int n = context_properties.Count-1; n >= 0; n--)
				{
					IContextProperty prop = (IContextProperty) context_properties[n];
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
		
		public void DoCallBack (CrossContextDelegate deleg)
		{
			lock (this)
			{
				if (callback_object == null) {
					Context oldContext = Context.SwitchToContext (this);
					callback_object = new ContextCallbackObject ();
					Context.SwitchToContext (oldContext);
				}
			}
			
			callback_object.DoCallBack (deleg);
		}
		
#if !MOONLIGHT
		public static LocalDataStoreSlot AllocateDataSlot ()
		{
			return new LocalDataStoreSlot (false);
		}
		
		public static LocalDataStoreSlot AllocateNamedDataSlot (string name)
		{
			lock (NamedSlots.namedSlots.SyncRoot)
			{
				LocalDataStoreSlot slot = AllocateDataSlot ();
				NamedSlots.namedSlots.Add (name, slot);
				return slot;
			}
		}
		
		public static void FreeNamedDataSlot (string name)
		{
			lock (NamedSlots.namedSlots.SyncRoot)
			{
				NamedSlots.namedSlots.Remove (name);
			}
		}
		
		public static object GetData (LocalDataStoreSlot slot)
		{
			Context ctx = Thread.CurrentContext;
			
			lock (ctx)
			{
				if (ctx.datastore != null && slot.slot < ctx.datastore.Length)
					return ctx.datastore [slot.slot];
				return null;
			}
		}
		
		public static LocalDataStoreSlot GetNamedDataSlot (string name)
		{
			lock (NamedSlots.namedSlots.SyncRoot)
			{
				LocalDataStoreSlot slot = NamedSlots.namedSlots [name] as LocalDataStoreSlot;
				if (slot == null) return AllocateNamedDataSlot (name);
				else return slot;
			}
		}
		
		public static void SetData (LocalDataStoreSlot slot, object data)
		{
			Context ctx = Thread.CurrentContext;
			lock (ctx)
			{
				if (ctx.datastore == null) {
					ctx.datastore = new object [slot.slot + 2];
				} else if (slot.slot >= ctx.datastore.Length) {
					object[] nslots = new object [slot.slot + 2];
					ctx.datastore.CopyTo (nslots, 0);
					ctx.datastore = nslots;
				}
				ctx.datastore [slot.slot] = data;
			}
		}
#endif
	}

	class DynamicPropertyCollection
	{
		ArrayList _properties = new ArrayList();

		class DynamicPropertyReg
		{
			public IDynamicProperty Property;
			public IDynamicMessageSink Sink;
		}

		public bool HasProperties
		{
			get { return _properties.Count > 0; }
		}

		public bool RegisterDynamicProperty(IDynamicProperty prop)
		{
			lock (this)
			{
				if (FindProperty (prop.Name) != -1) 
					throw new InvalidOperationException ("Another property by this name already exists");

				// Make a copy, do not interfere with threads running dynamic sinks
				ArrayList newProps = new ArrayList (_properties);

				DynamicPropertyReg reg = new DynamicPropertyReg();
				reg.Property = prop;
				IContributeDynamicSink contributor = prop as IContributeDynamicSink;
				if (contributor != null) reg.Sink = contributor.GetDynamicSink ();
				newProps.Add (reg);

				_properties = newProps;

				return true;	// When should be false?
			}
		}

		public bool UnregisterDynamicProperty(string name)
		{
			lock (this)
			{
				int i = FindProperty (name);
				if (i == -1) throw new RemotingException ("A property with the name " + name + " was not found");

				_properties.RemoveAt (i);
				return true;	// When should be false?
			}
		}

		public void NotifyMessage (bool start, IMessage msg, bool client_site, bool async)
		{
			ArrayList props = _properties;
			if (start)
			{
				foreach (DynamicPropertyReg reg in props)
					if (reg.Sink != null) reg.Sink.ProcessMessageStart (msg, client_site, async);
			}
			else
			{
				foreach (DynamicPropertyReg reg in props)
					if (reg.Sink != null) reg.Sink.ProcessMessageFinish (msg, client_site, async);
			}
		}

		int FindProperty (string name)
		{
			for (int n=0; n<_properties.Count; n++)
				if (((DynamicPropertyReg)_properties[n]).Property.Name == name)
					return n;
			return -1;
		}
	}
	
	class ContextCallbackObject: ContextBoundObject
	{
		public void DoCallBack (CrossContextDelegate deleg)
		{
		}
	}
}
