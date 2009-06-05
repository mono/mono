//
// System.Configuration.IgnoreSectionHandler.cs
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
#if (XML_DEP)
using System.Xml;
#else
using XmlNode = System.Object;
#endif

namespace System.Configuration
{
	/// <summary>
	/// Summary description for IgnoreSectionHandler.
	/// </summary>
	public class IgnoreSectionHandler : IConfigurationSectionHandler
	{
		/// <summary>
		///		Creates a new configuration handler and adds the specified configuration object to the collection.
		/// </summary>
		/// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
		/// <param name="configContext">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
		/// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
		/// <returns></returns>
		public virtual object Create(object parent, object configContext, XmlNode section)
		{
			return null;
		}
	}
}
