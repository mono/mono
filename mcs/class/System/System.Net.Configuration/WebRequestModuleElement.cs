//
// System.Net.Configuration.WebRequestModuleElement.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004 Novell, Inc. (http://www.novell.com)
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

#if NET_2_0 && XML_DEP

using System;
using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class WebRequestModuleElement : ConfigurationElement
	{
		#region Fields

		ConfigurationPropertyCollection properties;
		static ConfigurationProperty prefix = new ConfigurationProperty ("Prefix", typeof (string), null);
		static ConfigurationProperty type = new ConfigurationProperty ("Type", typeof (string), null);

		#endregion // Fields

		#region Constructors

		public WebRequestModuleElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			properties.Add (prefix);
			properties.Add (type);
		}

		public WebRequestModuleElement (string prefix, string type)
		{
			base [type] = type;
			Prefix = prefix;
		}

		public WebRequestModuleElement (string prefix, Type type)
			: this (prefix, type.FullName)
		{
		}

		#endregion // Constructors

		#region Properties

		public string Prefix {
			get { return (string) base [prefix]; }
			set { base [prefix] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public Type Type {
			get { return Type.GetType ((string) base [type]); }
			set { base [type] = value.FullName; }
		}

		#endregion // Properties
	}
}

#endif
