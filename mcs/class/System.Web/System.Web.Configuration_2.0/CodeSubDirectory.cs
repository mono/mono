//
// System.Web.Configuration.CodeSubDirectory
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.ComponentModel;

namespace System.Web.Configuration
{
	public sealed class CodeSubDirectory : ConfigurationElement
	{
		static ConfigurationProperty directoryNameProp;
		static ConfigurationPropertyCollection properties;

		static CodeSubDirectory ()
		{
			directoryNameProp = new ConfigurationProperty ("directoryName", typeof (string), "",
								       PropertyHelper.WhiteSpaceTrimStringConverter,
								       PropertyHelper.NonEmptyStringValidator,
								       ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (directoryNameProp);
		}

		public CodeSubDirectory (string directoryName)
		{
			this.DirectoryName = directoryName;
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[ConfigurationProperty ("directoryName", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		// LAMESPEC: MS lists no validator here but provides one in Properties.
		public string DirectoryName {
			get { return (string) base[directoryNameProp]; }
			set { base[directoryNameProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}
#endif // NET_2_0

