//
// System.Web.Configuration.MachineKeyValidationConverter
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005, 2010 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Configuration;
using System.Globalization;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class MachineKeyValidationConverter : ConfigurationConverterBase
	{
#if NET_4_0
		const string InvalidValue = "The enumeration value must be one of the following: SHA1, MD5, 3DES, AES, HMACSHA256, HMACSHA384, HMACSHA512."; 
#else
		const string InvalidValue = "The enumeration value must be one of the following: SHA1, MD5, 3DES, AES."; 
#endif
		public MachineKeyValidationConverter ()
		{
		}

		public override object ConvertFrom (ITypeDescriptorContext ctx, CultureInfo ci, object data)
		{
			switch ((string) data) {
			case "MD5":
				return MachineKeyValidation.MD5;
			case "SHA1":
				return MachineKeyValidation.SHA1;
			case "3DES":
				return MachineKeyValidation.TripleDES;
			case "AES":
				return MachineKeyValidation.AES;
#if NET_4_0
			case "HMACSHA256":
				return MachineKeyValidation.HMACSHA256;
			case "HMACSHA384":
				return MachineKeyValidation.HMACSHA384;
			case "HMACSHA512":
				return MachineKeyValidation.HMACSHA512;
#endif
			default:
				throw new ArgumentException (InvalidValue);
			}
		}

		public override object ConvertTo (ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
		{
#if NET_4_0
			if ((value == null) || (value.GetType () != typeof (MachineKeyValidation)))
				throw new ArgumentException (InvalidValue);
#else
			if (value.GetType () != typeof (MachineKeyValidation)) {
				/* MS throws this exception on an invalid */
				throw new FormatException (InvalidValue);
			}				
#endif

			switch ((MachineKeyValidation) value) {
			case MachineKeyValidation.MD5:
				return "MD5";
			case MachineKeyValidation.SHA1:
				return "SHA1";
			case MachineKeyValidation.TripleDES:
				return "3DES";
			case MachineKeyValidation.AES:
				return "AES";
#if NET_4_0
			case MachineKeyValidation.HMACSHA256:
				return "HMACSHA256";
			case MachineKeyValidation.HMACSHA384:
				return "HMACSHA384";
			case MachineKeyValidation.HMACSHA512:
				return "HMACSHA512";
			default:
				// includes MachineKeyValidation.Custom
				throw new ArgumentException (InvalidValue);
#else
			default:
				/* MS throws this exception on an invalid */
				throw new FormatException (InvalidValue);
#endif
			}
		}
	}
}

#endif
