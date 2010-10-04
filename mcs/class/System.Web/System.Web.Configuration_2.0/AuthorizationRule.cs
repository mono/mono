//
// System.Web.Configuration.AuthorizationRule
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Security.Principal;
using System.Configuration;
using System.ComponentModel;
using System.Xml;
using System.Globalization;
using System.Web.Util;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class AuthorizationRule : ConfigurationElement
	{
		static ConfigurationProperty rolesProp;
		static ConfigurationProperty usersProp;
		static ConfigurationProperty verbsProp;
		static ConfigurationPropertyCollection properties;

		AuthorizationRuleAction action;
		ConfigurationSaveMode saveMode = ConfigurationSaveMode.Full;
			
		static AuthorizationRule ()
		{
			rolesProp = new ConfigurationProperty ("roles", typeof (StringCollection), null,
							       PropertyHelper.CommaDelimitedStringCollectionConverter,
							       PropertyHelper.DefaultValidator,
							       ConfigurationPropertyOptions.None);
			usersProp = new ConfigurationProperty ("users", typeof (StringCollection), null,
							       PropertyHelper.CommaDelimitedStringCollectionConverter,
							       PropertyHelper.DefaultValidator,
							       ConfigurationPropertyOptions.None);
			verbsProp = new ConfigurationProperty ("verbs", typeof (StringCollection), null,
							       PropertyHelper.CommaDelimitedStringCollectionConverter,
							       PropertyHelper.DefaultValidator,
							       ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (rolesProp);
			properties.Add (usersProp);
			properties.Add (verbsProp);
		}

		public AuthorizationRule (AuthorizationRuleAction action)
		{
			this.action = action;
			base[rolesProp] = new CommaDelimitedStringCollection ();
			base[usersProp] = new CommaDelimitedStringCollection ();
			base[verbsProp] = new CommaDelimitedStringCollection ();
		}

		public override bool Equals (object obj)
		{
			AuthorizationRule auth = obj as AuthorizationRule;
			if (auth == null)
				return false;

			if (action != auth.Action)
				return false;

			if (Roles.Count != auth.Roles.Count
			    || Users.Count != auth.Users.Count
			    || Verbs.Count != auth.Verbs.Count)
				return false;

			int i;

			for (i = 0; i < Roles.Count; i ++)
				if (Roles[i] != auth.Roles[i])
					return false;

			for (i = 0; i < Users.Count; i ++)
				if (Users[i] != auth.Users[i])
					return false;

			for (i = 0; i < Verbs.Count; i ++)
				if (Verbs[i] != auth.Verbs[i])
					return false;
				
			return true;
		}

		public override int GetHashCode ()
		{
			int hashCode = (int)action;
			int i;

			for (i = 0; i < Roles.Count; i ++)
				hashCode += Roles[i].GetHashCode();

			for (i = 0; i < Users.Count; i ++)
				hashCode += Users[i].GetHashCode();

			for (i = 0; i < Verbs.Count; i ++)
				hashCode += Verbs[i].GetHashCode();

			return hashCode;
		}

		[MonoTODO ("Not implemented")]
		protected override bool IsModified ()
		{
			if (((CommaDelimitedStringCollection)Roles).IsModified || ((CommaDelimitedStringCollection)Users).IsModified || ((CommaDelimitedStringCollection)Verbs).IsModified)
				return true;

			return false;
		}

		void VerifyData ()
		{
			if (Roles.Count == 0 && Users.Count == 0)
				throw new ConfigurationErrorsException ("You must supply either a list of users or roles when creating an AuthorizationRule");
		}

		protected override void PostDeserialize ()
		{
			base.PostDeserialize();

			VerifyData ();
		}

		protected override void PreSerialize (XmlWriter writer)
		{
			base.PreSerialize (writer);

			VerifyData ();
		}

		protected override void Reset (ConfigurationElement parentElement)
		{
			AuthorizationRule r = (AuthorizationRule)parentElement;
			Action = r.Action;

			base.Reset (parentElement);
		}

		protected override void ResetModified ()
		{
			base.ResetModified ();
		}

		protected override bool SerializeElement (XmlWriter writer, bool serializeCollectionKey)
		{
			if (saveMode != ConfigurationSaveMode.Full && !IsModified ())
				return true;
			
			PreSerialize (writer);

			writer.WriteStartElement (action == AuthorizationRuleAction.Allow ? "allow" : "deny");
			if (Roles.Count > 0)
				writer.WriteAttributeString ("roles", Roles.ToString());
			if (Users.Count > 0)
				writer.WriteAttributeString ("users", Users.ToString());
			if (Verbs.Count > 0)
				writer.WriteAttributeString ("verbs", Verbs.ToString());

			writer.WriteEndElement ();

			return true;
		}

		protected override void SetReadOnly ()
		{
			base.SetReadOnly();
		}

		protected override void Unmerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
		{
			base.Unmerge (sourceElement, parentElement, saveMode);
			this.saveMode = saveMode;
		}

		public AuthorizationRuleAction Action {
			get { return action; }
			set { action = value; }
		}

		[TypeConverter (typeof (CommaDelimitedStringCollectionConverter))]
		[ConfigurationProperty ("roles")]
		public StringCollection Roles {
			get { return (StringCollection) base [rolesProp];}
		}

		[TypeConverter (typeof (CommaDelimitedStringCollectionConverter))]
		[ConfigurationProperty ("users")]
		public StringCollection Users {
			get { return (StringCollection) base [usersProp];}
		}

		[TypeConverter (typeof (CommaDelimitedStringCollectionConverter))]
		[ConfigurationProperty ("verbs")]
		public StringCollection Verbs {
			get { return (StringCollection) base [verbsProp];}
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


		internal bool CheckVerb (string verb)
		{
			foreach (string v in Verbs) {
				if (String.Compare (v, verb, true, Helpers.InvariantCulture) == 0)
					return true;
			}
			return false;
		}

		internal bool CheckUser (string user)
		{
			foreach (string u in Users) {
				if (String.Compare (u, user, true, Helpers.InvariantCulture) == 0 ||
				    u == "*" ||
				    (u == "?" && user == ""))
					return true;
			}
			return false;
		}

		internal bool CheckRole (IPrincipal user)
		{
			foreach (string r in Roles) {
				if (user.IsInRole (r))
					return true;
			}
			return false;
		}

	}

}

#endif

