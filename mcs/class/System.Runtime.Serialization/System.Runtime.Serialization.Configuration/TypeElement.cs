//
// TypeElement.cs
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
	public sealed class TypeElement : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty index;
		static ConfigurationProperty parameters;
		static ConfigurationProperty type;

		static TypeElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			index = new ConfigurationProperty ("index",
				typeof (int), "", null, null,
				ConfigurationPropertyOptions.None);
			parameters = new ConfigurationProperty ("",
				typeof (ParameterElementCollection), null, null, null,
				ConfigurationPropertyOptions.IsDefaultCollection);
			type = new ConfigurationProperty ("type",
				typeof (string), "", null, null,
				ConfigurationPropertyOptions.None);

			properties.Add (index);
			properties.Add (parameters);
			properties.Add (type);
		}

		public TypeElement ()
		{
		}
		
		public TypeElement (string typeName)
		{
		}

		[ConfigurationProperty ("index", DefaultValue = 0)]
		[IntegerValidator (MinValue = 0)]
		public int Index {
			get { return (int) base [index]; }
			set { base [index] = value; }
		}

		[ConfigurationProperty ("", DefaultValue = null, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public ParameterElementCollection Parameters {
			get { return (ParameterElementCollection) base [parameters]; }
		}

		[ConfigurationProperty ("type", DefaultValue = "")]
		[StringValidator (MinLength = 0)]
		public string Type {
			get { return (string) base [type]; }
			set { base [type] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
		}
	}
}
