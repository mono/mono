//
// System.Runtime.Remoting.InternalRemotingServices.cs
//
// Authors:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Novell, Inc.
//

using System;
using System.Diagnostics;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting
{
	public class InternalRemotingServices 
	{
		public InternalRemotingServices ()
		{
		}
		
		[MonoTODO]
		[Conditional("_LOGGING")]
		public static void DebugOutChnl (string s)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static SoapAttribute GetCachedSoapAttribute (object reflectionObject)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[Conditional("_DEBUG")]
		public static void RemotingAssert (bool condition, string message)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[Conditional("_LOGGING")]
		public static void RemotingTrace (params object[] messages)
		{
			throw new NotImplementedException ();
		}
		
		[CLSCompliant (false)]
		public static void SetServerIdentity (MethodCall m, object srvID)
		{
			Identity ident = srvID as Identity;
			if (ident == null) throw new ArgumentException ("srvID");
			
			RemotingServices.SetMessageTargetIdentity (m, ident);
		}
	}
}
