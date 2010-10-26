//
// System.Runtime.Remoting.InternalRemotingServices.cs
//
// Authors:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Novell, Inc.
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
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting
{
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class InternalRemotingServices 
	{
		static Hashtable _soapAttributes = new Hashtable ();
		
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
			lock (_soapAttributes.SyncRoot) {
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
			
			#if !DISABLE_REMOTING
			RemotingServices.SetMessageTargetIdentity (m, ident);
			#endif
		}
	}
}
