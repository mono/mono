//
// ExtendedProtectionPolicy.cs 
//
// Authors:
//      Atsushi Enomoto  <atsushi@ximian.com>
//

//
// Copyright (C) 2010 Novell, Inc (http://novell.com)
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
#if NET_4_0
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Security.Authentication.ExtendedProtection
{
	[MonoTODO]
	[Serializable]
	[TypeConverter (typeof (ExtendedProtectionPolicyTypeConverter))]
	public class ExtendedProtectionPolicy : ISerializable
	{
		public ExtendedProtectionPolicy (PolicyEnforcement policyEnforcement)
		{
			throw new NotImplementedException ();
		}

		public ExtendedProtectionPolicy (PolicyEnforcement policyEnforcement, ChannelBinding customChannelBinding)
		{
			throw new NotImplementedException ();
		}

		public ExtendedProtectionPolicy (PolicyEnforcement policyEnforcement, ProtectionScenario protectionScenario, ICollection customServiceNames)
		{
			throw new NotImplementedException ();
		}

		public ExtendedProtectionPolicy (PolicyEnforcement policyEnforcement, ProtectionScenario protectionScenario, ServiceNameCollection customServiceNames)
		{
			throw new NotImplementedException ();
		}

		protected ExtendedProtectionPolicy (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public ChannelBinding CustomChannelBinding {
			get { throw new NotImplementedException (); }
		}

		public ServiceNameCollection CustomServiceNames {
			get { throw new NotImplementedException (); }
		}

		public static bool OSSupportsExtendedProtection {
			get { throw new NotImplementedException (); }
		}

		public PolicyEnforcement PolicyEnforcement {
			get { throw new NotImplementedException (); }
		}

		public ProtectionScenario ProtectionScenario {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
