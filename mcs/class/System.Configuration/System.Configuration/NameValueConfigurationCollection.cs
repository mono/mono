//
// System.Configuration.NameValueConfigurationCollection.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace System.Configuration {

	[ConfigurationCollectionAttribute (typeof (NameValueConfigurationElement),
					   AddItemName = "add",
					   RemoveItemName = "remove",
					   ClearItemsName = "clear",
					   CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class NameValueConfigurationCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static NameValueConfigurationCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public NameValueConfigurationCollection ()
		{
		}

		public string[] AllKeys {
			get {
				return (string[])BaseGetAllKeys ();
			}
		}

		public new NameValueConfigurationElement this [ string name ] {
			get {
				return (NameValueConfigurationElement)BaseGet (name);
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get {
				return properties;
			}
		}

		public void Add (NameValueConfigurationElement nameValue)
		{
			BaseAdd (nameValue, false);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new NameValueConfigurationElement ("", "");
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			NameValueConfigurationElement e = (NameValueConfigurationElement)element;
			return e.Name;
		}

		public void Remove (NameValueConfigurationElement nameValue)
		{
			throw new NotImplementedException ();
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}
	}
}

