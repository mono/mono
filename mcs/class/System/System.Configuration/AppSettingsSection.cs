//
// System.Configuration.AppSettingsSection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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

#if NET_2_0 && XML_DEP
#if XML_DEP
using System;
using System.Collections.Specialized;
using System.Xml;

namespace System.Configuration {

	public sealed class AppSettingsSection : ConfigurationSection
	{
		public AppSettingsSection ()
		{
		}

		[MonoTODO]
		protected internal override  bool IsModified ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void ReadXml (XmlReader reader, object context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void Reset (ConfigurationElement parent_section, object context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void ResetModified ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override string WriteXml (
			ConfigurationElement parent, object context, string name, ConfigurationUpdateMode mode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string File {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public NameValueCollection Settings {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif
#endif
