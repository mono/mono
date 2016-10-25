//
// System.Configuration.SingleTagSectionHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
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
using System.Collections;
#if (XML_DEP)
using System.Xml;
#else
using XmlNode = System.Object;
#endif

#pragma warning disable 618

namespace System.Configuration
{
	/// <summary>
	/// Summary description for SingleTagSectionHandler.
	/// </summary>
	public class SingleTagSectionHandler : IConfigurationSectionHandler
	{
		/// <summary>
		///		Returns a collection of configuration section values.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="context"></param>
		/// <param name="section">The name of the configuration section.</param>
		/// <returns></returns>
		public virtual object Create(object parent, object context, XmlNode section)
		{
			Hashtable settingsCollection;
			
			if (parent == null)
				settingsCollection = new Hashtable ();
			else
				settingsCollection = (Hashtable) parent;

#if (XML_DEP)
			//Get all of the ChildNodes in the XML section.
			if(section.HasChildNodes)
			{
				throw (new ConfigurationException("Child Nodes not allowed."));
			}
			
			
			//Get the attributes for the childNode
			XmlAttributeCollection xmlAttributes = section.Attributes;

			for(int i=0; i < xmlAttributes.Count; i++)
			{
				settingsCollection.Add(xmlAttributes[i].Name, xmlAttributes[i].Value);
			}
#endif			
			
			return settingsCollection;
		}
	}
}
