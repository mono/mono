//
// SchemaImporterExtensionsSection.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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

#if NET_2_0 && CONFIGURATION_DEP
using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Text;

namespace System.Xml.Serialization.Configuration
{
	public sealed class SchemaImporterExtensionsSection : ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty extensions;

		static SchemaImporterExtensionsSection ()
		{
			properties = new ConfigurationPropertyCollection ();
			extensions = new ConfigurationProperty ("", typeof (SchemaImporterExtensionElement), null, ConfigurationPropertyOptions.IsDefaultCollection);
			properties.Add (extensions);
		}

		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public SchemaImporterExtensionElementCollection SchemaImporterExtensions {
			get { return (SchemaImporterExtensionElementCollection) this [extensions]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		protected override void InitializeDefault ()
		{
			// not sure what is expected here.
			// Configuration would work without it.
			base.InitializeDefault ();
		}
	}
}

#endif
