//
// UriSection.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (c) 2009 Novell, Inc. (http://www.novell.com)
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

using System.Configuration;

namespace System.Configuration 
{
	public sealed class UriSection : ConfigurationSection
	{
		#region Fields

		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty idn_prop;
		static ConfigurationProperty iriParsing_prop;

		#endregion // Fields

		#region Constructors

		static UriSection ()
		{
			idn_prop = new ConfigurationProperty ("idn", typeof (IdnElement), null);
			iriParsing_prop = new ConfigurationProperty ( "iriParsing", typeof (IriParsingElement), null);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (idn_prop);
			properties.Add (iriParsing_prop);
		}

		public UriSection ()
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("idn")]
		public IdnElement Idn {
			get { return (IdnElement) base [idn_prop]; }
		}

		[ConfigurationProperty ("iriParsing")]
		public IriParsingElement IriParsing {
			get { return (IriParsingElement) base [iriParsing_prop]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties
	}
}
#endif
