//
// System.Web.Configuration.AssemblyCollection
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2004,2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class AssemblyInfo: ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty assemblyProp;
		
		static AssemblyInfo ()
		{
			assemblyProp = new ConfigurationProperty ("assembly", typeof (string), null, TypeDescriptor.GetConverter (typeof (string)),
								  PropertyHelper.NonEmptyStringValidator,
								  ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (assemblyProp);
		}
		
		internal AssemblyInfo ()
		{
		}

		public AssemblyInfo (string assemblyName)
		{
			this.Assembly = assemblyName;
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("assembly", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Assembly {
			get { return (string) base [assemblyProp]; }
			set { base [assemblyProp] = value; }
		}
		
		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

