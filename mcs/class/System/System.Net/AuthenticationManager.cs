//
// System.Net.AuthenticationManager.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	public class AuthenticationManager {

		static ArrayList modules;

		public static IEnumerator RegisteredModules {
			get {
				if (!modules)
					modules = new ArrayList ();

				return modules;
			}
		}

		public static Authorization PreAuthenticate (WebRequest request,
							     ICredentialLookup credentials)
		{
			// FIXME: implement
		}

		public static void Register (IAuthenticationModule authenticationModule)
		{
			if (!modules)
				modules = new ArrayList ();

			modules.Add (authenticationModule);
		}

		public static Authorization Respond (WebHeaders ResponseHeaders,
						     WebRequest Request,
						     ICredentialLookup credentials)
		{
			// FIXME: implement
			return null;
		}

		public static void Unregister (IAuthenticationModule authenticationModule)
		{
			// FIXME: implement
		}

		pubilc static void Unregister (string authenticationScheme)
		{
			// FIXME: implement
		}
	}
}
