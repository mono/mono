//
// System.Web.Configuration.TagPrefixInfo
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web.UI;
using System.Xml;

namespace System.Web.Configuration
{
	public sealed class TagPrefixInfo : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty tagPrefixProp;
		static ConfigurationProperty namespaceProp;
		static ConfigurationProperty assemblyProp;
		static ConfigurationProperty tagNameProp;
		static ConfigurationProperty sourceProp;

		static ConfigurationElementProperty elementProperty;

		static TagPrefixInfo ()
		{
			tagPrefixProp = new ConfigurationProperty ("tagPrefix", typeof (string), "/",
								   TypeDescriptor.GetConverter (typeof (string)),
								   PropertyHelper.NonEmptyStringValidator,
								   ConfigurationPropertyOptions.IsRequired);
			namespaceProp = new ConfigurationProperty ("namespace", typeof (string));
			assemblyProp = new ConfigurationProperty ("assembly", typeof (string));
			tagNameProp = new ConfigurationProperty ("tagName", typeof (string));
			sourceProp = new ConfigurationProperty ("src", typeof (string));

			properties = new ConfigurationPropertyCollection ();
			properties.Add (tagPrefixProp);
			properties.Add (namespaceProp);
			properties.Add (assemblyProp);
			properties.Add (tagNameProp);
			properties.Add (sourceProp);

			elementProperty = new ConfigurationElementProperty (new CallbackValidator (typeof (TagPrefixInfo), ValidateElement));
		}

		internal TagPrefixInfo ()
		{
		}

		public TagPrefixInfo (string tagPrefix, string nameSpace, string assembly, string tagName, string source)
		{
			this.TagPrefix = tagPrefix;
			this.Namespace = nameSpace;
			this.Assembly = assembly;
			this.TagName = tagName;
			this.Source = source;
		}

		static void ValidateElement (object o)
		{
			/* XXX do some sort of element validation here? */
		}

		protected internal override ConfigurationElementProperty ElementProperty {
			get { return elementProperty; }
		}

		public override bool Equals (object prefix)
		{
			TagPrefixInfo info = prefix as TagPrefixInfo;
			if (info == null)
				return false;

			return (Namespace == info.Namespace
				&& Source == info.Source
				&& TagName == info.TagName
				&& TagPrefix == info.TagPrefix);
		}

		public override int GetHashCode ()
		{
			return Namespace.GetHashCode() + Source.GetHashCode() + TagName.GetHashCode() + TagPrefix.GetHashCode();
		}

		[ConfigurationProperty ("assembly")]
		public string Assembly {
			get { return (string) base[assemblyProp]; }
			set { base[assemblyProp] = value; }
		}

		[ConfigurationProperty ("namespace")]
		public string Namespace {
			get { return (string) base[namespaceProp]; }
			set { base[namespaceProp] = value; }
		}

		[ConfigurationProperty ("src")]
		public string Source {
			get { return (string) base[sourceProp]; }
			set { base[sourceProp] = value; }
		}

		[ConfigurationProperty ("tagName")]
		public string TagName {
			get { return (string) base[tagNameProp]; }
			set { base[tagNameProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("tagPrefix", DefaultValue = "/", Options = ConfigurationPropertyOptions.IsRequired)]
		public string TagPrefix {
			get { return (string) base[tagPrefixProp]; }
			set { base[tagPrefixProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
