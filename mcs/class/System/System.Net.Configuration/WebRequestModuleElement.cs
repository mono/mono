//
// System.Net.Configuration.WebRequestModuleElement.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) Tim Coleman, 2004
// (C) 2004,2005 Novell, Inc. (http://www.novell.com)
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

#if CONFIGURATION_DEP

using System;
using System.ComponentModel;
using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class WebRequestModuleElement : ConfigurationElement
	{
		#region Fields

		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty prefixProp;
		static ConfigurationProperty typeProp;

		#endregion // Fields

		#region Constructors

		static WebRequestModuleElement ()
		{
			prefixProp = new ConfigurationProperty ("prefix", typeof (string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
			typeProp = new ConfigurationProperty ("type", typeof (string));
			properties = new ConfigurationPropertyCollection ();

			properties.Add (prefixProp);
			properties.Add (typeProp);
		}

		public WebRequestModuleElement ()
		{
		}

		public WebRequestModuleElement (string prefix, string type)
		{
			base[typeProp] = type;
			Prefix = prefix;
		}

		public WebRequestModuleElement (string prefix, Type type)
			: this (prefix, type.FullName)
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("prefix", Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
		public string Prefix {
			get { return (string) base [prefixProp]; }
			set { base [prefixProp] = value; }
		}

		[ConfigurationProperty ("type")]
		[TypeConverter (typeof (TypeConverter))]
		public Type Type {
			get { return Type.GetType ((string) base [typeProp]); }
			set { base [typeProp] = value.FullName; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties
	}
}

#endif
