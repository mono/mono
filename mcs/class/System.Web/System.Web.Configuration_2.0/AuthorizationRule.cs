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
using System.Configuration;
using System.ComponentModel;
using System.Xml;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class AuthorizationRule : ConfigurationElement
	{
		static ConfigurationProperty rolesProp;
		static ConfigurationProperty usersProp;
		static ConfigurationProperty verbsProp;
		static ConfigurationPropertyCollection properties;

		AuthorizationRuleAction action;

		static AuthorizationRule ()
		{
			rolesProp = new ConfigurationProperty ("roles", typeof (StringCollection));
			usersProp = new ConfigurationProperty ("users", typeof (StringCollection));
			verbsProp = new ConfigurationProperty ("verbs", typeof (StringCollection));
			properties = new ConfigurationPropertyCollection ();

			properties.Add (rolesProp);
			properties.Add (usersProp);
			properties.Add (verbsProp);
		}

		public AuthorizationRule (AuthorizationRuleAction action)
		{
			this.action = action;
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			return base.Equals (obj);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		[MonoTODO]
		protected override bool IsModified ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void PostDeserialize ()
		{
			base.PostDeserialize();
		}

		[MonoTODO]
		protected override void PreSerialize (XmlWriter writer)
		{
			base.PreSerialize (writer);
		}

		[MonoTODO]
		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
		}

		[MonoTODO]
		protected override void ResetModified ()
		{
			base.ResetModified ();
		}

		[MonoTODO]
		protected override bool SerializeElement (XmlWriter writer, bool serializeCollectionKey)
		{
			return base.SerializeElement (writer, serializeCollectionKey);
		}

		[MonoTODO]
		protected override void SetReadOnly ()
		{
			base.SetReadOnly();
		}

		[MonoTODO]
		protected override void Unmerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
		{
			base.Unmerge (sourceElement, parentElement, saveMode);
		}

		public AuthorizationRuleAction Action {
			get { return action; }
			set { action = value; }
		}

#if notyet
		[TypeConverter (typeof (CommaDelimitedStringCollectionConverter))]
#endif
		[ConfigurationProperty ("roles")]
		public StringCollection Roles {
			get { return (StringCollection) base [rolesProp];}
		}

#if notyet
		[TypeConverter (typeof (CommaDelimitedStringCollectionConverter))]
#endif
		[ConfigurationProperty ("users")]
		public StringCollection Users {
			get { return (StringCollection) base [usersProp];}
		}

#if notyet
		[TypeConverter (typeof (CommaDelimitedStringCollectionConverter))]
#endif
		[ConfigurationProperty ("verbs")]
		public StringCollection Verbs {
			get { return (StringCollection) base [verbsProp];}
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

