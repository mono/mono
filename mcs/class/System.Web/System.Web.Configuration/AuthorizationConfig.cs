//
// System.Web.Configuration.AuthorizationConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Security.Principal;
using System.Web.UI;

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

			bool userMatch;
			bool roleMatch;
			bool verbMatch;
			foreach (UserData data in list) {
				if (data.Users == null)
					continue;

				userMatch = (data.Users == null);
				if (!userMatch)
					userMatch = data.CheckUser (user.Identity.Name);

				roleMatch = (data.Roles == null);
				if (!roleMatch)
					roleMatch = data.CheckRole (user);

				verbMatch = (data.Verbs == null);
				if (data.Verbs != null)
					verbMatch = data.CheckVerb (verb);

				if (userMatch && roleMatch && verbMatch)
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
					if (String.Compare (u, verb, true) == 0)
						return true;
				}

				return false;
			}

		}
	}
}

