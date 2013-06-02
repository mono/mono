//
// System.Configuration.DefaultSection
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
using System.Xml;

namespace System.Configuration {

	public sealed class DefaultSection : ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;

		static DefaultSection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		protected internal override void DeserializeSection (XmlReader xmlReader)
		{
			// not sure if it is the right thing to do,
			// but DefaultSection does not raise errors on
			// unrecognized contents.

			// FIXME: it is nothing more than hack: RawXml should
			// not be set more than once.
			if (RawXml == null)
				RawXml = xmlReader.ReadOuterXml ();
			else
				xmlReader.Skip ();
		}

		[MonoTODO]
		protected internal override bool IsModified ()
		{
			return base.IsModified ();
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

		[MonoTODO]
		protected internal override string SerializeSection (ConfigurationElement parentSection, string name, ConfigurationSaveMode saveMode)
		{
			return base.SerializeSection (parentSection, name, saveMode);
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

