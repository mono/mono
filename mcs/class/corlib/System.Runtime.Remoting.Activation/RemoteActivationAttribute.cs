//
// System.Runtime.Remoting.Activation.RemoteActivationAttribute.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
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
