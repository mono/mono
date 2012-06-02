//
// System.Configuration.IriParsingElement.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (c) 2009 Novell, Inc (http://www.novell.com)
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

namespace System.Configuration
{
	public sealed class IriParsingElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty enabled_prop;

		static IriParsingElement ()
		{
			enabled_prop = new ConfigurationProperty ("enabled", typeof (bool), false, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (enabled_prop);
		}

		public IriParsingElement ()
		{
		}

		[ConfigurationProperty ("enabled", DefaultValue = false,
					Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public bool Enabled {
			get { return (bool) base [enabled_prop]; }
			set { base [enabled_prop] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public override bool Equals (object o)
		{
			IriParsingElement e = o as IriParsingElement;
			if (e == null)
				return false;

			return e.Enabled == Enabled;
		}

		public override int GetHashCode ()
		{
			return Convert.ToInt32 (Enabled) ^ 0x7F;
		}
	}

}

#endif

