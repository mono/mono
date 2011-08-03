//
// System.Web.Configuration.CustomError
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

using System;
using System.ComponentModel;
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class CustomError : ConfigurationElement
	{
		static ConfigurationProperty redirectProp;
		static ConfigurationProperty statusCodeProp;
		static ConfigurationPropertyCollection properties;

		static CustomError ()
		{
			redirectProp = new ConfigurationProperty ("redirect", typeof (string), null,
								  TypeDescriptor.GetConverter (typeof (string)),
								  new StringValidator (1),
								  ConfigurationPropertyOptions.IsRequired);
			statusCodeProp = new ConfigurationProperty ("statusCode", typeof (int), null,
								    TypeDescriptor.GetConverter (typeof (int)),
								    new IntegerValidator (100, 999),
								    ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (redirectProp);
			properties.Add (statusCodeProp);
		}

		internal CustomError ()
		{
		}

		public CustomError (int statusCode, string redirect)
		{
			this.StatusCode = statusCode;
			this.Redirect = redirect;
		}

		public override bool Equals (object customError)
		{
			CustomError e = customError as CustomError;
			if (e == null)
				return false;

			return (Redirect == e.Redirect && StatusCode == e.StatusCode);
		}

		public override int GetHashCode ()
		{
			return Redirect.GetHashCode () + StatusCode;
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("redirect", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Redirect {
			get { return (string) base [redirectProp];}
			set { base[redirectProp] = value; }
		}

		[IntegerValidator (MinValue = 100, MaxValue = 999)]
		[ConfigurationProperty ("statusCode", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public int StatusCode {
			get { return (int) base [statusCodeProp];}
			set { base[statusCodeProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

