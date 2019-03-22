//
// System.Configuration.IgnoreSection.cs
//
// Authors:
//	Lluis Sanchez (lluis@novell.com)
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
using System.Collections.Specialized;
using System.Xml;
using System.IO;

namespace System.Configuration {

	public sealed class IgnoreSection : ConfigurationSection
	{
		string xml;

		static ConfigurationPropertyCollection properties;

		static IgnoreSection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public IgnoreSection ()
		{
		}

		protected internal override  bool IsModified ()
		{
			return false;
		}

		protected override void DeserializeRawXml (XmlReader rawXmlReader)
		{
			xml = rawXmlReader.ReadOuterXml ();
		}

		[MonoTODO]
		protected internal override void Reset (ConfigurationElement parentSection)
		{
			base.Reset (parentSection);
		}

		[MonoTODO]
		protected internal override void ResetModified ()
		{
			base.ResetModified ();
		}

		protected internal override string SerializeSection (ConfigurationElement parentSection, string name, ConfigurationSaveMode saveMode)
		{
			return xml;
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}
