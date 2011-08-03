//
// System.Web.Configuration.ProfilePropertySettingsCollection
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

using System;
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (ProfilePropertySettings), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class ProfilePropertySettingsCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static ProfilePropertySettingsCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}
		
		public void Add (ProfilePropertySettings propertySettings)
		{
			BaseAdd (propertySettings);
		}

		public void Clear ()
		{
			BaseClear ();
		}
		
		protected override ConfigurationElement CreateNewElement ()
		{
			return new ProfilePropertySettings ();
		}

		public ProfilePropertySettings Get (int index)
		{
			return (ProfilePropertySettings) BaseGet (index);
		}

		public ProfilePropertySettings Get (string name)
		{
			return (ProfilePropertySettings) BaseGet (name);
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ProfilePropertySettings)element).Name;
		}

		protected override bool OnDeserializeUnrecognizedElement (string elementName, XmlReader reader) 
		{
			/* Disabled: pending investigation
			 *
			if (elementName == "clear" || elementName == "group") {
				throw new ConfigurationErrorsException (String.Format ("{0} is not permitted here", elementName), reader);
			}
			*/
			
			return base.OnDeserializeUnrecognizedElement (elementName, reader);
		}

		public string GetKey (int index)
		{
			ProfilePropertySettings s = Get (index);
			if (s == null)
				return null;

			return s.Name;
		}

		public int IndexOf (ProfilePropertySettings propertySettings)
		{
			return BaseIndexOf (propertySettings);
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		public void Set (ProfilePropertySettings propertySettings)
		{
			ProfilePropertySettings existing = Get (propertySettings.Name);

			if (existing == null) {
				Add (propertySettings);
			}
			else {
				int index = BaseIndexOf (existing);
				RemoveAt (index);
				BaseAdd (index, propertySettings);
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

		protected virtual bool AllowClear {
			get {
				return false;
			}
		}

		public ProfilePropertySettings this[int index] {
			get { return Get (index); }
			set { if (Get (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}
		
		public new ProfilePropertySettings this [string name] {
			get { return (ProfilePropertySettings) base.BaseGet (name); }
		}
		
		protected override bool ThrowOnDuplicate {
			get {
				return true;
			}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
