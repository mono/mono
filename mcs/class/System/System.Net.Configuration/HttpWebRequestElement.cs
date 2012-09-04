//
// System.Net.Configuration.HttpWebRequestElement.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) Tim Coleman, 2004
// (C) 2004,2005 Novell, Inc. (http://www.novell.com)
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

namespace System.Net.Configuration 
{
	public sealed class HttpWebRequestElement : ConfigurationElement
	{
		#region Fields

		static ConfigurationProperty maximumErrorResponseLengthProp;
		static ConfigurationProperty maximumResponseHeadersLengthProp;
		static ConfigurationProperty maximumUnauthorizedUploadLengthProp;
		static ConfigurationProperty useUnsafeHeaderParsingProp;
		static ConfigurationPropertyCollection properties;

		#endregion // Fields

		#region Constructors

		static HttpWebRequestElement ()
		{
			maximumErrorResponseLengthProp = new ConfigurationProperty ("maximumErrorResponseLength", typeof (int), 64);
			maximumResponseHeadersLengthProp = new ConfigurationProperty ("maximumResponseHeadersLength", typeof (int), 64);
			maximumUnauthorizedUploadLengthProp = new ConfigurationProperty ("maximumUnauthorizedUploadLength", typeof (int), -1);
			useUnsafeHeaderParsingProp = new ConfigurationProperty ("useUnsafeHeaderParsing", typeof (bool), false);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (maximumErrorResponseLengthProp);
			properties.Add (maximumResponseHeadersLengthProp);
			properties.Add (maximumUnauthorizedUploadLengthProp);
			properties.Add (useUnsafeHeaderParsingProp);
		}

		public HttpWebRequestElement ()
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty("maximumErrorResponseLength", DefaultValue = "64")]
		public int MaximumErrorResponseLength {
			get { return (int) base[maximumErrorResponseLengthProp]; }
			set { base [maximumErrorResponseLengthProp] = value; }
		}

		[ConfigurationProperty("maximumResponseHeadersLength", DefaultValue = "64")]
		public int MaximumResponseHeadersLength {
			get { return (int) base [maximumResponseHeadersLengthProp]; }
			set { base [maximumResponseHeadersLengthProp] = value; }
		}

		[ConfigurationProperty("maximumUnauthorizedUploadLength", DefaultValue = "-1")]
		public int MaximumUnauthorizedUploadLength {
			get { return (int) base [maximumUnauthorizedUploadLengthProp]; }
			set { base [maximumUnauthorizedUploadLengthProp] = value; }
		}

		[ConfigurationProperty("useUnsafeHeaderParsing", DefaultValue = "False")]
		public bool UseUnsafeHeaderParsing {
			get { return (bool) base [useUnsafeHeaderParsingProp]; }
			set { base[useUnsafeHeaderParsingProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties


		#region Methods

		[MonoTODO]
		protected override void PostDeserialize ()
		{
			base.PostDeserialize ();
		}

		#endregion // Methods
	}
}

#endif
