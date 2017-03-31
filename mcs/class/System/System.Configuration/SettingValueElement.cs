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
#if XML_DEP
		XmlNode node;

#if (CONFIGURATION_DEP)
		XmlNode original;
#endif

#endif

		[MonoTODO]
		public SettingValueElement ()
		{
		}

#if (CONFIGURATION_DEP)
		[MonoTODO]
		protected override ConfigurationPropertyCollection Properties {
			get {
				return base.Properties;
			}
		}
#endif

#if (XML_DEP)
		public XmlNode ValueXml {
			get { return node; }
			set { node = value; }
		}

#if (CONFIGURATION_DEP)
		[MonoTODO]
		protected override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			original = new XmlDocument ().ReadNode (reader);
			node = original.CloneNode (true);
		}
#endif
#endif

		public override bool Equals (object settingValue)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

#if (CONFIGURATION_DEP)
		protected override bool IsModified ()
		{
			return original != node;
		}

		protected override void Reset (ConfigurationElement parentElement)
		{
			node = null;
		}

		protected override void ResetModified ()
		{
			node = original;
		}
#endif

#if (XML_DEP) && (CONFIGURATION_DEP)
		protected override bool SerializeToXmlElement (XmlWriter writer, string elementName)
		{
			if (node == null)
				return false;
			node.WriteTo (writer);
			return true;
		}
#endif

#if (CONFIGURATION_DEP)
		protected override void Unmerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
		{
			if (parentElement != null && sourceElement.GetType() != parentElement.GetType())
				throw new ConfigurationErrorsException ("Can't unmerge two elements of different type");

			bool isMinimalOrModified = saveMode == ConfigurationSaveMode.Minimal ||
				saveMode == ConfigurationSaveMode.Modified;

			foreach (PropertyInformation prop in sourceElement.ElementInformation.Properties)
			{
				if (prop.ValueOrigin == PropertyValueOrigin.Default)
					continue;
				
				PropertyInformation unmergedProp = ElementInformation.Properties [prop.Name];
				
				object sourceValue = prop.Value;
				if (parentElement == null || !HasValue (parentElement, prop.Name)) {
					unmergedProp.Value = sourceValue;
					continue;
				}

				if (sourceValue == null)
					continue;

				object parentValue = GetItem (parentElement, prop.Name);
				if (!PropertyIsElement (prop)) {
					if (!object.Equals (sourceValue, parentValue) || 
					    (saveMode == ConfigurationSaveMode.Full) ||
					    (saveMode == ConfigurationSaveMode.Modified && prop.ValueOrigin == PropertyValueOrigin.SetHere))
						unmergedProp.Value = sourceValue;
					continue;
				}

				var sourceElem = (ConfigurationElement) sourceValue;
				if (isMinimalOrModified && !ElementIsModified (sourceElem))
					continue;
				if (parentValue == null) {
					unmergedProp.Value = sourceValue;
					continue;
				}

				var parentElem = (ConfigurationElement) parentValue;
				ConfigurationElement copy = (ConfigurationElement) unmergedProp.Value;
				ElementUnmerge (copy, sourceElem, parentElem, saveMode);
			}
		}

		bool HasValue (ConfigurationElement element, string propName)
		{
			PropertyInformation info = element.ElementInformation.Properties [propName];
			return info != null && info.ValueOrigin != PropertyValueOrigin.Default;
		}

		object GetItem (ConfigurationElement element, string property)
		{
			PropertyInformation pi = ElementInformation.Properties [property];
			if (pi == null)
				throw new InvalidOperationException ("Property '" + property + "' not found in configuration element");

			return pi.Value;
		}
		
		bool PropertyIsElement (PropertyInformation prop)
		{
			return (typeof(ConfigurationElement).IsAssignableFrom (prop.Type));
		}
		
		bool ElementIsModified (ConfigurationElement element)
		{
			return (bool) element.GetType ().GetMethod ("IsModified", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (element, new object [0]);
		}
		
		void ElementUnmerge (ConfigurationElement target, ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
		{
			target.GetType ().GetMethod ("Unmerge", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (target, new object [] {sourceElement, parentElement, saveMode});
		}
#endif
	}

}

