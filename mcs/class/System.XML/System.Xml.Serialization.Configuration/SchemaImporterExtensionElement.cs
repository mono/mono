//
// SchemaImporterExtensionElement.cs
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
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Text;

namespace System.Xml.Serialization.Configuration
{
	public sealed class SchemaImporterExtensionElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty name, type;

		static SchemaImporterExtensionElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			name = new ConfigurationProperty ("name", typeof (string), "", ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
			type = new ConfigurationProperty ("type", typeof (Type), null, ConfigurationPropertyOptions.IsRequired);
			properties.Add (name);
			properties.Add (type);
		}

		public SchemaImporterExtensionElement ()
		{
		}

		public SchemaImporterExtensionElement (string name, string type)
		{
			Name = name;
			Type = Type.GetType (type);
		}

		public SchemaImporterExtensionElement (string name, Type type)
		{
			Name = name;
			Type = type;
		}

		// huh? default value Type ?
		[ConfigurationProperty ("name", DefaultValue = typeof (object), Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
		public string Name {
			get { return (string) this [name]; }
			set { this [name] = value; }
		}

		[ConfigurationProperty ("type", DefaultValue = typeof (object), Options = ConfigurationPropertyOptions.IsRequired)]
		[TypeConverter (typeof (TypeConverter))]
		public Type Type {
			get { return (Type) this [type]; }
			set { this [type] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
