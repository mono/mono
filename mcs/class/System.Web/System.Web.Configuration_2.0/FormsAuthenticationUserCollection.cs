//
// System.Web.Configuration.FormsAuthenticationUserCollection
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

#if NET_2_0

using System.Configuration;
using System.ComponentModel;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (FormsAuthenticationUser), AddItemName = "user", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public sealed class FormsAuthenticationUserCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static FormsAuthenticationUserCollection ()
		{
			properties = new ConfigurationPropertyCollection();
		}

		public FormsAuthenticationUserCollection ()
		{
		}

		public void Add (FormsAuthenticationUser user)
		{
			BaseAdd (user);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new FormsAuthenticationUser("", "");
		}

		public FormsAuthenticationUser Get (int index)
		{
			return (FormsAuthenticationUser) BaseGet (index);
		}

		public FormsAuthenticationUser Get (string name)
		{
			return (FormsAuthenticationUser) BaseGet (name);
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((FormsAuthenticationUser)element).Name;
		}

		public string GetKey (int index)
		{
			FormsAuthenticationUser user = Get (index);
			return user.Name;
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		public void Set (FormsAuthenticationUser user)
		{
			FormsAuthenticationUser existing = Get (user.Name);

			if (existing == null) {
				Add (user);
			}
			else {
				int index = BaseIndexOf (existing);
				RemoveAt (index);
				BaseAdd (index, user);
			}
		}

		public string[ ] AllKeys {
			get {
				string[] keys = new string[Count];
				for (int i = 0; i < Count; i ++)
					keys[i] = this[i].Name;
				return keys;
			}
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName {
			get { return "user"; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public FormsAuthenticationUser this[int index] {
			get { return (FormsAuthenticationUser) Get (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}

		public new FormsAuthenticationUser this[string name] {
			get { return (FormsAuthenticationUser) Get (name); }
		}

		protected override bool ThrowOnDuplicate {
			get { return false; }
		}
	}
}

#endif
