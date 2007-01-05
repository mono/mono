//
// System.Web.Services.Configuration.SoapEnvelopeProcessingElement
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

	public sealed class SoapEnvelopeProcessingElement : ConfigurationElement
	{
		static ConfigurationProperty strictProp;
		static ConfigurationProperty readTimeoutProp;
		static ConfigurationPropertyCollection properties;

		static SoapEnvelopeProcessingElement ()
		{
			strictProp = new ConfigurationProperty ("strict", typeof (bool), false);
			readTimeoutProp = new ConfigurationProperty ("readTimeout", typeof (int), Int32.MaxValue,
								     new InfiniteIntConverter(), null,
								     ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (strictProp);
			properties.Add (readTimeoutProp);

		}

		public SoapEnvelopeProcessingElement (int readTimeout, bool strict)
		{
			ReadTimeout = readTimeout;
			IsStrict = strict;
		}

		public SoapEnvelopeProcessingElement (int readTimeout)
		{
			ReadTimeout = readTimeout;
		}

		public SoapEnvelopeProcessingElement ()
		{
		}

		[ConfigurationProperty ("strict", DefaultValue = false)]
		public bool IsStrict {
			get { return (bool) base [strictProp];}
			set { base[strictProp] = value; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[ConfigurationProperty ("readTimeout", DefaultValue = int.MaxValue)]
		public int ReadTimeout {
			get { return (int) base [readTimeoutProp];}
			set { base[readTimeoutProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

