//
// System.Web.Configuration.BuildProvider
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Chris Toshok (toshok@ximian.com)
//
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.ComponentModel;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class BuildProvider : ConfigurationElement {

		static ConfigurationProperty extensionProp;
		static ConfigurationProperty typeProp;
		static ConfigurationPropertyCollection properties;

		static BuildProvider ()
		{
			extensionProp = new ConfigurationProperty ("extension", typeof (string), "",
								   TypeDescriptor.GetConverter (typeof (string)), PropertyHelper.NonEmptyStringValidator,
								   ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			typeProp = new ConfigurationProperty ("type", typeof (string), "",
							      TypeDescriptor.GetConverter (typeof (string)), PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired);
			properties = new ConfigurationPropertyCollection();

			properties.Add (extensionProp);
			properties.Add (typeProp);
		}

		internal BuildProvider ()
		{
		}

		public BuildProvider (string extension, string type)
		{
			this.Extension = extension;
			this.Type = type;
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("extension", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Extension {
			get { return (string) base[extensionProp]; }
			set {
				string ext;

				if (!String.IsNullOrEmpty (value))
					ext = value.ToLowerInvariant ();
				else
					ext = value;
				base [extensionProp] = ext;
			}
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("type", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Type {
			get { return (string) base[typeProp]; }
			set { base[typeProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public override bool Equals (object provider)
		{
			BuildProvider p = provider as BuildProvider;
			if (p == null)
				return false;

			return (Extension == p.Extension && Type == p.Type);
		}

		public override int GetHashCode ()
		{
			return (Extension.GetHashCode () + Type.GetHashCode ());
		}
	}
	
}
#endif // NET_2_0

