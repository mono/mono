//
// System.Runtime.Remoting.Contexts.Context..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Collections;

namespace System.Runtime.Remoting.Contexts {

	public class Context {
		static Context default_context;
		ArrayList context_properties;
		int context_id;
		bool frozen;
		static int global_count;
		
		static Context ()
		{
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
	}
}
