//
// System.Runtime.Remoting.InternalRemotingServices.cs
//
// Authors:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Novell, Inc.
//

using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting
{
	public class InternalRemotingServices 
	{
		static Hashtable _soapAttributes;
		
		public InternalRemotingServices ()
		{
		}
		
		[Conditional("_LOGGING")]
		public static void DebugOutChnl (string s)
		{
			// Mono does not support this internal method
			throw new NotSupportedException ();
		}
		
		public static SoapAttribute GetCachedSoapAttribute (object reflectionObject)
		{
			if (_soapAttributes == null)
			{
				lock (typeof(InternalRemotingServices))
				{
					if (_soapAttributes == null)
						_soapAttributes = new Hashtable ();
				}
			}
			
			lock (_soapAttributes.SyncRoot)
			{
				SoapAttribute att = _soapAttributes [reflectionObject] as SoapAttribute;
				if (att != null) return att;
				
				ICustomAttributeProvider ap = (ICustomAttributeProvider) reflectionObject;
				object[] atts = ap.GetCustomAttributes (typeof(SoapAttribute), true);
				if (atts.Length > 0) 
					att = (SoapAttribute) atts[0];
				else
				{
					if (reflectionObject is Type)
						att = new SoapTypeAttribute ();
					else if (reflectionObject is FieldInfo)
						att = new SoapFieldAttribute ();
					else if (reflectionObject is MethodBase)
						att = new SoapMethodAttribute ();
					else if (reflectionObject is ParameterInfo)
						att = new SoapParameterAttribute ();
				}
				
				att.SetReflectionObject (reflectionObject);
				_soapAttributes [reflectionObject] = att;
				return att;
			}
		}
		
		[Conditional("_DEBUG")]
		public static void RemotingAssert (bool condition, string message)
		{
			// Mono does not support this internal method
			throw new NotSupportedException ();
		}
		
		[Conditional("_LOGGING")]
		public static void RemotingTrace (params object[] messages)
		{
			// Mono does not support this internal method
			throw new NotSupportedException ();
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
