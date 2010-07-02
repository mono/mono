//
// Authors:
//   Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://novell.com/)
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
using System.Globalization;

namespace System.Web.Configuration
{
	sealed class VersionConverter : ConfigurationConverterBase
	{
		Version minVersion;
		string exceptionText;

		public VersionConverter ()
		{
		}
		
		public VersionConverter (int minMajor, int minMinor, string exceptionText = null)
		{
			minVersion = new Version (minMajor, minMinor);
			this.exceptionText = exceptionText;
		}
		
		public override object ConvertFrom (ITypeDescriptorContext ctx, CultureInfo ci, object data)
                {
			string input = data as string;

			if (String.IsNullOrEmpty (input))
				throw new ConfigurationErrorsException ("The input string is too short or null.");

			Version result;
			if (!Version.TryParse (input, out result))
				throw new ConfigurationErrorsException ("The input string wasn't in correct format.");

			if (minVersion != null && result < minVersion)
				throw new ConfigurationErrorsException (String.Format (exceptionText, result, minVersion));
			
			return result;
                }

                public override object ConvertTo (ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
                {
			Version ver = value as Version;

			if (ver == null)
				throw new ArgumentException ("Is not an instance of the Version type", "value");
			
                        if (type == typeof (string))
				return ver.ToString ();

			if (type == typeof (Version))
				return ver.Clone ();

                        throw new ConfigurationErrorsException ("Conversion to type '" + type + "' is not supported.");
                }
	}
}
