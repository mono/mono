//
// System.Runtime.Remoting.Activation.RemoteActivationAttribute.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Collections;

namespace System.Runtime.Remoting.Activation
{
	internal class RemoteActivationAttribute: Attribute, IContextAttribute
	{
		// This activation attribute is used when creating a client activated
		// CBO in the server. This attribute will enforce the creation of
		// a new context, and will provide the context properties collected in
		// the client.

		IList _contextProperties;

		public RemoteActivationAttribute ()
		{
		}

		public RemoteActivationAttribute(IList contextProperties)
		{
			_contextProperties = contextProperties;
		}

		public bool IsContextOK(Context ctx, IConstructionCallMessage ctor)
		{
			// CBOs remotely activated allways need a new context
			return false;
		}

		public void GetPropertiesForNewContext(IConstructionCallMessage ctor)
		{
			if (_contextProperties != null)
			{
				foreach (object prop in _contextProperties)
					ctor.ContextProperties.Add (prop);
			}
		}
	}
}
