//
// System.Runtime.Remoting.Services.EnterpriseServicesHelper.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Remoting.Services 
{
	public sealed class EnterpriseServicesHelper
	{
		public EnterpriseServicesHelper ()
		{
		}
		
		public static IConstructionReturnMessage CreateConstructionReturnMessage (IConstructionCallMessage ctorMsg, MarshalByRefObject retObj)
		{
			return new ConstructionResponse (retObj, null, ctorMsg);
		}

		[MonoTODO]
		public static void SwitchWrappers (RealProxy oldcp, RealProxy newcp)
		{
			throw new NotSupportedException ();
		}
		
		[MonoTODO]
		public static object WrapIUnknownWithComObject (IntPtr punk)
		{
			throw new NotSupportedException ();
		}
	}
}
