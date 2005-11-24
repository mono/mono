//
// System.Web.Configuration.MachineKeyValidationConverter
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
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

using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class MachineKeyValidationConverter : ConfigurationConverterBase
	{
		public MachineKeyValidationConverter ()
		{
		}

		public override object ConvertFrom (ITypeDescriptorContext ctx, CultureInfo ci, object data)
		{
			if ((string)data == "MD5")
				return MachineKeyValidation.MD5;
			else if ((string)data == "SHA1")
				return MachineKeyValidation.SHA1;
			else if ((string)data == "3DES")
				return MachineKeyValidation.TripleDES;
			else if ((string)data == "AES")
				return MachineKeyValidation.AES;
			else
				throw new ArgumentException ("The enumeration value must be one of the following: SHA1, MD5, 3DES, AES.");
		}

		public override object ConvertTo (ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
		{
			if (value.GetType () != typeof (MachineKeyValidation)) {
				/* MS throws this exception on an invalid */
				throw new FormatException ("invalid validation value");
			}				

			MachineKeyValidation v = (MachineKeyValidation)value;

			if (v == MachineKeyValidation.MD5) return "MD5";
			else if (v == MachineKeyValidation.SHA1) return "SHA1";
			else if (v == MachineKeyValidation.TripleDES) return "3DES";
			else if (v ==  MachineKeyValidation.AES) return "AES";
			else
				/* MS throws this exception on an invalid */
				throw new FormatException ("invalid validation value");
		}
	}
}

#endif
