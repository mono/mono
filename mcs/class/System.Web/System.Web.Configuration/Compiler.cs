//
// System.Web.Configuration.Compiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Configuration;

namespace System.Web.Configuration
{
#if !NET_2_0
	class Compiler
	{
		public string Language;
		public string Extension;
		public string Type;
		public int WarningLevel;
		public string CompilerOptions;
		public CodeDomProvider Provider;

		public override string ToString ()
		{
			return "Language: " + Language + "\n" +
				"Extension: " + Extension + "\n" +
				"Type: " + Type + "\n" +
				"WarningLevel: " + WarningLevel + "\n" +
				"CompilerOptions: " + CompilerOptions + "\n";
		}
	}
#else // NET_2_0
	public sealed class Compiler : ConfigurationElement
	{
		static ConfigurationPropertyCollection props;
		static ConfigurationProperty compilerOptions;
		static ConfigurationProperty extension;
		static ConfigurationProperty language;
		static ConfigurationProperty type;
		static ConfigurationProperty warningLevel;

		CodeDomProvider provider;

		static Compiler ()
		{
			Type strType = typeof (string);
			compilerOptions = new ConfigurationProperty ("compilerOptions", strType, "", 0);
			extension = new ConfigurationProperty ("extension", strType, "", 0);
			language = new ConfigurationProperty ("language", strType, "", 0);
			ConfigurationPropertyFlags flags = ConfigurationPropertyFlags.Required | ConfigurationPropertyFlags.IsKey;
			type = new ConfigurationProperty ("type", strType, "", flags);
			warningLevel = new ConfigurationProperty ("warningLevel", typeof (int), 0, 0);

			props = new ConfigurationPropertyCollection ();
			props.Add (compilerOptions);
			props.Add (extension);
			props.Add (language);
			props.Add (type);
			props.Add (warningLevel);
		}

		public string CompilerOptions {
			get { return (string) this [compilerOptions]; }
		}

		public string Extension {
			get { return (string) this [extension]; }
		}

		public string Language {
			get { return (string) this [language]; }
		}

		public string Type {
			get { return (string) this [type]; }
		}

		internal CodeDomProvider Provider {
			get { return provider; }
			set { provider = value; }
		}

		public int WarningLevel {
			get { return (int) this [warningLevel]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return props; }
		}
	}
#endif
}

