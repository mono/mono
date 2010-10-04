//
// System.Web.Configuration.AuthorizationRuleCollection
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
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	[ConfigurationCollection (typeof (AuthorizationRule), AddItemName="allow,deny", CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
	public sealed class AuthorizationRuleCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static AuthorizationRuleCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public void Add (AuthorizationRule rule)
		{
			BaseAdd (rule, false);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement (string elementName)
		{
			return new AuthorizationRule (elementName == "allow" ? AuthorizationRuleAction.Allow : AuthorizationRuleAction.Deny);
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new AuthorizationRule (AuthorizationRuleAction.Allow);
		}

		public AuthorizationRule Get (int index)
		{
			return (AuthorizationRule) BaseGet (index);
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			AuthorizationRule rule = (AuthorizationRule)element;

			return rule.Action.ToString();
		}

		public int IndexOf (AuthorizationRule rule)
		{
			return BaseIndexOf (rule);
		}

		protected override bool IsElementName (string elementname)
		{
			return (elementname == "allow" || elementname == "deny");
		}

		public void Remove (AuthorizationRule rule)
		{
			BaseRemove (rule.Action.ToString());
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		public void Set (int index, AuthorizationRule rule)
		{
			if (BaseGet(index) != null)
				BaseRemoveAt(index);
			BaseAdd(index, rule);
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.BasicMapAlternate; }
		}

		protected override string ElementName {
			get { return String.Empty; }
		}

		public AuthorizationRule this [int index] {
			get { return Get (index); }
			set { Set (index, value); }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}

}

#endif

