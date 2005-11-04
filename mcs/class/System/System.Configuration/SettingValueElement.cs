//
// System.Web.UI.WebControls.SettingValueElement.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
#if (XML_DEP)
using System.Xml;
#endif

namespace System.Configuration
{
	public sealed class SettingValueElement : ConfigurationElement
	{
		[MonoTODO]
		public SettingValueElement ()
		{
		}

		[MonoTODO]
		protected override ConfigurationPropertyCollection Properties {
			get {
				throw new NotImplementedException ();
			}
		}

#if (XML_DEP)
		[MonoTODO]
		public XmlNode ValueXml {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			throw new NotImplementedException ();
		}
#endif

		public override bool Equals (object settingValue)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		protected override bool IsModified ()
		{
			throw new NotImplementedException ();
		}

		protected override void Reset (ConfigurationElement parentElement)
		{
			throw new NotImplementedException ();
		}

		protected override void ResetModified ()
		{
			throw new NotImplementedException ();
		}

#if (XML_DEP)
		protected override bool SerializeToXmlElement (XmlWriter writer, string elementName)
		{
			throw new NotImplementedException ();
		}
#endif

		protected override void Unmerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
		{
			throw new NotImplementedException ();
		}
	}

}

#endif
