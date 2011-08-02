//
// System.Web.Configuration.NamespaceInfo
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
	public sealed class NamespaceInfo : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty namespaceProp;

		static NamespaceInfo ()
		{
			namespaceProp = new ConfigurationProperty ("namespace", typeof (string), null,
								   TypeDescriptor.GetConverter (typeof (string)),
								   PropertyHelper.NonEmptyStringValidator,
								   ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (namespaceProp);
		}

		public NamespaceInfo (string name)
		{
			Namespace = name;
		}

		public override bool Equals (object namespaceInformation)
		{
			NamespaceInfo info = namespaceInformation as NamespaceInfo;
			if (info == null)
				return false;

			return (Namespace == info.Namespace);
		}

		public override int GetHashCode ()
		{
			return Namespace.GetHashCode ();
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("namespace", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Namespace {
			get { return (string) base[namespaceProp]; }
			set { base[namespaceProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}
}

#endif
