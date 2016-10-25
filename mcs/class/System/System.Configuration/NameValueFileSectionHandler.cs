//
// System.Configuration.NameValueFileSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
using System.IO;
#if (XML_DEP)
using System.Xml;
#else
using XmlNode = System.Object;
#endif

#pragma warning disable 618

namespace System.Configuration
{
	public class NameValueFileSectionHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object configContext, XmlNode section)
		{
#if (XML_DEP)
			XmlNode file = null;
			if (section.Attributes != null)
				file = section.Attributes.RemoveNamedItem ("file");

			NameValueCollection pairs = ConfigHelper.GetNameValueCollection (
									parent as NameValueCollection,
									section,
									"key",
									"value");

			if (file != null && file.Value != String.Empty) {
				string fileName = ((IConfigXmlNode) section).Filename;
				fileName = Path.GetFullPath (fileName);
				string fullPath = Path.Combine (Path.GetDirectoryName (fileName), file.Value);
				if (!File.Exists (fullPath))
					return pairs;

				ConfigXmlDocument doc = new ConfigXmlDocument ();
				doc.Load (fullPath);
				if (doc.DocumentElement.Name != section.Name)
					throw new ConfigurationException ("Invalid root element", doc.DocumentElement);

				pairs = ConfigHelper.GetNameValueCollection (pairs, doc.DocumentElement,
									     "key", "value");
			}

			return pairs;
#else
			return null;
#endif			
		}
	}
}

