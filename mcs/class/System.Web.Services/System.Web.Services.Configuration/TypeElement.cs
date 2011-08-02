//
// System.Web.Services.Configuration.TypeElement
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.ComponentModel;

#if NET_2_0

namespace System.Web.Services.Configuration {

	public sealed class TypeElement : ConfigurationElement
	{
		static ConfigurationProperty typeProp;
		static ConfigurationPropertyCollection properties;

		static TypeElement ()
		{
			typeProp = new ConfigurationProperty ("type", typeof (Type), null, new TypeTypeConverter(),
												  null, ConfigurationPropertyOptions.IsKey);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (typeProp);

		}

		public TypeElement (Type type)
		{
			this.Type = type;
		}

		[MonoTODO]
		public TypeElement (string type)
			: this (Type.GetType (type))
		{
		}

		public TypeElement ()
		{
		}

		[TypeConverter (typeof (TypeTypeConverter))]
		[ConfigurationProperty ("type", Options = ConfigurationPropertyOptions.IsKey)]
		public Type Type {
			get { return (Type) base [typeProp];}
			set { base[typeProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

