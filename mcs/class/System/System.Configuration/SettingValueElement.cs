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

using System;
using System.Reflection;
#if (XML_DEP)
using System.Xml;
#endif

namespace System.Configuration
{
    public sealed class SettingValueElement
#if (CONFIGURATION_DEP)
		: ConfigurationElement
#endif
    {

#if (CONFIGURATION_DEP)
        private static volatile ConfigurationPropertyCollection _properties;
#endif

#if XML_DEP
        private static XmlDocument _document = new XmlDocument();

        private XmlNode _valueXml;
#endif
        private bool _isModified = false;

#if (CONFIGURATION_DEP)
        protected override ConfigurationPropertyCollection Properties
        {
            get {
                if (_properties == null) {
                    _properties = new ConfigurationPropertyCollection();
                }

                return _properties;
            }
        }
#endif

#if XML_DEP
        public XmlNode ValueXml
        {
            get { return _valueXml; }
            set { 
				_valueXml = value;
				_isModified = true;
            }
        }

#if (CONFIGURATION_DEP)
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            ValueXml = _document.ReadNode(reader);
        }
#endif
#endif

        public override bool Equals(object settingValue)
        {
            SettingValueElement u = settingValue as SettingValueElement;
            return (u != null && Equals(u.ValueXml, ValueXml));
        }

        public override int GetHashCode()
        {
            return ValueXml?.GetHashCode() ?? 0;
        }

#if (CONFIGURATION_DEP)
        protected override bool IsModified()
        {
            return _isModified;
        }

        protected override void ResetModified()
        {
            _isModified = false;
        }

		protected override void Reset(ConfigurationElement parentElement)
        {
            base.Reset(parentElement);
            ValueXml = ((SettingValueElement)parentElement).ValueXml;
        }
#endif

#if (XML_DEP) && (CONFIGURATION_DEP)
        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            if (ValueXml != null) {
                if (writer != null) {
                    ValueXml?.WriteTo(writer);
                }
                return true;
            }

            return false;
        }
#endif
        
#if (CONFIGURATION_DEP)
        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement,
                                                ConfigurationSaveMode saveMode)
        {
            base.Unmerge(sourceElement, parentElement, saveMode);
            ValueXml = ((SettingValueElement)sourceElement).ValueXml;
        }
#endif
    }
}
