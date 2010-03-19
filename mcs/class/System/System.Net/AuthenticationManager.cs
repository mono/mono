//
// System.Net.AuthenticationManager.cs
//
// Author:
// 	Miguel de Icaza (miguel@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
//

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

using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Configuration;

namespace System.Net
{
#if MOONLIGHT
	internal class AuthenticationManager {
#else
	public class AuthenticationManager {
#endif
		static ArrayList modules;
		static object locker = new object ();

		private AuthenticationManager ()
		{
		}

		static void EnsureModules ()
		{
			lock (locker) {
				if (modules != null)
					return;
				
				modules = new ArrayList ();
#if NET_2_1
				modules.Add (new BasicClient ());
				modules.Add (new DigestClient ());
				modules.Add (new NtlmClient ());
#elif NET_2_0 && CONFIGURATION_DEP
				object cfg = ConfigurationManager.GetSection ("system.net/authenticationModules");
				AuthenticationModulesSection s = cfg as AuthenticationModulesSection;
				if (s != null) {
					foreach (AuthenticationModuleElement element in s.AuthenticationModules) {
						IAuthenticationModule module = null;
						try {
							Type type = Type.GetType (element.Type, true);
							module = (IAuthenticationModule) Activator.CreateInstance (type);
						} catch {}
						modules.Add (module);
					}
				}
#else
				ConfigurationSettings.GetConfig ("system.net/authenticationModules");
#endif
			}
		}
		
		static ICredentialPolicy credential_policy = null;
		
		public static ICredentialPolicy CredentialPolicy
		{
			get {
				return(credential_policy);
							}
			set {
				credential_policy = value;
			}
		}
		
		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}
		
		[MonoTODO]
		public static StringDictionary CustomTargetNameDictionary
		{
			get {
				throw GetMustImplement ();
			}
		}

		public static IEnumerator RegisteredModules {
			get {
				EnsureModules ();
				return modules.GetEnumerator ();
			}
		}

		internal static void Clear ()
		{
			EnsureModules ();
			lock (modules)
				modules.Clear ();
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

		public static Authorization PreAuthenticate (WebRequest request, ICredentials credentials)
		{
			if (request == null)
				throw new ArgumentNullException ("request");

			if (credentials == null)
				return null;

			EnsureModules ();
			lock (modules) {
				foreach (IAuthenticationModule mod in modules) {
					Authorization auth = mod.PreAuthenticate (request, credentials);
					if (auth == null)
						continue;

					auth.Module = mod;
					return auth;
				}
			}

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

			DoUnregister (authenticationModule.AuthenticationType, true);
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

