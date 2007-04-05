//
// System.Web.Configuration.AuthorizationConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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

using System;
using System.Collections;
using System.Security.Principal;
using System.Web.UI;
using System.Globalization;

namespace System.Web.Configuration
{
	class AuthorizationConfig
	{
		AuthorizationConfig parent;
		ArrayList list;

		internal AuthorizationConfig (object parent)
		{
			this.parent = parent as AuthorizationConfig;
		}

		static string [] SplitAndTrim (string s)
		{
			if (s == null || s == "")
				return null;

			string [] all = s.Split (',');
			for (int i = 0; i < all.Length; i++)
				all [i] = all [i].Trim ();

			return all;
		}

		static bool CheckWildcards (string [] values)
		{
			if (values == null)
				return true;

			foreach (string s in values) {
				if (s == null || s.Length == 1)
					continue;

				if (s.IndexOf ('?') != -1 || s.IndexOf ('*') != -1)
					return false;
			}

			return true;
		}
		
		bool Add (bool allow, string users, string roles, string verbs)
		{
			string [] allUsers = SplitAndTrim (users);
			string [] allRoles = SplitAndTrim (roles);
			string [] allVerbs = SplitAndTrim (verbs);
			if (!CheckWildcards (allUsers) || !CheckWildcards (allRoles))
				return false;

			if (list == null)
				list = new ArrayList ();

			list.Add (new UserData (allow, allUsers, allRoles, allVerbs));
			return true;
		}

		internal bool Allow (string users, string roles, string verbs)
		{
			return Add (true, users, roles, verbs);
		}

		internal bool Deny (string users, string roles, string verbs)
		{
			return Add (false, users, roles, verbs);
		}

		internal bool IsValidUser (IPrincipal user, string verb)
		{
			if (user == null)
				return false;

			if (list == null) {
				if (parent != null)
					return parent.IsValidUser (user, verb);

				return true;
			}

			foreach (UserData data in list) {
				if (data.Verbs != null && !data.CheckVerb (verb))
					continue;

				if ((data.Users !=null && data.CheckUser(user.Identity.Name)) ||
				    (data.Roles != null && data.CheckRole(user)))
					return data.Allow;
			}
			
			if (parent != null)
				return parent.IsValidUser (user, verb);

			return true;
		}

		struct UserData
		{
			public bool Allow;
			public string [] Users;
			public string [] Roles;
			public string [] Verbs;

			public UserData (bool allow, string [] users, string [] roles, string [] verbs)
			{
				Allow = allow;
				Users = users;
				Roles = roles;
				Verbs = verbs;
			}

			public bool CheckUser (string user)
			{
				foreach (string u in Users) {
					if (String.Compare (u, user, true) == 0 ||
					    u == "*" ||
					    (u == "?" && user == ""))
						return true;
				}

				return false;
			}

			public bool CheckRole (IPrincipal user)
			{
				foreach (string r in Roles) {
					if (user.IsInRole (r))
						return true;
				}

				return false;
			}

			public bool CheckVerb (string verb)
			{
				foreach (string u in Verbs) {
					if (String.Compare (u, verb, true, CultureInfo.InvariantCulture) == 0)
						return true;
				}

				return false;
			}

		}
	}
}

