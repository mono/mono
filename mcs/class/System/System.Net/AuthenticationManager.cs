//
// System.Net.AuthenticationManager.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Collections;

namespace System.Net {

	public class AuthenticationManager {

		static ArrayList modules;

		public static IEnumerator RegisteredModules {
			get {
				if (modules == null)
					modules = new ArrayList ();

				return modules as IEnumerator;
			}
		}

		public static Authorization PreAuthenticate (WebRequest request,
							     ICredentials credentials)
		{
			// FIXME: implement
			return null;
		}

		public static void Register (IAuthenticationModule authenticationModule)
		{
			if (modules == null)
				modules = new ArrayList ();

			modules.Add (authenticationModule);
		}

		public static void Unregister (IAuthenticationModule authenticationModule)
		{
			// FIXME: implement
		}

		public static void Unregister (string authenticationScheme)
		{
			// FIXME: implement
		}
	}
}
