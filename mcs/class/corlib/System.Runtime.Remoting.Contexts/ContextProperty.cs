//
// System.Runtime.Remoting.Contexts.ContextProperty..cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Runtime.Remoting.Contexts {

	public class ContextProperty
	{
		string name;
		object prop;
		
		private ContextProperty (string name, object prop)
		{
			this.name = name;
			this.prop = prop;
		}
		
		public virtual string Name {
			get { return name; } 
		}

		public virtual object Property {
			get { return prop; }
		}
	}
}
