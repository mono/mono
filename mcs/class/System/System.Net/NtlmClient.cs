//
// System.Net.NtlmClient
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Motus Technologies. All rights reserved.
// (c) 2003 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Reflection;

namespace System.Net
{
	class NtlmClient : IAuthenticationModule
	{
		static Type ntlmAuthType;
		IAuthenticationModule authObject;

		static NtlmClient ()
		{
			Assembly ass = Assembly.Load ("Mono.Security");
			if (ass != null)
				ntlmAuthType = ass.GetType ("Mono.Http.NtlmClient", false);
		}
		
		public NtlmClient ()
		{
			if (ntlmAuthType != null)
				authObject = (IAuthenticationModule) Activator.CreateInstance (ntlmAuthType);
		}
	
		public Authorization Authenticate (string challenge, WebRequest webRequest, ICredentials credentials) 
		{
			if (authObject == null)
				return null;

			return authObject.Authenticate (challenge, webRequest, credentials);
		}

		public Authorization PreAuthenticate (WebRequest webRequest, ICredentials credentials) 
		{
			return null;
		}
	
		public string AuthenticationType { 
			get { return "NTLM"; }
		}
	
		public bool CanPreAuthenticate { 
			get { return false; }
		}
	}
}

