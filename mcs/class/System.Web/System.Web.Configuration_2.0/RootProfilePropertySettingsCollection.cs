//
// System.Web.Configuration.RootProfilePropertySettingsCollection
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
	public sealed class RootProfilePropertySettingsCollection : ProfilePropertySettingsCollection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty groupSettingsProp;

		static RootProfilePropertySettingsCollection ()
		{
			groupSettingsProp = new ConfigurationProperty ("group", typeof (ProfileGroupSettingsCollection), null);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (groupSettingsProp);
		}

		public RootProfilePropertySettingsCollection ()
		{
		}

		[MonoTODO]
		public override bool Equals (object rootProfilePropertySettingsCollection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool IsModified ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool OnDeserializeUnrecognizedElement (string elementName, XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Reset (ConfigurationElement parentElement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void ResetModified ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool SerializeElement (XmlWriter writer, bool serializeCollectionKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Unmerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool AllowClear {
			get { throw new NotImplementedException (); }
		}

		[ConfigurationProperty ("group")]
		public ProfileGroupSettingsCollection GroupSettings {
			get { return (ProfileGroupSettingsCollection) base [groupSettingsProp]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[MonoTODO]
		protected override bool ThrowOnDuplicate {
			get { throw new NotImplementedException (); }
		}
	}

}

#endif
