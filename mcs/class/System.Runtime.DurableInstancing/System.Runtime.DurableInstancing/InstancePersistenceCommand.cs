using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace System.Runtime.DurableInstancing
{
	public abstract class InstancePersistenceCommand
	{
		protected InstancePersistenceCommand (XName name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			Name = name;
		}

		protected internal virtual bool AutomaticallyAcquiringLock {
			get { return false; }
		}

		protected internal virtual bool IsTransactionEnlistmentOptional {
			get { return false; }
		}

		public XName Name { get; private set; }

		protected internal virtual void Validate (InstanceView view)
		{
			throw new NotImplementedException ();
		}
	}
}
