//
// DeclaredTypeElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005,2010 Novell, Inc.  http://www.novell.com
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
using System.Configuration;

namespace System.Runtime.Serialization.Configuration
{
	[MonoTODO]
	public sealed class DeclaredTypeElement : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty known_types;
		static ConfigurationProperty type;

		static DeclaredTypeElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			known_types = new ConfigurationProperty ("",
				typeof (TypeElementCollection), null, null, null,
				ConfigurationPropertyOptions.IsDefaultCollection);
			type = new ConfigurationProperty ("type",
				typeof (string), "", null, null,
				ConfigurationPropertyOptions.None);

			properties.Add (known_types);
			properties.Add (type);
		}

		public DeclaredTypeElement ()
		{
		}

		public DeclaredTypeElement (string typeName)
		{
			Type = typeName;
		}

		[ConfigurationProperty ("", DefaultValue = null, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public TypeElementCollection KnownTypes {
			get { return (TypeElementCollection) base [known_types]; }
		}

		[ConfigurationProperty ("type", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
		public string Type {
			get { return (string) base [type]; }
			set { base [type] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		protected override void PostDeserialize ()
		{
			// what to do here?
			base.PostDeserialize ();
		}
	}
}
