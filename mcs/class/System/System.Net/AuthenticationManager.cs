//
// System.Net.AuthenticationManager.cs
//
// Author:
// 	Miguel de Icaza (miguel@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
//

using System.Collections;

namespace System.Net
{
	public class AuthenticationManager
	{
		static ArrayList modules;

		static void EnsureModules ()
		{
			if (modules != null)
				return;

			lock (typeof (AuthenticationManager)) {
				if (modules != null)
					return;
				
				modules = new ArrayList ();
			}
		}
		
		public static IEnumerator RegisteredModules {
			get {
				EnsureModules ();
				return modules.GetEnumerator ();
			}
		}

		public static Authorization Authenticate (string challenge, WebRequest request, ICredentials credentials)
		{
			if (request == null)
				throw new ArgumentNullException ("request");

			if (credentials == null)
				throw new ArgumentNullException ("credentials");

			if (challenge == null)
				throw new ArgumentNullException ("challenge");

			return DoAuthenticate (challenge, request, credentials);
		}

		static Authorization DoAuthenticate (string challenge, WebRequest request, ICredentials credentials)
		{
			EnsureModules ();
			lock (modules) {
				foreach (IAuthenticationModule mod in modules) {
					Authorization auth = mod.Authenticate (challenge, request, credentials);
					if (auth == null)
						continue;

					auth.Module = mod;
					return auth;
				}
			}

			return null;
		}

		[MonoTODO]
		public static Authorization PreAuthenticate (WebRequest request, ICredentials credentials)
		{
			if (request == null)
				throw new ArgumentNullException ("request");

			if (credentials == null)
				return null;
			return null;
		}

		public static void Register (IAuthenticationModule authenticationModule)
		{
			if (authenticationModule == null)
				throw new ArgumentNullException ("authenticationModule");

			DoUnregister (authenticationModule.AuthenticationType, false);
			lock (modules)
				modules.Add (authenticationModule);
		}

		public static void Unregister (IAuthenticationModule authenticationModule)
		{
			if (authenticationModule == null)
				throw new ArgumentNullException ("authenticationModule");

			EnsureModules ();
			lock (modules) {
				if (modules.Contains (authenticationModule))
					modules.Remove (authenticationModule);
				else
					throw new InvalidOperationException ("No such module registered.");
			}
		}

		public static void Unregister (string authenticationScheme)
		{
			if (authenticationScheme == null)
				throw new ArgumentNullException ("authenticationScheme");
			
			DoUnregister (authenticationScheme, true);
		}

		static void DoUnregister (string authenticationScheme, bool throwEx)
		{
			EnsureModules ();
			lock (modules) {
				IAuthenticationModule module = null;
				foreach (IAuthenticationModule mod in modules) {
					string modtype = mod.AuthenticationType;
					if (String.Compare (modtype, authenticationScheme, true) == 0) {
						module = mod;
						break;
					}
				}

				if (module == null) {
					if (throwEx)
						throw new InvalidOperationException ("Scheme not registered.");
				} else {
					modules.Remove (module);
				}
			}
		}
	}
}

